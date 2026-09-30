// app/api/study-plan/route.ts
import { NextRequest, NextResponse } from 'next/server';

type Prerequisites = Record<string, string[]>;

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
  return ready.length === missing.size ? ready : null;
}

// POST /api/study-plan  { "الگوریتم": ["ساختار داده"], "ساختار داده": [] }
export async function POST(request: NextRequest) {
  const body: unknown = await request.json().catch(() => null);
  const valid =
    typeof body === 'object' && body !== null &&
    Object.values(body).every((v) => Array.isArray(v) && v.every((x) => typeof x === 'string'));
  if (!valid) {
    return NextResponse.json({ error: 'بدنهٔ درخواست باید { درس: [پیش‌نیازها] } باشد' }, { status: 400 });
  }

  const plan = courseOrder(body as Prerequisites);
  if (!plan) return NextResponse.json({ error: 'پیش‌نیازها دور دارند' }, { status: 422 });
  return NextResponse.json({ plan });
}
