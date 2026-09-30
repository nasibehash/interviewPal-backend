string[] queries = ["گوشی", "لپ‌تاپ", "گوشی", "هدفون", "گوشی", "لپ‌تاپ", "ساعت", "هدفون", "گوشی", "قاب", "لپ‌تاپ"];

var counts = queries.GroupBy(q => q).ToDictionary(g => g.Key, g => g.Count());
foreach (var (term, count) in Trending.TopK(counts, 3))
    Console.WriteLine($"{term}: {count}"); // گوشی: 4، لپ‌تاپ: 3، هدفون: 2

static class Trending
{
    public static List<(string Term, int Count)> TopK(IReadOnlyDictionary<string, int> counts, int k)
    {
        // PriorityQueue کوچک‌ترین اولویت را اول بیرون می‌دهد: min-heap به اندازهٔ k
        var heap = new PriorityQueue<string, int>();
        foreach (var (term, count) in counts)
        {
            heap.Enqueue(term, count);
            if (heap.Count > k) heap.Dequeue(); // کمترین شمارش را بیرون بینداز
        }

        var result = new List<(string, int)>();
        while (heap.TryDequeue(out var term, out var count)) result.Add((term, count));
        result.Reverse();
        return result;
    }
}
