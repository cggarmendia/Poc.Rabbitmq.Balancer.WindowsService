﻿using Poc.Rabbitmq.Core.Infrastructure.Provider.Dto.Common;

namespace Poc.Rabbitmq.Balancer.Application.Dto.Balance
{
    public class BalanceRequestDto

    {
    public string Signature { get; set; }
    public string RecordLocator { get; set; }
    public string Email { get; set; }
    public string ProcessType { get; set; }
    public decimal Amount { get; set; }
    public long InventoryLegId { get; set; }
    public string RefundType { get; set; }
    public decimal RefundValue { get; set; }
    public string RefundCurrencyCode { get; set; }
    public BookingDto Booking { get; set; }
    }
}
