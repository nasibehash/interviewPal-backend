interface HttpRequestConfig {
  readonly url: string;
  readonly method: 'GET' | 'POST';
  readonly headers: Readonly<Record<string, string>>;
  readonly query: Readonly<Record<string, string>>;
  readonly timeoutMs: number;
}

// سازنده تغییرناپذیر: هر متد یک نمونهٔ جدید برمی‌گرداند، پس سازندهٔ پایه امن به اشتراک گذاشته می‌شود
class RequestBuilder {
  private constructor(private readonly config: HttpRequestConfig) {}

  static to(url: string): RequestBuilder {
    return new RequestBuilder({ url, method: 'GET', headers: {}, query: {}, timeoutMs: 10_000 });
  }

  post(): RequestBuilder {
    return new RequestBuilder({ ...this.config, method: 'POST' });
  }

  header(name: string, value: string): RequestBuilder {
    return new RequestBuilder({ ...this.config, headers: { ...this.config.headers, [name]: value } });
  }

  param(name: string, value: string | number): RequestBuilder {
    return new RequestBuilder({ ...this.config, query: { ...this.config.query, [name]: String(value) } });
  }

  timeout(ms: number): RequestBuilder {
    if (ms <= 0) throw new RangeError('timeout باید مثبت باشد');
    return new RequestBuilder({ ...this.config, timeoutMs: ms });
  }

  build(): HttpRequestConfig {
    return this.config;
  }
}

const base = RequestBuilder.to('/api/products').header('Accept', 'application/json').timeout(5_000);
const search = base.param('q', 'گوشی').param('page', 2).build();
const other = base.post().build();

console.log(search.query, search.method); // { q: 'گوشی', page: '2' } GET
console.log(other.method, other.query); // POST {} ← سازندهٔ پایه دست‌نخورده ماند
