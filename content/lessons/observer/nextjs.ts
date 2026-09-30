// ─── lib/order-bus.ts ─── (فایل route.ts فقط اجازهٔ export کردن handlerهای HTTP را دارد، پس bus جدا می‌شود)
export type Listener = (order: { id: number; total: number }) => void;

// روی globalThis نگه داشته می‌شود تا با hot reload در حالت توسعه دو نمونه ساخته نشود
const globalBus = globalThis as unknown as { orderListeners?: Set<Listener> };
export const listeners = (globalBus.orderListeners ??= new Set<Listener>());

export function publishOrder(order: { id: number; total: number }): void {
  listeners.forEach((listener) => listener(order));
}

// ─── app/api/orders/stream/route.ts ─── Server-Sent Events: سرور رویداد را به مرورگرها «هل می‌دهد»
// import { listeners, type Listener } from '@/lib/order-bus';

export const dynamic = 'force-dynamic';

export function GET(request: Request) {
  const encoder = new TextEncoder();
  let unsubscribe: () => void = () => {};

  const stream = new ReadableStream({
    start(controller) {
      const listener: Listener = (order) =>
        controller.enqueue(encoder.encode(`data: ${JSON.stringify(order)}\n\n`));
      listeners.add(listener);
      unsubscribe = () => listeners.delete(listener);
      request.signal.addEventListener('abort', () => {
        unsubscribe(); // مرورگر بسته شد: اشتراک را لغو کن
        controller.close();
      });
    },
    cancel() {
      unsubscribe();
    },
  });

  return new Response(stream, {
    headers: { 'Content-Type': 'text/event-stream', 'Cache-Control': 'no-cache, no-transform' },
  });
}
