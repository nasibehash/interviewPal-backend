class CyclicDependencyError extends Error {
  constructor(readonly remaining: readonly string[]) {
    super(`پیش‌نیازها دور دارند: ${remaining.join('، ')}`);
  }
}

// Kahn: هر بار درسی را بردار که هیچ پیش‌نیاز گذرانده‌نشده‌ای ندارد
function topologicalOrder(prerequisites: Readonly<Record<string, readonly string[]>>): string[] {
  const missing = new Map<string, number>();
  const unlocks = new Map<string, string[]>();

  for (const [course, needs] of Object.entries(prerequisites)) {
    missing.set(course, needs.length);
    for (const need of needs) {
      unlocks.set(need, [...(unlocks.get(need) ?? []), course]);
      if (!missing.has(need)) missing.set(need, 0);
    }
  }

  const ready = [...missing].filter(([, n]) => n === 0).map(([course]) => course);
  const order: string[] = [];
  for (let head = 0; head < ready.length; head++) {
    const course = ready[head];
    order.push(course);
    for (const next of unlocks.get(course) ?? []) {
      const left = missing.get(next)! - 1;
      missing.set(next, left);
      if (left === 0) ready.push(next);
    }
  }

  if (order.length < missing.size) {
    throw new CyclicDependencyError([...missing].filter(([, n]) => n > 0).map(([c]) => c));
  }
  return order;
}

console.log(topologicalOrder({ الگوریتم: ['ساختار داده'], 'ساختار داده': ['برنامه‌نویسی'], 'برنامه‌نویسی': [] }));

try {
  topologicalOrder({ الف: ['ب'], ب: ['الف'], ج: [] });
} catch (error) {
  if (error instanceof CyclicDependencyError) console.log(error.remaining); // ['الف', 'ب']
}
