"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  MdAdminPanelSettings,
  MdArrowBack,
  MdLogout,
  MdMenuBook,
  MdAssignment,
} from "react-icons/md";
import { useContentManagement } from "./_hooks/use-content-management";
import { useContentStore } from "./_stores/use-content-store";
import ContentLibrary from "./_components/content-library";
import MetadataForm from "./_components/metadata-form";
import LessonEditor from "./_components/lesson-editor";
import BankEditor from "./_components/bank-editor";
import PublicationPanel from "./_components/publication-panel";
import { StatusModal } from "./_components/status-modal";

// หน้าหลักระบบจัดการหลักสูตรและคลังข้อสอบส่วนกลางสำหรับผู้ดูแลระบบ (TAdmin)
export default function SchoolBotAdminPage() {
  const router = useRouter();
  const manager = useContentManagement();
  const curriculum = useContentStore((s) => s.curriculum);
  const bank = useContentStore((s) => s.bank);
  const busy = useContentStore((s) => s.busy);
  const error = useContentStore((s) => s.error);

  const [activeTab, setActiveTab] = useState<"curricula" | "banks">("curricula");

  const curriculumKey = `${curriculum?.value.id ?? "new"}:${curriculum?.etag ?? ""}`;
  const bankKey = `${bank?.value.id ?? "new"}:${bank?.etag ?? ""}`;

  const handleAdminLogout = async () => {
    try {
      await fetch("/api/admin/login", { method: "DELETE" });
      router.push("/admin/login");
      router.refresh();
    } catch {
      router.push("/admin/login");
    }
  };

  return (
    <div className="min-h-screen bg-paper pb-20 pt-6">
      <div className="mx-auto max-w-5xl px-4 sm:px-6 space-y-6">
        {/* Navigation Bar */}
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-navy/10 pb-4">
          <div className="flex items-center gap-3">
            <Link
              href="/"
              className="inline-flex items-center gap-1 text-xs font-semibold text-navy/70 hover:text-navy transition"
            >
              <MdArrowBack className="text-base" />
              <span>หน้าเว็บครู</span>
            </Link>
            <span className="text-navy/30">|</span>
            <div className="inline-flex items-center gap-1.5 rounded-full bg-navy/10 px-3 py-1 text-xs font-bold text-navy">
              <MdAdminPanelSettings className="text-sm text-orange" />
              <span>ผู้ดูแลระบบ (TAdmin)</span>
            </div>
          </div>

          <button
            type="button"
            onClick={handleAdminLogout}
            className="inline-flex items-center gap-1.5 rounded-xl border border-navy/15 bg-white px-3 py-1.5 text-xs font-medium text-navy shadow-sm hover:bg-paper-dim transition"
          >
            <MdLogout className="text-sm" />
            <span>ออกจากระบบ Admin</span>
          </button>
        </div>

        {/* Page Title */}
        <div className="space-y-1">
          <h1 className="font-display text-2xl font-extrabold text-navy">
            จัดการบทเรียนและคลังข้อสอบ
          </h1>
          <p className="font-body text-xs text-ink/70">
            ระบบจัดทำหลักสูตรกลาง เกณฑ์ประเมินทักษะหุ่นยนต์ RoboX และข้อสอบมาตรฐาน
          </p>
        </div>

        {/* Global Error Alert */}
        {error && (
          <div
            role="alert"
            className="rounded-xl border border-red-200 bg-red-50 p-4 text-xs font-medium text-red-700"
          >
            {error}
          </div>
        )}

        {/* Tabs Bar */}
        <div className="flex items-center gap-2 border-b border-navy/10">
          <button
            type="button"
            onClick={() => setActiveTab("curricula")}
            className={`inline-flex items-center gap-2 border-b-2 px-4 py-2.5 text-sm font-bold transition ${
              activeTab === "curricula"
                ? "border-orange text-orange"
                : "border-transparent text-ink/60 hover:text-navy"
            }`}
          >
            <MdMenuBook className="text-base" />
            <span>หลักสูตรและบทเรียน</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab("banks")}
            className={`inline-flex items-center gap-2 border-b-2 px-4 py-2.5 text-sm font-bold transition ${
              activeTab === "banks"
                ? "border-orange text-orange"
                : "border-transparent text-ink/60 hover:text-navy"
            }`}
          >
            <MdAssignment className="text-base" />
            <span>คลังข้อสอบ</span>
          </button>
        </div>

        {/* Tab Content */}
        {activeTab === "curricula" ? (
          <div className="flex flex-col gap-6">
            <ContentLibrary kind="curricula" manager={manager} />
            <MetadataForm key={curriculumKey} manager={manager} />
            {curriculum && (
              <>
                <LessonEditor key={`lessons-${curriculumKey}`} manager={manager} />
                <PublicationPanel
                  key={`pub-${curriculumKey}`}
                  kind="curricula"
                  manager={manager}
                />
              </>
            )}
          </div>
        ) : (
          <div className="flex flex-col gap-6">
            <ContentLibrary kind="question-banks" manager={manager} />
            <BankEditor key={bankKey} manager={manager} />
            <PublicationPanel
              key={`pub-${bankKey}`}
              kind="question-banks"
              manager={manager}
            />
          </div>
        )}

        {/* Action Confirmation & Status Modal */}
        <StatusModal
          open={manager.modal.open}
          type={manager.modal.type}
          title={
            manager.modal.type === "confirm"
              ? "ยืนยันการทำรายการ"
              : manager.modal.type === "success"
              ? "บันทึกสำเร็จ"
              : "ไม่สามารถทำรายการได้"
          }
          message={manager.modal.message}
          loading={busy}
          onClose={manager.closeModal}
          onConfirm={manager.modal.type === "confirm" ? manager.acceptModal : undefined}
        />
      </div>
    </div>
  );
}
