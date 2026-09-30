class Logger {
  private static instance: Logger | undefined;
  private readonly lines: string[] = [];

  // private constructor: هیچ‌کس بیرون از کلاس نمی‌تواند new Logger() بزند
  private constructor() {}

  static get(): Logger {
    return (Logger.instance ??= new Logger());
  }

  // فقط برای تست: هر تست با نمونهٔ تازه شروع می‌کند
  static resetForTests(): void {
    Logger.instance = undefined;
  }

  info(message: string): void {
    this.lines.push(`[info] ${message}`);
  }

  get history(): readonly string[] {
    return this.lines;
  }
}

Logger.get().info('برنامه شروع شد');
Logger.get().info('کاربر وارد شد');
console.log(Logger.get().history); // هر دو پیام در یک نمونه‌اند

// new Logger(); ← خطای کامپایل: constructor خصوصی است
