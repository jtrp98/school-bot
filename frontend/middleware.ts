import { NextRequest, NextResponse } from "next/server";
import { AUTH_COOKIE, getExpectedToken } from "@/lib/auth";
import { ADMIN_AUTH_COOKIE, verifyAdminToken } from "@/lib/auth-admin";

const PUBLIC_PATHS = ["/login", "/api/login", "/admin/login", "/api/admin/login"];

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  if (PUBLIC_PATHS.includes(pathname)) {
    return NextResponse.next();
  }

  // แยกการตรวจสอบสิทธิ์ระหว่าง Admin Console กับ Teacher Web
  if (pathname.startsWith("/admin")) {
    const adminToken = request.cookies.get(ADMIN_AUTH_COOKIE)?.value;
    const adminSession = await verifyAdminToken(adminToken);

    if (adminSession) {
      return NextResponse.next();
    }

    const adminLoginUrl = new URL("/admin/login", request.url);
    adminLoginUrl.searchParams.set("from", pathname);
    return NextResponse.redirect(adminLoginUrl);
  }

  const token = request.cookies.get(AUTH_COOKIE)?.value;
  const expected = await getExpectedToken();

  if (token === expected) {
    return NextResponse.next();
  }

  const loginUrl = new URL("/login", request.url);
  loginUrl.searchParams.set("from", pathname);
  return NextResponse.redirect(loginUrl);
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|brand/).*)"],
};
