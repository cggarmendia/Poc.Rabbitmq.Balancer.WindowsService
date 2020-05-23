using System;
using Poc.Rabbitmq.Balancer.Application.Dto.Balance;
using Poc.Rabbitmq.Core.Domain.Constant;

namespace Poc.Rabbitmq.Balancer.Application.Implementation.Balance.Builder
{
    public static class BalanceBuilder
    {
        #region Public_Static_Methods
        public static BaseBalanceDto GetBalanceDto(BalanceRequestDto parameters)
        {
            BaseBalanceDto dto;

            if (parameters.ProcessType.Equals(ProcessTypeConst.CancelCreditShell))
            {
                dto = GetRefundOriginalPaymentDto(parameters);
            }
            else if (parameters.ProcessType.Equals(ProcessTypeConst.AgencyCancelAndAddPayment))
            {
                dto = GetAgencyPaymentDto(parameters);
            }
            else if (parameters.ProcessType.Equals(ProcessTypeConst.CreateVoluntary))
            {
                dto = GetVoluntaryCreditShellPaymentDto(parameters);
            }
            else if (parameters.ProcessType.Equals(ProcessTypeConst.PROCLI))
            {
                dto = GetProcliCreditShellPaymentDto(parameters);
            }
            else
            {
                throw new NotImplementedException($"Process type builder not implemented: {parameters.ProcessType}.");
            }

            return dto;
        }
        #endregion

        #region Public_Static_Methods
        private static CreditShellPaymentDto GetVoluntaryCreditShellPaymentDto(BalanceRequestDto parameters)
        {
            return new CreditShellPaymentDto()
            {
                Signature = parameters.Signature,
                BalanceDue = parameters.Amount,
                BookingCurrencyCode = parameters.RefundCurrencyCode,
                RefundType = parameters.RefundType,
                RefundValue = parameters.RefundValue,
                RefundCurrencyCode = parameters.RefundCurrencyCode,
                AccountTransactionCode = AccountTransactionCodeConst.CreditShellVolunatryRefund,
                IncrementAccountTransactionCode = AccountTransactionCodeConst.CreditShellIncrement
            };
        }

        private static CreditShellPaymentDto GetProcliCreditShellPaymentDto(BalanceRequestDto parameters)
        {
            return new CreditShellPaymentDto()
            {
                Signature = parameters.Signature,
                BalanceDue = parameters.Amount,
                BookingCurrencyCode = parameters.RefundCurrencyCode,
                RefundType = parameters.RefundType,
                RefundValue = parameters.RefundValue,
                RefundCurrencyCode = parameters.RefundCurrencyCode,
                AccountTransactionCode = AccountTransactionCodeConst.CreditShellProcli,
                IncrementAccountTransactionCode = AccountTransactionCodeConst.CreditShellIncrement
            };
        }

        private static AgencyPaymentDto GetAgencyPaymentDto(BalanceRequestDto parameters)
        {
            return new AgencyPaymentDto()
            {
                Signature = parameters.Signature,
                BalanceDue = parameters.Amount,
                AccountNumber = parameters.Booking.SourcePOS.OrganizationCode,
                CurrencyCode = parameters.RefundCurrencyCode
            };
        }

        private static RefundOriginalPaymentDto GetRefundOriginalPaymentDto(BalanceRequestDto parameters)
        {
            return new RefundOriginalPaymentDto()
            {
                Signature = parameters.Signature,
                Booking = parameters.Booking,
                AmountToRefund = parameters.Amount
            };
        }
        #endregion
    }
}
