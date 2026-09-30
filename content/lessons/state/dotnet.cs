var order = new Order();
Console.WriteLine(order.Status);   // Pending
order.Pay();
order.Ship();
Console.WriteLine(order.Status);   // Shipped

try { order.Cancel(); }
catch (InvalidOperationException e) { Console.WriteLine(e.Message); }

// الگوی State کلاسیک: هر وضعیت یک کلاس است و رفتار مجاز خودش را دارد
abstract class OrderState
{
    public abstract string Name { get; }
    public virtual OrderState Pay() => throw Invalid(nameof(Pay));
    public virtual OrderState Ship() => throw Invalid(nameof(Ship));
    public virtual OrderState Cancel() => throw Invalid(nameof(Cancel));

    protected InvalidOperationException Invalid(string action) =>
        new($"«{action}» در وضعیت «{Name}» مجاز نیست");
}

class Pending : OrderState
{
    public override string Name => "Pending";
    public override OrderState Pay() => new Paid();
    public override OrderState Cancel() => new Cancelled();
}

class Paid : OrderState
{
    public override string Name => "Paid";
    public override OrderState Ship() => new Shipped();
    public override OrderState Cancel() => new Cancelled();
}

class Shipped : OrderState { public override string Name => "Shipped"; }
class Cancelled : OrderState { public override string Name => "Cancelled"; }

// سفارش فقط وضعیت فعلی را نگه می‌دارد و کار را به آن می‌سپارد؛ هیچ if/switch روی وضعیت ندارد
class Order
{
    private OrderState _state = new Pending();

    public string Status => _state.Name;
    public void Pay() => _state = _state.Pay();
    public void Ship() => _state = _state.Ship();
    public void Cancel() => _state = _state.Cancel();
}
