// heap دودویی کوچک‌ترین‌محور
class MinHeap {
  items = [];

  get size() {
    return this.items.length;
  }

  push(value, priority) {
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

  pop() {
    const items = this.items;
    if (items.length === 0) return undefined;
    const top = items[0];
    const last = items.pop();
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

// k عبارت پرجست‌وجو: heap کوچک‌ترین‌ها به اندازهٔ k، نه مرتب‌سازی کل عبارت‌ها
function topSearches(counts, k) {
  const heap = new MinHeap();
  for (const [term, count] of counts) {
    heap.push(term, count);
    if (heap.size > k) heap.pop(); // کمترین شمارش را بیرون بینداز؛ heap همیشه بزرگ‌ترین k تا را نگه می‌دارد
  }

  const result = [];
  for (let entry = heap.pop(); entry; entry = heap.pop()) {
    result.push({ term: entry.value, count: entry.priority });
  }
  return result.reverse(); // از پرتکرارترین
}

function countTerms(queries) {
  const counts = new Map();
  for (const q of queries) counts.set(q, (counts.get(q) ?? 0) + 1);
  return counts;
}

const queries = ['گوشی', 'لپ‌تاپ', 'گوشی', 'هدفون', 'گوشی', 'لپ‌تاپ', 'ساعت', 'هدفون', 'گوشی', 'قاب', 'لپ‌تاپ'];
console.log(topSearches(countTerms(queries), 3));
// [{ term: 'گوشی', count: 4 }, { term: 'لپ‌تاپ', count: 3 }, { term: 'هدفون', count: 2 }]
