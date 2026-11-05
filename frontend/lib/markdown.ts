// lib/markdown.ts
import fs from "fs";
import path from "path";
import { marked } from "marked";

const CONTENT_ROOT = path.join(process.cwd(), "content");

/**
 * Reads a markdown file under /content and returns rendered HTML.
 * contentPath example: "foundation/robotics/01-robotics-101.md"
 */
export function renderMarkdownFile(contentPath: string): string {
  const fullPath = path.join(CONTENT_ROOT, contentPath);
  const raw = fs.readFileSync(fullPath, "utf-8");
  return marked.parse(raw, { async: false }) as string;
}

/**
 * Returns the first N characters of plain text (no markdown markup) —
 * used for short excerpts in list views.
 */
export function excerptFromMarkdownFile(
  contentPath: string,
  maxLen = 140
): string {
  const fullPath = path.join(CONTENT_ROOT, contentPath);
  const raw = fs.readFileSync(fullPath, "utf-8");
  const plain = raw
    .replace(/\*\*/g, "")
    .replace(/^#+\s*/gm, "")
    .replace(/\n+/g, " ")
    .trim();
  return plain.length > maxLen ? plain.slice(0, maxLen) + "…" : plain;
}
