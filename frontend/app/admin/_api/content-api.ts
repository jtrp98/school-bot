import type { Versioned } from "../_types/content";

export class AdminApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public messageTh: string,
    public code?: string,
  ) {
    super(message);
    this.name = "AdminApiError";
  }
}

// เรียกผ่าน Server BFF ของเว็บเพื่อตรวจสอบและลงนามคำยืนยันสิทธิ์ก่อนส่งต่อ Gateway
export async function contentRequest<T>(
  resource: string,
  method = "GET",
  body?: unknown,
  etag?: string | null,
  key?: string,
  expectedUser?: string,
): Promise<Versioned<T>> {
  const headers: Record<string, string> = {};
  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
  }
  if (etag) {
    headers["If-Match"] = etag;
  }
  if (key) {
    headers["Idempotency-Key"] = key;
  }
  if (expectedUser) {
    headers["X-Expected-Helper-User"] = expectedUser;
  }

  const response = await fetch(`/api/service-school-bot/admin/${resource}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
    cache: "no-store",
  });

  const json = await response.json().catch(() => null);

  if (!response.ok) {
    const message = json?.message_th || json?.error?.message || "ไม่สามารถทำรายการได้";
    throw new AdminApiError(
      json?.message_en || message,
      response.status,
      message,
      json?.data?.code || json?.error?.code,
    );
  }

  return json.data as Versioned<T>;
}
