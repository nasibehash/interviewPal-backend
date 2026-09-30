IDiscountStrategy[] strategies =
[
    new NoDiscount(),
    new PercentageDiscount("vip", 10m),
    new PercentageDiscount("firstOrder", 20m, cap: 100_000m),
];

var pricing = new PricingService(strategies);
Console.WriteLine(pricing.Payable(800_000m, "vip"));        // 720000
Console.WriteLine(pricing.Payable(800_000m, "firstOrder")); // 700000

// در ASP.NET Core هر قاعده با services.AddSingleton<IDiscountStrategy, ...>() ثبت
// و PricingService لیست IEnumerable<IDiscountStrategy> را از DI می‌گیرد.

interface IDiscountStrategy
{
    string Name { get; }
    decimal Discount(decimal total);
}

class NoDiscount : IDiscountStrategy
{
    public string Name => "none";
    public decimal Discount(decimal total) => 0m;
}

class PercentageDiscount(string name, decimal percent, decimal cap = decimal.MaxValue) : IDiscountStrategy
{
    public string Name => name;
    public decimal Discount(decimal total) => Math.Min(total * percent / 100m, cap);
}

class PricingService(IEnumerable<IDiscountStrategy> strategies)
{
    private readonly Dictionary<string, IDiscountStrategy> _byName = strategies.ToDictionary(s => s.Name);

    public decimal Payable(decimal total, string strategyName) =>
        _byName.TryGetValue(strategyName, out var strategy)
            ? total - strategy.Discount(total)
            : throw new ArgumentException($"قاعدهٔ تخفیف ناشناخته: {strategyName}");
}
