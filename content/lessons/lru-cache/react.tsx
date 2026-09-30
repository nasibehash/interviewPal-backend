import { useEffect, useState } from 'react';

class LruCache<K, V> {
  private readonly entries = new Map<K, V>();

  constructor(private readonly capacity: number) {}

  get(key: K): V | undefined {
    if (!this.entries.has(key)) return undefined;
    const value = this.entries.get(key) as V;
    this.entries.delete(key);
    this.entries.set(key, value);
    return value;
  }

  set(key: K, value: V): void {
    this.entries.delete(key);
    this.entries.set(key, value);
    if (this.entries.size > this.capacity) {
      const oldest = this.entries.keys().next();
      if (!oldest.done) this.entries.delete(oldest.value);
    }
  }
}

interface Product {
  id: number;
  name: string;
}

// کش در سطح ماژول است: بین رندرها و کامپوننت‌های مختلف مشترک می‌ماند
const productCache = new LruCache<number, Product>(100);

export function useProduct(id: number) {
  const [product, setProduct] = useState<Product | undefined>(() => productCache.get(id));

  useEffect(() => {
    const cached = productCache.get(id);
    if (cached) {
      setProduct(cached);
      return;
    }

    const controller = new AbortController();
    fetch(`/api/products/${id}`, { signal: controller.signal })
      .then((response) => response.json() as Promise<Product>)
      .then((fresh) => {
        productCache.set(id, fresh);
        setProduct(fresh);
      })
      .catch(() => {}); // لغو درخواست یا خطای شبکه: کش تغییری نمی‌کند
    return () => controller.abort();
  }, [id]);

  return product;
}
