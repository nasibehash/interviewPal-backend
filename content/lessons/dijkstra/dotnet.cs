var shipping = new Dictionary<string, Dictionary<string, int>>
{
    ["تهران"] = new() { ["قم"] = 20, ["اصفهان"] = 70, ["شیراز"] = 120, ["مشهد"] = 90 },
    ["قم"] = new() { ["تهران"] = 20, ["اصفهان"] = 30 },
    ["اصفهان"] = new() { ["تهران"] = 70, ["قم"] = 30, ["شیراز"] = 40 },
    ["شیراز"] = new() { ["تهران"] = 120, ["اصفهان"] = 40 },
    ["مشهد"] = new() { ["تهران"] = 90 },
};

var route = Router.CheapestRoute(shipping, "تهران", "شیراز");
Console.WriteLine($"{route?.Cost}: {string.Join(" -> ", route?.Path ?? [])}"); // 90: تهران -> قم -> اصفهان -> شیراز
Console.WriteLine(Router.CheapestRoute(shipping, "تهران", "نامعلوم") is null); // True

record Route(int Cost, List<string> Path);

static class Router
{
    public static Route? CheapestRoute(Dictionary<string, Dictionary<string, int>> graph, string from, string to)
    {
        var cost = new Dictionary<string, int> { [from] = 0 };
        var previous = new Dictionary<string, string>();
        var queue = new PriorityQueue<string, int>(); // heap دودویی داخلی .NET
        queue.Enqueue(from, 0);

        while (queue.TryDequeue(out var city, out var spent))
        {
            if (spent > cost.GetValueOrDefault(city, int.MaxValue)) continue; // ورودی قدیمی
            if (city == to) break;

            foreach (var (next, price) in graph.GetValueOrDefault(city) ?? [])
            {
                var total = spent + price;
                if (total >= cost.GetValueOrDefault(next, int.MaxValue)) continue;
                cost[next] = total;
                previous[next] = city;
                queue.Enqueue(next, total);
            }
        }

        if (!cost.ContainsKey(to)) return null;
        var path = new List<string> { to };
        while (path[^1] != from) path.Add(previous[path[^1]]);
        path.Reverse();
        return new Route(cost[to], path);
    }
}
