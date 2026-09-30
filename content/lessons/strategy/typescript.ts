interface Cart {
  total: number;
}

// قرارداد مشترک: هر قاعده فقط مقدار تخفیف را برمی‌گرداند
interface DiscountStrategy {
  readonly name: string;
  discount(cart: Cart): number;
}

class NoDiscount implements DiscountStrategy {
  readonly name = 'none';
  discount(): number {
    return 0;
  }
}

class PercentageDiscount implements DiscountStrategy {
  constructor(readonly name: string, private readonly percent: number, private readonly cap = Infinity) {}
  discount(cart: Cart): number {
    return Math.min(cart.total * (this.percent / 100), this.cap);
  }
}

// Checkout به قاعدهٔ مشخصی وابسته نیست، فقط به interface
class Checkout {
  constructor(private strategy: DiscountStrategy) {}

  useStrategy(strategy: DiscountStrategy): void {
    this.strategy = strategy;
  }

  payable(cart: Cart): number {
    return cart.total - this.strategy.discount(cart);
  }
}

const checkout = new Checkout(new NoDiscount());
const cart: Cart = { total: 800_000 };
console.log(checkout.payable(cart)); // 800000

checkout.useStrategy(new PercentageDiscount('firstOrder', 20, 100_000));
console.log(checkout.payable(cart)); // 700000
