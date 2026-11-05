"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { AdminApiError, contentRequest } from "../_api/content-api";
import { lessonInputs } from "../_utils/content-payload";
import {
  parseAndValidateImport,
  validateLessons,
  validateMetadata,
  type MetadataValues,
} from "../_utils/content-validation";
import type {
  Activity,
  Curriculum,
  Lesson,
  ListPage,
  QuestionBank,
  Versioned,
} from "../_types/content";
import { useContentStore } from "../_stores/use-content-store";

export type ModalType = "confirm" | "success" | "error";

// จัดลำดับการบันทึก การขออนุมัติ และคงรหัส Idempotency เดิมหากยังไม่ได้รับคำตอบจากเกตเวย์
export function useContentManagement() {
  const patch = useContentStore((s) => s.patch);
  const reset = useContentStore((s) => s.reset);
  const generation = useRef(0);
  const keys = useRef(new Map<string, string>());
  const draftKey = useRef<string | null>(null);
  const bankKey = useRef<string | null>(null);
  const pendingAction = useRef<(() => Promise<void>) | null>(null);

  const [modal, setModal] = useState<{
    open: boolean;
    type: ModalType;
    message: string;
  }>({ open: false, type: "confirm", message: "" });

  const execute = useCallback(
    async (operation: () => Promise<void>, showSuccess = false) => {
      const epoch = generation.current;
      patch({ busy: true, error: null });
      try {
        await operation();
        if (showSuccess && generation.current === epoch) {
          keys.current.clear();
          setModal({ open: true, type: "success", message: "บันทึกข้อมูลเรียบร้อยแล้ว" });
        }
      } catch (err) {
        const message =
          err instanceof AdminApiError
            ? err.messageTh
            : err instanceof Error
            ? err.message
            : "ไม่สามารถทำรายการได้";
        if (generation.current === epoch) {
          patch({ error: message });
          setModal({ open: true, type: "error", message });
        }
      } finally {
        if (generation.current === epoch) {
          patch({ busy: false });
        }
      }
    },
    [patch],
  );

  const request = useCallback(
    async <T,>(
      resource: string,
      method = "GET",
      body?: unknown,
      etag?: string | null,
      key?: string,
    ): Promise<Versioned<T>> => {
      return contentRequest<T>(resource, method, body, etag, key);
    },
    [],
  );

  const load = useCallback(
    () =>
      execute(async () => {
        const epoch = generation.current;
        const [curricula, banks, levels] = await Promise.all([
          request<ListPage>("curricula?limit=100"),
          request<ListPage>("question-banks?limit=100"),
          request<{ items: { id: number; title: string }[] }>("levels"),
        ]);
        if (generation.current === epoch) {
          patch({
            curricula: curricula.value,
            banks: banks.value,
            levels: levels.value.items,
          });
        }
      }),
    [execute, patch, request],
  );

  const invalidate = useCallback(() => {
    generation.current++;
    keys.current.clear();
    draftKey.current = null;
    bankKey.current = null;
    pendingAction.current = null;
    reset();
  }, [reset]);

  useEffect(() => {
    invalidate();
    void load();
    return invalidate;
  }, [load, invalidate]);

  const confirm = (message: string, operation: () => Promise<void>) => {
    pendingAction.current = operation;
    setModal({ open: true, type: "confirm", message });
  };

  const send = async <T,>(
    path: string,
    body: unknown,
    method = "POST",
    etag?: string | null,
  ) => {
    const fingerprint = JSON.stringify([path, body, method, etag]);
    const key = keys.current.get(fingerprint) ?? crypto.randomUUID();
    keys.current.set(fingerprint, key);
    return request<T>(path, method, body, etag, key);
  };

  const openCurriculum = (id: number) =>
    execute(async () => {
      const result = await request<Curriculum>(`curricula/${id}`);
      patch({ curriculum: result });
    });

  const openBank = (id: number) =>
    execute(async () => {
      const result = await request<QuestionBank>(`question-banks/${id}`);
      patch({ bank: result });
    });

  return {
    load,
    openCurriculum,
    openBank,
    modal,
    closeModal: () => setModal((v) => ({ ...v, open: false })),
    acceptModal: () => {
      const pending = pendingAction.current;
      if (pending) void execute(pending, true);
    },
    newCurriculum: () => {
      draftKey.current = null;
      patch({ curriculum: null });
    },
    newBank: () => {
      bankKey.current = null;
      patch({ bank: null });
    },
    saveMetadata: (values: MetadataValues) => {
      const err = validateMetadata(values);
      if (err) {
        patch({ error: err });
        setModal({ open: true, type: "error", message: err });
        return;
      }
      const selected = useContentStore.getState().curriculum;
      const existing = selected?.value;
      const body = {
        title: values.title,
        level_id: values.level_id,
        source_ref: values.source_ref,
        recommended_grades: values.grades
          .split(",")
          .map((v) => v.trim())
          .filter(Boolean),
        subjects: existing?.subjects ?? [],
      };
      draftKey.current ??= crypto.randomUUID();
      confirm("ยืนยันบันทึกข้อมูลหลักสูตร", async () => {
        const result = existing
          ? await send<{ id: number }>(`curricula/${existing.id}`, body, "PUT", selected?.etag)
          : await send<{ id: number }>("curricula", {
              ...body,
              curriculum_key: draftKey.current,
              revision: 1,
            });
        patch({ curriculum: await request<Curriculum>(`curricula/${result.value.id}`) });
        await load();
      });
    },
    createRevision: () => {
      const existing = useContentStore.getState().curriculum?.value;
      if (!existing) return;
      confirm("สร้างฉบับแก้ไขใหม่ โดยฉบับที่เผยแพร่เดิมยังคงมีผลตามปกติ", async () => {
        const lessons = lessonInputs(existing.lessons, true);
        const created = await send<{ id: number }>("curricula", {
          curriculum_key: existing.curriculum_key,
          revision: existing.revision + 1,
          title: existing.title,
          level_id: existing.level_id,
          source_ref: existing.source_ref,
          recommended_grades: existing.recommended_grades,
          subjects: existing.subjects,
        });
        const draft = await request<Curriculum>(`curricula/${created.value.id}`);
        await send(`curricula/${created.value.id}/content`, { lessons }, "PUT", draft.etag);
        patch({ curriculum: await request<Curriculum>(`curricula/${created.value.id}`) });
        await load();
      });
    },
    approveCriteria: (approvalRef: string) => {
      const current = useContentStore.getState().curriculum;
      if (!current) return;
      confirm("ยืนยันว่าได้ตรวจเกณฑ์ประเมินทั้งหมดแล้ว (ผู้อนุมัติต้องเป็นคนละคนกับผู้เขียน)", async () => {
        await send(
          `curricula/${current.value.id}/approve-criteria`,
          { approval_ref: approvalRef },
          "POST",
          current.etag,
        );
        patch({ curriculum: await request<Curriculum>(`curricula/${current.value.id}`) });
      });
    },
    saveLessons: (lessons: Lesson[]) => {
      const err = validateLessons(lessons);
      if (err) {
        patch({ error: err });
        setModal({ open: true, type: "error", message: err });
        return;
      }
      const current = useContentStore.getState().curriculum;
      if (!current) return;
      confirm("ยืนยันบันทึกบทเรียนและเกณฑ์ประเมินฉบับร่าง", async () => {
        await send(
          `curricula/${current.value.id}/content`,
          { lessons: lessonInputs(lessons) },
          "PUT",
          current.etag,
        );
        patch({ curriculum: await request<Curriculum>(`curricula/${current.value.id}`) });
      });
    },
    saveBank: (bankData: {
      title: QuestionBank["title"];
      source_ref: string;
      questions: QuestionBank["questions"];
    }) => {
      const current = useContentStore.getState().bank;
      confirm("ยืนยันบันทึกคลังข้อสอบฉบับร่าง", async () => {
        const result =
          current && current.value.lifecycle === "draft"
            ? await send<{ id: number }>(`question-banks/${current.value.id}`, bankData, "PUT", current.etag)
            : await send<{ id: number }>("question-banks", {
                ...bankData,
                bank_key: current?.value.bank_key ?? (bankKey.current ??= crypto.randomUUID()),
                revision: (current?.value.revision ?? 0) + 1,
              });
        patch({ bank: await request<QuestionBank>(`question-banks/${result.value.id}`) });
        await load();
      });
    },
    publish: (kind: "curricula" | "question-banks", approvalRef: string) => {
      const state = useContentStore.getState();
      const selected = kind === "curricula" ? state.curriculum : state.bank;
      if (!selected || !approvalRef.trim()) return;
      confirm("ยืนยันเผยแพร่ฉบับนี้ (ผู้เผยแพร่ต้องเป็นคนละคนกับผู้แก้ไขล่าสุด)", async () => {
        await send(
          `${kind}/${selected.value.id}/publish`,
          { approval_ref: approvalRef },
          "POST",
          selected.etag,
        );
        if (kind === "curricula") {
          patch({ curriculum: await request<Curriculum>(`${kind}/${selected.value.id}`) });
        } else {
          patch({ bank: await request<QuestionBank>(`${kind}/${selected.value.id}`) });
        }
        await load();
      });
    },
    importFile: async (file: File) => {
      const current = useContentStore.getState().curriculum;
      if (!current) return;
      await execute(async () => {
        if (file.size > 2 * 1024 * 1024) throw new Error("ไฟล์เกินขนาด 2 เมกะไบต์");
        let source: unknown;
        try {
          source = JSON.parse(await file.text());
        } catch {
          throw new Error("ไม่สามารถอ่านไฟล์ได้ กรุณาเลือกไฟล์ JSON ตามรูปแบบนำเข้า");
        }
        const parsed = parseAndValidateImport(source);
        const approved = parsed.rows.filter((r) => r.source_approved).length;
        confirm(
          `นำเข้า ${parsed.rows.length} แถว (อนุมัติแล้ว ${approved} แถว) แถวที่ยังไม่อนุมัติจะไม่ถูกเผยแพร่`,
          async () => {
            await send(`curricula/${current.value.id}/import`, parsed, "POST", current.etag);
            patch({ curriculum: await request<Curriculum>(`curricula/${current.value.id}`) });
          },
        );
      });
    },
    addAssessment: (bankId: number, kind: "quiz" | "test", accept: (activity: Activity) => void) =>
      execute(async () => {
        const bank = (await request<QuestionBank>(`question-banks/${bankId}`)).value;
        if (bank.lifecycle !== "published") {
          throw new Error("กรุณาเลือกคลังข้อสอบที่เผยแพร่แล้ว");
        }
        accept({
          activity_key: crypto.randomUUID(),
          ordinal: 1,
          title: kind === "quiz" ? "แบบฝึกหัด" : "ข้อสอบ",
          type: kind,
          indicators: "[]",
          config: JSON.stringify({
            kind: "question-v1",
            bank_id: bank.id,
            question_keys: bank.questions.map((q) => q.question_key),
            default_policy: {
              max_attempts: kind === "quiz" ? null : 2,
              selection: "best",
              time_limit_seconds: kind === "quiz" ? null : 120,
              reveal: kind === "quiz" ? "after_submit" : "after_close",
              shuffle_questions: true,
              shuffle_options: true,
            },
          }),
        });
      }),
    more: (kind: "curricula" | "question-banks") =>
      execute(async () => {
        const state = useContentStore.getState();
        const old = kind === "curricula" ? state.curricula : state.banks;
        if (!old.next_cursor) return;
        const response = await request<ListPage>(
          `${kind}?limit=100&cursor=${encodeURIComponent(old.next_cursor)}`,
        );
        patch(
          kind === "curricula"
            ? { curricula: { ...response.value, items: [...old.items, ...response.value.items] } }
            : { banks: { ...response.value, items: [...old.items, ...response.value.items] } },
        );
      }),
  };
}

export type ContentManagement = ReturnType<typeof useContentManagement>;
