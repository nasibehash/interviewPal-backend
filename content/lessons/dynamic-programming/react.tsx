import { useMemo, useState } from 'react';

function fewestCards(values: readonly number[], target: number): number[] | null {
  const best: number[] = new Array(target + 1).fill(Infinity);
  const last: number[] = new Array(target + 1).fill(0);
  best[0] = 0;
  for (let amount = 1; amount <= target; amount++) {
    for (const value of values) {
      if (value <= amount && best[amount - value] + 1 < best[amount]) {
        best[amount] = best[amount - value] + 1;
        last[amount] = value;
      }
    }
  }
  if (best[target] === Infinity) return null;
  const cards: number[] = [];
  for (let amount = target; amount > 0; amount -= last[amount]) cards.push(last[amount]);
  return cards;
}

export function GiftCardPayment({ cardValues }: { cardValues: readonly number[] }) {
  const [amount, setAmount] = useState(60);
  const cards = useMemo(() => fewestCards(cardValues, amount), [cardValues, amount]);

  return (
    <section>
      <label>
        مبلغ سفارش (هزار تومان)
        <input type="number" min={0} step={10} value={amount}
          onChange={(e) => setAmount(Math.max(0, Math.floor(Number(e.target.value)) || 0))} />
      </label>
      {cards ? (
        <p>{cards.length} کارت لازم است: {cards.join(' + ')}</p>
      ) : (
        <p>با کارت‌های موجود نمی‌شود دقیقاً این مبلغ را پرداخت کرد.</p>
      )}
    </section>
  );
}
