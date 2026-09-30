IProductRepository repository =
    new LoggingRepository(
        new RetryRepository(new FlakyRepository(), attempts: 3));

var product = await repository.FindAsync(7);
Console.WriteLine(product.Name);

record Product(int Id, string Name);

interface IProductRepository
{
    Task<Product> FindAsync(int id);
}

class FlakyRepository : IProductRepository
{
    private int _calls;

    public Task<Product> FindAsync(int id) =>
        ++_calls < 3 ? throw new TimeoutException("timeout") : Task.FromResult(new Product(id, $"محصول {id}"));
}

// هر دکوراتور همان رابط را پیاده می‌کند و یک IProductRepository دیگر را در خود دارد
class RetryRepository(IProductRepository inner, int attempts) : IProductRepository
{
    public async Task<Product> FindAsync(int id)
    {
        for (var attempt = 1; ; attempt++)
        {
            try { return await inner.FindAsync(id); }
            catch (TimeoutException) when (attempt < attempts)
            {
                Console.WriteLine($"تلاش {attempt} ناموفق");
            }
        }
    }
}

class LoggingRepository(IProductRepository inner) : IProductRepository
{
    public async Task<Product> FindAsync(int id)
    {
        Console.WriteLine($"FindAsync({id})");
        var product = await inner.FindAsync(id);
        Console.WriteLine("انجام شد");
        return product;
    }
}
