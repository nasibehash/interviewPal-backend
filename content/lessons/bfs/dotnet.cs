var friends = new Dictionary<string, string[]>
{
    ["ali"] = ["sara", "reza"],
    ["sara"] = ["ali", "mina"],
    ["reza"] = ["ali", "mina"],
    ["mina"] = ["sara", "reza", "omid"],
    ["omid"] = ["mina"],
    ["nima"] = [],
};

Console.WriteLine(string.Join(" -> ", Network.ShortestPath(friends, "ali", "omid") ?? [])); // ali -> sara -> mina -> omid
Console.WriteLine(Network.ShortestPath(friends, "ali", "nima") is null); // True

static class Network
{
    public static List<string>? ShortestPath(IReadOnlyDictionary<string, string[]> graph, string from, string to)
    {
        if (from == to) return [from];

        var parent = new Dictionary<string, string?> { [from] = null };
        var queue = new Queue<string>();
        queue.Enqueue(from);

        while (queue.TryDequeue(out var user))
        {
            foreach (var friend in graph.GetValueOrDefault(user, []))
            {
                if (!parent.TryAdd(friend, user)) continue; // قبلاً دیده شده
                if (friend == to) return PathTo(parent, to);
                queue.Enqueue(friend);
            }
        }
        return null;
    }

    private static List<string> PathTo(Dictionary<string, string?> parent, string node)
    {
        var path = new List<string>();
        for (string? current = node; current is not null; current = parent[current])
            path.Add(current);
        path.Reverse();
        return path;
    }
}
