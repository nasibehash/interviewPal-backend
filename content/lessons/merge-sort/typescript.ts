type Compare<T> = (a: T, b: T) => number;

function mergeSort<T>(items: readonly T[], compare: Compare<T>): T[] {
  if (items.length <= 1) return [...items];
  const middle = Math.floor(items.length / 2);
  return merge(mergeSort(items.slice(0, middle), compare), mergeSort(items.slice(middle), compare), compare);
}

function merge<T>(left: readonly T[], right: readonly T[], compare: Compare<T>): T[] {
  const result: T[] = [];
  let i = 0;
  let j = 0;
  while (i < left.length && j < right.length) {
    result.push(compare(left[i], right[j]) <= 0 ? left[i++] : right[j++]);
  }
  return [...result, ...left.slice(i), ...right.slice(j)];
}

// ساخت comparator چندکلیدی: اول ستون اول، در مساوی ستون بعدی
function by<T>(...keys: ((item: T) => number | string)[]): Compare<T> {
  return (a, b) => {
    for (const key of keys) {
      const x = key(a);
      const y = key(b);
      if (x < y) return -1;
      if (x > y) return 1;
    }
    return 0;
  };
}

interface Order {
  id: number;
  customer: string;
  total: number;
}

const orders: Order[] = [
  { id: 1, customer: 'ali', total: 500 },
  { id: 2, customer: 'sara', total: 300 },
  { id: 3, customer: 'ali', total: 300 },
  { id: 4, customer: 'reza', total: 500 },
];

console.log(mergeSort(orders, by((o) => o.total, (o) => o.customer)).map((o) => o.id)); // [3, 2, 1, 4]: اول مبلغ، در مساوی‌ها نام مشتری
