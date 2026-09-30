// جدول گذارها: از هر وضعیت، کدام رویداد مجاز است و به کجا می‌رود
const transitions = {
  pending: { pay: 'paid', cancel: 'cancelled' },
  paid: { ship: 'shipped', cancel: 'cancelled' },
  shipped: { deliver: 'delivered' },
  delivered: {}, // وضعیت پایانی
  cancelled: {},
};

class Order {
  #state = 'pending';

  get state() {
    return this.#state;
  }

  get allowedActions() {
    return Object.keys(transitions[this.#state]);
  }

  dispatch(action) {
    const next = transitions[this.#state][action];
    if (!next) throw new Error(`«${action}» در وضعیت «${this.#state}» مجاز نیست`);
    this.#state = next;
    return this;
  }
}

const order = new Order();
order.dispatch('pay').dispatch('ship');
console.log(order.state, order.allowedActions); // shipped [ 'deliver' ]

try {
  order.dispatch('cancel'); // سفارش ارسال‌شده را دیگر نمی‌شود لغو کرد
} catch (error) {
  console.log(error.message);
}
