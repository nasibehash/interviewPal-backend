function fewestCards(values: readonly number[], target: number): number[] | null {
  const best: number[] = new Array(target + 1).fill(Infinity);
  const last: number[] = new Array(target + 1).fill(0);
  best[0] = 0;

  for (let amount = 1; amount <= target; amount++) {
    for (const value of values) {
      if (value <= amount && best[amount - value] + 1 < best[amount]) {
        best[amount] = best[amount - value] + 1;
        last[amount] = value;
      }
    }
  }

  if (best[target] === Infinity) return null;
  const cards: number[] = [];
  for (let amount = target; amount > 0; amount -= last[amount]) cards.push(last[amount]);
  return cards;
}

// همین مسئله با memoization (بالا به پایین): فقط زیرمسئله‌هایی که لازم‌اند حل می‌شوند
function fewestCardsMemo(values: readonly number[], target: number): number {
  const memo = new Map<number, number>();
  const solve = (amount: number): number => {
    if (amount === 0) return 0;
    if (amount < 0) return Infinity;
    const cached = memo.get(amount);
    if (cached !== undefined) return cached;
    const result = 1 + Math.min(...values.map((v) => solve(amount - v)));
    memo.set(amount, result);
    return result;
  };
  return solve(target);
}

console.log(fewestCards([10, 30, 40], 60)); // [30, 30]
console.log(fewestCardsMemo([10, 30, 40], 60)); // 2
console.log(fewestCards([20, 50], 30)); // null
