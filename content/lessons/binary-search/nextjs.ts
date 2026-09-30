// app/api/products/route.ts
import { NextRequest, NextResponse } from 'next/server';

interface Product {
  id: number;
  name: string;
  price: number;
}

// یک بار موقع بالا آمدن سرور مرتب می‌شود، نه در هر درخواست
const catalog: Product[] = [
  { id: 1, name: 'کابل شارژ', price: 90_000 },
  { id: 2, name: 'پاوربانک', price: 450_000 },
  { id: 3, name: 'هدفون', price: 700_000 },
  { id: 4, name: 'اسپیکر', price: 1_200_000 },
].sort((a, b) => a.price - b.price);

function lowerBound(target: number): number {
  let lo = 0;
  let hi = catalog.length;
  while (lo < hi) {
    const mid = lo + Math.floor((hi - lo) / 2);
    if (catalog[mid].price < target) lo = mid + 1;
    else hi = mid;
  }
  return lo;
}

// GET /api/products?min=400000&max=1200000
export function GET(request: NextRequest) {
  const params = request.nextUrl.searchParams;
  const min = Number(params.get('min') ?? 0);
  const max = Number(params.get('max') ?? Number.MAX_SAFE_INTEGER - 1);
  if (Number.isNaN(min) || Number.isNaN(max)) {
    return NextResponse.json({ error: 'min و max باید عدد باشند' }, { status: 400 });
  }
  return NextResponse.json(catalog.slice(lowerBound(min), lowerBound(max + 1)));
}
