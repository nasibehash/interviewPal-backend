import { useEffect, useState } from 'react';

interface ApiUser {
  user_id: number;
  full_name: string;
  avatar_url: string | null;
  created: string;
}

// مدلی که کامپوننت‌ها می‌خواهند: نام‌گذاری camelCase و تاریخ واقعی
interface User {
  id: number;
  name: string;
  avatar: string;
  joinedAt: Date;
}

const DEFAULT_AVATAR = '/avatars/default.png';

// Adapter: داده‌های API را به مدل UI تبدیل می‌کند و مقدارهای خالی را نرمال می‌کند
export function toUser(raw: ApiUser): User {
  return {
    id: raw.user_id,
    name: raw.full_name,
    avatar: raw.avatar_url ?? DEFAULT_AVATAR,
    joinedAt: new Date(raw.created),
  };
}

export function useUser(id: number): User | null {
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    fetch(`/api/users/${id}`, { signal: controller.signal })
      .then((response) => response.json() as Promise<ApiUser>)
      .then((raw) => setUser(toUser(raw)))
      .catch(() => setUser(null));
    return () => controller.abort();
  }, [id]);

  return user;
}

export function UserCard({ id }: { id: number }) {
  const user = useUser(id);
  if (!user) return <p>در حال بارگذاری…</p>;
  return (
    <figure>
      <img src={user.avatar} alt="" width={48} height={48} />
      <figcaption>{user.name} — عضو از {user.joinedAt.toLocaleDateString('fa-IR')}</figcaption>
    </figure>
  );
}
