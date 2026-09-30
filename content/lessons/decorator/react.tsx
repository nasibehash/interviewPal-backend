import type { ComponentType } from 'react';

interface AuthState {
  user: { name: string } | null;
}

// در پروژهٔ واقعی از Context یا یک سرویس می‌آید
declare function useAuth(): AuthState;

// HOC (کامپوننت مرتبه‌بالاتر) یک دکوراتور است: کامپوننت را می‌گیرد و نسخهٔ محافظت‌شده را برمی‌گرداند
export function withAuth<P extends object>(Component: ComponentType<P>): ComponentType<P> {
  function Protected(props: P) {
    const { user } = useAuth();
    if (!user) return <p>برای دیدن این صفحه وارد شوید.</p>;
    return <Component {...props} />;
  }
  Protected.displayName = `withAuth(${Component.displayName ?? Component.name})`;
  return Protected;
}

function OrderHistory({ limit }: { limit: number }) {
  return <p>آخرین {limit} سفارش شما</p>;
}

// کامپوننت اصلی دست‌نخورده می‌ماند و رفتار «نیاز به ورود» بیرون از آن اضافه می‌شود
export const ProtectedOrderHistory = withAuth(OrderHistory);
// <ProtectedOrderHistory limit={5} />
