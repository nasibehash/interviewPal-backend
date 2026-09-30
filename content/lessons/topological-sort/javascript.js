// ترتیب گذراندن درس‌ها با پیش‌نیاز (الگوریتم Kahn)
function courseOrder(prerequisites) {
  // prerequisites: { درس: [پیش‌نیازها] }
  const missing = new Map(); // درس -> تعداد پیش‌نیازهای گذراننده‌نشده
  const unlocks = new Map(); // درس -> درس‌هایی که با گذراندنش باز می‌شوند

  for (const [course, needs] of Object.entries(prerequisites)) {
    missing.set(course, needs.length);
    for (const need of needs) {
      if (!unlocks.has(need)) unlocks.set(need, []);
      unlocks.get(need).push(course);
      if (!missing.has(need)) missing.set(need, 0); // درسی که خودش پیش‌نیاز ندارد
    }
  }

  const ready = [...missing].filter(([, count]) => count === 0).map(([course]) => course);
  const order = [];
  while (ready.length > 0) {
    const course = ready.shift();
    order.push(course);
    for (const next of unlocks.get(course) ?? []) {
      missing.set(next, missing.get(next) - 1);
      if (missing.get(next) === 0) ready.push(next);
    }
  }

  if (order.length !== missing.size) throw new Error('پیش‌نیازها دور دارند؛ ترتیبی وجود ندارد');
  return order;
}

console.log(courseOrder({ 'ساختار داده': ['برنامه‌نویسی'], 'الگوریتم': ['ساختار داده', 'ریاضی گسسته'], 'برنامه‌نویسی': [], 'ریاضی گسسته': [] }));
// ['برنامه‌نویسی', 'ریاضی گسسته', 'ساختار داده', 'الگوریتم']

try {
  courseOrder({ الف: ['ب'], ب: ['الف'] });
} catch (error) {
  console.log(error.message); // پیش‌نیازها دور دارند؛ ترتیبی وجود ندارد
}
