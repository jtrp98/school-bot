"use client";

import { useEffect, useState } from "react";
import { MdAdd, MdMenuBook, MdDelete } from "react-icons/md";
import { Card, Button, Input, NativeSelect } from "@/app/management/_components/ui";
import { useContentStore } from "../_stores/use-content-store";
import { contentTitle, type Lesson, type Mission } from "../_types/content";
import type { ContentManagement } from "../_hooks/use-content-management";
import AssessmentPolicyFields from "./assessment-policy-fields";
import AutomaticRules from "./automatic-rules";

// แก้ไขบทเรียน ภารกิจ และเกณฑ์ประเมินฉบับร่าง
export default function LessonEditor({ manager }: { manager: ContentManagement }) {
  const selected = useContentStore((s) => s.curriculum);
  const banks = useContentStore((s) => s.banks);
  const busy = useContentStore((s) => s.busy);

  const [bankId, setBankId] = useState<number>(0);
  const [lessons, setLessons] = useState<Lesson[]>([]);

  useEffect(() => {
    setLessons(selected?.value.lessons ?? []);
  }, [selected]);

  const readonly = selected?.value.lifecycle !== "draft";

  const updateLesson = (index: number, patch: Partial<Lesson>) => {
    setLessons((prev) =>
      prev.map((l, i) => (i === index ? { ...l, ...patch } : l)),
    );
  };

  const addMission = (lessonIndex: number, mode: "arm" | "block" | "code") => {
    const lesson = lessons[lessonIndex];
    const key = crypto.randomUUID();
    const rules =
      mode === "arm"
        ? {
            kind: "teacher-v1",
            items: [
              { id: crypto.randomUUID(), instruction: "ตรวจการเคลื่อนไหวตามคำสั่งอย่างปลอดภัย" },
            ],
          }
        : {
            kind: "automatic-v1",
            combine: "all",
            rules: [{ id: crypto.randomUUID(), type: "uses_op", op: "jump", minimum: 1 }],
          };

    const mission: Mission = {
      mission_key: key,
      ordinal: lesson.missions.length + 1,
      title: "",
      instructions: "",
      method: mode === "arm" ? "teacher" : "automatic",
      criteria: {
        revision: 1,
        rules: JSON.stringify(rules),
        source_ref: selected?.value.source_ref ?? "manual",
      },
    };

    updateLesson(lessonIndex, {
      missions: [...lesson.missions, mission],
      activities: [
        ...lesson.activities,
        {
          activity_key: crypto.randomUUID(),
          ordinal: lesson.activities.length + 1,
          title: `ภารกิจ ${mode === "arm" ? "แขนกล" : mode === "block" ? "บล็อกคำสั่ง" : "เขียนโปรแกรม"}`,
          type: "project",
          mode,
          indicators: "[]",
          config: JSON.stringify({ kind: "project-v1", mission_key: key }),
        },
      ],
    });
  };

  const changeRules = (
    lessonIndex: number,
    missionIndex: number,
    rules: Record<string, unknown>,
  ) => {
    const mission = lessons[lessonIndex].missions[missionIndex];
    const original = selected?.value.lessons
      .flatMap((l) => l.missions)
      .find((m) => m.mission_key === mission.mission_key);

    const updatedMissions = lessons[lessonIndex].missions.map((m, i) =>
      i === missionIndex
        ? {
            ...m,
            criteria: {
              ...m.criteria,
              revision: original ? original.criteria.revision + 1 : 1,
              rules: JSON.stringify(rules),
              approved_by: null,
              approved_at: null,
            },
          }
        : m,
    );

    updateLesson(lessonIndex, { missions: updatedMissions });
  };

  if (!selected) return null;

  return (
    <Card
      title="บทเรียน ภารกิจ และเกณฑ์ประเมิน"
      icon={<MdMenuBook />}
      description={readonly ? "ฉบับเผยแพร่แล้ว (อ่านอย่างเดียว)" : "เพิ่มบทเรียน กำหนดภารกิจ และเกณฑ์ตรวจ"}
    >
      <div className="flex flex-col gap-6">
        {lessons.map((lesson, index) => (
          <section
            key={lesson.lesson_key || index}
            className="flex flex-col gap-4 rounded-2xl border border-navy/15 bg-white p-5 shadow-sm"
          >
            <div className="flex items-center justify-between border-b border-navy/10 pb-3">
              <h3 className="font-display text-sm font-bold text-navy">
                บทเรียนที่ {index + 1}
              </h3>
              {!readonly && (
                <button
                  type="button"
                  onClick={() => setLessons((prev) => prev.filter((_, i) => i !== index))}
                  className="inline-flex items-center gap-1 text-xs text-red-600 hover:text-red-700 transition"
                >
                  <MdDelete /> <span>ลบบทเรียนนี้</span>
                </button>
              )}
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <label className="text-xs font-semibold text-ink/80">ชื่อบทเรียน</label>
                <Input
                  value={lesson.title}
                  disabled={readonly || busy}
                  placeholder="ชื่อบทเรียน"
                  onChange={(val) => updateLesson(index, { title: val })}
                />
              </div>

              <div>
                <label className="text-xs font-semibold text-ink/80">เป้าหมายการเรียนรู้</label>
                <Input
                  value={lesson.objective}
                  disabled={readonly || busy}
                  placeholder="เป้าหมายการเรียนรู้"
                  onChange={(val) => updateLesson(index, { objective: val })}
                />
              </div>

              <div className="md:col-span-2">
                <label className="text-xs font-semibold text-ink/80">เนื้อหาและคำแนะนำ</label>
                <Input
                  multiline
                  value={lesson.instructions}
                  disabled={readonly || busy}
                  placeholder="เนื้อหาและคำแนะนำในการเรียน"
                  onChange={(val) => updateLesson(index, { instructions: val })}
                />
              </div>
            </div>

            {/* Missions List */}
            {lesson.missions.map((mission, mi) => {
              let rules: Record<string, unknown> = {};
              try {
                rules = JSON.parse(mission.criteria.rules) as Record<string, unknown>;
              } catch {
                /* fallback */
              }
              const items = Array.isArray(rules.items)
                ? (rules.items as { id: string; instruction: string }[])
                : [];

              return (
                <div
                  key={mission.mission_key || mi}
                  className="flex flex-col gap-3 rounded-xl border border-navy/10 bg-paper-dim/30 p-4"
                >
                  <div className="flex items-center justify-between">
                    <span className="font-display text-xs font-bold text-navy">
                      ภารกิจที่ {mi + 1} ({mission.method === "teacher" ? "ครูตรวจตามเกณฑ์" : "ตรวจโปรแกรมอัตโนมัติ"})
                    </span>
                    <span
                      className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${
                        mission.criteria.approved_by
                          ? "bg-emerald-100 text-emerald-800"
                          : "bg-amber-100 text-amber-800"
                      }`}
                    >
                      {mission.criteria.approved_by ? "อนุมัติเกณฑ์แล้ว" : "รออนุมัติเกณฑ์"}
                    </span>
                  </div>

                  <div className="grid gap-3 md:grid-cols-2">
                    <div>
                      <label className="text-xs font-semibold text-ink/70">ชื่อภารกิจ</label>
                      <Input
                        value={mission.title}
                        disabled={readonly || busy}
                        placeholder="ชื่อภารกิจ"
                        onChange={(val) => {
                          const updated = lesson.missions.map((m, i) =>
                            i === mi ? { ...m, title: val } : m,
                          );
                          updateLesson(index, { missions: updated });
                        }}
                      />
                    </div>
                    <div>
                      <label className="text-xs font-semibold text-ink/70">สิ่งที่นักเรียนต้องทำ</label>
                      <Input
                        value={mission.instructions}
                        disabled={readonly || busy}
                        placeholder="สิ่งที่นักเรียนต้องทำ"
                        onChange={(val) => {
                          const updated = lesson.missions.map((m, i) =>
                            i === mi ? { ...m, instructions: val } : m,
                          );
                          updateLesson(index, { missions: updated });
                        }}
                      />
                    </div>
                  </div>

                  {/* Teacher Criteria items */}
                  {items.map((item, ri) => (
                    <div key={item.id || ri}>
                      <label className="text-xs font-semibold text-ink/70">
                        เกณฑ์ข้อที่ {ri + 1}
                      </label>
                      <Input
                        value={item.instruction}
                        disabled={readonly || busy}
                        onChange={(val) =>
                          changeRules(index, mi, {
                            ...rules,
                            items: items.map((r, i) =>
                              i === ri ? { ...r, instruction: val } : r,
                            ),
                          })
                        }
                      />
                    </div>
                  ))}

                  {/* Automatic Rules */}
                  {mission.method === "automatic" && (
                    <AutomaticRules
                      value={mission.criteria.rules}
                      readonly={readonly || busy}
                      onChange={(newRulesStr) => {
                        try {
                          changeRules(index, mi, JSON.parse(newRulesStr) as Record<string, unknown>);
                        } catch {
                          /* ignore */
                        }
                      }}
                    />
                  )}
                </div>
              );
            })}

            {/* Assessment Activities */}
            {lesson.activities.map((activity, ai) => (
              <AssessmentPolicyFields
                key={activity.activity_key || ai}
                activity={activity}
                readonly={readonly || busy}
                onChange={(updated) => {
                  const acts = lesson.activities.map((a, i) => (i === ai ? updated : a));
                  updateLesson(index, { activities: acts });
                }}
              />
            ))}

            {/* Action buttons inside lesson */}
            {!readonly && (
              <div className="flex flex-wrap items-center gap-2 pt-2 border-t border-navy/10">
                <Button
                  content="เพิ่มภารกิจบล็อกคำสั่ง"
                  variant="secondary-outline"
                  icon={<MdAdd />}
                  disabled={busy}
                  onClick={() => addMission(index, "block")}
                />
                <Button
                  content="เพิ่มภารกิจเขียนโค้ด"
                  variant="secondary-outline"
                  icon={<MdAdd />}
                  disabled={busy}
                  onClick={() => addMission(index, "code")}
                />
                <Button
                  content="เพิ่มภารกิจแขนกล"
                  variant="secondary-outline"
                  icon={<MdAdd />}
                  disabled={busy}
                  onClick={() => addMission(index, "arm")}
                />
                <Button
                  content="เพิ่มแบบฝึกหัด"
                  variant="secondary-outline"
                  disabled={busy || !bankId}
                  onClick={() =>
                    bankId &&
                    manager.addAssessment(bankId, "quiz", (act) =>
                      updateLesson(index, { activities: [...lesson.activities, act] }),
                    )
                  }
                />
                <Button
                  content="เพิ่มข้อสอบ"
                  variant="secondary-outline"
                  disabled={busy || !bankId}
                  onClick={() =>
                    bankId &&
                    manager.addAssessment(bankId, "test", (act) =>
                      updateLesson(index, { activities: [...lesson.activities, act] }),
                    )
                  }
                />
              </div>
            )}
          </section>
        ))}

        {!readonly && (
          <div className="flex flex-col gap-4 rounded-xl border border-dashed border-navy/20 p-4">
            <div className="flex items-center gap-3">
              <label className="text-xs font-semibold text-ink/80 shrink-0">
                คลังข้อสอบสำหรับสร้างกิจกรรมใหม่:
              </label>
              <div className="w-72">
                <NativeSelect
                  value={bankId}
                  disabled={busy}
                  onChange={(e) => setBankId(Number(e.target.value))}
                  aria-label="คลังข้อสอบสำหรับกิจกรรมใหม่"
                >
                  <option value={0}>-- เลือกคลังข้อสอบที่เผยแพร่แล้ว --</option>
                  {banks.items
                    .filter((b) => b.lifecycle === "published")
                    .map((b) => (
                      <option key={b.id} value={b.id}>
                        {contentTitle(b.title)} (rev. {b.revision})
                      </option>
                    ))}
                </NativeSelect>
              </div>
            </div>

            <div className="flex items-center justify-between pt-2">
              <Button
                content="เพิ่มบทเรียนใหม่"
                variant="secondary-outline"
                icon={<MdAdd />}
                disabled={busy}
                onClick={() =>
                  setLessons((prev) => [
                    ...prev,
                    {
                      lesson_key: crypto.randomUUID(),
                      ordinal: prev.length + 1,
                      title: "",
                      objective: "",
                      instructions: "",
                      missions: [],
                      activities: [],
                    },
                  ])
                }
              />

              <Button
                content="บันทึกบทเรียนฉบับร่าง"
                disabled={busy || lessons.length === 0}
                onClick={() => manager.saveLessons(lessons)}
              />
            </div>
          </div>
        )}
      </div>
    </Card>
  );
}
