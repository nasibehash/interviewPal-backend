var activities = new List<Activity>
{
    new("رستوران", 1m), new("بازار", 1.5m), new("موزه", 2m),
    new("باغ", 2m), new("قایق‌سواری", 3m), new("کوه‌نوردی", 5m),
};

foreach (var plan in Trips.Itineraries(activities, size: 3, maxHours: 6m))
    Console.WriteLine(string.Join(" + ", plan));

record Activity(string Name, decimal Hours);

static class Trips
{
    public static List<List<string>> Itineraries(IEnumerable<Activity> activities, int size, decimal maxHours)
    {
        var sorted = activities.OrderBy(a => a.Hours).ToList();
        var results = new List<List<string>>();
        var chosen = new List<Activity>();

        void Backtrack(int start, decimal hoursUsed)
        {
            if (chosen.Count == size)
            {
                results.Add(chosen.Select(a => a.Name).ToList());
                return;
            }
            for (var i = start; i < sorted.Count; i++)
            {
                var total = hoursUsed + sorted[i].Hours;
                if (total > maxHours) break; // هرس: بعدی‌ها طولانی‌ترند
                chosen.Add(sorted[i]);
                Backtrack(i + 1, total);
                chosen.RemoveAt(chosen.Count - 1); // برگشت
            }
        }

        Backtrack(0, 0m);
        return results;
    }
}
