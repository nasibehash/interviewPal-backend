import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { of, tap } from 'rxjs';

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

const cache = new LruCache<string, HttpResponse<unknown>>(50);

/** فقط درخواست‌های GET کش می‌شوند؛ ثبت با provideHttpClient(withInterceptors([cacheInterceptor])). */
export const cacheInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.method !== 'GET') return next(req);

  const hit = cache.get(req.urlWithParams);
  if (hit) return of(hit.clone());

  return next(req).pipe(
    tap((event) => {
      if (event instanceof HttpResponse) cache.set(req.urlWithParams, event);
    }),
  );
};
