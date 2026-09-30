// رابطی که برنامهٔ ما می‌خواهد
ICourierClient courier = new LegacyCourierAdapter(new LegacyCourierApi());
var shipment = courier.Track("A-100");
Console.WriteLine($"{shipment.TrackingId}: {shipment.Status}, تحویل {shipment.DeliveryDate:yyyy-MM-dd}");

enum ShipmentStatus { Pending, Shipped, Delivered, Unknown }
record Shipment(string TrackingId, ShipmentStatus Status, DateOnly? DeliveryDate);

interface ICourierClient
{
    Shipment Track(string code);
}

// کتابخانهٔ قدیمی/خارجی که نمی‌توانیم عوضش کنیم
class LegacyCourierApi
{
    public (string tracking_code, int state_code, string? eta) GetParcel(string code) => (code, 2, "2026-03-10");
}

// Adapter: رابط ما را پیاده می‌کند و به کتابخانهٔ قدیمی ترجمه می‌کند
class LegacyCourierAdapter(LegacyCourierApi legacy) : ICourierClient
{
    public Shipment Track(string code)
    {
        var (trackingCode, stateCode, eta) = legacy.GetParcel(code);
        var status = stateCode switch
        {
            1 => ShipmentStatus.Pending,
            2 => ShipmentStatus.Shipped,
            3 => ShipmentStatus.Delivered,
            _ => ShipmentStatus.Unknown, // وضعیت جدید پیک برنامه را نمی‌شکند
        };
        return new Shipment(trackingCode, status, eta is null ? null : DateOnly.Parse(eta));
    }
}
