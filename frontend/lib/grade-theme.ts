import type { CurriculumGrade } from "@/lib/new-curriculum";

// โทนสีรายชั้น ไล่จากฟ้า (ป.1) ไปทางส้มอมชมพู (ป.6) ให้ต่อเนื่องกับสีโลโก้
// accent ทุกสีผ่าน contrast 4.5:1 บนพื้นขาวและพื้น tint
// ใช้ร่วมกันระหว่างหน้าแรกและหน้าหลักสูตร (ใหม่)
export interface GradeTheme {
  card: string;
  tile: string;
  badge: string;
  dot: string;
  accent: string;
  line: string;
  /** พื้นอ่อนสำหรับหัวหน้า/วงกลมเลขสัปดาห์ */
  soft: string;
  /** วงแหวน/ขอบบาง */
  ring: string;
  /** hover พื้นแถว */
  hover: string;
}

export const gradeThemes: Record<CurriculumGrade["slug"], GradeTheme> = {
  p1: {
    card: "border-[#68C8F2]/45 from-[#E6F5FC] hover:border-[#68C8F2] hover:shadow-[#68C8F2]/25",
    tile: "bg-[#68C8F2]",
    badge: "text-[#1B6E9A] ring-[#68C8F2]/60",
    dot: "bg-[#68C8F2]",
    accent: "text-[#1B6E9A]",
    line: "border-[#68C8F2]/40",
    soft: "bg-[#E6F5FC]",
    ring: "ring-[#68C8F2]/50",
    hover: "hover:bg-[#E6F5FC]/60",
  },
  p2: {
    card: "border-[#5FD0BF]/45 from-[#E3F6F2] hover:border-[#5FD0BF] hover:shadow-[#5FD0BF]/25",
    tile: "bg-[#5FD0BF]",
    badge: "text-[#11786A] ring-[#5FD0BF]/60",
    dot: "bg-[#5FD0BF]",
    accent: "text-[#11786A]",
    line: "border-[#5FD0BF]/40",
    soft: "bg-[#E3F6F2]",
    ring: "ring-[#5FD0BF]/50",
    hover: "hover:bg-[#E3F6F2]/60",
  },
  p3: {
    card: "border-[#A3DB8C]/55 from-[#ECF8E6] hover:border-[#A3DB8C] hover:shadow-[#A3DB8C]/25",
    tile: "bg-[#A3DB8C]",
    badge: "text-[#3A7D22] ring-[#A3DB8C]/70",
    dot: "bg-[#8FCF75]",
    accent: "text-[#3A7D22]",
    line: "border-[#A3DB8C]/50",
    soft: "bg-[#ECF8E6]",
    ring: "ring-[#A3DB8C]/60",
    hover: "hover:bg-[#ECF8E6]/60",
  },
  p4: {
    card: "border-[#FFD44F]/55 from-[#FFF6D9] hover:border-[#F2C230] hover:shadow-[#FFD44F]/30",
    tile: "bg-[#FFD44F]",
    badge: "text-[#8A6400] ring-[#FFD44F]/80",
    dot: "bg-[#F2C230]",
    accent: "text-[#8A6400]",
    line: "border-[#FFD44F]/50",
    soft: "bg-[#FFF6D9]",
    ring: "ring-[#FFD44F]/70",
    hover: "hover:bg-[#FFF6D9]/60",
  },
  p5: {
    card: "border-orange/45 from-[#FFEEE3] hover:border-orange hover:shadow-orange/25",
    tile: "bg-orange",
    badge: "text-[#B04A17] ring-orange/60",
    dot: "bg-orange",
    accent: "text-[#B04A17]",
    line: "border-orange/40",
    soft: "bg-[#FFEEE3]",
    ring: "ring-orange/50",
    hover: "hover:bg-[#FFEEE3]/60",
  },
  p6: {
    card: "border-[#F28AA0]/45 from-[#FCEBEF] hover:border-[#F28AA0] hover:shadow-[#F28AA0]/25",
    tile: "bg-[#F28AA0]",
    badge: "text-[#A93B57] ring-[#F28AA0]/60",
    dot: "bg-[#F28AA0]",
    accent: "text-[#A93B57]",
    line: "border-[#F28AA0]/40",
    soft: "bg-[#FCEBEF]",
    ring: "ring-[#F28AA0]/50",
    hover: "hover:bg-[#FCEBEF]/60",
  },
};

// สีหัวข้อใหญ่ในแผนการสอน วนตามลำดับหัวข้อ — ฟ้ากับส้มสลับกันเป็นหลัก แทรกสีอื่นจากหน้าแรก
export const sectionTones = [
  { tile: "bg-[#68C8F2]", text: "text-[#1B6E9A]", border: "border-[#68C8F2]", soft: "bg-[#E6F5FC]" },
  { tile: "bg-orange", text: "text-[#B04A17]", border: "border-orange", soft: "bg-[#FFEEE3]" },
  { tile: "bg-[#5FD0BF]", text: "text-[#11786A]", border: "border-[#5FD0BF]", soft: "bg-[#E3F6F2]" },
  { tile: "bg-[#FFD44F]", text: "text-[#8A6400]", border: "border-[#F2C230]", soft: "bg-[#FFF6D9]" },
  { tile: "bg-[#F28AA0]", text: "text-[#A93B57]", border: "border-[#F28AA0]", soft: "bg-[#FCEBEF]" },
  { tile: "bg-[#A3DB8C]", text: "text-[#3A7D22]", border: "border-[#8FCF75]", soft: "bg-[#ECF8E6]" },
] as const;

export function sectionTone(number: number) {
  return sectionTones[(Math.max(number, 1) - 1) % sectionTones.length];
}
