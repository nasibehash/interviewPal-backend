interface Row {
  line: number;
  date: string;
  amount: number;
  merchant: string;
}

function groupBy<T, K>(items: readonly T[], keyOf: (item: T) => K): Map<K, T[]> {
  const groups = new Map<K, T[]>();
  for (const item of items) {
    const key = keyOf(item);
    const group = groups.get(key);
    if (group) group.push(item);
    else groups.set(key, [item]);
  }
  return groups;
}

const duplicateKey = (r: Row): string => `${r.date}|${r.amount}|${r.merchant.trim().toLowerCase()}`;

function findDuplicates(rows: readonly Row[]): Row[][] {
  return [...groupBy(rows, duplicateKey).values()].filter((group) => group.length > 1);
}

// کلاسیک: اندیس دو عددی که مجموعشان target است، بدون مرتب‌سازی، در یک پیمایش
function twoSum(nums: readonly number[], target: number): [number, number] | null {
  const seen = new Map<number, number>(); // مقدار -> اندیس
  for (let i = 0; i < nums.length; i++) {
    const j = seen.get(target - nums[i]);
    if (j !== undefined) return [j, i];
    seen.set(nums[i], i);
  }
  return null;
}

const rows: Row[] = [
  { line: 2, date: '2026-03-01', amount: 250_000, merchant: 'Digikala' },
  { line: 3, date: '2026-03-01', amount: 90_000, merchant: 'Snapp' },
  { line: 4, date: '2026-03-01', amount: 250_000, merchant: ' digikala ' },
];
console.log(findDuplicates(rows).map((g) => g.map((r) => r.line))); // [[2, 4]]
console.log(twoSum([8, 3, 11, 7], 10)); // [1, 3]
