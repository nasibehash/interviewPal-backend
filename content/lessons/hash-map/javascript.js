// گروه‌بندی با Map: هر کلید یک بار محاسبه می‌شود و جست‌وجو تقریباً O(1) است
function groupBy(items, keyOf) {
  const groups = new Map();
  for (const item of items) {
    const key = keyOf(item);
    const group = groups.get(key);
    if (group) group.push(item);
    else groups.set(key, [item]);
  }
  return groups;
}

// تراکنش‌های تکراری در فایل واردشده: همان تاریخ، مبلغ و پذیرنده
function findDuplicates(rows) {
  const groups = groupBy(rows, (r) => `${r.date}|${r.amount}|${r.merchant.trim().toLowerCase()}`);
  return [...groups.values()].filter((group) => group.length > 1);
}

const rows = [
  { line: 2, date: '2026-03-01', amount: 250_000, merchant: 'Digikala' },
  { line: 3, date: '2026-03-01', amount: 90_000, merchant: 'Snapp' },
  { line: 4, date: '2026-03-01', amount: 250_000, merchant: ' digikala ' },
  { line: 5, date: '2026-03-02', amount: 90_000, merchant: 'Snapp' },
];

for (const group of findDuplicates(rows)) {
  console.log('تکراری در خط‌های', group.map((r) => r.line)); // [2, 4]
}
