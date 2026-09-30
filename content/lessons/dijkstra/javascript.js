// صف اولویت (heap دودویی): کوچک‌ترین اولویت همیشه اول بیرون می‌آید
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

const shipping = {
  تهران: { قم: 20, اصفهان: 70, شیراز: 120, مشهد: 90 },
  قم: { تهران: 20, اصفهان: 30 },
  اصفهان: { تهران: 70, قم: 30, شیراز: 40 },
  شیراز: { تهران: 120, اصفهان: 40 },
  مشهد: { تهران: 90 },
};


// ارزان‌ترین مسیر ارسال بین دو شهر؛ هزینهٔ هر یال نامنفی است
function cheapestRoute(graph, from, to) {
  const cost = new Map([[from, 0]]);
  const previous = new Map();
  const heap = new MinHeap();
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
  while (path[0] !== from) path.unshift(previous.get(path[0]));
  return { cost: cost.get(to), path };
}

console.log(cheapestRoute(shipping, 'تهران', 'شیراز')); // { cost: 90, path: ['تهران', 'قم', 'اصفهان', 'شیراز'] }
console.log(cheapestRoute(shipping, 'مشهد', 'قم')); // { cost: 110, path: ['مشهد', 'تهران', 'قم'] }
