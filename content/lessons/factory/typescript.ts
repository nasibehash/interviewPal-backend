interface Receipt {
  method: string;
  amount: number;
  fee: number;
}

interface PaymentGateway {
  charge(amount: number): Receipt;
}

type PaymentMethod = 'card' | 'wallet' | 'installment';

class FeeGateway implements PaymentGateway {
  constructor(private readonly method: string, private readonly feeRate: number) {}
  charge(amount: number): Receipt {
    return { method: this.method, amount, fee: Math.round(amount * this.feeRate) };
  }
}

function createGateway(method: PaymentMethod): PaymentGateway {
  switch (method) {
    case 'card':
      return new FeeGateway('card', 0.01);
    case 'wallet':
      return new FeeGateway('wallet', 0);
    case 'installment':
      return new FeeGateway('installment', 0.03);
    default: {
      // اگر روش جدیدی به PaymentMethod اضافه شود و اینجا فراموش شود، کامپایلر خطا می‌دهد
      const unreachable: never = method;
      throw new Error(`روش پرداخت ناشناخته: ${unreachable}`);
    }
  }
}

console.log(createGateway('card').charge(500_000));
console.log(createGateway('installment').charge(500_000));
