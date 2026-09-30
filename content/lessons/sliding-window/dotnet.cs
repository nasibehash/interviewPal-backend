var limiter = new SlidingWindowLimiter(limit: 3, window: TimeSpan.FromSeconds(60));
var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

foreach (var seconds in new[] { 0, 10, 20, 30, 61 })
    Console.WriteLine($"t={seconds}s -> {limiter.Allow("ali", start.AddSeconds(seconds))}");
// True, True, True, False, True

class SlidingWindowLimiter(int limit, TimeSpan window)
{
    private readonly Dictionary<string, Queue<DateTime>> _hits = new();

    // این کلاس thread-safe نیست؛ در ASP.NET Core از lock یا RateLimiter داخلی استفاده کن
    public bool Allow(string key, DateTime now)
    {
        if (!_hits.TryGetValue(key, out var queue))
            _hits[key] = queue = new Queue<DateTime>();

        while (queue.Count > 0 && queue.Peek() <= now - window)
            queue.Dequeue(); // پنجره از چپ جمع می‌شود

        if (queue.Count >= limit) return false;
        queue.Enqueue(now);
        return true;
    }
}
