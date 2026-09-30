var products = new List<Product>
{
    new("کابل", 90_000m),
    new("ماوس", 250_000m),
    new("پاوربانک", 450_000m),
    new("هدفون", 550_000m),
    new("اسپیکر", 1_200_000m),
};

var pair = GiftCards.FindPair(products, 1_000_000m);
Console.WriteLine(pair is { } p ? $"{p.First.Name} + {p.Second.Name}" : "پیدا نشد"); // پاوربانک + هدفون
Console.WriteLine(GiftCards.FindPair(products, 5m) is null); // True

record Product(string Name, decimal Price);

static class GiftCards
{
    // لیست باید بر اساس قیمت مرتب باشد
    public static (Product First, Product Second)? FindPair(IReadOnlyList<Product> sorted, decimal balance)
    {
        int left = 0, right = sorted.Count - 1;
        while (left < right)
        {
            var sum = sorted[left].Price + sorted[right].Price;
            if (sum == balance) return (sorted[left], sorted[right]);
            if (sum < balance) left++;
            else right--;
        }
        return null;
    }
}
