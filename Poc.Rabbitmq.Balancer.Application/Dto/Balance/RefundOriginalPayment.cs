using Poc.Rabbitmq.Core.Infrastructure.Provider.Dto.Common;

namespace Poc.Rabbitmq.Balancer.Application.Dto.Balance
{
    public class RefundOriginalPaymentDto : BaseBalanceDto
    { 
        public BookingDto Booking{ get; set; }
        public string Signature { get; set; }
        public decimal AmountToRefund { get; set; }
    }
}
