// app/api/track/[code]/route.ts — لایهٔ ضدتخریب: مدل پیک بیرونی هرگز به کلاینت نمی‌رسد
import { NextResponse } from 'next/server';

interface CourierResponse {
  tracking_code: string;
  state: string;
  eta: string | null;
}

interface Shipment {
  trackingId: string;
  status: 'pending' | 'shipped' | 'delivered' | 'unknown';
  deliveryDate: string | null;
}

const STATUSES: Record<string, Shipment['status']> = {
  WAITING: 'pending',
  IN_TRANSIT: 'shipped',
  DELIVERED: 'delivered',
};

function toShipment(raw: CourierResponse): Shipment {
  return {
    trackingId: raw.tracking_code,
    status: STATUSES[raw.state] ?? 'unknown',
    deliveryDate: raw.eta ? new Date(raw.eta).toISOString() : null,
  };
}

export async function GET(_request: Request, { params }: { params: Promise<{ code: string }> }) {
  const { code } = await params;
  const response = await fetch(`https://courier.example.com/v2/track/${encodeURIComponent(code)}`, {
    headers: { Authorization: `Bearer ${process.env.COURIER_TOKEN}` },
  });
  if (!response.ok) return NextResponse.json({ error: 'مرسوله پیدا نشد' }, { status: 404 });

  return NextResponse.json(toShipment((await response.json()) as CourierResponse));
}
