import { InjectionToken, Provider } from '@angular/core';

export interface PaymentGateway {
  charge(amount: number): Promise<{ id: string }>;
}

class SandboxGateway implements PaymentGateway {
  async charge(): Promise<{ id: string }> {
    return { id: 'sandbox-123' }; // بدون پول واقعی
  }
}

class LiveGateway implements PaymentGateway {
  async charge(amount: number): Promise<{ id: string }> {
    const response = await fetch('/api/pay', { method: 'POST', body: JSON.stringify({ amount }) });
    return (await response.json()) as { id: string };
  }
}

export const PAYMENT_GATEWAY = new InjectionToken<PaymentGateway>('PAYMENT_GATEWAY');
export const SANDBOX = new InjectionToken<boolean>('SANDBOX', { factory: () => false });

// useFactory همان الگوی Factory است: DI با اجرای این تابع تصمیم می‌گیرد کدام کلاس ساخته شود
export const providePaymentGateway = (): Provider => ({
  provide: PAYMENT_GATEWAY,
  useFactory: (sandbox: boolean): PaymentGateway => (sandbox ? new SandboxGateway() : new LiveGateway()),
  deps: [SANDBOX],
});

// app.config.ts:  providers: [{ provide: SANDBOX, useValue: !isProduction }, providePaymentGateway()]
// هر سرویسی که PAYMENT_GATEWAY را inject کند، بدون دانستن پیاده‌سازی از آن استفاده می‌کند.
