var products = new List<Product>
{
    new("کابل شارژ", 90_000m),
    new("پاوربانک", 450_000m),
    new("هدفون", 700_000m),
    new("اسپیکر", 1_200_000m),
    new("ساعت هوشمند", 2_500_000m),
};

// محصولات بین ۴۰۰ هزار تا ۱.۲ میلیون (لیست از قبل بر اساس قیمت مرتب است)
var inRange = Catalog.InPriceRange(products, 400_000m, 1_200_000m);
Console.WriteLine(string.Join(", ", inRange.Select(p => p.Name)));

// List<T>.BinarySearch اولین عنصر را تضمین نمی‌کند؛ برای عنصر غایب هم مکمل بیتی برمی‌گرداند
var numbers = new List<int> { 1, 3, 3, 3, 5 };
Console.WriteLine($"LowerBound(3) = {Catalog.LowerBound(numbers, 3)}"); // 1

record Product(string Name, decimal Price);

static class Catalog
{
    public static IReadOnlyList<Product> InPriceRange(IReadOnlyList<Product> sorted, decimal min, decimal max)
    {
        // قیمت‌ها گام ۱ ندارند، پس «اولین قیمت بزرگ‌تر از max» را با یک جستجوی جدا پیدا می‌کنیم
        var from = LowerBound(sorted, p => p.Price >= min);
        var to = LowerBound(sorted, p => p.Price > max);
        return sorted.Skip(from).Take(to - from).ToList();
    }

    // اولین اندیسی که شرط برایش true است؛ شرط باید روی لیست مرتب «یکنوا» باشد (false...false true...true)
    public static int LowerBound<T>(IReadOnlyList<T> items, Func<T, bool> isAtOrAfter)
    {
        int lo = 0, hi = items.Count;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2; // نه (lo + hi) / 2 که می‌تواند سرریز کند
            if (isAtOrAfter(items[mid])) hi = mid;
            else lo = mid + 1;
        }
        return lo;
    }

    public static int LowerBound(IReadOnlyList<int> items, int target) => LowerBound(items, x => x >= target);
}
