import { createContext, useContext, type ReactNode } from 'react';

interface Analytics {
  track(event: string, data?: Record<string, unknown>): void;
}

// نمونهٔ واقعی: در سطح ماژول، یک بار ساخته می‌شود
const realAnalytics: Analytics = {
  track: (event, data) => console.log('analytics', event, data),
};

// Context راه تزریق و جایگزینی singleton است: در برنامه واقعی، در تست جعلی
const AnalyticsContext = createContext<Analytics>(realAnalytics);

export const useAnalytics = (): Analytics => useContext(AnalyticsContext);

export function AnalyticsProvider({ client, children }: { client: Analytics; children: ReactNode }) {
  return <AnalyticsContext.Provider value={client}>{children}</AnalyticsContext.Provider>;
}

export function BuyButton({ productId }: { productId: number }) {
  const analytics = useAnalytics();
  return <button onClick={() => analytics.track('buy_click', { productId })}>خرید</button>;
}

// در تست: <AnalyticsProvider client={fakeAnalytics}><BuyButton productId={1} /></AnalyticsProvider>
