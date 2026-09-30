// app/api/categories/[id]/route.ts
import { NextResponse } from 'next/server';

interface Category {
  id: number;
  name: string;
  children: Category[];
}

const catalog: Category = {
  id: 1, name: 'کالای دیجیتال', children: [
    { id: 2, name: 'موبایل', children: [
      { id: 4, name: 'گوشی', children: [] },
      { id: 5, name: 'لوازم جانبی', children: [{ id: 7, name: 'قاب', children: [] }] },
    ] },
    { id: 3, name: 'لپ‌تاپ', children: [] },
  ],
};

function findPath(node: Category, id: number, path: Category[] = []): Category[] | null {
  const here = [...path, node];
  if (node.id === id) return here;
  for (const child of node.children) {
    const found = findPath(child, id, here);
    if (found) return found;
  }
  return null;
}

function collectIds(node: Category, ids: number[] = []): number[] {
  ids.push(node.id);
  for (const child of node.children) collectIds(child, ids);
  return ids;
}

// GET /api/categories/5 -> breadcrumb + شناسهٔ دسته و همهٔ زیردسته‌ها برای فیلتر محصولات
export async function GET(_request: Request, { params }: { params: Promise<{ id: string }> }) {
  const id = Number((await params).id);
  const path = Number.isInteger(id) ? findPath(catalog, id) : null;
  if (!path) return NextResponse.json({ error: 'دسته پیدا نشد' }, { status: 404 });

  return NextResponse.json({
    breadcrumb: path.map((c) => c.name),
    categoryIds: collectIds(path[path.length - 1]),
  });
}
