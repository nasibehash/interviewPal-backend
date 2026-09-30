// سازندهٔ کوئری جست‌وجوی محصول: فیلترها اختیاری‌اند و ترتیبشان مهم نیست
class ProductQuery {
  #filters = [];
  #params = [];
  #orderBy = 'created_at DESC';
  #limit = 20;

  inCategory(id) {
    this.#filters.push('category_id = ?');
    this.#params.push(id);
    return this; // برگرداندن this زنجیرهٔ روان (fluent) را ممکن می‌کند
  }

  priceBetween(min, max) {
    this.#filters.push('price BETWEEN ? AND ?');
    this.#params.push(min, max);
    return this;
  }

  inStock() {
    this.#filters.push('stock > 0');
    return this;
  }

  sortBy(column) {
    const allowed = { price: 'price', newest: 'created_at DESC' };
    if (!Object.hasOwn(allowed, column)) throw new Error(`ستون مرتب‌سازی مجاز نیست: ${column}`);
    this.#orderBy = allowed[column];
    return this;
  }

  limit(count) {
    this.#limit = Math.min(count, 100);
    return this;
  }

  build() {
    const where = this.#filters.length ? ` WHERE ${this.#filters.join(' AND ')}` : '';
    return {
      sql: `SELECT * FROM products${where} ORDER BY ${this.#orderBy} LIMIT ${this.#limit}`,
      params: [...this.#params], // کپی: تغییر بعدی سازنده روی نتیجه اثر نمی‌گذارد
    };
  }
}

console.log(new ProductQuery().inCategory(5).priceBetween(100_000, 500_000).inStock().sortBy('price').build());
console.log(new ProductQuery().build().sql);
