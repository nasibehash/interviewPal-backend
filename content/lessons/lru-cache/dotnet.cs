var cache = new LruCache<int, string>(capacity: 2);
cache.Set(1, "الف");
cache.Set(2, "ب");
cache.TryGet(1, out _);   // ۱ تازه‌ترین شد
cache.Set(3, "ج");        // ظرفیت پر است: ۲ (قدیمی‌ترین) بیرون می‌رود

Console.WriteLine(cache.TryGet(2, out _)); // False
Console.WriteLine(cache.TryGet(1, out var value) + " " + value); // True الف

class LruCache<TKey, TValue>(int capacity) where TKey : notnull
{
    // لیست پیوندی ترتیب استفاده را نگه می‌دارد (اول = تازه‌ترین)؛ Dictionary دسترسی O(1) به گره را می‌دهد
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _nodes = new();

    public bool TryGet(TKey key, out TValue? value)
    {
        if (_nodes.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _order.AddFirst(node); // جابه‌جایی گره O(1) است و نیازی به حذف/ساخت دوباره نیست
            value = node.Value.Value;
            return true;
        }
        value = default;
        return false;
    }

    public void Set(TKey key, TValue value)
    {
        if (_nodes.TryGetValue(key, out var existing))
        {
            _order.Remove(existing);
            _nodes.Remove(key);
        }

        _nodes[key] = _order.AddFirst((key, value));

        if (_nodes.Count > capacity)
        {
            var oldest = _order.Last!;
            _order.RemoveLast();
            _nodes.Remove(oldest.Value.Key);
        }
    }
}
