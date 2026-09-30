// app/api/gift-cards/plan/route.ts
import { NextRequest, NextResponse } from 'next/server';

const MAX_AMOUNT = 100_000; // سقف مبلغ؛ جدول DP به اندازهٔ مبلغ حافظه می‌گیرد

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

// GET /api/gift-cards/plan?amount=60
export function GET(request: NextRequest) {
  const amount = Number(request.nextUrl.searchParams.get('amount'));
  if (!Number.isInteger(amount) || amount < 1 || amount > MAX_AMOUNT) {
    return NextResponse.json({ error: `amount باید عدد صحیح بین ۱ و ${MAX_AMOUNT} باشد` }, { status: 400 });
  }

  const cards = fewestCards([10, 30, 40], amount);
  return cards
    ? NextResponse.json({ count: cards.length, cards })
    : NextResponse.json({ error: 'این مبلغ با کارت‌های موجود ممکن نیست' }, { status: 422 });
}
