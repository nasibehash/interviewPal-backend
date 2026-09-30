import { Component, computed, signal } from '@angular/core';

type Prerequisites = Readonly<Record<string, readonly string[]>>;

function courseOrder(prerequisites: Prerequisites): string[] | null {
  const missing = new Map<string, number>();
  const unlocks = new Map<string, string[]>();
  for (const [course, needs] of Object.entries(prerequisites)) {
    missing.set(course, needs.length);
    for (const need of needs) {
      unlocks.set(need, [...(unlocks.get(need) ?? []), course]);
      if (!missing.has(need)) missing.set(need, 0);
    }
  }

  const ready = [...missing].filter(([, n]) => n === 0).map(([c]) => c);
  for (let head = 0; head < ready.length; head++) {
    for (const next of unlocks.get(ready[head]) ?? []) {
      const left = missing.get(next)! - 1;
      missing.set(next, left);
      if (left === 0) ready.push(next);
    }
  }
  return ready.length === missing.size ? ready : null; // null = دور
}

@Component({
  selector: 'app-study-plan',
  template: `
    @if (plan(); as courses) {
      <ol>
        @for (course of courses; track course) {
          <li>{{ course }}</li>
        }
      </ol>
    } @else {
      <p role="alert">پیش‌نیازها دور دارند و برنامه‌ای ساخته نمی‌شود.</p>
    }
  `,
})
export class StudyPlan {
  readonly prerequisites = signal<Prerequisites>({
    'ساختار داده': ['برنامه‌نویسی'],
    'الگوریتم': ['ساختار داده', 'ریاضی گسسته'],
    'برنامه‌نویسی': [],
    'ریاضی گسسته': [],
  });

  protected readonly plan = computed(() => courseOrder(this.prerequisites()));
}
