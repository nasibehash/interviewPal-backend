import { HttpInterceptorFn } from '@angular/common/http';
import { throwError } from 'rxjs';

/** سمت کلاینت: جلوی ارسال بیش از `limit` درخواست در هر `windowMs` میلی‌ثانیه را می‌گیرد. */
const LIMIT = 10;
const WINDOW_MS = 10_000;
const sentAt: number[] = [];

export const rateLimitInterceptor: HttpInterceptorFn = (req, next) => {
  const now = Date.now();
  while (sentAt.length > 0 && sentAt[0] <= now - WINDOW_MS) sentAt.shift();

  if (sentAt.length >= LIMIT) {
    const retryIn = Math.ceil((sentAt[0] + WINDOW_MS - now) / 1000);
    return throwError(() => new Error(`تعداد درخواست‌ها زیاد است؛ ${retryIn} ثانیه بعد دوباره تلاش کن.`));
  }

  sentAt.push(now);
  return next(req);
};

// app.config.ts:
//   provideHttpClient(withInterceptors([rateLimitInterceptor]))
