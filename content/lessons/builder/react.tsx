import { useMemo, useState } from 'react';

interface ProductFilter {
  q?: string;
  minPrice?: number;
  inStock?: boolean;
}

// Builder برای URL جست‌وجو: فقط فیلترهای پر شده وارد query می‌شوند
function buildSearchUrl(filter: ProductFilter): string {
  const params = new URLSearchParams();
  if (filter.q) params.set('q', filter.q);
  if (filter.minPrice !== undefined) params.set('minPrice', String(filter.minPrice));
  if (filter.inStock) params.set('inStock', '1');
  const query = params.toString();
  return query ? `/products?${query}` : '/products';
}

export function ProductFilters() {
  const [filter, setFilter] = useState<ProductFilter>({});
  const url = useMemo(() => buildSearchUrl(filter), [filter]);

  return (
    <form>
      <input placeholder="جست‌وجو" onChange={(e) => setFilter((f) => ({ ...f, q: e.target.value || undefined }))} />
      <label>
        <input type="checkbox" onChange={(e) => setFilter((f) => ({ ...f, inStock: e.target.checked }))} />
        فقط موجودها
      </label>
      <a href={url}>نمایش نتایج</a>
    </form>
  );
}
