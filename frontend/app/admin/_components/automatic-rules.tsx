"use client";

import { MdAdd, MdDelete } from "react-icons/md";
import { Input, NativeSelect } from "@/app/management/_components/ui";

type Rule = {
  id: string;
  type: string;
  op?: string;
  minimum?: number;
  maximum?: number;
  commands?: { op: string; value?: number }[];
};

type Rules = { kind: string; combine: string; rules: Rule[] };

const commands = [
  { value: "jump", label: "กระโดด" },
  { value: "move_left", label: "เดินซ้าย" },
  { value: "move_right", label: "เดินขวา" },
];

export default function AutomaticRules({
  value,
  readonly,
  onChange,
}: {
  value: string;
  readonly: boolean;
  onChange: (value: string) => void;
}) {
  let rules: Rules;
  try {
    rules = JSON.parse(value) as Rules;
  } catch {
    return <p role="alert" className="text-xs text-red-600">รูปแบบเกณฑ์ไม่ถูกต้อง</p>;
  }

  if (!Array.isArray(rules.rules)) {
    return <p role="alert" className="text-xs text-ink/60">ยังไม่มีรายการเกณฑ์ตรวจอัตโนมัติ</p>;
  }

  const update = (index: number, changes: Partial<Rule>) => {
    onChange(
      JSON.stringify({
        ...rules,
        rules: rules.rules.map((r, i) => (i === index ? { ...r, ...changes } : r)),
      }),
    );
  };

  return (
    <div className="flex flex-col gap-3 rounded-xl border border-navy/10 bg-paper-dim/40 p-4">
      <div className="flex items-center gap-3">
        <label className="text-xs font-semibold text-ink/80">เงื่อนไขผ่าน:</label>
        <div className="w-48">
          <NativeSelect
            value={rules.combine}
            disabled={readonly}
            onChange={(e) =>
              onChange(JSON.stringify({ ...rules, combine: e.target.value }))
            }
            aria-label="เงื่อนไขผ่าน"
          >
            <option value="all">ผ่านทุกเกณฑ์ (AND)</option>
            <option value="any">ผ่านอย่างน้อยหนึ่งเกณฑ์ (OR)</option>
          </NativeSelect>
        </div>
      </div>

      {rules.rules.map((rule, index) => (
        <div
          key={rule.id}
          className="grid gap-3 rounded-lg border border-navy/10 bg-white p-3 md:grid-cols-2"
        >
          <div>
            <label className="text-xs font-semibold text-ink/70">
              เกณฑ์ที่ {index + 1}: ชนิด
            </label>
            <NativeSelect
              value={rule.type}
              disabled={readonly}
              onChange={(e) =>
                update(index, {
                  type: e.target.value,
                  op: "jump",
                  minimum: 1,
                  maximum: 10,
                  commands: [{ op: "jump" }],
                })
              }
              aria-label={`เกณฑ์ที่ ${index + 1}`}
            >
              <option value="uses_op">ใช้คำสั่งที่กำหนด</option>
              <option value="steps_at_most">จำกัดจำนวนก้าวสูงสุด</option>
              <option value="trace_equals">ลำดับคำสั่งตรงเป๊ะ</option>
            </NativeSelect>
          </div>

          {rule.type === "uses_op" && (
            <div className="flex gap-2">
              <div className="flex-1">
                <label className="text-xs font-semibold text-ink/70">คำสั่ง</label>
                <NativeSelect
                  value={rule.op ?? "jump"}
                  disabled={readonly}
                  onChange={(e) => update(index, { op: e.target.value })}
                >
                  {commands.map((c) => (
                    <option key={c.value} value={c.value}>
                      {c.label}
                    </option>
                  ))}
                </NativeSelect>
              </div>
              <div className="w-28">
                <label className="text-xs font-semibold text-ink/70">ขั้นต่ำ (ครั้ง)</label>
                <Input
                  type="number"
                  value={String(rule.minimum ?? 1)}
                  disabled={readonly}
                  onChange={(val) => update(index, { minimum: Number(val) || 1 })}
                />
              </div>
            </div>
          )}

          {rule.type === "steps_at_most" && (
            <div>
              <label className="text-xs font-semibold text-ink/70">จำนวนก้าวสูงสุด</label>
              <Input
                type="number"
                value={String(rule.maximum ?? 10)}
                disabled={readonly}
                onChange={(val) => update(index, { maximum: Number(val) || 0 })}
              />
            </div>
          )}

          {rule.type === "trace_equals" && (
            <div className="flex flex-col gap-2 md:col-span-2">
              <label className="text-xs font-semibold text-ink/70">ลำดับคำสั่งที่ต้องตรงกัน</label>
              {(rule.commands ?? []).map((cmd, ci) => (
                <div key={ci} className="flex items-center gap-2">
                  <span className="text-xs text-ink/50">#{ci + 1}</span>
                  <NativeSelect
                    value={cmd.op}
                    disabled={readonly}
                    onChange={(e) =>
                      update(index, {
                        commands: rule.commands?.map((c, i) =>
                          i === ci ? { op: e.target.value, value: c.value ?? 1 } : c,
                        ),
                      })
                    }
                  >
                    {commands.map((c) => (
                      <option key={c.value} value={c.value}>
                        {c.label}
                      </option>
                    ))}
                  </NativeSelect>
                  {cmd.op !== "jump" && (
                    <Input
                      type="number"
                      value={String(cmd.value ?? 1)}
                      disabled={readonly}
                      onChange={(val) =>
                        update(index, {
                          commands: rule.commands?.map((c, i) =>
                            i === ci ? { ...c, value: Number(val) || 1 } : c,
                          ),
                        })
                      }
                    />
                  )}
                </div>
              ))}
              {!readonly && (
                <button
                  type="button"
                  onClick={() =>
                    update(index, {
                      commands: [...(rule.commands ?? []), { op: "jump" }],
                    })
                  }
                  className="inline-flex items-center gap-1 text-xs text-navy hover:text-orange transition"
                >
                  <MdAdd /> <span>เพิ่มคำสั่งในลำดับ</span>
                </button>
              )}
            </div>
          )}

          {!readonly && rules.rules.length > 1 && (
            <div className="md:col-span-2 flex justify-end">
              <button
                type="button"
                onClick={() =>
                  onChange(
                    JSON.stringify({
                      ...rules,
                      rules: rules.rules.filter((_, i) => i !== index),
                    }),
                  )
                }
                className="inline-flex items-center gap-1 text-xs text-red-600 hover:text-red-700 transition"
              >
                <MdDelete /> <span>นำเกณฑ์ออกจากร่าง</span>
              </button>
            </div>
          )}
        </div>
      ))}

      {!readonly && rules.rules.length < 64 && (
        <button
          type="button"
          onClick={() =>
            onChange(
              JSON.stringify({
                ...rules,
                rules: [
                  ...rules.rules,
                  { id: crypto.randomUUID(), type: "uses_op", op: "jump", minimum: 1 },
                ],
              }),
            )
          }
          className="inline-flex items-center gap-1 text-xs font-semibold text-navy hover:text-orange transition"
        >
          <MdAdd /> <span>เพิ่มเกณฑ์ตรวจ</span>
        </button>
      )}
    </div>
  );
}
