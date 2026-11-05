import { createPrivateKey, randomUUID, sign } from "node:crypto";

// ลงนามคำยืนยันสิทธิ์เจ้าหน้าที่ (Staff Assertion JWT) อายุ 60 วินาทีตามสัญญากับ API Gateway
export function createStaffAssertion(
  helperUserId: unknown,
  adminId2: unknown,
  privatePem: string,
  now = Math.floor(Date.now() / 1000),
): string {
  if (
    !/^\d+$/.test(String(helperUserId)) ||
    Number(helperUserId) <= 0 ||
    !/^\d+$/.test(String(adminId2)) ||
    Number(adminId2) <= 0
  ) {
    throw new Error("Staff identity unavailable");
  }

  const key = createPrivateKey(privatePem);
  if (key.asymmetricKeyType !== "ec" || key.asymmetricKeyDetails?.namedCurve !== "prime256v1") {
    throw new Error("Staff signing key unavailable");
  }

  const encode = (value: unknown) => Buffer.from(JSON.stringify(value)).toString("base64url");
  const signingInput = `${encode({ alg: "ES256", typ: "JWT" })}.${encode({
    iss: "sb-web-helper",
    aud: "school-bot-admin",
    sub: String(helperUserId),
    admin_id2: Number(adminId2),
    scope: "schoolbot:content",
    iat: now,
    nbf: now,
    exp: now + 60,
    jti: randomUUID(),
  })}`;

  return `${signingInput}.${sign("sha256", Buffer.from(signingInput), {
    key,
    dsaEncoding: "ieee-p1363",
  }).toString("base64url")}`;
}
