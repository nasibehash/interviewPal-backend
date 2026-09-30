import { useMemo } from 'react';

interface Category {
  id: number;
  name: string;
  children: readonly Category[];
}

function findPath(node: Category, id: number, path: readonly Category[] = []): Category[] | null {
  const here = [...path, node];
  if (node.id === id) return here;
  for (const child of node.children) {
    const found = findPath(child, id, here);
    if (found) return found;
  }
  return null;
}

export function Breadcrumb({ root, selectedId }: { root: Category; selectedId: number }) {
  const trail = useMemo(() => findPath(root, selectedId) ?? [], [root, selectedId]);

  return (
    <nav aria-label="مسیر دسته‌بندی">
      {trail.map((c, i) =>
        i === trail.length - 1 ? (
          <span key={c.id} aria-current="page">{c.name}</span>
        ) : (
          <span key={c.id}>
            <a href={`/category/${c.id}`}>{c.name}</a> ›{' '}
          </span>
        ),
      )}
    </nav>
  );
}

// نمایش خود درخت هم DFS است: کامپوننت بازگشتی، هر گره فرزندانش را رندر می‌کند
export function CategoryTree({ node }: { node: Category }) {
  return (
    <li>
      {node.name}
      {node.children.length > 0 && (
        <ul>
          {node.children.map((child) => (
            <CategoryTree key={child.id} node={child} />
          ))}
        </ul>
      )}
    </li>
  );
}
