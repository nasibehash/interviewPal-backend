// app/gift-card/actions.ts
'use server';

interface Product {
  id: number;
  name: string;
  price: number;
}

// در پروژهٔ واقعی از دیتابیس می‌آید: SELECT ... ORDER BY price
async function loadSortedProducts(): Promise<Product[]> {
  return [
    { id: 1, name: 'کابل', price: 90_000 },
    { id: 2, name: 'ماوس', price: 250_000 },
    { id: 3, name: 'پاوربانک', price: 450_000 },
    { id: 4, name: 'هدفون', price: 550_000 },
  ];
}

export async function findGiftCardPair(formData: FormData): Promise<string> {
  const balance = Number(formData.get('balance'));
  if (!Number.isFinite(balance) || balance <= 0) return 'مبلغ نامعتبر است';

  const products = await loadSortedProducts();
  let left = 0;
  let right = products.length - 1;
  while (left < right) {
    const sum = products[left].price + products[right].price;
    if (sum === balance) return `${products[left].name} + ${products[right].name}`;
    if (sum < balance) left++;
    else right--;
  }
  return 'ترکیبی پیدا نشد';
}
