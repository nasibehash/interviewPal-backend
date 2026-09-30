var checkout = new CheckoutFacade(new Inventory(), new Payments(), new Shipping());

Console.WriteLine(await checkout.PlaceOrderAsync(new Order(1, ["a", "b"], 450_000m)));
Console.WriteLine(await checkout.PlaceOrderAsync(new Order(2, ["c"], 2_000_000m)));

record Order(int Id, string[] Items, decimal Total);

class Inventory
{
    public Task ReserveAsync(string[] items) { Console.WriteLine($"انبار: رزرو {items.Length} کالا"); return Task.CompletedTask; }
    public Task ReleaseAsync(string[] items) { Console.WriteLine($"انبار: آزادسازی {items.Length} کالا"); return Task.CompletedTask; }
}

class Payments
{
    public Task ChargeAsync(decimal amount) =>
        amount > 1_000_000m ? throw new InvalidOperationException("موجودی کافی نیست") : Task.CompletedTask;
}

class Shipping
{
    public Task CreateAsync(int orderId) { Console.WriteLine($"ارسال: مرسولهٔ سفارش {orderId}"); return Task.CompletedTask; }
}

// Facade: مصرف‌کننده فقط PlaceOrderAsync را می‌شناسد
class CheckoutFacade(Inventory inventory, Payments payments, Shipping shipping)
{
    public async Task<string> PlaceOrderAsync(Order order)
    {
        await inventory.ReserveAsync(order.Items);
        try
        {
            await payments.ChargeAsync(order.Total);
            await shipping.CreateAsync(order.Id);
            return $"سفارش {order.Id} ثبت شد";
        }
        catch (InvalidOperationException e)
        {
            await inventory.ReleaseAsync(order.Items); // جبران
            return $"سفارش {order.Id} ناموفق: {e.Message}";
        }
    }
}
