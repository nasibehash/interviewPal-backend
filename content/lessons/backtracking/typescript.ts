interface Activity {
  name: string;
  hours: number;
}

function itineraries(activities: readonly Activity[], size: number, maxHours: number, limit = 100): string[][] {
  const sorted = [...activities].sort((a, b) => a.hours - b.hours);
  const results: string[][] = [];
  const chosen: Activity[] = [];

  const backtrack = (start: number, hoursUsed: number): void => {
    if (results.length >= limit) return; // جلوی انفجار ترکیبی
    if (chosen.length === size) {
      results.push(chosen.map((a) => a.name));
      return;
    }
    for (let i = start; i < sorted.length; i++) {
      const total = hoursUsed + sorted[i].hours;
      if (total > maxHours) break; // هرس
      chosen.push(sorted[i]);
      backtrack(i + 1, total);
      chosen.pop();
    }
  };

  backtrack(0, 0);
  return results;
}

// مثال کلاسیک دیگر: همهٔ جایگشت‌ها (ترتیب‌های ممکن)
function permutations<T>(items: readonly T[]): T[][] {
  if (items.length <= 1) return [[...items]];
  return items.flatMap((item, i) =>
    permutations([...items.slice(0, i), ...items.slice(i + 1)]).map((rest) => [item, ...rest]),
  );
}

const activities: Activity[] = [
  { name: 'رستوران', hours: 1 },
  { name: 'بازار', hours: 1.5 },
  { name: 'موزه', hours: 2 },
  { name: 'باغ', hours: 2 },
  { name: 'قایق‌سواری', hours: 3 },
  { name: 'کوه‌نوردی', hours: 5 },
];

console.log(itineraries(activities, 3, 6).length);
console.log(permutations(['الف', 'ب', 'ج']).length); // 6
