"use client";

import { MdMenuBook, MdAssignment, MdRefresh, MdAdd } from "react-icons/md";
import { Card, Button, DataTable } from "@/app/management/_components/ui";
import { useContentStore } from "../_stores/use-content-store";
import { contentTitle, type Summary } from "../_types/content";
import type { ContentManagement } from "../_hooks/use-content-management";

// รายการหลักสูตรหรือคลังข้อสอบที่อ่านจากเซิร์ฟเวอร์ พร้อมปุ่มเปิดร่างและโหลดหน้าถัดไป
export default function ContentLibrary({
  kind,
  manager,
}: {
  kind: "curricula" | "question-banks";
  manager: ContentManagement;
}) {
  const curricula = useContentStore((s) => s.curricula);
  const banks = useContentStore((s) => s.banks);
  const busy = useContentStore((s) => s.busy);
  const page = kind === "curricula" ? curricula : banks;

  const isCurricula = kind === "curricula";
  const title = isCurricula ? "หลักสูตรกลางและบทเรียน" : "คลังข้อสอบกลาง";
  const icon = isCurricula ? <MdMenuBook /> : <MdAssignment />;

  return (
    <Card
      title={title}
      icon={icon}
      description={`จัดการรายการและฉบับร่างของ${title}ในระบบ`}
    >
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between gap-3">
          <Button
            content="โหลดข้อมูลล่าสุด"
            variant="secondary-outline"
            icon={<MdRefresh className="text-base" />}
            disabled={busy}
            onClick={() => void manager.load()}
          />
          <Button
            content="สร้างฉบับร่าง"
            icon={<MdAdd className="text-base" />}
            disabled={busy}
            onClick={isCurricula ? manager.newCurriculum : manager.newBank}
          />
        </div>

        <DataTable<Summary>
          rows={page.items}
          rowKey={(item) => item.id}
          loading={busy}
          emptyText={`ยังไม่มีรายการ${title}`}
          columns={[
            {
              id: "index",
              header: "ลำดับ",
              cell: (_item) => page.items.indexOf(_item) + 1,
            },
            {
              id: "title",
              header: "ชื่อรายการ",
              cell: (row) => (
                <button
                  type="button"
                  onClick={() =>
                    void (isCurricula ? manager.openCurriculum(row.id) : manager.openBank(row.id))
                  }
                  className="font-medium text-navy hover:text-orange hover:underline text-left transition"
                >
                  {contentTitle(row.title)}
                </button>
              ),
            },
            {
              id: "revision",
              header: "ฉบับที่",
              cell: (row) => `rev. ${row.revision}`,
            },
            {
              id: "lifecycle",
              header: "สถานะ",
              cell: (row) => (
                <span
                  className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold ${
                    row.lifecycle === "published"
                      ? "bg-emerald-100 text-emerald-800"
                      : row.lifecycle === "draft"
                      ? "bg-amber-100 text-amber-800"
                      : "bg-gray-100 text-gray-700"
                  }`}
                >
                  {row.lifecycle === "published"
                    ? "เผยแพร่แล้ว"
                    : row.lifecycle === "draft"
                    ? "ฉบับร่าง"
                    : "เก็บถาวร"}
                </span>
              ),
            },
          ]}
        />

        {page.next_cursor && (
          <div className="pt-2 text-center">
            <Button
              content="โหลดรายการเพิ่ม"
              variant="secondary-outline"
              disabled={busy}
              onClick={() => void manager.more(kind)}
            />
          </div>
        )}
      </div>
    </Card>
  );
}
