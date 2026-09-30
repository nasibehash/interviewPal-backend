import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface AppConfig {
  apiUrl: string;
  featureFlags: Record<string, boolean>;
}

// providedIn: 'root' یعنی یک نمونه برای کل برنامه؛ خود DI اصل Singleton را مدیریت می‌کند
@Injectable({ providedIn: 'root' })
export class ConfigService {
  private readonly http = inject(HttpClient);
  private readonly state = signal<AppConfig>({ apiUrl: '/api', featureFlags: {} });

  readonly config = this.state.asReadonly();

  async load(): Promise<void> {
    const loaded = await fetch('/assets/config.json').then((r) => r.json() as Promise<AppConfig>);
    this.state.set(loaded);
  }

  isEnabled(flag: string): boolean {
    return this.state().featureFlags[flag] ?? false;
  }
}

// هر جا inject(ConfigService) بزنی همان نمونه را می‌گیری.
// توجه: هر injector جدا (مثلاً providers یک lazy route) نمونهٔ جداگانه‌ای می‌سازد؛
// Singleton در Angular «یکی به ازای هر injector» است، نه یکی برای کل جهان.
