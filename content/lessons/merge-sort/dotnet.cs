var orders = new List<Order>
{
    new(1, "ali", 500m),
    new(2, "sara", 300m),
    new(3, "ali", 300m),
    new(4, "reza", 500m),
};

// توجه: List<T>.Sort پایدار نیست (introsort)؛ OrderBy در LINQ پایدار است
Console.WriteLine("MergeSort : " + string.Join(", ", Sorting.MergeSort(orders, (a, b) => a.Total.CompareTo(b.Total)).Select(o => o.Id))); // 2, 3, 1, 4
Console.WriteLine("OrderBy   : " + string.Join(", ", orders.OrderBy(o => o.Total).Select(o => o.Id))); // 2, 3, 1, 4

record Order(int Id, string Customer, decimal Total);

static class Sorting
{
    public static List<T> MergeSort<T>(IReadOnlyList<T> items, Comparison<T> compare)
    {
        if (items.Count <= 1) return [.. items];
        int middle = items.Count / 2;
        var left = MergeSort(items.Take(middle).ToList(), compare);
        var right = MergeSort(items.Skip(middle).ToList(), compare);

        var result = new List<T>(items.Count);
        int i = 0, j = 0;
        while (i < left.Count && j < right.Count)
            result.Add(compare(left[i], right[j]) <= 0 ? left[i++] : right[j++]);
        result.AddRange(left.Skip(i));
        result.AddRange(right.Skip(j));
        return result;
    }
}
