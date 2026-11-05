"use client";

import { DataTable } from "./ui";
import { t } from "../messages";

export default function ProjectEvidence({ content }: { content?: Record<string, unknown> }) {
  if (typeof content?.source === "string") {
    return (
      <pre className="max-h-80 overflow-auto whitespace-pre-wrap rounded-xl bg-paper-dim p-4 font-mono text-xs text-ink/80 border border-navy/10">
        {content.source}
      </pre>
    );
  }

  if (Array.isArray(content?.poses)) {
    const poses = content.poses
      .filter((p): p is Record<string, unknown> => !!p && typeof p === "object")
      .map((p, index) => ({
        index,
        name: typeof p.name === "string" ? p.name : String(index + 1),
        joints: Array.isArray(p.joints) ? p.joints.filter((v) => typeof v === "number").join(" / ") : "",
        time: typeof p.time_ms === "number" ? p.time_ms : "",
      }));

    return (
      <DataTable
        rows={poses}
        rowKey={(p) => p.index}
        columns={[
          { id: "pose", header: t("pose"), cell: (p) => p.name },
          { id: "joints", header: t("joints"), cell: (p) => p.joints },
          { id: "time", header: t("duration"), cell: (p) => p.time },
        ]}
      />
    );
  }

  return <p className="text-xs text-ink/50 italic">{t("workNeedsViewer")}</p>;
}
