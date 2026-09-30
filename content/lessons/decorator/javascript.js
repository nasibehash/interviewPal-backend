// دکوراتور تابعی: تابع را می‌گیرد و نسخهٔ «تقویت‌شده» همان را برمی‌گرداند
const withLogging = (fn, name) => async (...args) => {
  console.log(`→ ${name}(${args.join(', ')})`);
  const result = await fn(...args);
  console.log(`← ${name} انجام شد`);
  return result;
};

const withRetry = (fn, attempts) => async (...args) => {
  let lastError;
  for (let attempt = 1; attempt <= attempts; attempt++) {
    try {
      return await fn(...args);
    } catch (error) {
      lastError = error;
      console.log(`تلاش ${attempt} ناموفق: ${error.message}`);
    }
  }
  throw lastError;
};

// تابع اصلی: گاهی خطای شبکه می‌دهد
let calls = 0;
async function fetchProduct(id) {
  if (++calls < 3) throw new Error('timeout');
  return { id, name: `محصول ${id}` };
}

// ترکیب لایه‌ها: بیرونی‌ترین اول اجرا می‌شود
const resilientFetch = withLogging(withRetry(fetchProduct, 3), 'fetchProduct');
console.log(await resilientFetch(7));
