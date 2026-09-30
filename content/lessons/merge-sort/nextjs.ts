// app/api/orders/route.ts
import { NextRequest, NextResponse } from 'next/server';

interface Order {
  id: number;
  customer: string;
  total: number;
  createdAt: string;
}

async function loadOrders(): Promise<Order[]> {
  return [
    { id: 1, customer: 'ali', total: 500, createdAt: '2026-03-01' },
    { id: 2, customer: 'sara', total: 300, createdAt: '2026-03-02' },
    { id: 3, customer: 'ali', total: 300, createdAt: '2026-03-03' },
  ];
}

const sorters = {
  total: (a: Order, b: Order) => a.total - b.total,
  customer: (a: Order, b: Order) => a.customer.localeCompare(b.customer),
} as const;

// GET /api/orders?sort=total
export async function GET(request: NextRequest) {
  const sort = request.nextUrl.searchParams.get('sort') ?? 'total';
  if (!(sort in sorters)) {
    return NextResponse.json({ error: 'sort نامعتبر است' }, { status: 400 });
  }

  const orders = await loadOrders(); // از قبل بر اساس تاریخ ثبت مرتب است
  // Array.prototype.sort در ES2019 به بعد پایدار است (مرتب‌سازی ادغامی/Timsort):
  // در هم‌مبلغ‌ها ترتیب تاریخ ثبت حفظ می‌شود
  orders.sort(sorters[sort as keyof typeof sorters]);
  return NextResponse.json(orders);
}
