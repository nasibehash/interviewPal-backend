import { useMemo } from 'react';

class MinHeap<T> {
  private readonly items: { priority: number; value: T }[] = [];

  get size(): number {
    return this.items.length;
  }

  push(value: T, priority: number): void {
    const items = this.items;
    items.push({ priority, value });
    let i = items.length - 1;
    while (i > 0) {
      const parent = (i - 1) >> 1;
      if (items[parent].priority <= items[i].priority) break;
      [items[parent], items[i]] = [items[i], items[parent]];
      i = parent;
    }
  }

  pop(): { priority: number; value: T } | undefined {
    const items = this.items;
    if (items.length === 0) return undefined;
    const top = items[0];
    const last = items.pop()!;
    if (items.length > 0) {
      items[0] = last;
      let i = 0;
      for (;;) {
        const left = 2 * i + 1;
        const right = left + 1;
        let smallest = i;
        if (left < items.length && items[left].priority < items[smallest].priority) smallest = left;
        if (right < items.length && items[right].priority < items[smallest].priority) smallest = right;
        if (smallest === i) break;
        [items[smallest], items[i]] = [items[i], items[smallest]];
        i = smallest;
      }
    }
    return top;
  }
}

type Graph = Readonly<Record<string, Readonly<Record<string, number>>>>;

// ارزان‌ترین مسیر ارسال بین دو شهر؛ هزینهٔ هر یال نامنفی است
function cheapestRoute(graph: Graph, from: string, to: string): { cost: number; path: string[] } | null {
  const cost = new Map<string, number>([[from, 0]]);
  const previous = new Map<string, string>();
  const heap = new MinHeap<string>();
  heap.push(from, 0);

  for (let entry = heap.pop(); entry; entry = heap.pop()) {
    const { value: city, priority: spent } = entry;
    if (spent > (cost.get(city) ?? Infinity)) continue; // ورودی قدیمی: مسیر ارزان‌تری قبلاً پیدا شده
    if (city === to) break;

    for (const [next, price] of Object.entries(graph[city] ?? {})) {
      const total = spent + price;
      if (total < (cost.get(next) ?? Infinity)) {
        cost.set(next, total);
        previous.set(next, city);
        heap.push(next, total);
      }
    }
  }

  if (!cost.has(to)) return null;
  const path = [to];
  while (path[0] !== from) path.unshift(previous.get(path[0])!);
  return { cost: cost.get(to)!, path };
}

export function ShippingQuote({ graph, destination }: { graph: Graph; destination: string }) {
  const quote = useMemo(() => cheapestRoute(graph, 'تهران', destination), [graph, destination]);

  if (!quote) return <p>به این شهر ارسال نمی‌کنیم.</p>;
  return (
    <>
      <p>هزینهٔ ارسال: {quote.cost} هزار تومان</p>
      <p>مسیر: {quote.path.join(' ← ')}</p>
    </>
  );
}
