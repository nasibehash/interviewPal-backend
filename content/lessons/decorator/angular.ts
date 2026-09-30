import { Injectable, Provider } from '@angular/core';

interface Product {
  id: number;
  name: string;
}

export abstract class ProductRepository {
  abstract find(id: number): Promise<Product>;
}

@Injectable()
class HttpProductRepository extends ProductRepository {
  async find(id: number): Promise<Product> {
    const response = await fetch(`/api/products/${id}`);
    return (await response.json()) as Product;
  }
}

class CachingProductRepository extends ProductRepository {
  private readonly cache = new Map<number, Product>();

  constructor(private readonly inner: ProductRepository) {
    super();
  }

  async find(id: number): Promise<Product> {
    const hit = this.cache.get(id);
    if (hit) return hit;
    const product = await this.inner.find(id);
    this.cache.set(id, product);
    return product;
  }
}

// دکوراتور در DI: مصرف‌کننده ProductRepository را inject می‌کند و نمی‌داند کش هم دارد
export const provideProductRepository = (): Provider => ({
  provide: ProductRepository,
  useFactory: () => new CachingProductRepository(new HttpProductRepository()),
});

// HttpInterceptor هم همین ایده است: هر interceptor درخواست را «می‌پیچد» و به بعدی می‌دهد.
