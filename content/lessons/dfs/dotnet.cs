var catalog = new Category(1, "کالای دیجیتال",
[
    new(2, "موبایل",
    [
        new(4, "گوشی", []),
        new(5, "لوازم جانبی", [new(7, "قاب", [])]),
    ]),
    new(3, "لپ‌تاپ", [new(6, "گیمینگ", [])]),
]);

Console.WriteLine(string.Join(" > ", catalog.FindPath(7) ?? [])); // کالای دیجیتال > موبایل > لوازم جانبی > قاب
Console.WriteLine(catalog.FindPath(99) is null); // True
Console.WriteLine(string.Join(", ", catalog.Find(2)!.Descendants().Select(c => c.Id))); // 2, 4, 5, 7

record Category(int Id, string Name, IReadOnlyList<Category> Children)
{
    // مسیر تا یک دسته با DFS و backtrack
    public List<string>? FindPath(int id)
    {
        if (Id == id) return [Name];
        foreach (var child in Children)
        {
            var found = child.FindPath(id);
            if (found is not null) return [Name, .. found];
        }
        return null;
    }

    // خود دسته و همهٔ زیردسته‌ها به ترتیب DFS (iterator lazy)
    public IEnumerable<Category> Descendants()
    {
        yield return this;
        foreach (var child in Children)
            foreach (var node in child.Descendants())
                yield return node;
    }

    public Category? Find(int id) => Descendants().FirstOrDefault(c => c.Id == id);
}
