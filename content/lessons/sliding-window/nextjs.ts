// app/api/search/route.ts
import { NextRequest, NextResponse } from 'next/server';

const LIMIT = 30;
const WINDOW_MS = 60_000;

// توجه: این Map فقط داخل یک پروسه معتبر است. با چند instance یا serverless از Redis استفاده کن.
const hits = new Map<string, number[]>();

function allow(ip: string, now = Date.now()): { ok: boolean; retryAfter: number } {
  const window = hits.get(ip) ?? [];
  while (window.length > 0 && window[0] <= now - WINDOW_MS) window.shift();

  if (window.length >= LIMIT) {
    return { ok: false, retryAfter: Math.ceil((window[0] + WINDOW_MS - now) / 1000) };
  }
  window.push(now);
  hits.set(ip, window);
  return { ok: true, retryAfter: 0 };
}

export function GET(request: NextRequest) {
  const ip = request.headers.get('x-forwarded-for')?.split(',')[0]?.trim() ?? 'unknown';
  const { ok, retryAfter } = allow(ip);
  if (!ok) {
    return NextResponse.json(
      { error: 'تعداد درخواست‌ها زیاد است' },
      { status: 429, headers: { 'Retry-After': String(retryAfter) } },
    );
  }
  return NextResponse.json({ results: [] });
}
