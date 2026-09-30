type Pair = readonly [left: number, right: number];

// اندیس دو عنصر مرتب که مجموعشان target است، یا null
function findPair(sorted: readonly number[], target: number): Pair | null {
  let left = 0;
  let right = sorted.length - 1;
  while (left < right) {
    const sum = sorted[left] + sorted[right];
    if (sum === target) return [left, right];
    if (sum < target) left++;
    else right--;
  }
  return null;
}

interface Product {
  name: string;
  price: number;
}

function giftCardPair(sortedProducts: readonly Product[], balance: number): [Product, Product] | null {
  const pair = findPair(sortedProducts.map((p) => p.price), balance);
  return pair ? [sortedProducts[pair[0]], sortedProducts[pair[1]]] : null;
}

const catalog: Product[] = [
  { name: 'کابل', price: 90_000 },
  { name: 'ماوس', price: 250_000 },
  { name: 'پاوربانک', price: 450_000 },
  { name: 'هدفون', price: 550_000 },
  { name: 'اسپیکر', price: 1_200_000 },
];

console.log(giftCardPair(catalog, 1_000_000)?.map((p) => p.name));
console.log(giftCardPair(catalog, 5)); // null
