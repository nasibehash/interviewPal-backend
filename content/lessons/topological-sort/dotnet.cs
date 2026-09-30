var prerequisites = new Dictionary<string, string[]>
{
    ["الگوریتم"] = ["ساختار داده", "ریاضی گسسته"],
    ["ساختار داده"] = ["برنامه‌نویسی"],
    ["برنامه‌نویسی"] = [],
    ["ریاضی گسسته"] = [],
};

Console.WriteLine(string.Join(" -> ", StudyPlan.Order(prerequisites)));
// یک ترتیب معتبر (ترتیب درس‌های هم‌سطح ممکن است فرق کند): ریاضی گسسته -> برنامه‌نویسی -> ساختار داده -> الگوریتم

try
{
    StudyPlan.Order(new Dictionary<string, string[]> { ["الف"] = ["ب"], ["ب"] = ["الف"] });
}
catch (InvalidOperationException e)
{
    Console.WriteLine(e.Message);
}

static class StudyPlan
{
    public static List<string> Order(IReadOnlyDictionary<string, string[]> prerequisites)
    {
        var missing = new Dictionary<string, int>();
        var unlocks = new Dictionary<string, List<string>>();

        foreach (var (course, needs) in prerequisites)
        {
            missing[course] = needs.Length;
            foreach (var need in needs)
            {
                if (!unlocks.TryGetValue(need, out var list)) unlocks[need] = list = [];
                list.Add(course);
                missing.TryAdd(need, 0);
            }
        }

        var ready = new Queue<string>(missing.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var order = new List<string>();
        while (ready.TryDequeue(out var course))
        {
            order.Add(course);
            foreach (var next in unlocks.GetValueOrDefault(course, []))
                if (--missing[next] == 0) ready.Enqueue(next);
        }

        return order.Count == missing.Count
            ? order
            : throw new InvalidOperationException("پیش‌نیازها دور دارند؛ ترتیبی وجود ندارد");
    }
}
