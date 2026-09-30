// نقشهٔ رویدادها: نام رویداد ← نوع داده‌اش. اشتباه در نام یا داده در زمان کامپایل گرفته می‌شود
interface OrderEvents {
  orderPlaced: { id: number; total: number };
  orderCancelled: { id: number; reason: string };
}

type Handler<T> = (payload: T) => void;

class TypedEventBus<Events extends object> {
  private readonly handlers: { [K in keyof Events]?: Set<Handler<Events[K]>> } = {};

  on<K extends keyof Events>(event: K, handler: Handler<Events[K]>): () => void {
    const set = (this.handlers[event] ??= new Set());
    set.add(handler);
    return () => set.delete(handler);
  }

  emit<K extends keyof Events>(event: K, payload: Events[K]): void {
    this.handlers[event]?.forEach((handler) => handler(payload));
  }
}

const bus = new TypedEventBus<OrderEvents>();
const off = bus.on('orderPlaced', ({ id, total }) => console.log(`سفارش ${id}: ${total}`));
bus.on('orderCancelled', ({ id, reason }) => console.log(`لغو ${id}: ${reason}`));

bus.emit('orderPlaced', { id: 1, total: 450_000 });
off();
bus.emit('orderPlaced', { id: 2, total: 90_000 }); // شنونده‌ای نیست
bus.emit('orderCancelled', { id: 1, reason: 'انصراف مشتری' });
// bus.emit('orderPlaced', { id: 3 }); ← خطای کامپایل: total ندارد
