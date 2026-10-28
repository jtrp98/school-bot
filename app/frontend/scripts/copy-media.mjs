import { cp, mkdir } from "node:fs/promises";
await mkdir("public/blockly-media", { recursive: true });
await cp("node_modules/blockly/media", "public/blockly-media", {
  recursive: true,
});
