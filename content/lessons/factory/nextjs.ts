// lib/payments.ts — فقط سرور
interface PaymentProvider {
  charge(amount: number, orderId: string): Promise<{ transactionId: string }>;
}

class FakeProvider implements PaymentProvider {
  async charge(_amount: number, orderId: string) {
    return { transactionId: `fake-${orderId}` };
  }
}

class HttpProvider implements PaymentProvider {
  constructor(private readonly baseUrl: string, private readonly apiKey: string) {}

  async charge(amount: number, orderId: string) {
    const response = await fetch(`${this.baseUrl}/charge`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${this.apiKey}` },
      body: JSON.stringify({ amount, orderId }),
    });
    if (!response.ok) throw new Error('پرداخت ناموفق بود');
    return (await response.json()) as { transactionId: string };
  }
}

// کارخانه بر اساس متغیر محیطی؛ بقیهٔ برنامه فقط PaymentProvider را می‌شناسد
export function createPaymentProvider(): PaymentProvider {
  switch (process.env.PAYMENT_PROVIDER) {
    case 'http': {
      const { PAYMENT_URL, PAYMENT_KEY } = process.env;
      if (!PAYMENT_URL || !PAYMENT_KEY) throw new Error('PAYMENT_URL و PAYMENT_KEY تنظیم نشده‌اند');
      return new HttpProvider(PAYMENT_URL, PAYMENT_KEY);
    }
    case 'fake':
    case undefined:
      return new FakeProvider();
    default:
      throw new Error(`PAYMENT_PROVIDER نامعتبر: ${process.env.PAYMENT_PROVIDER}`);
  }
}

// app/api/pay/route.ts
export async function POST(request: Request) {
  const { amount, orderId } = (await request.json()) as { amount: number; orderId: string };
  const result = await createPaymentProvider().charge(amount, orderId);
  return Response.json(result);
}
