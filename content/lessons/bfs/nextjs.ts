// app/api/connections/route.ts
import { NextRequest, NextResponse } from 'next/server';

// در پروژهٔ واقعی از دیتابیس گراف یا جدول دوستی خوانده می‌شود
const graph: Record<string, string[]> = {
  ali: ['sara', 'reza'],
  sara: ['ali', 'mina'],
  reza: ['ali', 'mina'],
  mina: ['sara', 'reza', 'omid'],
  omid: ['mina'],
};

const MAX_DEPTH = 6; // بیشتر از «شش درجهٔ جدایی» معنی ندارد و هزینه را محدود می‌کند

function shortestPath(from: string, to: string): string[] | null {
  const parent = new Map<string, string | null>([[from, null]]);
  const depth = new Map<string, number>([[from, 0]]);
  const queue = [from];

  for (let head = 0; head < queue.length; head++) {
    const user = queue[head];
    if (depth.get(user)! >= MAX_DEPTH) continue;
    for (const friend of graph[user] ?? []) {
      if (parent.has(friend)) continue;
      parent.set(friend, user);
      depth.set(friend, depth.get(user)! + 1);
      if (friend === to) {
        const path: string[] = [];
        for (let n: string | null | undefined = to; n != null; n = parent.get(n)) path.push(n);
        return path.reverse();
      }
      queue.push(friend);
    }
  }
  return null;
}

// GET /api/connections?from=ali&to=omid
export function GET(request: NextRequest) {
  const from = request.nextUrl.searchParams.get('from');
  const to = request.nextUrl.searchParams.get('to');
  if (!from || !to || !(from in graph)) {
    return NextResponse.json({ error: 'from و to الزامی‌اند' }, { status: 400 });
  }
  return NextResponse.json({ path: from === to ? [from] : shortestPath(from, to) });
}
