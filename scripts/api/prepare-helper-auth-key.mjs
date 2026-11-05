import { createPrivateKey, createPublicKey, generateKeyPairSync, randomUUID } from "node:crypto";
import { closeSync, existsSync, fsyncSync, lstatSync, openSync, readFileSync, renameSync, unlinkSync, writeFileSync } from "node:fs";
import { isAbsolute, join } from "node:path";

// Called inside the launcher's owner-only directory and exclusive setup lock; key material never goes to stdout.
const directory = process.argv[2];
if (!directory || !isAbsolute(directory) || lstatSync(directory).isSymbolicLink()) throw new Error("An absolute protected signing directory is required.");
const privatePath = join(directory, "helper-private.pem");
const publicPath = join(directory, "helper-public.pem");
for (const path of [privatePath, publicPath]) if (existsSync(path) && lstatSync(path).isSymbolicLink()) throw new Error("Signing files must not be links.");
if (!existsSync(privatePath)) {
  const { privateKey } = generateKeyPairSync("ec", { namedCurve: "prime256v1" });
  const file = openSync(privatePath, "wx", 0o600);
  try { writeFileSync(file, privateKey.export({ type: "pkcs8", format: "pem" })); fsyncSync(file); }
  finally { closeSync(file); }
}
if (lstatSync(privatePath).size > 16384) throw new Error("Existing signing key is invalid.");
const privateKey = createPrivateKey(readFileSync(privatePath));
if (privateKey.asymmetricKeyType !== "ec" || privateKey.asymmetricKeyDetails?.namedCurve !== "prime256v1") throw new Error("Existing signing key must be P-256.");
const temporaryPath = join(directory, `public-${randomUUID()}.tmp`);
try {
  writeFileSync(temporaryPath, createPublicKey(privateKey).export({ type: "spki", format: "pem" }), { flag: "wx", mode: 0o600 });
  renameSync(temporaryPath, publicPath);
} finally { if (existsSync(temporaryPath)) unlinkSync(temporaryPath); }
