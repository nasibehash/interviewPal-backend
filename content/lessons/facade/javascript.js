// چهار زیرسیستم با رابط‌های جدا (در واقعیت هرکدام یک سرویس یا API است)
const inventory = {
  reserve: async (items) => console.log(`انبار: رزرو ${items.length} کالا`),
  release: async (items) => console.log(`انبار: آزادسازی ${items.length} کالا`),
};
const payments = {
  charge: async (amount) => {
    if (amount > 1_000_000) throw new Error('موجودی کافی نیست');
    console.log(`پرداخت: ${amount} تومان کسر شد`);
    return { paymentId: 'p-1' };
  },
};
const shipping = { create: async (order) => console.log(`ارسال: مرسولهٔ سفارش ${order.id}`) };
const notifications = { sendReceipt: async (email) => console.log(`اعلان: رسید برای ${email}`) };

// Facade: یک متد ساده که ترتیب و جبران خطا را می‌داند
class CheckoutFacade {
  async placeOrder(order) {
    await inventory.reserve(order.items);
    try {
      await payments.charge(order.total);
      await shipping.create(order);
      await notifications.sendReceipt(order.email);
      return { ok: true };
    } catch (error) {
      await inventory.release(order.items); // جبران: کالای رزروشده را آزاد کن
      return { ok: false, reason: error.message };
    }
  }
}

const checkout = new CheckoutFacade();
console.log(await checkout.placeOrder({ id: 1, items: ['a', 'b'], total: 450_000, email: 'ali@example.com' }));
console.log(await checkout.placeOrder({ id: 2, items: ['c'], total: 2_000_000, email: 'sara@example.com' }));
