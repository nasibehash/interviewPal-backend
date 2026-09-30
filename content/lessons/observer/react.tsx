import { useSyncExternalStore } from 'react';

// یک store بیرون از React: هر کسی می‌تواند مشترک شود و React هم با همین رابط مشترک می‌شود
function createCartStore() {
  let items: readonly string[] = [];
  const listeners = new Set<() => void>();

  return {
    subscribe(listener: () => void) {
      listeners.add(listener);
      return () => listeners.delete(listener); // لغو اشتراک
    },
    getSnapshot: () => items,
    add(item: string) {
      items = [...items, item]; // مرجع جدید = خبر تغییر
      listeners.forEach((listener) => listener());
    },
  };
}

export const cartStore = createCartStore();

export function CartBadge() {
  // React فقط وقتی snapshot عوض شود دوباره رندر می‌کند
  const items = useSyncExternalStore(cartStore.subscribe, cartStore.getSnapshot, cartStore.getSnapshot);
  return <span aria-label="تعداد اقلام سبد">{items.length}</span>;
}

export function AddButton({ name }: { name: string }) {
  return <button onClick={() => cartStore.add(name)}>افزودن به سبد</button>;
}
