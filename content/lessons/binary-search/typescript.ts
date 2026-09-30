// نسخهٔ generic: هر آرایهٔ مرتبی که بتوان از هر عنصرش یک عدد (کلید) درآورد
function lowerBound<T>(items: readonly T[], target: number, key: (item: T) => number): number {
  let lo = 0;
  let hi = items.length; // بازهٔ جستجو: [lo, hi)
  while (lo < hi) {
    const mid = lo + Math.floor((hi - lo) / 2);
    if (key(items[mid]) < target) lo = mid + 1;
    else hi = mid;
  }
  return lo;
}

interface Product {
  name: string;
  price: number;
}

// فقط عنصرهای بازهٔ [min, max] را برمی‌گرداند؛ ورودی باید بر اساس قیمت مرتب باشد
function inPriceRange(sorted: readonly Product[], min: number, max: number): readonly Product[] {
  const price = (p: Product) => p.price;
  return sorted.slice(lowerBound(sorted, min, price), lowerBound(sorted, max + 1, price));
}

const products: Product[] = [
  { name: 'کابل شارژ', price: 90_000 },
  { name: 'پاوربانک', price: 450_000 },
  { name: 'هدفون', price: 700_000 },
  { name: 'اسپیکر', price: 1_200_000 },
  { name: 'ساعت هوشمند', price: 2_500_000 },
];

console.log(inPriceRange(products, 400_000, 1_200_000).map((p) => p.name));
console.log(lowerBound([1, 3, 3, 3, 5], 3, (n) => n)); // 1: اولین ۳
