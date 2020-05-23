namespace Poc.Rabbitmq.Balancer.Application.Dto
{
    public class BookingBalancerResponseDto
    {
        public bool Result { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public decimal AmountRefounded { get; set; }
    }
}
