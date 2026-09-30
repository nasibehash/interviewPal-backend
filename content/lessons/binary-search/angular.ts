import { Component, Injectable, computed, inject, signal } from '@angular/core';

export interface Product {
  id: number;
  name: string;
  price: number;
}

function lowerBound<T>(items: readonly T[], target: number, key: (item: T) => number): number {
  let lo = 0;
  let hi = items.length;
  while (lo < hi) {
    const mid = lo + Math.floor((hi - lo) / 2);
    if (key(items[mid]) < target) lo = mid + 1;
    else hi = mid;
  }
  return lo;
}

@Injectable({ providedIn: 'root' })
export class PriceFilter {
  /** فرض: سرور محصولات را بر اساس قیمت مرتب می‌فرستد. */
  readonly products = signal<Product[]>([]);
  readonly min = signal(0);
  readonly max = signal(Infinity);

  // با هر حرکت اسلایدر فقط دو جستجوی دودویی اجرا می‌شود، نه یک filter روی همهٔ لیست
  readonly visible = computed(() => {
    const items = this.products();
    const price = (p: Product) => p.price;
    return items.slice(lowerBound(items, this.min(), price), lowerBound(items, this.max() + 1, price));
  });
}

@Component({
  selector: 'app-price-list',
  template: `
    <input type="range" min="0" max="3000000" step="50000" aria-label="حداقل قیمت" (input)="onMin($event)" />
    <ul>
      @for (p of filter.visible(); track p.id) {
        <li>{{ p.name }} — {{ p.price }}</li>
      }
    </ul>
  `,
})
export class PriceList {
  protected readonly filter = inject(PriceFilter);

  protected onMin(event: Event): void {
    this.filter.min.set(Number((event.target as HTMLInputElement).value));
  }
}
