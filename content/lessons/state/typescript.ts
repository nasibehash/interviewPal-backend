// union از وضعیت‌های ممکن: «وضعیت نامعتبر» اصلاً قابل‌نوشتن نیست
type OrderState =
  | { status: 'pending' }
  | { status: 'paid'; paymentId: string }
  | { status: 'shipped'; paymentId: string; trackingCode: string }
  | { status: 'delivered'; deliveredAt: Date }
  | { status: 'cancelled'; reason: string };

type OrderEvent =
  | { type: 'pay'; paymentId: string }
  | { type: 'ship'; trackingCode: string }
  | { type: 'deliver'; at: Date }
  | { type: 'cancel'; reason: string };

// تابع خالص: (وضعیت، رویداد) ← وضعیت جدید
function transition(state: OrderState, event: OrderEvent): OrderState {
  switch (state.status) {
    case 'pending':
      if (event.type === 'pay') return { status: 'paid', paymentId: event.paymentId };
      if (event.type === 'cancel') return { status: 'cancelled', reason: event.reason };
      break;
    case 'paid':
      if (event.type === 'ship') return { status: 'shipped', paymentId: state.paymentId, trackingCode: event.trackingCode };
      if (event.type === 'cancel') return { status: 'cancelled', reason: event.reason };
      break;
    case 'shipped':
      if (event.type === 'deliver') return { status: 'delivered', deliveredAt: event.at };
      break;
  }
  throw new Error(`«${event.type}» در وضعیت «${state.status}» مجاز نیست`);
}

let state: OrderState = { status: 'pending' };
state = transition(state, { type: 'pay', paymentId: 'p-1' });
state = transition(state, { type: 'ship', trackingCode: 'T-99' });
console.log(state); // shipped همراه با trackingCode: فقط در همین وضعیت وجود دارد

try {
  transition(state, { type: 'cancel', reason: 'انصراف' });
} catch (error) {
  console.log((error as Error).message);
}
