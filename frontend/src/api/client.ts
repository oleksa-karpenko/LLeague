const API_BASE = (import.meta.env.VITE_API_BASE as string) ?? 'http://localhost:8080';

export class ApiError extends Error {
  status: number;

  // NOTE: declare the field + assign it in the body. Do NOT use a parameter
  // property (`constructor(public status: number)`) — the newest Vite template
  // enables `erasableSyntaxOnly`, which bans non-erasable TS syntax.
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

export function getToken(): string | null {
  return localStorage.getItem('token');
}
export function setToken(token: string) {
  localStorage.setItem('token', token);
}
export function clearToken() {
  localStorage.removeItem('token');
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body && !headers.has('Content-Type'))
    headers.set('Content-Type', 'application/json');

  const token = getToken();
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });

  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = await res.json();
      if (body?.error) message = body.error;   // our ErrorResponse shape
    } catch {
      /* non-JSON error body */
    }
    throw new ApiError(res.status, message);
  }

  if (res.status === 204) return undefined as T;  // No Content
  return (await res.json()) as T;
}