// بسته‌های گردشگری: همهٔ ترکیب‌های `size` فعالیتی که مجموع زمانشان از `maxHours` بیشتر نشود
function itineraries(activities, size, maxHours) {
  const sorted = [...activities].sort((a, b) => a.hours - b.hours);
  const results = [];
  const chosen = [];

  function backtrack(start, hoursUsed) {
    if (chosen.length === size) {
      results.push(chosen.map((a) => a.name));
      return;
    }
    for (let i = start; i < sorted.length; i++) {
      const total = hoursUsed + sorted[i].hours;
      // مرتب‌شده‌ایم: اگر این یکی جا نمی‌شود، بعدی‌ها (طولانی‌تر) هم جا نمی‌شوند → هرس کل شاخه
      if (total > maxHours) break;
      chosen.push(sorted[i]); // انتخاب
      backtrack(i + 1, total); // ادامه با بقیهٔ گزینه‌ها
      chosen.pop(); // برگشت: انتخاب را پس بگیر و گزینهٔ بعدی را امتحان کن
    }
  }

  backtrack(0, 0);
  return results;
}

const activities = [
  { name: 'رستوران', hours: 1 },
  { name: 'بازار', hours: 1.5 },
  { name: 'موزه', hours: 2 },
  { name: 'باغ', hours: 2 },
  { name: 'قایق‌سواری', hours: 3 },
  { name: 'کوه‌نوردی', hours: 5 },
];

for (const plan of itineraries(activities, 3, 6)) console.log(plan.join(' + '));
console.log(itineraries(activities, 3, 2)); // []
