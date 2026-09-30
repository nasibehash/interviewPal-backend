class LruCache<K, V> {
  private readonly entries = new Map<K, V>();

  constructor(private readonly capacity: number) {
    if (!Number.isInteger(capacity) || capacity < 1) throw new RangeError('capacity باید عدد صحیح مثبت باشد');
  }

  get size(): number {
    return this.entries.size;
  }

  get(key: K): V | undefined {
    if (!this.entries.has(key)) return undefined;
    const value = this.entries.get(key) as V;
    this.entries.delete(key);
    this.entries.set(key, value);
    return value;
  }

  set(key: K, value: V): void {
    this.entries.delete(key);
    this.entries.set(key, value);
    if (this.entries.size > this.capacity) {
      const oldest = this.entries.keys().next();
      if (!oldest.done) this.entries.delete(oldest.value);
    }
  }
}

// memoize با کش LRU برای تابع‌های پرهزینه
function memoize<A extends string | number, R>(fn: (arg: A) => R, capacity: number): (arg: A) => R {
  const cache = new LruCache<A, R>(capacity);
  return (arg) => {
    const hit = cache.get(arg);
    if (hit !== undefined) return hit;
    const result = fn(arg);
    cache.set(arg, result);
    return result;
  };
}

let calls = 0;
const slowSquare = memoize((n: number) => (calls++, n * n), 2);
[2, 3, 2, 4, 3].forEach((n) => slowSquare(n));
console.log(calls); // 4: مقدار ۲ از کش آمد ولی ۳ بعد از رفتن از کش دوباره حساب شد
