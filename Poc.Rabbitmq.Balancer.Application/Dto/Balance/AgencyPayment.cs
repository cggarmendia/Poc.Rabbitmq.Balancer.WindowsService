namespace Poc.Rabbitmq.Balancer.Application.Dto.Balance
{
    public class AgencyPaymentDto : BaseBalanceDto
    {
        public string Signature { get; set; }
        public string CurrencyCode { get; set; }
        public decimal BalanceDue { get; set; }
        public string AccountNumber { get; set; }
    }
}
