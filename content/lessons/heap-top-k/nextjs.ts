// app/api/trending/route.ts
import { NextRequest, NextResponse } from 'next/server';

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

interface Trending {
  term: string;
  count: number;
}

// k عبارت پرجست‌وجو: heap کوچک‌ترین‌ها به اندازهٔ k، نه مرتب‌سازی کل عبارت‌ها
function topSearches(counts: ReadonlyMap<string, number>, k: number): Trending[] {
  const heap = new MinHeap<string>();
  for (const [term, count] of counts) {
    heap.push(term, count);
    if (heap.size > k) heap.pop(); // کمترین شمارش را بیرون بینداز؛ heap همیشه بزرگ‌ترین k تا را نگه می‌دارد
  }

  const result: Trending[] = [];
  for (let entry = heap.pop(); entry; entry = heap.pop()) {
    result.push({ term: entry.value, count: entry.priority });
  }
  return result.reverse(); // از پرتکرارترین
}

function countTerms(queries: readonly string[]): Map<string, number> {
  const counts = new Map<string, number>();
  for (const q of queries) counts.set(q, (counts.get(q) ?? 0) + 1);
  return counts;
}

// در پروژهٔ واقعی: SELECT term, COUNT(*) ... GROUP BY term از لاگ جست‌وجو (یا Redis sorted set)
async function loadCounts(): Promise<Map<string, number>> {
  return countTerms(['گوشی', 'لپ‌تاپ', 'گوشی', 'هدفون', 'گوشی', 'لپ‌تاپ', 'ساعت']);
}

// GET /api/trending?k=3
export async function GET(request: NextRequest) {
  const k = Number(request.nextUrl.searchParams.get('k') ?? 5);
  if (!Number.isInteger(k) || k < 1 || k > 50) {
    return NextResponse.json({ error: 'k باید بین ۱ و ۵۰ باشد' }, { status: 400 });
  }
  return NextResponse.json(topSearches(await loadCounts(), k));
}
