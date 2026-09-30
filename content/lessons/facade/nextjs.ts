// app/api/checkout/route.ts — کلاینت فقط یک درخواست می‌زند؛ هماهنگی زیرسیستم‌ها پشت Facade است
import { NextResponse } from 'next/server';

interface CheckoutBody {
  items: string[];
  total: number;
  email: string;
}

const inventory = {
  reserve: async (_items: string[]) => {},
  release: async (_items: string[]) => {},
};
const payments = {
  charge: async (_amount: number, _idempotencyKey: string) => ({ paymentId: 'p-1' }),
};
const notifications = { sendReceipt: async (_email: string) => {} };

export async function POST(request: Request) {
  const body = (await request.json()) as Partial<CheckoutBody>;
  if (!Array.isArray(body.items) || body.items.length === 0 || !(body.total && body.total > 0) || !body.email) {
    return NextResponse.json({ error: 'سفارش نامعتبر است' }, { status: 400 });
  }

  await inventory.reserve(body.items);
  try {
    // کلید idempotency: اگر کاربر دو بار کلیک کرد دو بار پول کسر نشود
    const key = request.headers.get('Idempotency-Key') ?? crypto.randomUUID();
    const { paymentId } = await payments.charge(body.total, key);
    await notifications.sendReceipt(body.email);
    return NextResponse.json({ paymentId });
  } catch {
    await inventory.release(body.items);
    return NextResponse.json({ error: 'پرداخت ناموفق بود' }, { status: 402 });
  }
}
