import { useState } from 'react';

interface Cart {
  total: number;
}

type DiscountStrategy = (cart: Cart) => number;

// نگاشت نام ← تابع؛ به‌جای switch در کامپوننت
const strategies = {
  none: () => 0,
  vip: (cart) => cart.total * 0.1,
  campaign: (cart) => Math.min(50_000, cart.total),
} satisfies Record<string, DiscountStrategy>;

type StrategyName = keyof typeof strategies;

export function CartSummary({ cart }: { cart: Cart }) {
  const [name, setName] = useState<StrategyName>('none');
  const payable = cart.total - strategies[name](cart);

  return (
    <section>
      <select value={name} onChange={(e) => setName(e.target.value as StrategyName)} aria-label="نوع تخفیف">
        <option value="none">بدون تخفیف</option>
        <option value="vip">مشتری ویژه</option>
        <option value="campaign">کمپین</option>
      </select>
      <p>مبلغ قابل پرداخت: {payable}</p>
    </section>
  );
}
