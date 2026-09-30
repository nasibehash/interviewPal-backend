// کش LRU: وقتی جا تمام شد، «کمتر از همه اخیراً استفاده‌شده» بیرون می‌رود
// Map ترتیب درج را نگه می‌دارد، پس اولین کلید همیشه قدیمی‌ترین است
class LruCache {
  constructor(capacity) {
    this.capacity = capacity;
    this.entries = new Map();
  }

  get(key) {
    if (!this.entries.has(key)) return undefined;
    const value = this.entries.get(key);
    this.entries.delete(key); // برای «تازه‌ترین» شدن: حذف و دوباره درج در انتها
    this.entries.set(key, value);
    return value;
  }

  set(key, value) {
    this.entries.delete(key);
    this.entries.set(key, value);
    if (this.entries.size > this.capacity) {
      this.entries.delete(this.entries.keys().next().value); // قدیمی‌ترین را بینداز
    }
  }
}

// کش پاسخ API محصولات: ظرفیت ۲ برای نمایش
const cache = new LruCache(2);
async function fetchProduct(id) {
  const cached = cache.get(id);
  if (cached) return { ...cached, fromCache: true };
  const product = { id, name: `محصول ${id}` }; // در واقعیت: await fetch(`/api/products/${id}`)
  cache.set(id, product);
  return { ...product, fromCache: false };
}

for (const id of [1, 2, 1, 3, 2]) console.log(id, (await fetchProduct(id)).fromCache);
// 1 false، 2 false، 1 true، 3 false (۲ بیرون می‌رود)، 2 false (باید دوباره گرفته شود)
