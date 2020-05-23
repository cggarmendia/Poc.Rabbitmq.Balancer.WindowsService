using System;
using System.Diagnostics;
using Poc.Rabbitmq.Balancer.Application.Contract;
using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy;
using Poc.Rabbitmq.Balancer.Application.Dto;
using Poc.Rabbitmq.Balancer.Application.Dto.Balance;
using Poc.Rabbitmq.Core.Domain.Exception;
using Poc.Rabbitmq.Core.Domain.Exception.BookingBalancer;
using Poc.Rabbitmq.Core.Domain.Exception.Infrastructure.Booking;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Authentication;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Contracts.Booking;
using Poc.Rabbitmq.Core.Infrastructure.Provider.Dto.Authentication;
using Poc.Rabbitmq.Core.Specification.Contracts.Strategy;
using Poc.Rabbitmq.Core.Specification.Dto.Validation;

namespace Poc.Rabbitmq.Balancer.Application.Implementation
{
    public class BookingBalancerApplication : IBookingBalancerApplication
    {
        #region Properties
        private const string CommentConst = "Done by Poc.Rabbitmq.Balancer";
        private readonly IAuthenticationService _authenticationService;
        private readonly IBalancerStrategyContext _balancerStrategyContext;
        private readonly IBookingCommiter _bookingCommiter;
        private readonly IBookingService _bookingService;
        private readonly IValidationStrategyContext _validationStrategyContext;
        #endregion

        #region Ctor.
        public BookingBalancerApplication(IAuthenticationService authenticationService,
            IBalancerStrategyContext balancerStrategyContext,
            IBookingCommiter bookingCommiter,
            IBookingService bookingService,
            IValidationStrategyContext validationStrategyContext)
        {
            _authenticationService = authenticationService;
            _balancerStrategyContext = balancerStrategyContext;
            _bookingCommiter = bookingCommiter;
            _bookingService = bookingService;
            _validationStrategyContext = validationStrategyContext;
        }
        #endregion

        #region Public_Methods
        public BookingBalancerResponseDto ApplyLogic(BookingBalancerDto parameters)
        {
            SessionDto sessionDto = null;
            try
            {
                sessionDto = _authenticationService.LogOn();

                var pureBooking = _bookingService.GetPureBooking(parameters.RecordLocator, 
                    sessionDto.SessionToken);

                _validationStrategyContext.SetStrategy(parameters.ProcessType);
                _validationStrategyContext.DoValidations( new ValidationDto()
                {
                    Booking = pureBooking,
                    AmountToRefund = parameters.Amount
                });

                _balancerStrategyContext.SetStrategy(parameters.ProcessType);
                var amountRefounded = _balancerStrategyContext.DoBalance(new BalanceRequestDto()
                {
                    Booking = pureBooking,
                    RefundCurrencyCode = parameters.RefundCurrencyCode,
                    RefundValue = parameters.RefundValue,
                    RefundType = parameters.RefundType,
                    RecordLocator = parameters.RecordLocator,
                    Amount = parameters.Amount,
                    Signature = sessionDto.SecureToken,
                    ProcessType = parameters.ProcessType,
                    Email = parameters.Email,
                    InventoryLegId = parameters.InventoryLegId
                });

                _bookingCommiter.BookingCommit(parameters.RecordLocator, CommentConst, 
                    string.Empty, sessionDto.SecureToken);

                return new BookingBalancerResponseDto()
                {
                    Result = true,
                    AmountRefounded = amountRefounded
                };
            }
            catch (System.TimeoutException ex)
            {
                var errorMessage =
                    $"Error in: {GetType().FullName}, method: ApplyLogic, exception: TimeoutException, message: {ex.Message}.";       
                throw new Exception(errorMessage);
            }
            catch (ConcurrentCommitOperationException ex)
            {
                var errorMessage =
                    $"Error in: {GetType().FullName}, method: ApplyLogic, exception: ConcurrentCommitOperationException, message: {ex.Message}.";
                throw new Exception(errorMessage);
            }
            catch (BaseException ex)
            {
                var errorMessage =
                    $@"Principal Error in: {GetType().Name}, method : Handle, booking: {parameters.RecordLocator}, innerExceptionType: {ex.GetType()}, innerExceptionMessage: {ex.Message}";
                Trace.TraceError(errorMessage, ex);

                return new BookingBalancerResponseDto()
                {
                    Result = false,
                    ErrorCode = ex.GetType().Name,
                    ErrorMessage = errorMessage,
                    AmountRefounded = GetAmountRefounded(ex)
                };
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $@"Principal Error in: {GetType().Name}, method : Handle, booking: {parameters.RecordLocator}, innerExceptionType: {ex.GetType()}, innerExceptionMessage: {ex.Message}";
                Trace.TraceError(errorMessage, ex);

                return new BookingBalancerResponseDto()
                {
                    Result = false,
                    ErrorCode = "System.Exception",
                    ErrorMessage = errorMessage,
                    AmountRefounded = decimal.Zero
                };
            }
            finally
            {
                if (sessionDto != null)
                {
                    _authenticationService.LogOut(sessionDto);
                }
            }
        }
        #endregion

        #region Private_Methods
        private static decimal GetAmountRefounded(BaseException ex)
        {
            return ex.GetType().Name.Equals(nameof(PendingTransferException)) 
                ? ((PendingTransferException)ex).AmountRefounded 
                : decimal.Zero;
        }
        #endregion
    }
}
