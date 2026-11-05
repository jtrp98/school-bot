"use client";

import { useEffect, useState } from "react";
import { MdEditNote } from "react-icons/md";
import { Card, Button, Input, NativeSelect } from "@/app/management/_components/ui";
import { useContentStore } from "../_stores/use-content-store";
import { contentTitle } from "../_types/content";
import type { ContentManagement } from "../_hooks/use-content-management";

// แก้ไขข้อมูลหลักของร่าง ส่วนฉบับเผยแพร่แล้วต้องเริ่มฉบับแก้ไขใหม่
export default function MetadataForm({ manager }: { manager: ContentManagement }) {
  const selected = useContentStore((s) => s.curriculum);
  const busy = useContentStore((s) => s.busy);
  const levels = useContentStore((s) => s.levels);
  const current = selected?.value;

  const [title, setTitle] = useState("");
  const [levelId, setLevelId] = useState<number>(0);
  const [sourceRef, setSourceRef] = useState("");
  const [grades, setGrades] = useState("");

  useEffect(() => {
    if (current) {
      setTitle(contentTitle(current.title));
      setLevelId(current.level_id ?? (levels[0]?.id || 0));
      setSourceRef(current.source_ref ?? "");
      setGrades(current.recommended_grades?.join(", ") ?? "");
    } else {
      setTitle("");
      setLevelId(levels[0]?.id || 0);
      setSourceRef("");
      setGrades("");
    }
  }, [current, levels]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    manager.saveMetadata({
      title,
      level_id: Number(levelId),
      source_ref: sourceRef,
      grades,
    });
  };

  const isPublished = current?.lifecycle === "published";

  return (
    <Card
      title={current ? `ข้อมูลหลักสูตรฉบับที่ ${current.revision}` : "สร้างหลักสูตรฉบับร่าง"}
      icon={<MdEditNote />}
      description={isPublished ? "หลักสูตรนี้เผยแพร่แล้ว (อ่านอย่างเดียว)" : "กรอกข้อมูลทั่วไปและระดับชั้นของหลักสูตร"}
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="grid gap-4 md:grid-cols-2">
          <label className="block text-xs font-semibold text-ink/80">
            ชื่อหลักสูตร <span className="text-red-500">*</span>
            <div className="mt-1">
              <Input
                value={title}
                onChange={setTitle}
                disabled={busy || isPublished}
                placeholder="เช่น การเขียนโปรแกรมหุ่นยนต์ RoboX ขั้นพื้นฐาน"
                maxLength={200}
                aria-label="ชื่อหลักสูตร"
              />
            </div>
          </label>

          <label className="block text-xs font-semibold text-ink/80">
            ระดับหลักสูตร <span className="text-red-500">*</span>
            <div className="mt-1">
              <NativeSelect
                value={levelId}
                onChange={(e) => setLevelId(Number(e.target.value))}
                disabled={busy || isPublished}
                aria-label="ระดับหลักสูตร"
              >
                <option value={0}>-- เลือกระดับ --</option>
                {levels.map((lvl) => (
                  <option key={lvl.id} value={lvl.id}>
                    {lvl.title}
                  </option>
                ))}
              </NativeSelect>
            </div>
          </label>

          <label className="block text-xs font-semibold text-ink/80">
            แหล่งอ้างอิงเนื้อหา <span className="text-red-500">*</span>
            <div className="mt-1">
              <Input
                value={sourceRef}
                onChange={setSourceRef}
                disabled={busy || isPublished}
                placeholder="เช่น sheet:robotic-2026-v1"
                maxLength={2048}
                aria-label="แหล่งอ้างอิงเนื้อหา"
              />
            </div>
          </label>

          <label className="block text-xs font-semibold text-ink/80">
            ชั้นที่แนะนำ (คั่นด้วยจุลภาค)
            <div className="mt-1">
              <Input
                value={grades}
                onChange={setGrades}
                disabled={busy || isPublished}
                placeholder="เช่น P1, P2, P3"
                maxLength={500}
                aria-label="ชั้นที่แนะนำ"
              />
            </div>
          </label>
        </div>

        <div className="pt-2">
          {isPublished ? (
            <Button
              content="สร้างฉบับแก้ไขใหม่"
              variant="secondary-outline"
              disabled={busy}
              onClick={manager.createRevision}
            />
          ) : (
            <button
              type="submit"
              disabled={busy}
              className="inline-flex items-center justify-center gap-2 rounded-xl bg-orange px-4 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-orange/90 disabled:opacity-50 transition"
            >
              บันทึกหลักสูตรฉบับร่าง
            </button>
          )}
        </div>
      </form>
    </Card>
  );
}
