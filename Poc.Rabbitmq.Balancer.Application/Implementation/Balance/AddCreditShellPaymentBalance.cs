using System;
using System.Linq;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy;
using Poc.Rabbitmq.Balancer.Application.Dto.Balance;
using Poc.Rabbitmq.Core.Domain.Constant;
using Poc.Rabbitmq.Core.Domain.Exception.Infrastructure.Payment;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Payment;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Dto.Payment;

namespace Poc.Rabbitmq.Balancer.Application.Implementation.Balance
{
    public class AddCreditShellPaymentBalance : IBalanceStrategy
    {
        #region Properties
        private const string PaymentTextConst = "Done by Poc.Rabbitmq.Balancer";
        private readonly IPaymentService _paymentService;
        #endregion

        #region Ctors.
        public AddCreditShellPaymentBalance(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        #endregion

        #region Public_Methods
        public decimal Balance(BaseBalanceDto dto)
        {
            var parameter = (CreditShellPaymentDto)dto;

            var totalAmount = 0M;

            var balanceDueRound = decimal.Round(parameter.BalanceDue, 2, MidpointRounding.ToEven);
            AddPayment(new PaymentToBookingDto()
            {
                PaymentMethodType = PaymentMethodType.CustomerAccount,
                AccountNumber = string.Empty,
                AccountTransactionCode = parameter.AccountTransactionCode,
                BalanceDue = parameter.BalanceDue,
                CurrencyCode = parameter.BookingCurrencyCode,
                PaymentText = PaymentTextConst,
                Signature = parameter.Signature
            });
            totalAmount += balanceDueRound;

            if (parameter.RefundValue > 0M)
            {
                decimal agencyPaymentBalanceDue;
                string currencyCode;
                if (parameter.RefundType.Equals(IncrementTypeConst.Cash))
                {
                    agencyPaymentBalanceDue = parameter.RefundValue * -1;
                    currencyCode = parameter.RefundCurrencyCode;
                }
                else
                {
                    agencyPaymentBalanceDue = parameter.BalanceDue * (parameter.RefundValue / 100);
                    currencyCode = parameter.BookingCurrencyCode;
                }

                var agencyPaymentBalanceDueRound = decimal.Round(agencyPaymentBalanceDue, 2, MidpointRounding.ToEven);
                AddPayment(new PaymentToBookingDto()
                {
                    PaymentMethodType = PaymentMethodType.AgencyAccount,
                    AccountNumber = AccountNumberConst.Increment,
                    AccountTransactionCode = String.Empty,
                    BalanceDue = agencyPaymentBalanceDueRound,
                    CurrencyCode = currencyCode,
                    PaymentText = PaymentTextConst,
                    Signature = parameter.Signature
                });
                totalAmount += agencyPaymentBalanceDueRound;

                AddPayment(new PaymentToBookingDto()
                {
                    PaymentMethodType = PaymentMethodType.CustomerAccount,
                    AccountNumber = string.Empty,
                    AccountTransactionCode = parameter.IncrementAccountTransactionCode,
                    BalanceDue = agencyPaymentBalanceDueRound,
                    CurrencyCode = parameter.BookingCurrencyCode,
                    PaymentText = PaymentTextConst,
                    Signature = parameter.Signature
                });
            }

            return totalAmount;
        }
        #endregion
        
        #region Private_Methods
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
        #endregion
    }
}
