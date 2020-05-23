using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy;
using Poc.Rabbitmq.Balancer.Application.Dto.Balance;
using Poc.Rabbitmq.Core.Domain.Configuration;
using Poc.Rabbitmq.Core.Domain.Exception.BookingBalancer;
using Poc.Rabbitmq.Core.Domain.Exception.Infrastructure.Payment;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Booking;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Fee;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Payment;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Dto.Common;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Dto.Payment;

namespace Poc.Rabbitmq.Balancer.Application.Implementation.Balance
{
    public class RefundOriginalPaymentBalance : IBalanceStrategy
    {
        #region Properties
        private const string CommentConst = "Done by Poc.Rabbitmq.Balancer";
        private readonly IPaymentService _paymentService;
        private readonly IFeeService _feeService;
        private readonly IConfiguration _configuration;
        private readonly IBookingCommiter _bookingCommiter;
        #endregion

        #region Ctors.
        public RefundOriginalPaymentBalance(IPaymentService paymentService, 
            IFeeService feeService,
            IConfiguration configuration,
            IBookingCommiter bookingCommiter)
        {
            _paymentService = paymentService;
            _feeService = feeService;
            _configuration = configuration;
            _bookingCommiter = bookingCommiter;
        }
        #endregion

        #region Public_Methods
        public decimal Balance(BaseBalanceDto dto)
        {
            try
            {
                var parameter = (RefundOriginalPaymentDto) dto;
            
                var amountToRefundPending = parameter.AmountToRefund;
                var amountRefunded = decimal.Zero;
                var validPayments = GetValidPayments(parameter.Booking);

                foreach (var validPayment in validPayments)
                {
                    if (amountToRefundPending == decimal.Zero)
                        break;

                    var amountToRefundInCurrentPayment = GetAmountToRefundInCurrentPayment(validPayment.CollectedAmount, ref amountToRefundPending);

                    amountRefunded += TryAddPayment(parameter, amountToRefundInCurrentPayment, validPayment);
                }

                if (amountRefunded < parameter.AmountToRefund)
                {
                    var amountPendingTransfer = parameter.AmountToRefund - amountRefunded;
                    _feeService.SellServiceFee(parameter.Booking.CurrencyCode, _configuration.CrmPendingTransferFeeCode, parameter.Signature,
                        amountPendingTransfer);
                    _bookingCommiter.BookingCommit(parameter.Booking.RecordLocator, $"{CommentConst}, Pending to refund.", string.Empty, parameter.Signature);
                    var amountPendingTransferRounded = Math.Abs(decimal.Round(amountPendingTransfer, 2,
                        MidpointRounding.ToEven));
                    throw new PendingTransferException($"the booking has a pending transfer <<{amountPendingTransferRounded}>>.", amountRefunded);
                }

                return amountRefunded;
            }
            catch (PendingTransferException ex)
            {
                var errorMessage =
                    $"Error in: {GetType().FullName}, method: RefundOriginalPayment, exception: PendingTransferException, message: {ex.Message}.";
                throw new PendingTransferException(errorMessage, ex.AmountRefounded);
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"Error in: {GetType().FullName}, method: RefundOriginalPayment, exception: RefundOriginalPaymentException, message: {ex.Message}.";
                throw new RefundOriginalPaymentException(errorMessage);
            }
        }
        #endregion

        #region Private_Methods
        private decimal TryAddPayment(RefundOriginalPaymentDto parameter, decimal amountToRefundInCurrentPayment,
            PaymentDto validPayment)
        {
            var amountRefunded = decimal.Zero;
            try
            {
                AddPayment(GetPaymentToBookingParam(parameter, amountToRefundInCurrentPayment, validPayment));
                amountRefunded += amountToRefundInCurrentPayment;
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $@"Error in: {GetType().Name}, method : Balance, booking: {parameter.Booking.RecordLocator}, innerExceptionType: {ex.GetType()}, innerExceptionMessage: {ex.Message}";
                Trace.TraceError(errorMessage, ex);
            }
            return amountRefunded;
        }

