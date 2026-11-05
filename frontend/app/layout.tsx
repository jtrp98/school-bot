import type { Metadata } from "next";
import "@fontsource/mitr/500.css";
import "@fontsource/mitr/600.css";
import "@fontsource/mitr/700.css";
import "@fontsource/sarabun/400.css";
import "@fontsource/sarabun/500.css";
import "@fontsource/sarabun/600.css";
import "@fontsource/sarabun/700.css";
import "./globals.css";
import Nav from "@/components/Nav";

export const metadata: Metadata = {
  title: "School Bot — คู่มือครู Robotics & AI (Robo X)",
  description:
    "เว็บคู่มือสำหรับครูสอนหลักสูตร Robotics and AI ป.1-6 ด้วยหุ่นยนต์ Robo X",
  robots: {
    index: false,
    follow: false,
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="th">
      <body className="font-body antialiased">
        <Nav />
        <main>{children}</main>
        <footer className="border-t border-navy/10 bg-white px-6 py-8 text-sm text-ink/60">
          <div className="mx-auto flex max-w-6xl flex-col gap-1">
            <p className="font-display font-medium text-navy">
              School Bot · หลักสูตร Robotics &amp; AI กับ Robo X
            </p>
            <p>เอกสารฉบับร่าง — สำหรับใช้ภายในทีมหลักสูตรและครูผู้สอนเท่านั้น</p>
          </div>
        </footer>
      </body>
    </html>
  );
}
