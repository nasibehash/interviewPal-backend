import { Injectable, inject, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
class InventoryService {
  async reserve(_items: string[]): Promise<void> {}
  async release(_items: string[]): Promise<void> {}
}

@Injectable({ providedIn: 'root' })
class PaymentService {
  async charge(_amount: number): Promise<void> {}
}

@Injectable({ providedIn: 'root' })
class ShippingService {
  async create(_orderId: number): Promise<void> {}
}

interface OrderDraft {
  id: number;
  items: string[];
  total: number;
}

// کامپوننت‌ها فقط CheckoutFacade را می‌شناسند، نه سه سرویس و ترتیب کارشان را
@Injectable({ providedIn: 'root' })
export class CheckoutFacade {
  private readonly inventory = inject(InventoryService);
  private readonly payments = inject(PaymentService);
  private readonly shipping = inject(ShippingService);

  private readonly _status = signal<'idle' | 'working' | 'done' | 'failed'>('idle');
  private readonly _error = signal<string | null>(null);
  readonly status = this._status.asReadonly();
  readonly error = this._error.asReadonly();

  async placeOrder(order: OrderDraft): Promise<void> {
    this._status.set('working');
    this._error.set(null);
    await this.inventory.reserve(order.items);
    try {
      await this.payments.charge(order.total);
      await this.shipping.create(order.id);
      this._status.set('done');
    } catch (error) {
      await this.inventory.release(order.items); // جبران
      this._error.set(error instanceof Error ? error.message : 'خطای ناشناخته');
      this._status.set('failed');
    }
  }
}
