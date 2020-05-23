namespace Poc.Rabbitmq.Balancer.Application.Dto.Balance
{
    public class CreditShellPaymentDto : BaseBalanceDto
    {
        public string Signature { get; set; }
        public string BookingCurrencyCode { get; set; }
        public decimal BalanceDue{ get; set; }
        public decimal RefundValue { get; set; }
        public string RefundType { get; set; }
        public string RefundCurrencyCode { get; set; }
        public string AccountTransactionCode { get; set; }
        public string IncrementAccountTransactionCode { get; set; }
    }
}
