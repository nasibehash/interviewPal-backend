import { useMemo } from 'react';

type Graph = Readonly<Record<string, readonly string[]>>;

function shortestPath(graph: Graph, from: string, to: string): string[] | null {
  if (from === to) return [from];
  const parent = new Map<string, string | null>([[from, null]]);
  const queue = [from];
  for (let head = 0; head < queue.length; head++) {
    for (const friend of graph[queue[head]] ?? []) {
      if (parent.has(friend)) continue;
      parent.set(friend, queue[head]);
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

export function ConnectionBadge({ graph, me, other }: { graph: Graph; me: string; other: string }) {
  const path = useMemo(() => shortestPath(graph, me, other), [graph, me, other]);

  if (!path) return <p>ارتباطی بین شما و {other} وجود ندارد.</p>;
  if (path.length === 2) return <p>{other} دوست مستقیم شماست.</p>;
  return (
    <p>
      درجهٔ آشنایی {path.length - 1}: {path.join(' ← ')}
    </p>
  );
}
