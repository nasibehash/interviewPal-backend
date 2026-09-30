var rows = new List<Row>
{
    new(2, new DateOnly(2026, 3, 1), 250_000m, "Digikala"),
    new(3, new DateOnly(2026, 3, 1), 90_000m, "Snapp"),
    new(4, new DateOnly(2026, 3, 1), 250_000m, " digikala "),
    new(5, new DateOnly(2026, 3, 2), 90_000m, "Snapp"),
};

foreach (var group in Import.FindDuplicates(rows))
    Console.WriteLine("تکراری در خط‌های " + string.Join(", ", group.Select(r => r.Line))); // 2, 4

// HashSet.Add اگر عنصر قبلاً بوده باشد false برمی‌گرداند: ساده‌ترین تشخیص تکراری
var seen = new HashSet<string>();
Console.WriteLine(seen.Add("a") + " " + seen.Add("a")); // True False

record Row(int Line, DateOnly Date, decimal Amount, string Merchant)
{
    // record برابری مقداری دارد، اما اینجا نام پذیرنده را نرمال می‌کنیم
    public (DateOnly, decimal, string) Key => (Date, Amount, Merchant.Trim().ToLowerInvariant());
}

static class Import
{
    // GroupBy پشت صحنه از جدول هش استفاده می‌کند؛ کلید tuple برابری مقداری دارد
    public static IEnumerable<IGrouping<(DateOnly, decimal, string), Row>> FindDuplicates(IEnumerable<Row> rows) =>
        rows.GroupBy(r => r.Key).Where(g => g.Count() > 1);
}
