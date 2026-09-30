interface Order {
  id: number;
  items: string[];
  total: number;
  email: string;
}

interface Inventory {
  reserve(items: string[]): Promise<void>;
  release(items: string[]): Promise<void>;
}
interface Payments {
  charge(amount: number): Promise<{ paymentId: string }>;
}
interface Shipping {
  create(order: Order): Promise<void>;
}
interface Notifications {
  sendReceipt(email: string): Promise<void>;
}

type CheckoutResult = { ok: true } | { ok: false; reason: string };

// زیرسیستم‌ها از بیرون تزریق می‌شوند: Facade تست‌پذیر است و جزئیات را از مصرف‌کننده پنهان می‌کند
class CheckoutFacade {
  constructor(
    private readonly inventory: Inventory,
    private readonly payments: Payments,
    private readonly shipping: Shipping,
    private readonly notifications: Notifications,
  ) {}

  async placeOrder(order: Order): Promise<CheckoutResult> {
    await this.inventory.reserve(order.items);
    try {
      await this.payments.charge(order.total);
      await this.shipping.create(order);
      await this.notifications.sendReceipt(order.email);
      return { ok: true };
    } catch (error) {
      await this.inventory.release(order.items);
      return { ok: false, reason: error instanceof Error ? error.message : 'خطای ناشناخته' };
    }
  }
}

const log: string[] = [];
const facade = new CheckoutFacade(
  { reserve: async () => void log.push('reserve'), release: async () => void log.push('release') },
  { charge: async () => { throw new Error('کارت رد شد'); } },
  { create: async () => void log.push('ship') },
  { sendReceipt: async () => void log.push('email') },
);

facade.placeOrder({ id: 1, items: ['a'], total: 100, email: 'a@b.c' }).then((result) => console.log(result, log));
// { ok: false, reason: 'کارت رد شد' } [ 'reserve', 'release' ]
