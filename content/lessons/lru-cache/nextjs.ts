// lib/exchange-rates.ts — فقط روی سرور اجرا می‌شود
import 'server-only';

class LruCache<K, V> {
  private readonly entries = new Map<K, V>();

  constructor(private readonly capacity: number) {}

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

interface Rate {
  value: number;
  fetchedAt: number;
}

// یک نمونه برای کل پروسهٔ سرور. با چند instance هر کدام کش خودش را دارد؛ برای اشتراک از Redis استفاده کن
const cache = new LruCache<string, Rate>(200);
const TTL_MS = 60_000;

export async function getExchangeRate(currency: string): Promise<number> {
  const cached = cache.get(currency);
  if (cached && Date.now() - cached.fetchedAt < TTL_MS) return cached.value;

  const response = await fetch(`https://rates.example.com/v1/${encodeURIComponent(currency)}`);
  if (!response.ok) throw new Error(`دریافت نرخ ${currency} ناموفق بود`);
  const { value } = (await response.json()) as { value: number };

  cache.set(currency, { value, fetchedAt: Date.now() });
  return value;
}
