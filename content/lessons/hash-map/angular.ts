import { Component, computed, signal } from '@angular/core';

interface Row {
  line: number;
  date: string;
  amount: number;
  merchant: string;
}

@Component({
  selector: 'app-import-preview',
  template: `
    <table>
      @for (row of rows(); track row.line) {
        <tr [class.duplicate]="duplicateLines().has(row.line)">
          <td>{{ row.line }}</td>
          <td>{{ row.merchant }}</td>
          <td>{{ row.amount }}</td>
        </tr>
      }
    </table>
    @if (duplicateLines().size > 0) {
      <p>{{ duplicateLines().size }} ردیف تکراری پیدا شد.</p>
    }
  `,
})
export class ImportPreview {
  readonly rows = signal<Row[]>([]);

  // Map کلید -> اولین ردیف؛ ردیف بعدی با همان کلید تکراری است. یک پیمایش، O(n)
  protected readonly duplicateLines = computed(() => {
    const firstByKey = new Map<string, Row>();
    const lines = new Set<number>();
    for (const row of this.rows()) {
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
  });
}
