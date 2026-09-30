import { Injectable, computed, signal } from '@angular/core';

interface Command {
  readonly label: string;
  execute(): void;
  undo(): void;
}

@Injectable({ providedIn: 'root' })
export class UndoService {
  private readonly done = signal<Command[]>([]);
  private readonly undone = signal<Command[]>([]);

  readonly canUndo = computed(() => this.done().length > 0);
  readonly canRedo = computed(() => this.undone().length > 0);
  readonly lastLabel = computed(() => this.done().at(-1)?.label ?? null);

  run(command: Command): void {
    command.execute();
    this.done.update((d) => [...d, command]);
    this.undone.set([]);
  }

  undo(): void {
    const command = this.done().at(-1);
    if (!command) return;
    command.undo();
    this.done.update((d) => d.slice(0, -1));
    this.undone.update((u) => [...u, command]);
  }

  redo(): void {
    const command = this.undone().at(-1);
    if (!command) return;
    command.execute();
    this.undone.update((u) => u.slice(0, -1));
    this.done.update((d) => [...d, command]);
  }
}

// نمونهٔ فرمان: تغییر نام کارت وظیفه؛ قبلی را نگه می‌دارد تا undo ممکن باشد
export const renameTask = (task: { title: string }, next: string): Command => {
  const previous = task.title;
  return {
    label: `تغییر نام به «${next}»`,
    execute: () => (task.title = next),
    undo: () => (task.title = previous),
  };
};
// <button [disabled]="!undo.canUndo()" (click)="undo.undo()">بازگردانی: {{ undo.lastLabel() }}</button>
