import { Component, computed, signal } from '@angular/core';

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

@Component({
  selector: 'app-orders-table',
  template: `
    <button type="button" (click)="sortBy('customer')">مشتری</button>
    <button type="button" (click)="sortBy('total')">مبلغ</button>
    <ul>
      @for (o of sorted(); track o.id) {
        <li>{{ o.id }} — {{ o.customer }} — {{ o.total }}</li>
      }
    </ul>
  `,
})
export class OrdersTable {
  readonly orders = signal<Order[]>([]);
  protected readonly column = signal<Column>('total');
  protected readonly ascending = signal(true);

  // مرتب‌سازی پایدار: با کلیک روی ستون دوم، ترتیب قبلی هم‌ارزها به هم نمی‌خورد
  protected readonly sorted = computed(() => {
    const column = this.column();
    const direction = this.ascending() ? 1 : -1;
    return mergeSort(this.orders(), (a, b) => {
      const x = a[column];
      const y = b[column];
      return x < y ? -direction : x > y ? direction : 0;
    });
  });

  protected sortBy(column: Column): void {
    if (this.column() === column) this.ascending.update((v) => !v);
    else {
      this.column.set(column);
      this.ascending.set(true);
    }
  }
}
