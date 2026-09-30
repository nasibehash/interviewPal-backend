// lib/with-auth.ts — دکوراتور برای Route Handlerها
import { NextRequest, NextResponse } from 'next/server';

type Handler = (request: NextRequest) => Promise<Response> | Response;

// در پروژهٔ واقعی توکن را با کتابخانهٔ احراز هویت بررسی کن
declare function verifySession(token: string | undefined): Promise<{ userId: string } | null>;

export function withAuth(handler: Handler): Handler {
  return async (request) => {
    const session = await verifySession(request.cookies.get('session')?.value);
    if (!session) return NextResponse.json({ error: 'وارد نشده‌اید' }, { status: 401 });
    return handler(request);
  };
}

export function withLogging(handler: Handler): Handler {
  return async (request) => {
    const started = performance.now();
    const response = await handler(request);
    console.log(`${request.method} ${request.nextUrl.pathname} → ${response.status} (${Math.round(performance.now() - started)}ms)`);
    return response;
  };
}

// app/api/orders/route.ts
// export const GET = withLogging(withAuth(async () => NextResponse.json([])));
// ترتیب مهم است: لاگ بیرونی است، پس درخواست‌های ردشده (۴۰۱) هم لاگ می‌شوند.
