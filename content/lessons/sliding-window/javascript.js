// محدودکنندهٔ نرخ: حداکثر `limit` درخواست در هر `windowMs` میلی‌ثانیهٔ اخیر
class SlidingWindowLimiter {
  constructor(limit, windowMs) {
    this.limit = limit;
    this.windowMs = windowMs;
    this.hits = new Map(); // کاربر -> زمان درخواست‌های داخل پنجره (صعودی)
  }

  allow(user, now = Date.now()) {
    const window = this.hits.get(user) ?? [];
    // لبهٔ چپ پنجره: هر درخواست منقضی فقط یک بار از ابتدای صف کنار می‌رود
    while (window.length > 0 && window[0] <= now - this.windowMs) window.shift();

    if (window.length >= this.limit) return false;
    window.push(now); // لبهٔ راست پنجره
    this.hits.set(user, window);
    return true;
  }
}

const limiter = new SlidingWindowLimiter(3, 60_000);
const results = [0, 10_000, 20_000, 30_000, 61_000].map((t) => limiter.allow('ali', t));
console.log(results); // [true, true, true, false, true]
