interface ProductRepository {
  find(id: number): Promise<{ id: number; name: string }>;
}

class ApiProductRepository implements ProductRepository {
  async find(id: number) {
    return { id, name: `محصول ${id}` }; // در واقعیت: fetch(...)
  }
}

// دکوراتور: همان رابط را پیاده می‌کند و یک شیء از همان رابط را در خود دارد
class CachingRepository implements ProductRepository {
  private readonly cache = new Map<number, { id: number; name: string }>();

  constructor(private readonly inner: ProductRepository) {}

  async find(id: number) {
    const hit = this.cache.get(id);
    if (hit) return hit;
    const product = await this.inner.find(id);
    this.cache.set(id, product);
    return product;
  }
}

class LoggingRepository implements ProductRepository {
  constructor(private readonly inner: ProductRepository, private readonly log: (message: string) => void) {}

  async find(id: number) {
    const started = Date.now();
    const product = await this.inner.find(id);
    this.log(`find(${id}) در ${Date.now() - started}ms`);
    return product;
  }
}

// لایه‌ها دلخواه ترکیب می‌شوند؛ مصرف‌کننده فقط ProductRepository را می‌شناسد
const repository: ProductRepository = new LoggingRepository(new CachingRepository(new ApiProductRepository()), console.log);
async function demo(): Promise<void> {
  await repository.find(1);
  await repository.find(1); // بار دوم از کش می‌آید ولی همچنان لاگ می‌شود
}
void demo();
