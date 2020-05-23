
using Poc.Rabbitmq.Balancer.Application.Dto.Balance;

namespace Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy
{
    public interface IBalanceStrategy
    {
        decimal Balance(BaseBalanceDto parameter);
    }
}