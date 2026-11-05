"use client";

import { useEffect, useState } from "react";
import { MdAssignment, MdEdit, MdDelete, MdAdd } from "react-icons/md";
import { Card, Button, Input, DataTable } from "@/app/management/_components/ui";
import { useContentStore } from "../_stores/use-content-store";
import { contentTitle, type Question } from "../_types/content";
import type { ContentManagement } from "../_hooks/use-content-management";
import QuestionForm from "./question-form";

// แก้ไขคลังข้อสอบฉบับร่าง และจัดการรายการคำถาม
export default function BankEditor({ manager }: { manager: ContentManagement }) {
  const selected = useContentStore((s) => s.bank);
  const busy = useContentStore((s) => s.busy);
  const bank = selected?.value;

  const [title, setTitle] = useState("");
  const [sourceRef, setSourceRef] = useState("");
  const [questions, setQuestions] = useState<Question[]>([]);
  const [editing, setEditing] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (bank) {
      setTitle(contentTitle(bank.title));
      setSourceRef(bank.source_ref ?? "");
      setQuestions(bank.questions ?? []);
    } else {
      setTitle("");
      setSourceRef("");
      setQuestions([]);
    }
    setEditing(null);
    setError(null);
  }, [bank]);

  const readonly = bank?.lifecycle === "published";

  const handleSaveBank = () => {
    if (!title.trim()) {
      setError("กรุณาระบุชื่อคลังข้อสอบ");
      return;
    }
    if (!sourceRef.trim()) {
      setError("กรุณาระบุแหล่งอ้างอิง");
      return;
    }
    if (questions.length === 0) {
      setError("กรุณาเพิ่มคำถามอย่างน้อย 1 ข้อ");
      return;
    }
    setError(null);

    manager.saveBank({
      title: { ...bank?.title, th: title },
      source_ref: sourceRef,
      questions,
    });
  };

  return (
    <Card
      title={bank ? `คลังข้อสอบฉบับที่ ${bank.revision}` : "สร้างคลังข้อสอบฉบับร่าง"}
      icon={<MdAssignment />}
      description={
        readonly
          ? "คลังข้อสอบนี้เผยแพร่แล้ว (การแก้ไขจะบันทึกเป็นฉบับใหม่)"
          : "กำหนดชื่อ แหล่งอ้างอิง และเพิ่มคำถามในคลังข้อสอบ"
      }
    >
      <div className="flex flex-col gap-5">
        {error && (
          <div role="alert" className="rounded-xl border border-red-200 bg-red-50 p-3 text-xs text-red-700">
            {error}
          </div>
        )}

        <div className="grid gap-4 md:grid-cols-2">
          <div>
            <label className="text-xs font-semibold text-ink/80">
              ชื่อคลังข้อสอบ <span className="text-red-500">*</span>
            </label>
            <Input
              value={title}
              disabled={busy}
              placeholder="เช่น คลังข้อสอบวัดทักษะ RoboX ป.1"
              onChange={setTitle}
            />
          </div>

          <div>
            <label className="text-xs font-semibold text-ink/80">
              แหล่งอ้างอิง <span className="text-red-500">*</span>
            </label>
            <Input
              value={sourceRef}
              disabled={busy}
              placeholder="เช่น bank:curriculum-2026-p1"
              onChange={setSourceRef}
            />
          </div>
        </div>

        {/* Questions Table */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h4 className="font-display text-xs font-bold text-navy">
              รายการคำถาม ({questions.length} ข้อ)
            </h4>
            <Button
              content="เพิ่มคำถาม"
              variant="secondary-outline"
              icon={<MdAdd />}
              disabled={busy || editing !== null}
              onClick={() => setEditing(-1)}
            />
          </div>

          <DataTable<Question>
            rows={questions}
            rowKey={(q) => q.question_key}
            loading={busy}
            emptyText="ยังไม่มีคำถามในคลังข้อสอบนี้"
            columns={[
              {
                id: "index",
                header: "ข้อที่",
                cell: (_q) => questions.indexOf(_q) + 1,
              },
              {
                id: "prompt",
                header: "โจทย์คำถาม",
                cell: (q) => contentTitle(q.prompt),
              },
              {
                id: "kind",
                header: "ประเภท",
                cell: (q) => (q.kind === "boolean" ? "ถูก/ผิด" : "เลือกตอบ"),
              },
              {
                id: "max_score",
                header: "คะแนนเต็ม",
                cell: (q) => q.max_score,
              },
              {
                id: "actions",
                header: "จัดการ",
                cell: (_q) => {
                  const idx = questions.indexOf(_q);
                  return (
                    <div className="flex items-center gap-2">
                      <button
                        type="button"
                        aria-label="แก้ไขคำถาม"
                        onClick={() => setEditing(idx)}
                        disabled={busy}
                        className="rounded-lg p-1.5 text-ink/60 hover:bg-sky/20 hover:text-navy transition"
                      >
                        <MdEdit className="text-base" />
                      </button>
                      <button
                        type="button"
                        aria-label="ลบคำถาม"
                        onClick={() => setQuestions((prev) => prev.filter((_, i) => i !== idx))}
                        disabled={busy}
                        className="rounded-lg p-1.5 text-ink/60 hover:bg-red-50 hover:text-red-600 transition"
                      >
                        <MdDelete className="text-base" />
                      </button>
                    </div>
                  );
                },
              },
            ]}
          />
        </div>

        {/* Question editor subform */}
        {editing !== null && (
          <QuestionForm
            key={editing}
            question={editing >= 0 ? questions[editing] : undefined}
            onCancel={() => setEditing(null)}
            onSave={(q) => {
              setQuestions((prev) =>
                editing < 0 ? [...prev, q] : prev.map((item, idx) => (idx === editing ? q : item)),
              );
              setEditing(null);
            }}
          />
        )}

        <div className="pt-2">
          <Button
            content={bank?.lifecycle === "published" ? "บันทึกเป็นฉบับใหม่" : "บันทึกคลังข้อสอบฉบับร่าง"}
            disabled={busy || editing !== null}
            onClick={handleSaveBank}
          />
        </div>
      </div>
    </Card>
  );
}
