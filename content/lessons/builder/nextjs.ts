// lib/search-url.ts — تولید URL صفحهٔ جست‌وجو با پارامترهای اختیاری
interface SearchParams {
  q?: string;
  category?: string;
  minPrice?: number;
  page?: number;
}

export class SearchUrlBuilder {
  private readonly params = new URLSearchParams();

  constructor(private readonly path = '/products') {}

  q(value?: string): this {
    if (value?.trim()) this.params.set('q', value.trim());
    return this;
  }

  category(value?: string): this {
    if (value) this.params.set('category', value);
    return this;
  }

  minPrice(value?: number): this {
    if (value !== undefined && value > 0) this.params.set('minPrice', String(value));
    return this;
  }

  page(value?: number): this {
    if (value && value > 1) this.params.set('page', String(value)); // صفحهٔ ۱ همان پیش‌فرض است
    return this;
  }

  build(): string {
    const query = this.params.toString();
    return query ? `${this.path}?${query}` : this.path;
  }
}

export const searchUrl = ({ q, category, minPrice, page }: SearchParams): string =>
  new SearchUrlBuilder().q(q).category(category).minPrice(minPrice).page(page).build();

// مثال: <Link href={searchUrl({ q: 'گوشی', page: 2 })}> ← /products?q=%DA%AF...&page=2
console.log(searchUrl({ q: 'گوشی', page: 2 }));
console.log(searchUrl({})); // /products
