// روش ۱ (ساده‌ترین در JS): ماژول ES فقط یک بار اجرا می‌شود، پس export یک نمونه همان Singleton است
// config.js:  export const config = loadConfig();

// روش ۲: کلاس با نمونهٔ ایستا، ساخت تنبل (lazy) در اولین استفاده
class DatabasePool {
  static #instance = null;
  static created = 0;

  static get instance() {
    return (DatabasePool.#instance ??= new DatabasePool());
  }

  constructor() {
    DatabasePool.created++; // ساخت گران: باز کردن ۱۰ اتصال به دیتابیس
  }

  query(sql) {
    return `اجرا شد: ${sql}`;
  }
}

const a = DatabasePool.instance;
const b = DatabasePool.instance;
console.log(a === b); // true
console.log(DatabasePool.created); // 1
console.log(b.query('SELECT 1'));

// هشدار: singleton وضعیت سراسری است و تست را سخت می‌کند؛
// بهتر است آن را از بیرون به کلاس‌ها بدهی (تزریق وابستگی) تا در تست جایگزین شود.
