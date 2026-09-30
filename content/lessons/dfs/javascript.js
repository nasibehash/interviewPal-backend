// درخت دسته‌بندی محصولات
const catalog = {
  id: 1, name: 'کالای دیجیتال', children: [
    { id: 2, name: 'موبایل', children: [
      { id: 4, name: 'گوشی', children: [] },
      { id: 5, name: 'لوازم جانبی', children: [{ id: 7, name: 'قاب', children: [] }] },
    ] },
    { id: 3, name: 'لپ‌تاپ', children: [{ id: 6, name: 'گیمینگ', children: [] }] },
  ],
};

// ۱) مسیر (breadcrumb) تا یک دسته: عمیق می‌رویم، اگر نبود برمی‌گردیم (backtrack)
function findPath(node, id, path = []) {
  const here = [...path, node.name];
  if (node.id === id) return here;
  for (const child of node.children) {
    const found = findPath(child, id, here);
    if (found) return found;
  }
  return null;
}

// ۲) شناسهٔ دسته و همهٔ زیردسته‌ها، برای فیلتر محصولات: نسخهٔ غیربازگشتی با پشته
function descendantIds(root, id) {
  const start = find(root, id);
  if (!start) return [];
  const result = [];
  const stack = [start];
  while (stack.length > 0) {
    const node = stack.pop();
    result.push(node.id);
    stack.push(...node.children);
  }
  return result;
}

function find(node, id) {
  if (node.id === id) return node;
  for (const child of node.children) {
    const found = find(child, id);
    if (found) return found;
  }
  return null;
}

console.log(findPath(catalog, 7).join(' > ')); // کالای دیجیتال > موبایل > لوازم جانبی > قاب
console.log(findPath(catalog, 99)); // null
console.log(descendantIds(catalog, 2).sort((a, b) => a - b)); // [2, 4, 5, 7]
