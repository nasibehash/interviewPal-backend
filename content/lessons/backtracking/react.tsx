import { useMemo, useState } from 'react';

interface Activity {
  name: string;
  hours: number;
}

function itineraries(activities: readonly Activity[], size: number, maxHours: number, limit: number): string[][] {
  const sorted = [...activities].sort((a, b) => a.hours - b.hours);
  const results: string[][] = [];
  const chosen: Activity[] = [];

  const backtrack = (start: number, hoursUsed: number): void => {
    if (results.length >= limit) return;
    if (chosen.length === size) {
      results.push(chosen.map((a) => a.name));
      return;
    }
    for (let i = start; i < sorted.length; i++) {
      const total = hoursUsed + sorted[i].hours;
      if (total > maxHours) break;
      chosen.push(sorted[i]);
      backtrack(i + 1, total);
      chosen.pop();
    }
  };

  backtrack(0, 0);
  return results;
}

export function TripPlanner({ activities }: { activities: readonly Activity[] }) {
  const [hours, setHours] = useState(6);
  const plans = useMemo(() => itineraries(activities, 3, hours, 20), [activities, hours]);

  return (
    <section>
      <label>
        زمان آزاد (ساعت)
        <input type="number" min={1} max={12} value={hours} onChange={(e) => setHours(Number(e.target.value) || 1)} />
      </label>
      <ul>
        {plans.length === 0 && <li>با این زمان برنامه‌ای پیدا نشد.</li>}
        {plans.map((plan) => (
          <li key={plan.join()}>{plan.join(' + ')}</li>
        ))}
      </ul>
    </section>
  );
}
