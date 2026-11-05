import { NextRequest, NextResponse } from "next/server";
import {
  ADMIN_AUTH_COOKIE,
  createAdminToken,
  verifyAdminCredentials,
} from "@/lib/auth-admin";

export async function POST(request: NextRequest) {
  const body = await request.json().catch(() => null);
  const username = body?.username;
  const password = body?.password;

  if (typeof username !== "string" || typeof password !== "string") {
    return NextResponse.json({ error: "กรุณาระบุชื่อผู้ใช้และรหัสผ่าน" }, { status: 400 });
  }

  const session = verifyAdminCredentials(username, password);
  if (!session) {
    return NextResponse.json(
      { error: "ชื่อผู้ใช้หรือรหัสผ่านผู้ดูแลระบบไม่ถูกต้อง" },
      { status: 401 },
    );
  }

  const token = await createAdminToken(session);
  const response = NextResponse.json({ ok: true, session });
  response.cookies.set(ADMIN_AUTH_COOKIE, token, {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/",
    maxAge: 60 * 60 * 24 * 7,
  });
  return response;
}

export async function DELETE() {
  const response = NextResponse.json({ ok: true });
  response.cookies.delete(ADMIN_AUTH_COOKIE);
  return response;
}
