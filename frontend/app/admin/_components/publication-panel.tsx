"use client";

import { useState } from "react";
import { MdVerifiedUser, MdUploadFile } from "react-icons/md";
import { Card, Button, Input } from "@/app/management/_components/ui";
import { useContentStore } from "../_stores/use-content-store";
import type { ContentManagement } from "../_hooks/use-content-management";

// แผงอนุมัติเกณฑ์ประเมิน เผยแพร่หลักสูตรตามหลัก Four-Eyes Principle และนำเข้าไฟล์ JSON
export default function PublicationPanel({
  kind,
  manager,
}: {
  kind: "curricula" | "question-banks";
  manager: ContentManagement;
}) {
  const curriculum = useContentStore((s) => s.curriculum);
  const bank = useContentStore((s) => s.bank);
  const busy = useContentStore((s) => s.busy);

  const [reference, setReference] = useState("");
  const selected = kind === "curricula" ? curriculum : bank;

  if (!selected || selected.value.lifecycle !== "draft") {
    return null;
  }

  const isCurriculum = kind === "curricula";

  return (
    <Card
      title="ตรวจสอบและเผยแพร่ (Publication & Four-Eyes)"
      icon={<MdVerifiedUser />}
      description="การอนุมัติและเผยแพร่ต้องทำโดยเจ้าหน้าที่ที่เป็นคนละคนกับผู้สร้าง/แก้ไขล่าสุด"
    >
      <div className="flex flex-col gap-4">
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-3.5 text-xs text-amber-900 leading-relaxed">
          <p className="font-semibold">ข้อกำหนดความปลอดภัย (Four-Eyes Principle):</p>
          <p>
            ผู้เขียนและผู้เผยแพร่ต้องเป็นคนละคน ระบบจะตรวจสอบสิทธิ์และลายเซ็นของ TAdmin กับประวัติการบันทึกก่อนอนุมัติและเผยแพร่จริง
          </p>
        </div>

        <div>
          <label className="text-xs font-semibold text-ink/80">
            เอกสารอ้างอิงการอนุมัติ (Approval Reference) <span className="text-red-500">*</span>
          </label>
          <Input
            value={reference}
            disabled={busy}
            placeholder="เช่น มติที่ประชุมทีมหลักสูตร ครั้งที่ 3/2026 หรือเลขที่หนังสืออนุมัติ"
            maxLength={1000}
            onChange={setReference}
          />
        </div>

        <div className="flex flex-wrap items-center gap-3 pt-1">
          {isCurriculum && (
            <Button
              content="อนุมัติเกณฑ์ประเมิน"
              variant="secondary-outline"
              disabled={busy || !reference.trim()}
              onClick={() => manager.approveCriteria(reference)}
            />
          )}

          <Button
            content="เผยแพร่ฉบับนี้"
            disabled={busy || !reference.trim()}
            onClick={() => manager.publish(kind, reference)}
          />
        </div>

        {isCurriculum && (
          <div className="mt-2 border-t border-navy/10 pt-4">
            <label
              htmlFor="school-bot-import"
              className="inline-flex items-center gap-2 rounded-xl border border-navy/20 bg-paper-dim/40 px-4 py-2.5 text-xs font-semibold text-navy hover:bg-paper-dim transition cursor-pointer"
            >
              <MdUploadFile className="text-base text-orange" />
              <span>นำเข้าไฟล์บทเรียนจากชีต (.json)</span>
              <input
                id="school-bot-import"
                aria-label="นำเข้าไฟล์บทเรียนจากชีต"
                type="file"
                accept="application/json,.json"
                disabled={busy}
                className="hidden"
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file) void manager.importFile(file);
                  e.target.value = "";
                }}
              />
            </label>
          </div>
        )}
      </div>
    </Card>
  );
}
