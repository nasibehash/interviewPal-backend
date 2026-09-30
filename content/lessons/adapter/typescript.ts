// مدل داخلی (Target)
type ShipmentStatus = 'pending' | 'shipped' | 'delivered' | 'unknown';

interface Shipment {
  trackingId: string;
  status: ShipmentStatus;
  deliveryDate: Date | null;
}

// شکل‌های خارجی (Adaptee): دقیقاً همان‌طور که سرویس خارجی می‌فرستد
interface CourierAResponse {
  tracking_code: string;
  state: 'WAITING' | 'IN_TRANSIT' | 'DELIVERED';
  eta?: string;
}
interface CourierBResponse {
  id: number;
  status: number;
  estimated?: number;
}

interface CourierAdapter<Raw> {
  toShipment(raw: Raw): Shipment;
}

const courierA: CourierAdapter<CourierAResponse> = {
  toShipment: (raw) => ({
    trackingId: raw.tracking_code,
    status: ({ WAITING: 'pending', IN_TRANSIT: 'shipped', DELIVERED: 'delivered' } as const)[raw.state],
    deliveryDate: raw.eta ? new Date(raw.eta) : null,
  }),
};

const courierB: CourierAdapter<CourierBResponse> = {
  toShipment: (raw) => {
    const statuses: Record<number, ShipmentStatus> = { 1: 'pending', 2: 'shipped', 3: 'delivered' };
    return {
      trackingId: `B-${raw.id}`,
      status: statuses[raw.status] ?? 'unknown', // وضعیت ناشناخته برنامه را نمی‌شکند
      deliveryDate: raw.estimated ? new Date(raw.estimated * 1000) : null,
    };
  },
};

console.log(courierA.toShipment({ tracking_code: 'A-100', state: 'IN_TRANSIT', eta: '2026-03-10' }));
console.log(courierB.toShipment({ id: 55, status: 3, estimated: 1773100800 }));
