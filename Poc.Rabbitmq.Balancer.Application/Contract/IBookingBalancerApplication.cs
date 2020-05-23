using Poc.Rabbitmq.Balancer.Application.Dto;

namespace Poc.Rabbitmq.Balancer.Application.Contract
{
    public interface IBookingBalancerApplication
    {
        BookingBalancerResponseDto ApplyLogic(BookingBalancerDto parameters);
    }
}
