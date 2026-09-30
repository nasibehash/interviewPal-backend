import { useMemo, useState } from 'react';

interface Product {
  id: number;
  name: string;
  price: number;
}

function findPair(sorted: readonly Product[], target: number): [Product, Product] | null {
  let left = 0;
  let right = sorted.length - 1;
  while (left < right) {
    const sum = sorted[left].price + sorted[right].price;
    if (sum === target) return [sorted[left], sorted[right]];
    if (sum < target) left++;
    else right--;
  }
  return null;
}

export function GiftCardPicker({ sortedProducts }: { sortedProducts: readonly Product[] }) {
  const [balance, setBalance] = useState(1_000_000);
  const pair = useMemo(() => findPair(sortedProducts, balance), [sortedProducts, balance]);

  return (
    <section>
      <label>
        موجودی کارت هدیه
        <input type="number" value={balance} onChange={(e) => setBalance(Number(e.target.value))} />
      </label>
      {pair ? (
        <p>{pair[0].name} + {pair[1].name} = {balance}</p>
      ) : (
        <p>دو محصولی که دقیقاً به این مبلغ برسند پیدا نشد.</p>
      )}
    </section>
  );
}
