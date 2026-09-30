import { useMemo } from 'react';

interface Row {
  line: number;
  date: string;
  amount: number;
  merchant: string;
}

/** شمارهٔ خط ردیف‌هایی که با ردیف دیگری کاملاً یکسان‌اند. */
export function useDuplicateLines(rows: readonly Row[]): ReadonlySet<number> {
  return useMemo(() => {
    const firstByKey = new Map<string, Row>();
    const lines = new Set<number>();
    for (const row of rows) {
      const key = `${row.date}|${row.amount}|${row.merchant.trim().toLowerCase()}`;
      const first = firstByKey.get(key);
      if (first) {
        lines.add(first.line);
        lines.add(row.line);
      } else {
        firstByKey.set(key, row);
      }
    }
    return lines;
  }, [rows]);
}

export function ImportPreview({ rows }: { rows: readonly Row[] }) {
  const duplicates = useDuplicateLines(rows);
  return (
    <table>
      <tbody>
        {rows.map((row) => (
          <tr key={row.line} style={{ background: duplicates.has(row.line) ? '#fee2e2' : undefined }}>
            <td>{row.line}</td>
            <td>{row.merchant}</td>
            <td>{row.amount}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
