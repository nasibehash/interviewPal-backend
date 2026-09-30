// پرداخت با کارت‌های هدیه: کمترین تعداد کارت که مجموعشان دقیقاً برابر مبلغ باشد
function fewestCards(values, target) {
  const best = new Array(target + 1).fill(Infinity); // best[a] = کمترین تعداد کارت برای مبلغ a
  const last = new Array(target + 1).fill(0); // آخرین کارت استفاده‌شده، برای ساختن جواب
  best[0] = 0;

  for (let amount = 1; amount <= target; amount++) {
    for (const value of values) {
      if (value <= amount && best[amount - value] + 1 < best[amount]) {
        best[amount] = best[amount - value] + 1;
        last[amount] = value;
      }
    }
  }

  if (best[target] === Infinity) return null; // با این کارت‌ها ممکن نیست
  const cards = [];
  for (let amount = target; amount > 0; amount -= last[amount]) cards.push(last[amount]);
  return cards;
}

// روش حریصانه: همیشه بزرگ‌ترین کارت ممکن. برای بعضی مبلغ‌ها بهینه نیست
function greedyCards(values, target) {
  const cards = [];
  for (const value of [...values].sort((a, b) => b - a)) {
    while (target >= value) {
      cards.push(value);
      target -= value;
    }
  }
  return target === 0 ? cards : null;
}

const cardValues = [10, 30, 40]; // هزار تومان
console.log(fewestCards(cardValues, 60)); // [30, 30]
console.log(greedyCards(cardValues, 60)); // [40, 10, 10] ← ۳ کارت به‌جای ۲
console.log(fewestCards([20, 50], 30)); // null
