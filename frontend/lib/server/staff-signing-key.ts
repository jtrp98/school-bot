import { open } from "node:fs/promises";
import { existsSync } from "node:fs";
import { resolve } from "node:path";

// โหลดกุญแจลับสำหรับลงนามคำขอยืนยันสิทธิ์เจ้าหน้าที่ โดยรองรับทั้ง env variable, mount file และ dev path
export async function loadStaffSigningKey(): Promise<string> {
  const inline = process.env.SCHOOL_BOT_STAFF_SIGNING_KEY_PEM;
  if (inline?.trim()) return inline;

  let path = process.env.SCHOOL_BOT_STAFF_SIGNING_KEY_FILE;
  if (!path) {
    const candidates = [
      resolve(process.cwd(), "../.tmp/helper-auth/helper-private.pem"),
      resolve(process.cwd(), ".tmp/helper-auth/helper-private.pem"),
    ];
    path = candidates.find((p) => existsSync(p));
  }

  if (!path) throw new Error("Staff signing key unavailable");

  const file = await open(path, "r");
  try {
    const size = (await file.stat()).size;
    if (size <= 0 || size > 16384) throw new Error("Staff signing key unavailable");
    const content = Buffer.alloc(16385);
    let sizeRead = 0;
    while (sizeRead < content.length) {
      const { bytesRead } = await file.read(content, sizeRead, content.length - sizeRead, sizeRead);
      if (bytesRead === 0) break;
      sizeRead += bytesRead;
    }
    if (sizeRead === 0 || sizeRead > 16384) throw new Error("Staff signing key unavailable");
    return content.subarray(0, sizeRead).toString("utf8");
  } finally {
    await file.close();
  }
}
