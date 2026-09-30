// app/api/trips/route.ts
import { NextRequest, NextResponse } from 'next/server';

interface Activity {
  name: string;
  hours: number;
}

const activities: Activity[] = [
  { name: 'رستوران', hours: 1 },
  { name: 'بازار', hours: 1.5 },
  { name: 'موزه', hours: 2 },
  { name: 'باغ', hours: 2 },
  { name: 'قایق‌سواری', hours: 3 },
  { name: 'کوه‌نوردی', hours: 5 },
].sort((a, b) => a.hours - b.hours);

const MAX_RESULTS = 50; // هر درخواست باید هزینهٔ محدود داشته باشد

function itineraries(size: number, maxHours: number): string[][] {
  const results: string[][] = [];
  const chosen: Activity[] = [];

  const backtrack = (start: number, hoursUsed: number): void => {
    if (results.length >= MAX_RESULTS) return;
    if (chosen.length === size) {
      results.push(chosen.map((a) => a.name));
      return;
    }
    for (let i = start; i < activities.length; i++) {
      const total = hoursUsed + activities[i].hours;
      if (total > maxHours) break;
      chosen.push(activities[i]);
      backtrack(i + 1, total);
      chosen.pop();
    }
  };

  backtrack(0, 0);
  return results;
}

// GET /api/trips?size=3&hours=6
export function GET(request: NextRequest) {
  const size = Number(request.nextUrl.searchParams.get('size') ?? 3);
  const hours = Number(request.nextUrl.searchParams.get('hours') ?? 6);
  if (!Number.isInteger(size) || size < 1 || size > 5 || !(hours > 0 && hours <= 24)) {
    return NextResponse.json({ error: 'size بین ۱ تا ۵ و hours بین ۰ تا ۲۴ باشد' }, { status: 400 });
  }
  return NextResponse.json({ plans: itineraries(size, hours) });
}
