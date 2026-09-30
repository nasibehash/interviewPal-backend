int[] cardValues = [10, 30, 40]; // هزار تومان

Console.WriteLine(string.Join(" + ", Payments.FewestCards(cardValues, 60) ?? [])); // 30 + 30
Console.WriteLine(string.Join(" + ", Payments.GreedyCards(cardValues, 60) ?? [])); // 40 + 10 + 10 (۳ کارت)
Console.WriteLine(Payments.FewestCards([20, 50], 30) is null); // True

static class Payments
{
    public static List<int>? FewestCards(int[] values, int target)
    {
        var best = new int[target + 1];   // best[a] = کمترین تعداد کارت برای مبلغ a
        var last = new int[target + 1];   // آخرین کارت استفاده‌شده
        Array.Fill(best, int.MaxValue);
        best[0] = 0;

        for (var amount = 1; amount <= target; amount++)
            foreach (var value in values)
                if (value <= amount && best[amount - value] != int.MaxValue && best[amount - value] + 1 < best[amount])
                {
                    best[amount] = best[amount - value] + 1;
                    last[amount] = value;
                }

        if (best[target] == int.MaxValue) return null;
        var cards = new List<int>();
        for (var amount = target; amount > 0; amount -= last[amount]) cards.Add(last[amount]);
        return cards;
    }

    // حریصانه: همیشه بزرگ‌ترین کارت ممکن؛ ساده و سریع ولی همیشه بهینه نیست
    public static List<int>? GreedyCards(int[] values, int target)
    {
        var cards = new List<int>();
        foreach (var value in values.OrderByDescending(v => v))
            while (target >= value)
            {
                cards.Add(value);
                target -= value;
            }
        return target == 0 ? cards : null;
    }
}
