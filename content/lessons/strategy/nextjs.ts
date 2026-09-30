// app/checkout/actions.ts
'use server';

interface Cart {
  total: number;
}

type DiscountStrategy = (cart: Cart) => number;

const strategies: Record<string, DiscountStrategy> = {
  none: () => 0,
  vip: (cart) => cart.total * 0.1,
  campaign: (cart) => Math.min(50_000, cart.total),
};

// قاعده در سرور انتخاب و اعمال می‌شود؛ کلاینت فقط «نام» قاعده را می‌فرستد و مبلغ را خودش حساب نمی‌کند
export async function quote(formData: FormData): Promise<{ payable: number } | { error: string }> {
  const name = String(formData.get('discount') ?? 'none');
  const total = Number(formData.get('total'));

  // Object.hasOwn جلوی کلیدهایی مثل "constructor" و "__proto__" را می‌گیرد
  if (!Object.hasOwn(strategies, name) || !Number.isFinite(total) || total < 0) {
    return { error: 'ورودی نامعتبر' };
  }
  return { payable: total - strategies[name]({ total }) };
}
