import { Injectable, InjectionToken, inject, Provider } from '@angular/core';

export interface DiscountStrategy {
  readonly name: string;
  discount(total: number): number;
}

// همهٔ قاعده‌ها با multi: true زیر یک توکن جمع می‌شوند
export const DISCOUNT_STRATEGIES = new InjectionToken<DiscountStrategy[]>('DISCOUNT_STRATEGIES');

export const provideDiscounts = (): Provider[] => [
  { provide: DISCOUNT_STRATEGIES, multi: true, useValue: { name: 'vip', discount: (t: number) => t * 0.1 } },
  { provide: DISCOUNT_STRATEGIES, multi: true, useValue: { name: 'campaign', discount: (t: number) => Math.min(50_000, t) } },
];

@Injectable({ providedIn: 'root' })
export class PricingService {
  private readonly strategies = new Map(inject(DISCOUNT_STRATEGIES).map((s) => [s.name, s]));

  payable(total: number, strategyName: string | null): number {
    if (!strategyName) return total;
    const strategy = this.strategies.get(strategyName);
    if (!strategy) throw new Error(`قاعدهٔ تخفیف ناشناخته: ${strategyName}`);
    return total - strategy.discount(total);
  }
}

// app.config.ts:  providers: [provideDiscounts()]
// افزودن قاعدهٔ جدید (حتی از یک feature جدا) فقط یک provider دیگر است و PricingService تغییر نمی‌کند.
