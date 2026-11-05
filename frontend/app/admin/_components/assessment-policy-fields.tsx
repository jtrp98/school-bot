"use client";

import { Input, NativeSelect } from "@/app/management/_components/ui";
import type { Activity } from "../_types/content";

type Policy = {
  max_attempts: number | null;
  selection: string;
  time_limit_seconds: number | null;
  reveal: string;
  shuffle_questions: boolean;
  shuffle_options: boolean;
};

// ปรับแต่งนโยบายกลางของกิจกรรมการทดสอบ โดยไม่กระทบผลสอบเดิมของนักเรียน
export default function AssessmentPolicyFields({
  activity,
  readonly,
  onChange,
}: {
  activity: Activity;
  readonly: boolean;
  onChange: (activity: Activity) => void;
}) {
  if (activity.type !== "quiz" && activity.type !== "test") {
    return <p className="text-xs text-ink/70">{activity.title}</p>;
  }

  let config: Record<string, unknown>;
  try {
    config = JSON.parse(activity.config ?? "{}");
  } catch {
    return <p role="alert" className="text-xs text-red-600">รูปแบบกิจกรรมไม่ถูกต้อง</p>;
  }

  const policy = config.default_policy as Policy;
  if (!policy) {
    return <p role="alert" className="text-xs text-ink/60">กิจกรรมนี้ยังไม่มีนโยบายสอบ</p>;
  }

  const update = (changes: Partial<Policy>) => {
    onChange({
      ...activity,
      config: JSON.stringify({
        ...config,
        default_policy: { ...policy, ...changes },
      }),
    });
  };

  return (
    <div className="grid gap-4 rounded-xl border border-navy/10 bg-paper-dim/40 p-4 md:grid-cols-2">
      <div>
        <label className="text-xs font-semibold text-ink/80">ชื่อกิจกรรม</label>
        <Input
          value={activity.title}
          disabled={readonly}
          onChange={(val) => onChange({ ...activity, title: val })}
        />
      </div>

      <div>
        <label className="text-xs font-semibold text-ink/80">
          จำนวนครั้งที่สอบได้ (ว่างคือไม่จำกัด)
        </label>
        <Input
          type="number"
          value={policy.max_attempts ? String(policy.max_attempts) : ""}
          disabled={readonly}
          placeholder="ไม่จำกัด"
          onChange={(val) => update({ max_attempts: val ? Number(val) : null })}
        />
      </div>

      <div>
        <label className="text-xs font-semibold text-ink/80">
          เวลาสอบเป็นวินาที (ว่างคือไม่จำกัด)
        </label>
        <Input
          type="number"
          value={policy.time_limit_seconds ? String(policy.time_limit_seconds) : ""}
          disabled={readonly}
          placeholder="ไม่จำกัด"
          onChange={(val) => update({ time_limit_seconds: val ? Number(val) : null })}
        />
      </div>

      <div>
        <label className="text-xs font-semibold text-ink/80">คะแนนที่เลือกใช้</label>
        <NativeSelect
          value={policy.selection}
          disabled={readonly || activity.type === "test"}
          onChange={(e) => update({ selection: e.target.value })}
        >
          <option value="best">คะแนนสูงสุด (Best)</option>
          <option value="first">คะแนนครั้งแรก (First)</option>
          <option value="latest">คะแนนครั้งล่าสุด (Latest)</option>
        </NativeSelect>
      </div>

      <div className="md:col-span-2">
        <label className="text-xs font-semibold text-ink/80">การแสดงผลและเฉลย</label>
        <NativeSelect
          value={policy.reveal}
          disabled={readonly}
          onChange={(e) => update({ reveal: e.target.value })}
        >
          <option value="after_submit">แสดงผลและเฉลยหลังส่งคำตอบ</option>
          <option value="after_close">แสดงผลและเฉลยหลังปิดรอบสอบ</option>
          <option value="never">ไม่แสดงเฉลย</option>
        </NativeSelect>
      </div>

      <div className="flex items-center gap-6 md:col-span-2">
        <label className="inline-flex items-center gap-2 text-xs font-medium text-ink/80 cursor-pointer">
          <input
            type="checkbox"
            checked={policy.shuffle_questions}
            disabled={readonly}
            onChange={(e) => update({ shuffle_questions: e.target.checked })}
            className="rounded border-navy/20 text-orange focus:ring-orange"
          />
          <span>สลับลำดับคำถาม</span>
        </label>

        <label className="inline-flex items-center gap-2 text-xs font-medium text-ink/80 cursor-pointer">
          <input
            type="checkbox"
            checked={policy.shuffle_options}
            disabled={readonly}
            onChange={(e) => update({ shuffle_options: e.target.checked })}
            className="rounded border-navy/20 text-orange focus:ring-orange"
          />
          <span>สลับตัวเลือกคำตอบ</span>
        </label>
      </div>
    </div>
  );
}
