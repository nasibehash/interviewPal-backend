import { Component, computed, signal } from '@angular/core';

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

@Component({
  selector: 'app-gift-card-payment',
  template: `
    <label>
      مبلغ سفارش (هزار تومان)
      <input type="number" min="0" step="10" [value]="amount()" (input)="onAmount($event)" />
    </label>
    @if (cards(); as list) {
      <p>{{ list.length }} کارت لازم است: {{ list.join(' + ') }}</p>
    } @else {
      <p>با کارت‌های موجود نمی‌شود دقیقاً این مبلغ را پرداخت کرد.</p>
    }
  `,
})
export class GiftCardPayment {
  readonly cardValues = signal<readonly number[]>([10, 30, 40]);
  protected readonly amount = signal(60);

  // جدول DP فقط وقتی مبلغ یا کارت‌ها عوض شود دوباره ساخته می‌شود
  protected readonly cards = computed(() => fewestCards(this.cardValues(), this.amount()));

  protected onAmount(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);
    this.amount.set(Number.isInteger(value) && value >= 0 ? value : 0);
  }
}
