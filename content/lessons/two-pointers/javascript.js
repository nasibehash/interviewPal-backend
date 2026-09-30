// دو محصول از یک لیست مرتب که مجموع قیمتشان دقیقاً برابر موجودی کارت هدیه باشد
function findPair(sortedPrices, target) {
  let left = 0;
  let right = sortedPrices.length - 1;
  while (left < right) {
    const sum = sortedPrices[left] + sortedPrices[right];
    if (sum === target) return [left, right];
    if (sum < target) left++; // مجموع کم است: کوچک‌ترین را بزرگ‌تر کن
    else right--; // مجموع زیاد است: بزرگ‌ترین را کوچک‌تر کن
  }
  return null;
}

const products = [
  { name: 'کابل', price: 90_000 },
  { name: 'ماوس', price: 250_000 },
  { name: 'پاوربانک', price: 450_000 },
  { name: 'هدفون', price: 550_000 },
  { name: 'اسپیکر', price: 1_200_000 },
];

const pair = findPair(products.map((p) => p.price), 1_000_000);
console.log(pair && pair.map((i) => products[i].name)); // ['پاوربانک', 'هدفون']
console.log(findPair([1, 2, 3], 100)); // null
