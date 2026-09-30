var trie = new Trie();
foreach (var word in new[] { "گوشی", "گوشی سامسونگ", "گوشی شیائومی", "گوشواره", "لپ‌تاپ" })
    trie.Insert(word);

Console.WriteLine(string.Join(" | ", trie.Suggest("گوش")));   // گوشی | گوشی سامسونگ | گوشی شیائومی | گوشواره
Console.WriteLine(trie.Suggest("لپ‌تاپ اپل").Count);          // 0

class Trie
{
    private sealed class Node
    {
        public Dictionary<char, Node> Children { get; } = new();
        public bool IsWord { get; set; }
    }

    private readonly Node _root = new();

    // «ي» و «ك» عربی را به «ی» و «ک» فارسی تبدیل می‌کند
    private static string Normalize(string text) =>
        text.Trim().ToLowerInvariant().Replace('ي', 'ی').Replace('ك', 'ک');

    public void Insert(string word)
    {
        var node = _root;
        foreach (var c in Normalize(word))
        {
            if (!node.Children.TryGetValue(c, out var next))
                node.Children[c] = next = new Node();
            node = next;
        }
        node.IsWord = true;
    }

    public List<string> Suggest(string prefix, int limit = 5)
    {
        var typed = Normalize(prefix);
        var node = _root;
        foreach (var c in typed)
            if (!node.Children.TryGetValue(c, out node)) return [];

        var results = new List<string>();
        void Collect(Node current, string text)
        {
            if (results.Count >= limit) return;
            if (current.IsWord) results.Add(text);
            foreach (var (c, child) in current.Children) Collect(child, text + c);
        }
        Collect(node, typed);
        return results;
    }
}
