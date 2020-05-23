using System;
using System.Diagnostics;
using Poc.Rabbitmq.Balancer.Application.Contract;
using Poc.Rabbitmq.Balancer.Application.Dto;
using Poc.Rabbitmq.Balancer.WindowsService.Helpers;
using Vueling.Extensions.Library.DI;
using Vueling.Messaging.RabbitMqEndpoint.Contracts.ServiceLibrary.Consumers.Events;
using Vueling.Messaging.RabbitMqEndpoint.Contracts.ServiceLibrary.Policies;
using Vueling.Messaging.RabbitMqEndpoint.Contracts.ServiceLibrary.Publishers.Events;

namespace Poc.Rabbitmq.Balancer.WindowsService.CommandHandlers
{
    [RegisterService]
    public class BookingBalancerHandler : IHandleEvent<BookingBalancerRequest>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IBookingBalancerApplication _businessLogic;

        public BookingBalancerHandler(IEventPublisher eventPublisher, IBookingBalancerApplication businessLogic)
        {
            _eventPublisher = eventPublisher;
            _businessLogic = businessLogic;
        }

        public void Handle(IEventProxy proxy)
        {
            var command = proxy.GetEvent<BookingBalancerRequest>();

            try
            {
                Handle(command);
                proxy.Completed();
            }            
            catch (Exception ex)
            {
                var errorMessage =
                    $@"Principal Error in: {GetType().Name}, method : Handle, booking: {command.RecordLocator}, innerExceptionType: {ex.GetType()}, innerExceptionMessage: {ex.Message}";
                Trace.TraceError(errorMessage, ex);
                proxy.Failed(PolicyHint.IsTransient, errorMessage);
            }
        }

        public void Handle(BookingBalancerRequest command)
        {
            LoggingHelper.TraceInformation(() => $"BookingBalancerHandler.Handle Start RecordLocator: {command.RecordLocator}");
            
            var response = _businessLogic.ApplyLogic( new BookingBalancerDto()
            {
                RecordLocator = command.RecordLocator,
                Email = command.Email,                
                RefundValue = command.RefundValue,
                RefundType = command.RefundType,
                ProcessType = command.ProcessType,
                RefundCurrencyCode = command.RefundCurrencyCode,
                Amount = command.Amount
            });

            _eventPublisher.PublishEvent(new CrmNotifierRequest()
            {
                Result = response.Result,
                ErrorCode = response.ErrorCode,
                ErrorMessage = response.ErrorMessage,
                RecordLocator = command.RecordLocator,
                Email = command.Email,
                ExternalId = command.ExternalId,
                Amount = response.AmountRefounded,
                OriginalAmount = command.Amount,
                ContactFirstname = command.ContactFirstname ?? string.Empty,
                BookingId = command.BookingId,
                ExpirationDate = command.ExpirationDate,
                Currency = command.Currency,
                ContactLastname = command.ContactLastname ?? string.Empty,
                CultureCode = command.CultureCode ?? string.Empty,
                RefundValue = command.RefundValue,
                RefundCurrencyCode = command.RefundCurrencyCode,
                RefundType = command.RefundType,
                ProcessType = command.ProcessType,
                FlightProcessedList = command.FlightProcessedList
            });

            LoggingHelper.TraceInformation(() => $"BookingBalancerHandler.Handle End RecordLocator: {command.RecordLocator}");
        }
    }
}
