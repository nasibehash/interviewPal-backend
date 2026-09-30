type Graph<T extends string> = Readonly<Record<T, readonly T[]>>;

function shortestPath<T extends string>(graph: Graph<T>, from: T, to: T): T[] | null {
  if (from === to) return [from];

  const parent = new Map<T, T | null>([[from, null]]);
  const queue: T[] = [from];

  for (let head = 0; head < queue.length; head++) {
    const user = queue[head];
    for (const friend of graph[user]) {
      if (parent.has(friend)) continue;
      parent.set(friend, user);
      if (friend === to) return pathTo(parent, to);
      queue.push(friend);
    }
  }
  return null;
}

function pathTo<T>(parent: ReadonlyMap<T, T | null>, node: T): T[] {
  const path: T[] = [];
  for (let current: T | null | undefined = node; current != null; current = parent.get(current)) {
    path.push(current);
  }
  return path.reverse();
}

// پیشنهاد دوست: همهٔ افراد در فاصلهٔ دقیقاً ۲ (دوست دوستان) که هنوز دوست مستقیم نیستند
function suggestions<T extends string>(graph: Graph<T>, user: T): T[] {
  const direct = new Set<T>(graph[user]);
  const result = new Set<T>();
  for (const friend of direct) {
    for (const candidate of graph[friend]) {
      if (candidate !== user && !direct.has(candidate)) result.add(candidate);
    }
  }
  return [...result];
}

const friends = {
  ali: ['sara', 'reza'],
  sara: ['ali', 'mina'],
  reza: ['ali', 'mina'],
  mina: ['sara', 'reza', 'omid'],
  omid: ['mina'],
} as const;

console.log(shortestPath<keyof typeof friends>(friends, 'ali', 'omid'));
console.log(suggestions<keyof typeof friends>(friends, 'ali')); // ['mina']
