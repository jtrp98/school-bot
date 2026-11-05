export interface InstallGuide {
  slug: string;
  name: string;
  platform: "มือถือ (iOS/Android)" | "คอมพิวเตอร์ (Windows)";
  usedInGrades: number[];
  steps: string[];
  downloadNote: string;
  href?: string;
  sampleCodeHref?: string;
}

export const installGuides: InstallGuide[] = [
  {
    slug: "app",
    name: "โปรแกรมควบคุมผ่าน APP",
    platform: "มือถือ (iOS/Android)",
    usedInGrades: [1, 2],
    steps: [
      "TODO: ค้นหาแอปในสโตร์",
      "TODO: เชื่อมต่อผ่าน Wi‑Fi/Bluetooth กับตัวควบคุม",
    ],
    downloadNote: "เปิดคู่มือโปรแกรมควบคุมผ่าน APP",
    href: "https://docs.google.com/document/d/1vB7FLxNIHseyfA7uer2gUll_ljqMvXPLf5NZhFbztYY/edit?tab=t.0",
  },
  {
    slug: "joystick",
    name: "Joy Stick",
    platform: "คอมพิวเตอร์ (Windows)",
    usedInGrades: [1, 2, 3, 4, 5, 6],
    steps: [
      "คลิกเพื่อเปิดคู่มือการติดตั้งและการใช้งาน Joy Stick",
      "ทำตามขั้นตอนในเอกสารบนเครื่องจริงก่อนเริ่มสอน",
    ],
    downloadNote: "เปิดคู่มือ Joy Stick",
    href: "https://docs.google.com/document/d/1_IUEIj-xiE7kAuaS--IFXcBwv9j8tNCtSybmxxMXDsQ/edit?tab=t.0",
  },
  {
    slug: "pc-software",
    name: "โปรแกรมควบคุมผ่าน PC",
    platform: "คอมพิวเตอร์ (Windows)",
    usedInGrades: [2, 3, 4, 5, 6],
    steps: [
      "TODO: ติดตั้งไฟล์ติดตั้งจากโฟลเดอร์ Appendix",
      "TODO: ติดตั้งไดรเวอร์ CH340",
    ],
    downloadNote: "เปิดคู่มือโปรแกรม PC",
    href: "https://docs.google.com/document/d/1J_78Kia0pTqmvsBp5D98E2gi50ZarS4UammYEZsH49Q/edit?tab=t.0",
  },
  {
    slug: "wondercode",
    name: "โปรแกรม Scratch (Wondercode)",
    platform: "คอมพิวเตอร์ (Windows)",
    usedInGrades: [3, 4, 5, 6],
    steps: [
      "TODO: ติดตั้ง WonderCode setup.exe",
      "TODO: เลือกภาษา English แล้วติดตั้งตามค่าเริ่มต้น",
      'TODO: เพิ่ม Extension แล้วเลือก Robot > xArm (ใน WonderCode ยังใช้ชื่อเดิม "xArm")',
      "TODO: เชื่อมต่อพอร์ต COM ให้ถูกต้อง",
    ],
    downloadNote: "เปิดคู่มือโปรแกรม Scratch (Wondercode)",
    href: "https://docs.google.com/document/d/1-jtvo1foqPvNcYWdssbdAlRA8Bl-lppWVeiSmbJrhoM/edit?tab=t.0",
    sampleCodeHref:
      "https://drive.google.com/drive/folders/1yC8-HUVx_aS9lqFVLtuT1nJ7kr3zPLR2",
  },
  {
    slug: "python",
    name: "โปรแกรม Python Editor",
    platform: "คอมพิวเตอร์ (Windows)",
    usedInGrades: [6],
    steps: [
      "TODO: เปิดผ่าน WonderCode โดยกดสลับโหมด Python Programming",
    ],
    downloadNote: "เปิดคู่มือโปรแกรม Python Editor",
    href: "https://docs.google.com/document/d/1dctd0eVI0S56DWGAu9BgaOlPKrxUJylOJaiUWo077fk/edit?tab=t.0",
  },
];
