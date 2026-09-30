var email = new EmailBuilder()
    .From("shop@example.com")
    .To("ali@example.com")
    .Cc("support@example.com")
    .Subject("سفارش شما ثبت شد")
    .Body("سفارش شمارهٔ ۱۲۳ با موفقیت ثبت شد.")
    .Build();

Console.WriteLine($"{email.From} -> {string.Join(", ", email.To)} | {email.Subject}");

try { new EmailBuilder().To("ali@example.com").Build(); }
catch (InvalidOperationException e) { Console.WriteLine(e.Message); }

record Email(string From, IReadOnlyList<string> To, IReadOnlyList<string> Cc, string Subject, string Body);

class EmailBuilder
{
    private string? _from, _subject, _body;
    private readonly List<string> _to = [], _cc = [];

    public EmailBuilder From(string address) { _from = address; return this; }
    public EmailBuilder To(string address) { _to.Add(address); return this; }
    public EmailBuilder Cc(string address) { _cc.Add(address); return this; }
    public EmailBuilder Subject(string subject) { _subject = subject; return this; }
    public EmailBuilder Body(string body) { _body = body; return this; }

    // اعتبارسنجی یک‌جا در Build: هیچ‌وقت آبجکت ناقص ساخته نمی‌شود
    public Email Build()
    {
        if (_from is null) throw new InvalidOperationException("فرستنده (From) الزامی است");
        if (_to.Count == 0) throw new InvalidOperationException("حداقل یک گیرنده (To) لازم است");
        if (string.IsNullOrWhiteSpace(_subject)) throw new InvalidOperationException("موضوع (Subject) الزامی است");
        return new Email(_from, [.. _to], [.. _cc], _subject, _body ?? "");
    }
}
