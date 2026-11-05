import { NextRequest, NextResponse } from "next/server";
import { ADMIN_AUTH_COOKIE, verifyAdminToken } from "@/lib/auth-admin";
import { allowedContentRoute } from "@/lib/server/school-bot-policy";
import { createStaffAssertion } from "@/lib/server/staff-assertion";
import { loadStaffSigningKey } from "@/lib/server/staff-signing-key";

export const dynamic = "force-dynamic";
const maxBytes = 2 * 1024 * 1024;

function failure(status: number, message: string, code = "UNAVAILABLE", requestId?: string): NextResponse {
  return NextResponse.json(
    {
      status,
      message_th: message,
      message_en: "SchoolBot request unavailable",
      data: { code, request_id: requestId },
    },
    { status, headers: { "Cache-Control": "no-store" } },
  );
}

// ตัวกลางส่งต่อคำขอ (BFF Proxy) สำหรับ TAdmin โดยสร้างคำยืนยันสิทธิ์ลงนาม ES256 เพื่อคุยกับ Gateway
async function proxy(
  request: NextRequest,
  context: { params: { path: string[] } },
): Promise<NextResponse> {
  const adminToken = request.cookies.get(ADMIN_AUTH_COOKIE)?.value;
  const adminSession = await verifyAdminToken(adminToken);

  if (!adminSession) {
    return failure(401, "กรุณาเข้าสู่ระบบผู้ดูแลระบบ (TAdmin)", "UNAUTHORIZED");
  }

  const path = context.params.path.join("/");
  if (!allowedContentRoute(request.method, path)) {
    return failure(404, "ไม่พบรายการที่ขอ", "NOT_FOUND");
  }

  const origin = request.headers.get("Origin");
  if (request.method !== "GET" && origin && origin !== request.nextUrl.origin) {
    return failure(403, "แหล่งคำขอไม่ถูกต้อง", "FORBIDDEN");
  }

  const gateway =
    process.env.SCHOOL_BOT_GATEWAY_ORIGIN ||
    process.env.NEXT_PUBLIC_SCHOOL_BOT_API_URL ||
    "http://127.0.0.1:5180";

  let assertion: string;
  try {
    const signingKey = await loadStaffSigningKey();
    assertion = createStaffAssertion(adminSession.sub, adminSession.admin_id2, signingKey);
  } catch {
    return failure(503, "ไม่สามารถยืนยันบัญชีผู้ดูแลได้ กรุณาติดต่อผู้ดูแลระบบ", "AUTH_NOT_CONFIGURED");
  }

  let upstreamOrigin: string;
  try {
    const upstreamUrl = new URL(gateway);
    upstreamOrigin = upstreamUrl.origin;
  } catch {
    return failure(503, "การเชื่อมต่อระบบบทเรียนยังไม่พร้อม");
  }

  let body = "";
  if (request.method !== "GET") {
    const reader = request.body?.getReader();
    const chunks: Uint8Array[] = [];
    let size = 0;
    if (reader) {
      for (;;) {
        const next = await reader.read();
        if (next.done) break;
        size += next.value.length;
        if (size > maxBytes) {
          await reader.cancel();
          return failure(413, "ข้อมูลเกินขนาดที่กำหนด (ไม่เกิน 2MB)", "PAYLOAD_TOO_LARGE");
        }
        chunks.push(next.value);
      }
    }
    body = Buffer.concat(chunks).toString("utf8");
  }

  const headers: Record<string, string> = {
    "X-SchoolBot-Staff-Assertion": assertion,
    "Content-Type": "application/json",
  };
  for (const name of ["If-Match", "Idempotency-Key", "X-Request-Id"]) {
    const value = request.headers.get(name);
    if (value) headers[name] = value;
  }

  try {
    const targetUrl = `${upstreamOrigin}/api/service-school-bot/admin/${path}${request.nextUrl.search}`;
    const result = await fetch(targetUrl, {
      method: request.method,
      headers,
      body: body || undefined,
      cache: "no-store",
    });

    const resultData = await result.json().catch(() => null);

    if (!result.ok) {
      const error = resultData?.error;
      const messages: Record<string, string> = {
        FOUR_EYES_REQUIRED: "ผู้เขียนและผู้เผยแพร่ต้องเป็นคนละคน",
        VERSION_CONFLICT: "ข้อมูลเปลี่ยนแล้ว กรุณาโหลดใหม่ก่อนแก้ไข",
        CRITERIA_NOT_APPROVED: "เกณฑ์ประเมินยังไม่ผ่านการอนุมัติ",
        ALREADY_PUBLISHED: "หลักสูตรนี้ได้รับการเผยแพร่แล้ว",
      };
      return failure(
        result.status,
        messages[error?.code] ?? resultData?.message_th ?? "ไม่สามารถบันทึกข้อมูลได้ กรุณาตรวจข้อมูลและลองอีกครั้ง",
        error?.code,
        error?.request_id,
      );
    }

    return NextResponse.json(
      {
        status: result.status,
        message_th: "สำเร็จ",
        message_en: "Success",
        data: { value: resultData, etag: result.headers.get("etag") ?? null },
      },
      { status: result.status, headers: { "Cache-Control": "no-store" } },
    );
  } catch {
    return failure(503, "ไม่สามารถเชื่อมต่อระบบบทเรียนได้ กรุณาลองอีกครั้ง");
  }
}

export { proxy as GET, proxy as POST, proxy as PUT };
