import { Component, computed, signal } from '@angular/core';

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

@Component({
  selector: 'app-gift-card',
  template: `
    <label>
      موجودی کارت هدیه
      <input type="number" [value]="balance()" (input)="onBalance($event)" />
    </label>
    @if (pair(); as p) {
      <p>{{ p[0].name }} + {{ p[1].name }} = {{ balance() }}</p>
    } @else {
      <p>دو محصولی که دقیقاً به این مبلغ برسند پیدا نشد.</p>
    }
  `,
})
export class GiftCard {
  /** از سرور بر اساس قیمت مرتب می‌آید. */
  readonly products = signal<Product[]>([]);
  protected readonly balance = signal(1_000_000);

  // با هر تغییر موجودی، یک پیمایش O(n) روی لیست مرتب
  protected readonly pair = computed(() => findPair(this.products(), this.balance()));

  protected onBalance(event: Event): void {
    this.balance.set(Number((event.target as HTMLInputElement).value));
  }
}
