import { Component, computed, signal } from '@angular/core';

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

@Component({
  selector: 'app-breadcrumb',
  template: `
    <nav aria-label="مسیر دسته‌بندی">
      @for (c of trail(); track c.id; let last = $last) {
        @if (last) {
          <span aria-current="page">{{ c.name }}</span>
        } @else {
          <a href="/category/{{ c.id }}">{{ c.name }}</a> ›
        }
      }
    </nav>
  `,
})
export class Breadcrumb {
  readonly root = signal<Category | null>(null);
  readonly selectedId = signal(0);

  // با تغییر دستهٔ انتخاب‌شده، مسیر دوباره با DFS ساخته می‌شود
  protected readonly trail = computed(() => {
    const root = this.root();
    return root ? (findPath(root, this.selectedId()) ?? []) : [];
  });
}
