import { useCallback, useState } from 'react';

type Status = 'idle' | 'working' | 'done' | 'failed';

async function post(url: string, body: unknown): Promise<void> {
  const response = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  if (!response.ok) throw new Error(`${url} ناموفق بود`);
}

// هوک Facade: سه فراخوانی API و منطق جبران پشت یک تابع ساده
export function useCheckout() {
  const [status, setStatus] = useState<Status>('idle');
  const [error, setError] = useState<string | null>(null);

  const placeOrder = useCallback(async (order: { id: number; items: string[]; total: number }) => {
    setStatus('working');
    setError(null);
    try {
      await post('/api/inventory/reserve', { items: order.items });
    } catch (e) {
      setStatus('failed');
      setError((e as Error).message);
      return;
    }
    try {
      await post('/api/payments', { amount: order.total });
      await post('/api/shipments', { orderId: order.id });
      setStatus('done');
    } catch (e) {
      await post('/api/inventory/release', { items: order.items }).catch(() => {});
      setStatus('failed');
      setError((e as Error).message);
    }
  }, []);

  return { placeOrder, status, error };
}

export function PayButton({ order }: { order: { id: number; items: string[]; total: number } }) {
  const { placeOrder, status, error } = useCheckout();
  return (
    <>
      <button disabled={status === 'working'} onClick={() => placeOrder(order)}>پرداخت</button>
      {status === 'done' && <p>سفارش ثبت شد.</p>}
      {error && <p role="alert">{error}</p>}
    </>
  );
}
