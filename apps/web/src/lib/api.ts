interface ApiResponse<T> {
  success: boolean;
  data: T;
  errors: { code: string; message: string; field?: string }[];
  correlationId: string;
}

class ApiClient {
  private baseUrl = '/api/v1';

  private getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  private async request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const token = this.getToken();
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string> ?? {}),
    };

    if (token) headers['Authorization'] = `Bearer ${token}`;

    const res = await fetch(`${this.baseUrl}${path}`, { ...options, headers });

    if (!res.ok) {
      let msg = `Request failed (${res.status})`;
      try {
        const json: ApiResponse<T> = await res.json();
        if (!json.success) {
          msg = json.errors?.[0]?.message ?? msg;
        }
      } catch {
        const text = await res.text().catch(() => '');
        msg = text || msg;
      }
      throw new Error(msg);
    }

    try {
      const json: ApiResponse<T> = await res.json();
      if (!json.success) {
        const msg = json.errors?.[0]?.message ?? 'Request failed';
        throw new Error(msg);
      }
      return json.data;
    } catch (err) {
      if (err instanceof Error && err.message !== 'Unexpected end of JSON input') throw err;
      throw new Error('Invalid response from server');
    }
  }

  async get<T>(path: string): Promise<T> {
    return this.request<T>(path);
  }

  async post<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>(path, {
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  async put<T>(path: string, body: unknown): Promise<T> {
    return this.request<T>(path, {
      method: 'PUT',
      body: JSON.stringify(body),
    });
  }

  async delete(path: string): Promise<void> {
    await this.request<null>(path, { method: 'DELETE' });
  }

  async upload<T>(path: string, formData: FormData): Promise<T> {
    const token = this.getToken();
    const headers: Record<string, string> = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const res = await fetch(`${this.baseUrl}${path}`, {
      method: 'POST',
      headers,
      body: formData,
    });

    if (!res.ok) {
      let msg = `Upload failed (${res.status})`;
      try {
        const json = await res.json();
        if (json.errors?.[0]?.message) msg = json.errors[0].message;
      } catch {}
      throw new Error(msg);
    }

    const json: ApiResponse<T> = await res.json();
    if (!json.success) throw new Error(json.errors?.[0]?.message ?? 'Upload failed');
    return json.data;
  }
}

export const api = new ApiClient();

export function setTokens(access: string, refresh: string) {
  localStorage.setItem('accessToken', access);
  localStorage.setItem('refreshToken', refresh);
}

export function clearTokens() {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
}

export function getRefreshToken(): string | null {
  return localStorage.getItem('refreshToken');
}
