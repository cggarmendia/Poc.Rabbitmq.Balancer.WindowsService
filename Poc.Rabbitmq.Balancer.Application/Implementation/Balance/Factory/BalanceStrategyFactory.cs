using System.Linq;
using System.Threading;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Factory;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy;
using Poc.Rabbitmq.Core.Infrastructure.Cache.Contracts;

namespace Poc.Rabbitmq.Balancer.Application.Implementation.Balance.Factory
{
    public class BalanceStrategyFactory : IBalanceStrategyFactory
    {
        private readonly ICacheComponent _cacheComponent;
        private readonly Mutex _mutex;

        public BalanceStrategyFactory(ICacheComponent cacheComponent)
        {
            _cacheComponent = cacheComponent;
            _mutex = new Mutex();
        }

        public T GetBalanceStrategy<T>(params object[] objects) where T : class, IBalanceStrategy
        {
            var balanceName = typeof(T).FullName;

            var balanceInstance = default(T);

            if (_cacheComponent.ContainsKey(balanceName))
            {
                balanceInstance = _cacheComponent.TryGetValue<T>(balanceName);
            }
            else
            {
                try
                {
                    if (_mutex.WaitOne())
                    {
                        var constructorInfo = typeof(T).GetConstructor(objects.Select(objectInstance => objectInstance.GetType()).ToArray());
                        if (constructorInfo != null)
                        {
                            balanceInstance = (T)constructorInfo.Invoke(objects);

                            if (!_cacheComponent.ContainsKey(balanceName))
                                _cacheComponent.TryAdd(balanceName, balanceInstance);
                        }
                    }
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }

            return balanceInstance;
        }
    }
}
