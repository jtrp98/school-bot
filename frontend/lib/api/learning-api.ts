import type { Versioned } from "@/app/management/_types/learning";

export class GatewayApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public messageTh: string,
    public requestId?: string,
    public details: unknown[] = [],
    public userFacing = true,
    public errorCode: string | null = null,
  ) {
    super(message);
    this.name = "GatewayApiError";
  }
}

const defaultBase = process.env.NEXT_PUBLIC_SCHOOL_BOT_API_URL ?? "/api/service-school-bot";

export async function learningRequest<T>(
  path: string,
  method = "GET",
  body?: unknown,
  etag?: string | null,
  key?: string,
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

  const url = `${defaultBase}${path}`;
  const response = await fetch(url, {
    method,
    cache: "no-store",
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  const data = await response.json().catch(() => null);
  if (!response.ok) {
    throw new GatewayApiError(
      data?.error?.message ?? data?.message ?? "SchoolBot request failed",
      response.status,
      data?.messageTh ?? "ไม่สามารถทำรายการได้ กรุณาโหลดข้อมูลล่าสุดแล้วลองอีกครั้ง",
      data?.error?.request_id ?? data?.requestId,
      [],
      true,
      data?.error?.code ?? data?.errorCode ?? null,
    );
  }

  return { data, etag: response.headers.get("ETag") };
}