        private PaymentDto AddPayment(PaymentToBookingDto paymentToBookingParam)
        {
            try
            {
                var addPaymentToBookingResponse = _paymentService.AddPayment(paymentToBookingParam);

                if (addPaymentToBookingResponse?.ValidationPayment != null &&
                    addPaymentToBookingResponse.ValidationPayment.PaymentValidationErrors.Any())
                {
                    throw new Exception(addPaymentToBookingResponse.ValidationPayment.PaymentValidationErrors.First().ErrorDescription);
                }

                return addPaymentToBookingResponse.ValidationPayment.Payment;
            }
            catch (Exception ex)
            {
                var errorMessage = $@"Error in: {GetType().FullName}, method : AddPayment, paymentMethodType: {paymentToBookingParam.PaymentMethodType}, innerExceptionType: {ex.GetType()}, innerExceptionMessage: {ex.Message}";
                throw new AddPaymentException(errorMessage, ex);
            }
        }

        private static decimal GetAmountToRefundInCurrentPayment(decimal refundablePaymentAmount, ref decimal amountToRefundPending)
        {
            var amountToRefundInCurrentPayment = decimal.Zero;
            if (refundablePaymentAmount <= amountToRefundPending)
            {
                amountToRefundInCurrentPayment = refundablePaymentAmount;
                amountToRefundPending = amountToRefundPending - refundablePaymentAmount;
            }
            else
            {
                amountToRefundInCurrentPayment = amountToRefundPending;
                amountToRefundPending = decimal.Zero;
            }

            return amountToRefundInCurrentPayment;
        }

        private PaymentToBookingDto GetPaymentToBookingParam(RefundOriginalPaymentDto parameter,
            decimal amountToRefundPending,
            PaymentDto validPayment)
        {
            var paymentMethodType = validPayment.PaymentMethodType;

            var paymentToBookingParam = new PaymentToBookingDto()
            {
                Signature = parameter.Signature,
                BalanceDue = -amountToRefundPending,
                CurrencyCode = validPayment.CurrencyCode,
                AccountNumber = validPayment.AccountNumber,
                IsCreditFile = false,
                PaymentMethodType = paymentMethodType,
                PaymentMethodCode = validPayment.PaymentMethodCode,
                PaymentText = CommentConst,
                ExpirationDate = validPayment.Expiration,
                PaymentFields = validPayment.PaymentFieldsList,
                ParentPaymentId = validPayment.PaymentId,
                AccountNumberId = validPayment.AccountNumberId
            };

            if (paymentMethodType == PaymentMethodType.ExternalAccount || paymentMethodType == PaymentMethodType.PrePaid)
            {
                paymentToBookingParam.AccountNumberId = validPayment.AccountNumberId;
                paymentToBookingParam.ExpirationDate = validPayment.Expiration;
            }

            return paymentToBookingParam;
        }

        private IOrderedEnumerable<PaymentDto> GetValidPayments(BookingDto booking)
        {
            var validPaymentsWithOriginalCollectedAmount = _paymentService.GetValidPayments(booking.Payments);

            var validPaymentWithCollectedAmountUpdated = UpdateValidPaymentCollectedAmount(validPaymentsWithOriginalCollectedAmount, booking);

            return validPaymentWithCollectedAmountUpdated.OrderBy(payment => payment.CollectedAmount);
        }

        private IEnumerable<PaymentDto> UpdateValidPaymentCollectedAmount(IEnumerable<PaymentDto> validPaymentsWithOriginalCollectedAmount,
            BookingDto booking)
        {
            var result = new List<PaymentDto>();
            foreach (var validPayment in validPaymentsWithOriginalCollectedAmount)
            {
                validPayment.CollectedAmount = validPayment.CollectedAmount + booking.Payments
                                                   .Where(p => p.ParentPaymentId.Equals(validPayment.PaymentId))
                                                   .Sum(p => p.CollectedAmount);
                if (validPayment.CollectedAmount > 0)
                {
                    result.Add(validPayment);
                }
            }

            return result;
        }
        #endregion
    }
}
