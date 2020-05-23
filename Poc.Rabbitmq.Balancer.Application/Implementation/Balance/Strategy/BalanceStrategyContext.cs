using System;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Factory;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy;
using Poc.Rabbitmq.Balancer.Application.Dto.Balance;
using Poc.Rabbitmq.Balancer.Application.Implementation.Balance.Builder;
using Poc.Rabbitmq.Core.Domain.Configuration;
using Poc.Rabbitmq.Core.Domain.Constant;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Booking;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Fee;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Payment;

namespace Poc.Rabbitmq.Balancer.Application.Implementation.Balance.Strategy
{
    public class BalanceStrategyContext : IBalancerStrategyContext
    {
        #region Properties
        private readonly IPaymentService _paymentService;
        private readonly IFeeService _feeService;
        private readonly IConfiguration _configuration;
        private readonly IBookingCommiter _bookingCommiter;
        private readonly IBalanceStrategyFactory _balanceStrategyFactory;
        private IBalanceStrategy BalanceStrategy { get; set; }
        #endregion

        #region Ctors.
        public BalanceStrategyContext(IPaymentService paymentService,
            IFeeService feeService,
            IConfiguration configuration,
            IBookingCommiter bookingCommiter,
            IBalanceStrategyFactory balanceStrategyFactory)
        {
            _paymentService = paymentService;
            _feeService = feeService;
            _configuration = configuration;
            _bookingCommiter = bookingCommiter;
            _balanceStrategyFactory = balanceStrategyFactory;
        }
        #endregion

        #region Public_Methods
        public void SetStrategy(string processType)
        {
            if (processType.Equals(ProcessTypeConst.CancelCreditShell))
            {
                BalanceStrategy = _balanceStrategyFactory.GetBalanceStrategy<RefundOriginalPaymentBalance>(_paymentService, _feeService, _configuration, _bookingCommiter);
            }
            else if (processType.Equals(ProcessTypeConst.AgencyCancelAndAddPayment))
            {
                BalanceStrategy = _balanceStrategyFactory.GetBalanceStrategy<AddAgencyPaymentBalance>(_paymentService);
            }
            else if (processType.Equals(ProcessTypeConst.CreateVoluntary))
            {
                BalanceStrategy = _balanceStrategyFactory.GetBalanceStrategy<AddCreditShellPaymentBalance>();
            }
            else if (processType.Equals(ProcessTypeConst.PROCLI))
            {
                BalanceStrategy = _balanceStrategyFactory.GetBalanceStrategy<AddCreditShellPaymentBalance>();
            }
            else
            {
                throw  new NotImplementedException($"");
            }
        }

        public decimal DoBalance(BalanceRequestDto parameters)
        {
            return  BalanceStrategy.Balance(BalanceBuilder.GetBalanceDto(parameters));
        }
        #endregion
    }
}
