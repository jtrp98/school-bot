"use client";

import { useState } from "react";
import { Button, Input, NativeSelect } from "@/app/management/_components/ui";
import { validateQuestion } from "../_utils/content-validation";
import type { Question } from "../_types/content";

// ฟอร์มสร้างหรือแก้ไขคำถามในคลังข้อสอบ
export default function QuestionForm({
  question,
  onSave,
  onCancel,
}: {
  question?: Question;
  onSave: (value: Question) => void;
  onCancel: () => void;
}) {
  const [prompt, setPrompt] = useState(question?.prompt.th ?? "");
  const [kind, setKind] = useState<"single_choice" | "boolean">(question?.kind ?? "single_choice");
  const [maxScore, setMaxScore] = useState(question?.max_score ?? "1");
  const [options, setOptions] = useState<string[]>(
    question?.options.map((o) => o.text.th ?? "") ?? ["", "", "", ""],
  );
  const [correct, setCorrect] = useState<number>(
    question
      ? question.options.findIndex((o) => question.correct_option_keys.includes(o.option_key))
      : 0,
  );
  const [error, setError] = useState<string | null>(null);

  const handleKindChange = (newKind: "single_choice" | "boolean") => {
    setKind(newKind);
    if (newKind === "boolean") {
      setOptions(["ถูก", "ผิด"]);
      setCorrect(0);
    } else if (options.length < 4) {
      setOptions(["", "", "", ""]);
      setCorrect(0);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const err = validateQuestion({
      prompt,
      kind,
      max_score: maxScore,
      options,
      correct,
    });
    if (err) {
      setError(err);
      return;
    }

    const savedOptions = options.map((text, index) => ({
      option_key: question?.options[index]?.option_key ?? crypto.randomUUID(),
      text: { ...question?.options[index]?.text, th: text },
    }));

    onSave({
      question_key: question?.question_key ?? crypto.randomUUID(),
      kind,
      prompt: { ...question?.prompt, th: prompt },
      max_score: maxScore,
      options: savedOptions,
      correct_option_keys: [savedOptions[correct]?.option_key ?? savedOptions[0].option_key],
    });
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="flex flex-col gap-4 rounded-2xl border border-navy/20 bg-paper-dim/30 p-5 shadow-sm"
    >
      <h4 className="font-display text-sm font-bold text-navy">
        {question ? "แก้ไขคำถาม" : "เพิ่มคำถามใหม่"}
      </h4>

      {error && (
        <div role="alert" className="rounded-xl border border-red-200 bg-red-50 p-3 text-xs text-red-700">
          {error}
        </div>
      )}

      <div>
        <label className="text-xs font-semibold text-ink/80">
          โจทย์คำถาม <span className="text-red-500">*</span>
        </label>
        <Input
          multiline
          value={prompt}
          placeholder="ระบุข้อความคำถาม"
          onChange={setPrompt}
        />
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div>
          <label className="text-xs font-semibold text-ink/80">ประเภทข้อสอบ</label>
          <NativeSelect
            value={kind}
            onChange={(e) => handleKindChange(e.target.value as "single_choice" | "boolean")}
          >
            <option value="single_choice">เลือกคำตอบเดียว (Multiple Choice)</option>
            <option value="boolean">ถูก หรือ ผิด (True/False)</option>
          </NativeSelect>
        </div>

        <div>
          <label className="text-xs font-semibold text-ink/80">
            คะแนนเต็ม <span className="text-red-500">*</span>
          </label>
          <Input
            type="number"
            value={maxScore}
            placeholder="1"
            onChange={setMaxScore}
          />
        </div>
      </div>

      {/* Options */}
      <div className="flex flex-col gap-3">
        <label className="text-xs font-semibold text-ink/80">
          ตัวเลือกคำตอบ <span className="text-red-500">*</span>
        </label>
        {options.map((opt, index) => (
          <div key={index} className="flex items-center gap-2">
            <span className="text-xs font-bold text-ink/50 w-6">#{index + 1}</span>
            <div className="flex-1">
              <Input
                value={opt}
                disabled={kind === "boolean"}
                placeholder={`ตัวเลือกที่ ${index + 1}`}
                onChange={(val) =>
                  setOptions((prev) => prev.map((o, i) => (i === index ? val : o)))
                }
              />
            </div>
            <label className="flex items-center gap-1.5 text-xs text-ink/70 shrink-0 cursor-pointer">
              <input
                type="radio"
                name="correct-choice"
                checked={correct === index}
                onChange={() => setCorrect(index)}
                className="text-orange focus:ring-orange"
              />
              <span>คำตอบที่ถูก</span>
            </label>
          </div>
        ))}

        {kind === "single_choice" && (
          <div className="flex items-center gap-2 pt-1">
            <Button
              content="เพิ่มตัวเลือก"
              variant="secondary-outline"
              disabled={options.length >= 8}
              onClick={() => setOptions((prev) => [...prev, ""])}
            />
            <Button
              content="ลดตัวเลือก"
              variant="secondary-outline"
              disabled={options.length <= 2}
              onClick={() => {
                setOptions((prev) => prev.slice(0, -1));
                if (correct >= options.length - 1) setCorrect(0);
              }}
            />
          </div>
        )}
      </div>

      <div className="flex items-center justify-end gap-3 border-t border-navy/10 pt-4">
        <Button content="ยกเลิก" variant="secondary-outline" onClick={onCancel} />
        <button
          type="submit"
          className="rounded-xl bg-orange px-4 py-2.5 text-xs font-semibold text-white shadow-sm hover:bg-orange/90 transition"
        >
          บันทึกคำถามในร่าง
        </button>
      </div>
    </form>
  );
}
