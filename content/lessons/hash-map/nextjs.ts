// app/import/actions.ts
'use server';

export interface ImportResult {
  rows: number;
  duplicateLines: number[];
}

// ورودی: متن CSV با ستون‌های date,amount,merchant
export async function validateImport(formData: FormData): Promise<ImportResult> {
  const file = formData.get('file');
  if (!(file instanceof File)) throw new Error('فایلی ارسال نشده است');

  const lines = (await file.text()).split('\n').map((l) => l.trim()).filter(Boolean).slice(1); // بدون سرستون
  const firstLineByKey = new Map<string, number>();
  const duplicateLines = new Set<number>();

  lines.forEach((text, index) => {
    const lineNumber = index + 2; // شمارهٔ خط در فایل
    const [date, amount, merchant = ''] = text.split(',').map((c) => c.trim());
    const key = `${date}|${amount}|${merchant.toLowerCase()}`;

    const first = firstLineByKey.get(key);
    if (first !== undefined) {
      duplicateLines.add(first);
      duplicateLines.add(lineNumber);
    } else {
      firstLineByKey.set(key, lineNumber);
    }
  });

  return { rows: lines.length, duplicateLines: [...duplicateLines].sort((a, b) => a - b) };
}
