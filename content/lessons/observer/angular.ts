import { Component, DestroyRef, Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject } from 'rxjs';

interface OrderPlaced {
  id: number;
  total: number;
}

@Injectable({ providedIn: 'root' })
export class OrderEvents {
  // Subject همان Observable است که خودت می‌توانی در آن مقدار منتشر کنی
  private readonly placedSubject = new Subject<OrderPlaced>();
  readonly placed$ = this.placedSubject.asObservable(); // مصرف‌کننده‌ها فقط گوش می‌دهند، next نمی‌زنند

  publish(order: OrderPlaced): void {
    this.placedSubject.next(order);
  }
}

@Component({
  selector: 'app-sales-badge',
  template: `<p>فروش امروز: {{ total() }}</p>`,
})
export class SalesBadge {
  protected readonly total = signal(0);

  constructor() {
    // takeUntilDestroyed: با نابودی کامپوننت اشتراک لغو می‌شود، پس نشتی حافظه نداریم
    inject(OrderEvents)
      .placed$.pipe(takeUntilDestroyed(inject(DestroyRef)))
      .subscribe((order) => this.total.update((t) => t + order.total));
  }
}
