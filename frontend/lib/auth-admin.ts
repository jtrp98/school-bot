export const ADMIN_AUTH_COOKIE = "sb_admin_auth";

export interface AdminSession {
  admin_id2: number;
  sub: number;
  username: string;
  name: string;
}

const encoder = new TextEncoder();

function getAdminConfig() {
  return {
    username: process.env.ADMIN_USERNAME || "Admin",
    password: process.env.ADMIN_PASSWORD || "SBAdmin2026",
    admin_id2: Number(process.env.SCHOOL_BOT_STAFF_ADMIN_ID2 || 1),
    sub: Number(process.env.SCHOOL_BOT_STAFF_USER_ID || 1),
    secret: process.env.SESSION_SECRET || "school-bot-robo-x-2026",
  };
}

async function signString(payload: string, secret: string): Promise<string> {
  const key = await crypto.subtle.importKey(
    "raw",
    encoder.encode(secret),
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign"],
  );
  const signature = await crypto.subtle.sign("HMAC", key, encoder.encode(payload));
  return Array.from(new Uint8Array(signature))
    .map((b) => b.toString(16).padStart(2, "0"))
    .join("");
}

export async function createAdminToken(session: AdminSession): Promise<string> {
  const { secret } = getAdminConfig();
  const payload = Buffer.from(JSON.stringify(session)).toString("base64url");
  const signature = await signString(payload, secret);
  return `${payload}.${signature}`;
}

export async function verifyAdminToken(token?: string | null): Promise<AdminSession | null> {
  if (!token || !token.includes(".")) {
    return null;
  }
  const [payload, signature] = token.split(".");
  const { secret } = getAdminConfig();
  const expectedSignature = await signString(payload, secret);
  if (signature !== expectedSignature) {
    return null;
  }
  try {
    const raw = Buffer.from(payload, "base64url").toString("utf8");
    const parsed = JSON.parse(raw) as AdminSession;
    if (parsed && typeof parsed.admin_id2 === "number" && parsed.admin_id2 > 0) {
      return parsed;
    }
  } catch {
    return null;
  }
  return null;
}

export function verifyAdminCredentials(username: string, password: string): AdminSession | null {
  const config = getAdminConfig();
  if (username === config.username && password === config.password) {
    return {
      admin_id2: config.admin_id2,
      sub: config.sub,
      username: config.username,
      name: "ผู้ดูแลระบบกลาง (Staff Admin)",
    };
  }
  return null;
}
