// کوتاه‌ترین مسیر در شبکهٔ دوستی (گراف بدون وزن) با پیمایش سطحی
function shortestPath(graph, from, to) {
  if (from === to) return [from];

  const parent = new Map([[from, null]]); // هم «دیده‌شده» است هم مسیر برگشت
  const queue = [from];
  let head = 0; // به‌جای shift() که O(n) است

  while (head < queue.length) {
    const user = queue[head++];
    for (const friend of graph[user] ?? []) {
      if (parent.has(friend)) continue; // قبلاً با مسیر کوتاه‌تر یا مساوی دیده شده
      parent.set(friend, user);
      if (friend === to) return pathTo(parent, to);
      queue.push(friend);
    }
  }
  return null; // بین این دو نفر هیچ ارتباطی نیست
}

function pathTo(parent, node) {
  const path = [];
  for (let current = node; current !== null; current = parent.get(current)) path.push(current);
  return path.reverse();
}

const friends = {
  ali: ['sara', 'reza'],
  sara: ['ali', 'mina'],
  reza: ['ali', 'mina'],
  mina: ['sara', 'reza', 'omid'],
  omid: ['mina'],
  nima: [],
};

console.log(shortestPath(friends, 'ali', 'omid')); // ['ali', 'sara', 'mina', 'omid']
console.log(shortestPath(friends, 'ali', 'nima')); // null
