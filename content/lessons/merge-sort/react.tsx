import { useMemo, useState } from 'react';

interface Order {
  id: number;
  customer: string;
  total: number;
}

type Column = 'customer' | 'total';

function mergeSort<T>(items: readonly T[], compare: (a: T, b: T) => number): T[] {
  if (items.length <= 1) return [...items];
  const middle = Math.floor(items.length / 2);
  const left = mergeSort(items.slice(0, middle), compare);
  const right = mergeSort(items.slice(middle), compare);
  const result: T[] = [];
  let i = 0;
  let j = 0;
  while (i < left.length && j < right.length) {
    result.push(compare(left[i], right[j]) <= 0 ? left[i++] : right[j++]);
  }
  return [...result, ...left.slice(i), ...right.slice(j)];
}

export function OrdersTable({ orders }: { orders: readonly Order[] }) {
  const [column, setColumn] = useState<Column>('total');
  const [ascending, setAscending] = useState(true);

  // useMemo: مرتب‌سازی فقط با تغییر داده یا ستون؛ و هرگز state اصلی را تغییر نمی‌دهیم
  const sorted = useMemo(() => {
    const direction = ascending ? 1 : -1;
    return mergeSort(orders, (a, b) => {
      const x = a[column];
      const y = b[column];
      return x < y ? -direction : x > y ? direction : 0;
    });
  }, [orders, column, ascending]);

  const sortBy = (next: Column) => {
    if (next === column) setAscending((v) => !v);
    else {
      setColumn(next);
      setAscending(true);
    }
  };

  return (
    <>
      <button onClick={() => sortBy('customer')}>مشتری</button>
      <button onClick={() => sortBy('total')}>مبلغ</button>
      <ul>
        {sorted.map((o) => (
          <li key={o.id}>{o.id} — {o.customer} — {o.total}</li>
        ))}
      </ul>
    </>
  );
}
