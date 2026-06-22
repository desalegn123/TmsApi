using System.ComponentModel.DataAnnotations;

public class PaymentOptions
{
    public required string GatewayUrl { get; init; }
    [Range(100, 100000)] public decimal MaxDepositeBirr;
}