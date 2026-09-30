import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

interface ProductFilter {
  q?: string;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  page?: number;
}

@Injectable({ providedIn: 'root' })
export class ProductApi {
  private readonly http = inject(HttpClient);

  search(filter: ProductFilter) {
    return this.http.get('/api/products', { params: this.toParams(filter) });
  }

  // HttpParams خودش یک Builder تغییرناپذیر است: set/append نمونهٔ جدید برمی‌گرداند
  private toParams(filter: ProductFilter): HttpParams {
    let params = new HttpParams();
    if (filter.q) params = params.set('q', filter.q);
    if (filter.minPrice !== undefined) params = params.set('minPrice', filter.minPrice);
    if (filter.maxPrice !== undefined) params = params.set('maxPrice', filter.maxPrice);
    if (filter.inStock) params = params.set('inStock', true);
    if (filter.page && filter.page > 1) params = params.set('page', filter.page);
    return params; // فقط فیلترهای واقعاً انتخاب‌شده در URL می‌آیند
  }
}
