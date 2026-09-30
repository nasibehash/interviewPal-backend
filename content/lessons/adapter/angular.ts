import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

export type ShipmentStatus = 'pending' | 'shipped' | 'delivered' | 'unknown';

/** مدلی که کامپوننت‌ها می‌شناسند. */
export interface Shipment {
  trackingId: string;
  status: ShipmentStatus;
  deliveryDate: Date | null;
}

/** شکل واقعی پاسخ سرویس بیرونی؛ فقط همین فایل آن را می‌داند. */
interface CourierApiResponse {
  tracking_code: string;
  state: string;
  eta: string | null;
}

const STATUSES: Record<string, ShipmentStatus> = {
  WAITING: 'pending',
  IN_TRANSIT: 'shipped',
  DELIVERED: 'delivered',
};

@Injectable({ providedIn: 'root' })
export class ShipmentApi {
  private readonly http = inject(HttpClient);

  track(code: string): Observable<Shipment> {
    return this.http
      .get<CourierApiResponse>(`/api/courier/${encodeURIComponent(code)}`)
      .pipe(map((raw) => this.adapt(raw))); // Adapter: تبدیل در مرز برنامه
  }

  private adapt(raw: CourierApiResponse): Shipment {
    return {
      trackingId: raw.tracking_code,
      status: STATUSES[raw.state] ?? 'unknown',
      deliveryDate: raw.eta ? new Date(raw.eta) : null,
    };
  }
}
// اگر پیک فردا نام فیلدها را عوض کند، فقط adapt تغییر می‌کند؛ هیچ کامپوننتی نه.
