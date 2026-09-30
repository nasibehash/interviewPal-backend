import { useCallback, useRef, useState } from 'react';

/** هوکی که بیش از `limit` اجرا در `windowMs` را رد می‌کند (مثلاً دکمهٔ «ارسال کد پیامکی»). */
export function useRateLimit(limit: number, windowMs: number) {
  const hits = useRef<number[]>([]);
  const [blockedFor, setBlockedFor] = useState(0);

  const attempt = useCallback(
    (action: () => void): boolean => {
      const now = Date.now();
      const window = hits.current;
      while (window.length > 0 && window[0] <= now - windowMs) window.shift();

      if (window.length >= limit) {
        setBlockedFor(Math.ceil((window[0] + windowMs - now) / 1000));
        return false;
      }
      window.push(now);
      setBlockedFor(0);
      action();
      return true;
    },
    [limit, windowMs],
  );

  return { attempt, blockedFor };
}

export function SendCodeButton({ send }: { send: () => void }) {
  const { attempt, blockedFor } = useRateLimit(3, 60_000);
  return (
    <>
      <button onClick={() => attempt(send)}>ارسال کد</button>
      {blockedFor > 0 && <p>تا {blockedFor} ثانیهٔ دیگر نمی‌توانی دوباره کد بگیری.</p>}
    </>
  );
}
