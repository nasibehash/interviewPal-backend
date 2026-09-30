var orders = new OrderService();

var email = new EmailSender();
var dashboard = new Dashboard();
orders.OrderPlaced += email.OnOrderPlaced;
orders.OrderPlaced += dashboard.OnOrderPlaced;

orders.Place(new Order(1, 450_000m));
orders.OrderPlaced -= dashboard.OnOrderPlaced; // لغو اشتراک؛ فراموش کردنش نشتی حافظه است
orders.Place(new Order(2, 90_000m));

record Order(int Id, decimal Total);

class OrderService
{
    // event: فقط از داخل کلاس می‌شود فراخوانی کرد؛ بیرون فقط += و -= مجاز است
    public event EventHandler<Order>? OrderPlaced;

    public void Place(Order order)
    {
        // ...ذخیرهٔ سفارش...
        OrderPlaced?.Invoke(this, order);
    }
}

class EmailSender
{
    public void OnOrderPlaced(object? sender, Order order) => Console.WriteLine($"ایمیل تأیید برای سفارش {order.Id}");
}

class Dashboard
{
    public void OnOrderPlaced(object? sender, Order order) => Console.WriteLine($"داشبورد: فروش {order.Total}");
}
