import { useReducer } from 'react';

type Status = 'pending' | 'paid' | 'shipped' | 'delivered' | 'cancelled';
type Action = 'pay' | 'ship' | 'deliver' | 'cancel';

const TRANSITIONS: Record<Status, Partial<Record<Action, Status>>> = {
  pending: { pay: 'paid', cancel: 'cancelled' },
  paid: { ship: 'shipped', cancel: 'cancelled' },
  shipped: { deliver: 'delivered' },
  delivered: {},
  cancelled: {},
};

const LABELS: Record<Action, string> = { pay: 'پرداخت', ship: 'ارسال', deliver: 'تحویل', cancel: 'لغو' };

// reducer فقط گذار مجاز را اعمال می‌کند و در غیر این صورت state را دست‌نخورده برمی‌گرداند
const reducer = (status: Status, action: Action): Status => TRANSITIONS[status][action] ?? status;

export function OrderStatus() {
  const [status, dispatch] = useReducer(reducer, 'pending');
  const actions = Object.keys(TRANSITIONS[status]) as Action[];

  return (
    <section>
      <p>وضعیت سفارش: {status}</p>
      {actions.map((action) => (
        <button key={action} onClick={() => dispatch(action)}>{LABELS[action]}</button>
      ))}
    </section>
  );
}
