interface LimiterOptions {
  limit: number;
  windowMs: number;
}

class SlidingWindowLimiter {
  private readonly hits = new Map<string, number[]>();

  constructor(private readonly options: LimiterOptions) {}

  allow(key: string, now: number = Date.now()): boolean {
    const window = this.hits.get(key) ?? [];
    while (window.length > 0 && window[0] <= now - this.options.windowMs) window.shift();

    if (window.length >= this.options.limit) return false;
    window.push(now);
    this.hits.set(key, window);
    return true;
  }
}

const limiter = new SlidingWindowLimiter({ limit: 3, windowMs: 60_000 });
console.log([0, 10_000, 20_000, 30_000, 61_000].map((t) => limiter.allow('ali', t)));

// نسخهٔ پنجرهٔ ثابت روی آرایه: بیشترین فروش در k روز پشت‌سرهم
function bestWindowSum(values: readonly number[], k: number): number {
  let sum = values.slice(0, k).reduce((a, b) => a + b, 0);
  let best = sum;
  for (let i = k; i < values.length; i++) {
    sum += values[i] - values[i - k]; // یکی وارد، یکی خارج؛ نه جمع دوباره
    best = Math.max(best, sum);
  }
  return best;
}
console.log(bestWindowSum([3, 9, 2, 8, 1, 7], 3)); // 19 (9 + 2 + 8)
