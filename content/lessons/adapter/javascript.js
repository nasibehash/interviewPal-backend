// دو شرکت پست، دو قالب پاسخ متفاوت. مدل داخلی ما: { trackingId, status, deliveryDate }

// پیک الف: { tracking_code: 'A-100', state: 'IN_TRANSIT', eta: '2026-03-10' }
const fromCourierA = (raw) => ({
  trackingId: raw.tracking_code,
  status: { WAITING: 'pending', IN_TRANSIT: 'shipped', DELIVERED: 'delivered' }[raw.state] ?? 'unknown',
  deliveryDate: raw.eta ? new Date(raw.eta) : null,
});

// پیک ب: { id: 55, status: 3, estimated: 1773100800 } ← وضعیت عددی و تاریخ به‌صورت epoch ثانیه
const fromCourierB = (raw) => ({
  trackingId: `B-${raw.id}`,
  status: { 1: 'pending', 2: 'shipped', 3: 'delivered' }[raw.status] ?? 'unknown',
  deliveryDate: raw.estimated ? new Date(raw.estimated * 1000) : null,
});

const adapters = { a: fromCourierA, b: fromCourierB };

// بقیهٔ برنامه فقط مدل داخلی را می‌بیند
function normalize(courier, raw) {
  return adapters[courier](raw);
}

console.log(normalize('a', { tracking_code: 'A-100', state: 'IN_TRANSIT', eta: '2026-03-10' }));
console.log(normalize('b', { id: 55, status: 3, estimated: 1773100800 }));
console.log(normalize('b', { id: 56, status: 99 })); // status: 'unknown' ← وضعیت جدیدِ پیک هم برنامه را نمی‌شکند
