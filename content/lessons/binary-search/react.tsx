import { useMemo, useState } from 'react';

interface Product {
  id: number;
  name: string;
  price: number;
}

function lowerBound<T>(items: readonly T[], target: number, key: (item: T) => number): number {
  let lo = 0;
  let hi = items.length;
  while (lo < hi) {
    const mid = lo + Math.floor((hi - lo) / 2);
    if (key(items[mid]) < target) lo = mid + 1;
    else hi = mid;
  }
  return lo;
}

/** sorted باید از قبل بر اساس قیمت مرتب شده باشد (مثلاً از سرور). */
export function usePriceRange(sorted: readonly Product[]) {
  const [min, setMin] = useState(0);
  const [max, setMax] = useState(Infinity);

  const visible = useMemo(() => {
    const price = (p: Product) => p.price;
    return sorted.slice(lowerBound(sorted, min, price), lowerBound(sorted, max + 1, price));
  }, [sorted, min, max]);

  return { visible, setMin, setMax };
}

export function PriceList({ products }: { products: readonly Product[] }) {
  const { visible, setMin } = usePriceRange(products);
  return (
    <>
      <input type="range" min={0} max={3_000_000} step={50_000} aria-label="حداقل قیمت"
        onChange={(e) => setMin(Number(e.target.value))} />
      <ul>
        {visible.map((p) => (
          <li key={p.id}>{p.name} — {p.price}</li>
        ))}
      </ul>
    </>
  );
}
