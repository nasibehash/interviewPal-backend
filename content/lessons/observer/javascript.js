// یک event bus کوچک: منتشرکننده نمی‌داند چه کسانی گوش می‌دهند
class EventBus {
  handlers = new Map();

  on(event, handler) {
    if (!this.handlers.has(event)) this.handlers.set(event, new Set());
    this.handlers.get(event).add(handler);
    return () => this.handlers.get(event).delete(handler); // تابع لغو اشتراک
  }

  emit(event, payload) {
    for (const handler of this.handlers.get(event) ?? []) handler(payload);
  }
}

const bus = new EventBus();

// سه بخش مستقل به «ثبت سفارش» واکنش نشان می‌دهند
bus.on('orderPlaced', (order) => console.log(`ایمیل تأیید برای سفارش ${order.id}`));
bus.on('orderPlaced', (order) => console.log(`کسر موجودی ${order.items} کالا`));
const stopDashboard = bus.on('orderPlaced', (order) => console.log(`داشبورد: فروش ${order.total}`));

function placeOrder(order) {
  // ...ذخیرهٔ سفارش...
  bus.emit('orderPlaced', order); // سرویس سفارش فقط اعلام می‌کند
}

placeOrder({ id: 1, items: 3, total: 450_000 });
stopDashboard(); // بدون نشت حافظه: دیگر داشبورد خبر نمی‌گیرد
placeOrder({ id: 2, items: 1, total: 90_000 });
