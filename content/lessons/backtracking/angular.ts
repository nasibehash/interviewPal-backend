import { Component, computed, signal } from '@angular/core';

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

@Component({
  selector: 'app-trip-planner',
  template: `
    <label>
      زمان آزاد (ساعت)
      <input type="number" min="1" max="12" [value]="hours()" (input)="onHours($event)" />
    </label>
    <ul>
      @for (plan of plans(); track plan.join()) {
        <li>{{ plan.join(' + ') }}</li>
      } @empty {
        <li>با این زمان برنامه‌ای پیدا نشد.</li>
      }
    </ul>
  `,
})
export class TripPlanner {
  readonly activities = signal<Activity[]>([]);
  protected readonly hours = signal(6);

  // هرس باعث می‌شود با زیاد شدن فعالیت‌ها هم پیشنهادها سریع ساخته شوند؛ limit جلوی انفجار را می‌گیرد
  protected readonly plans = computed(() => itineraries(this.activities(), 3, this.hours(), 20));

  protected onHours(event: Event): void {
    this.hours.set(Number((event.target as HTMLInputElement).value) || 1);
  }
}
