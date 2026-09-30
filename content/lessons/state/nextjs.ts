// app/orders/actions.ts
'use server';

type Status = 'pending' | 'paid' | 'shipped' | 'delivered' | 'cancelled';
type Action = 'pay' | 'ship' | 'deliver' | 'cancel';

const TRANSITIONS: Record<Status, Partial<Record<Action, Status>>> = {
  pending: { pay: 'paid', cancel: 'cancelled' },
  paid: { ship: 'shipped', cancel: 'cancelled' },
  shipped: { deliver: 'delivered' },
  delivered: {},
  cancelled: {},
};

// در پروژهٔ واقعی: دیتابیس
const orders = new Map<number, Status>([[1, 'pending']]);

// وضعیت را هرگز از کلاینت نپذیر («status = delivered»)؛ فقط «رویداد» را بپذیر و گذار را سرور بررسی کند
export async function applyAction(orderId: number, action: Action): Promise<Status> {
  const current = orders.get(orderId);
  if (!current) throw new Error('سفارش پیدا نشد');

  const next = TRANSITIONS[current][action];
  if (!next) throw new Error(`«${action}» در وضعیت «${current}» مجاز نیست`);

  // در دیتابیس باید شرطی باشد تا دو درخواست هم‌زمان یک سفارش را دوبار عوض نکنند:
  // UPDATE orders SET status = @next WHERE id = @id AND status = @current
  orders.set(orderId, next);
  return next;
}
