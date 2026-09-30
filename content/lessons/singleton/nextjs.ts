// lib/db.ts
// در حالت توسعه Next.js با هر تغییر فایل ماژول‌ها را دوباره لود می‌کند (hot reload)
// و اگر client را در سطح ماژول بسازی، هر بار یک اتصال جدید باز می‌شود تا دیتابیس پر شود.
// راه‌حل: نمونه را روی globalThis نگه دار تا از reload جان سالم به در ببرد.

class DbClient {
  constructor(readonly url: string) {
    console.log('اتصال جدید به دیتابیس ساخته شد');
  }

  async query(sql: string): Promise<unknown[]> {
    return [sql];
  }
}

const globalForDb = globalThis as unknown as { db?: DbClient };

export const db = globalForDb.db ?? new DbClient(process.env.DATABASE_URL ?? 'postgres://localhost/shop');

if (process.env.NODE_ENV !== 'production') globalForDb.db = db;

// استفاده در هر Route Handler یا Server Component:
// import { db } from '@/lib/db';
// const products = await db.query('SELECT * FROM products');
