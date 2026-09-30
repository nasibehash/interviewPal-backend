// هر قاعدهٔ تخفیف یک تابع است؛ انتخاب قاعده در زمان اجرا انجام می‌شود، نه با زنجیرهٔ if/else
const discountStrategies = {
  none: () => 0,
  vip: (cart) => cart.total * 0.1,
  campaign: (cart) => Math.min(50_000, cart.total),
  firstOrder: (cart) => Math.min(cart.total * 0.2, 100_000),
};

function payable(cart, strategyName) {
  if (!Object.hasOwn(discountStrategies, strategyName)) {
    throw new Error(`قاعدهٔ تخفیف ناشناخته: ${strategyName}`);
  }
  return cart.total - discountStrategies[strategyName](cart);
}

const cart = { total: 800_000 };
console.log(payable(cart, 'none')); // 800000
console.log(payable(cart, 'vip')); // 720000
console.log(payable(cart, 'firstOrder')); // 700000 (۲۰٪ = ۱۶۰ هزار ولی سقف ۱۰۰ هزار)

// قاعدهٔ جدید = یک تابع جدید؛ کد پرداخت دست نمی‌خورد
discountStrategies.blackFriday = (c) => c.total * 0.3;
console.log(payable(cart, 'blackFriday')); // 560000
