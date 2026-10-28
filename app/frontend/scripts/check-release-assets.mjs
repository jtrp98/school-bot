import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../dist/', import.meta.url));
const sourceExtensions = new Set(['.map', '.ts', '.tsx', '.jsx', '.go', '.cs', '.cpp', '.c', '.h', '.hpp', '.pdb']);
let count = 0;
async function check(directory) {
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    const filename = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      if (['.git', 'node_modules', 'src'].includes(entry.name)) throw new Error(`Development directory in release assets: ${path.relative(root, filename)}`);
      await check(filename);
      continue;
    }
    if (!entry.isFile() || sourceExtensions.has(path.extname(entry.name).toLowerCase())) {
      throw new Error(`Source, symbols or source map in release assets: ${path.relative(root, filename)}`);
    }
    if (['.js', '.css', '.html'].includes(path.extname(entry.name))) {
      const text = await readFile(filename, 'utf8');
      if (/^\s*\/\/[#@]\s*sourceMappingURL\s*=/m.test(text) || /\/\*[#@]\s*sourceMappingURL\s*=/.test(text)) {
        throw new Error(`Source map reference in release assets: ${path.relative(root, filename)}`);
      }
    }
    count++;
  }
}
await check(root);
console.log(`Release assets checked: ${count} files; no original sources, symbols or source maps.`);
