using System;
using System.Linq;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy;
using Poc.Rabbitmq.Balancer.Application.Dto.Balance;
using Poc.Rabbitmq.Core.Domain.Exception.Infrastructure.Payment;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Payment;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Dto.Payment;

namespace Poc.Rabbitmq.Balancer.Application.Implementation.Balance
{
    public class AddAgencyPaymentBalance : IBalanceStrategy
    {
        #region Properties
        private const string PaymentTextConst = "Done by Poc.Rabbitmq.Balancer";
        private readonly IPaymentService _paymentService;
        #endregion

        #region Ctors.
        public AddAgencyPaymentBalance(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        #endregion

        #region Public_Methods
        public decimal Balance(BaseBalanceDto dto)
        {
            var parameter = (AgencyPaymentDto) dto;

            var balanceDueRound = decimal.Round(parameter.BalanceDue, 2, MidpointRounding.ToEven);
            AddPayment(new PaymentToBookingDto()
            {
                PaymentMethodType = PaymentMethodType.AgencyAccount,
                AccountNumber = parameter.AccountNumber,
                AccountTransactionCode = string.Empty,
                BalanceDue = parameter.BalanceDue,
                CurrencyCode = parameter.CurrencyCode,
                PaymentText = PaymentTextConst,
                Signature = parameter.Signature
            });
            return balanceDueRound;
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
