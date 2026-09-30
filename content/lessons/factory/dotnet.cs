var factory = new PaymentGatewayFactory();

foreach (var method in new[] { "card", "wallet", "installment" })
{
    var receipt = factory.Create(method).Charge(500_000m);
    Console.WriteLine($"{receipt.Method}: {receipt.Amount} + کارمزد {receipt.Fee}");
}

try { factory.Create("bitcoin"); }
catch (ArgumentException e) { Console.WriteLine(e.Message); }

record Receipt(string Method, decimal Amount, decimal Fee);

interface IPaymentGateway
{
    Receipt Charge(decimal amount);
}

class FeeGateway(string method, decimal feeRate) : IPaymentGateway
{
    public Receipt Charge(decimal amount) => new(method, amount, Math.Round(amount * feeRate));
}

class PaymentGatewayFactory
{
    // switch expression: هر روش یک شاخه؛ روش ناشناخته خطای صریح می‌دهد
    public IPaymentGateway Create(string method) => method switch
    {
        "card" => new FeeGateway("card", 0.01m),
        "wallet" => new FeeGateway("wallet", 0m),
        "installment" => new FeeGateway("installment", 0.03m),
        _ => throw new ArgumentException($"روش پرداخت ناشناخته: {method}", nameof(method)),
    };
}
