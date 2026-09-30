// اولین اندیسی که قیمتش از target کمتر نباشد (lower bound)
function lowerBound(items, target, key = (x) => x) {
  let lo = 0;
  let hi = items.length; // بازهٔ جستجو: [lo, hi)
  while (lo < hi) {
    const mid = lo + Math.floor((hi - lo) / 2);
    if (key(items[mid]) < target) lo = mid + 1;
    else hi = mid;
  }
  return lo;
}

// محصولات از قبل بر اساس قیمت مرتب شده‌اند
const products = [
  { name: 'کابل شارژ', price: 90_000 },
  { name: 'پاوربانک', price: 450_000 },
  { name: 'هدفون', price: 700_000 },
  { name: 'اسپیکر', price: 1_200_000 },
  { name: 'ساعت هوشمند', price: 2_500_000 },
];

// فیلتر بازهٔ قیمت: دو جستجوی دودویی، نه یک filter روی کل لیست
function inPriceRange(min, max) {
  const from = lowerBound(products, min, (p) => p.price);
  const to = lowerBound(products, max + 1, (p) => p.price);
  return products.slice(from, to);
}

console.log(inPriceRange(400_000, 1_200_000).map((p) => p.name)); // ['پاوربانک', 'هدفون', 'اسپیکر']
console.log(inPriceRange(3_000_000, 4_000_000)); // []
