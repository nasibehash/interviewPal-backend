// مرتب‌سازی ادغامی پایدار: عناصر مساوی ترتیب اصلی خود را حفظ می‌کنند
function mergeSort(items, compare) {
  if (items.length <= 1) return items;
  const middle = Math.floor(items.length / 2);
  const left = mergeSort(items.slice(0, middle), compare);
  const right = mergeSort(items.slice(middle), compare);
  return merge(left, right, compare);
}

function merge(left, right, compare) {
  const result = [];
  let i = 0;
  let j = 0;
  while (i < left.length && j < right.length) {
    // <= 0 (نه < 0): در مساوی، عنصر چپ اول می‌آید و پایداری حفظ می‌شود
    if (compare(left[i], right[j]) <= 0) result.push(left[i++]);
    else result.push(right[j++]);
  }
  return result.concat(left.slice(i), right.slice(j));
}

const orders = [
  { id: 1, customer: 'ali', total: 500 },
  { id: 2, customer: 'sara', total: 300 },
  { id: 3, customer: 'ali', total: 300 },
  { id: 4, customer: 'reza', total: 500 },
];

// اول بر اساس مبلغ؛ سفارش‌های هم‌مبلغ به ترتیب ثبت (id) می‌مانند
console.log(mergeSort(orders, (a, b) => a.total - b.total).map((o) => o.id)); // [2, 3, 1, 4]
