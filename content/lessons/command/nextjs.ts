// app/tasks/actions.ts — هر Server Action یک «فرمان» است: نام، ورودی و اثر مشخص دارد
'use server';

interface Task {
  id: number;
  title: string;
  done: boolean;
}

const tasks = new Map<number, Task>([[1, { id: 1, title: 'نوشتن گزارش', done: false }]]);

// لاگ فرمان‌ها (برای undo سمت سرور یا audit)؛ در واقعیت در دیتابیس ذخیره می‌شود
const journal: { action: string; taskId: number; before: Task }[] = [];

export async function renameTask(formData: FormData): Promise<void> {
  const id = Number(formData.get('id'));
  const title = String(formData.get('title') ?? '').trim();
  const task = tasks.get(id);
  if (!task || title.length === 0 || title.length > 100) throw new Error('ورودی نامعتبر');

  journal.push({ action: 'rename', taskId: id, before: { ...task } }); // حالت قبلی برای بازگردانی
  tasks.set(id, { ...task, title });
}

export async function undoLast(): Promise<void> {
  const entry = journal.pop();
  if (entry) tasks.set(entry.taskId, entry.before);
}
