import { Injectable, computed, signal } from '@angular/core';

type Graph = Readonly<Record<string, readonly string[]>>;

@Injectable({ providedIn: 'root' })
export class Connections {
  readonly graph = signal<Graph>({});
  readonly from = signal('');
  readonly to = signal('');

  /** «از طریق چه کسانی با او آشنا هستم؟» null یعنی ارتباطی نیست. */
  readonly path = computed(() => {
    const graph = this.graph();
    const from = this.from();
    const to = this.to();
    if (!from || !to) return null;
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
  });

  /** درجهٔ ارتباط: ۱ = دوست مستقیم، ۲ = دوست دوست … */
  readonly degree = computed(() => {
    const path = this.path();
    return path ? path.length - 1 : null;
  });
}
