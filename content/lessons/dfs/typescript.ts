interface Category {
  id: number;
  name: string;
  children: readonly Category[];
}

const catalog: Category = {
  id: 1, name: 'کالای دیجیتال', children: [
    { id: 2, name: 'موبایل', children: [
      { id: 4, name: 'گوشی', children: [] },
      { id: 5, name: 'لوازم جانبی', children: [{ id: 7, name: 'قاب', children: [] }] },
    ] },
    { id: 3, name: 'لپ‌تاپ', children: [{ id: 6, name: 'گیمینگ', children: [] }] },
  ],
};

function findPath(node: Category, id: number, path: readonly string[] = []): string[] | null {
  const here = [...path, node.name];
  if (node.id === id) return here;
  for (const child of node.children) {
    const found = findPath(child, id, here);
    if (found) return found;
  }
  return null;
}

// generator: هر گره را به‌صورت تنبل (lazy) و به ترتیب DFS تولید می‌کند
function* walk(node: Category): Generator<Category> {
  yield node;
  for (const child of node.children) yield* walk(child);
}

const find = (root: Category, id: number): Category | undefined => {
  for (const node of walk(root)) if (node.id === id) return node;
};

function descendantIds(root: Category, id: number): number[] {
  const start = find(root, id);
  return start ? [...walk(start)].map((c) => c.id) : [];
}

console.log(findPath(catalog, 7)?.join(' > '));
console.log(descendantIds(catalog, 2)); // [2, 4, 5, 7]
