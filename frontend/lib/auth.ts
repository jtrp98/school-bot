export const AUTH_COOKIE = "sb_auth";

const encoder = new TextEncoder();

function getCredentials() {
  return {
    username: process.env.SITE_USERNAME || "Teacher",
    password: process.env.SITE_PASSWORD || "SB2026",
    secret: process.env.SESSION_SECRET || "school-bot-robo-x-2026",
  };
}

export async function getExpectedToken() {
  const { username, password, secret } = getCredentials();
  const digest = await crypto.subtle.digest(
    "SHA-256",
    encoder.encode(`${username}:${password}:${secret}`),
  );
  return Array.from(new Uint8Array(digest))
    .map((byte) => byte.toString(16).padStart(2, "0"))
    .join("");
}

export function checkCredentials(username: string, password: string) {
  const valid = getCredentials();
  return username === valid.username && password === valid.password;
}
