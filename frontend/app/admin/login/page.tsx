import { Suspense } from "react";
import AdminLoginForm from "./AdminLoginForm";

export const metadata = {
  title: "เข้าสู่ระบบผู้ดูแลระบบ (TAdmin) — School Bot Admin",
};

export default function AdminLoginPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-paper px-4 py-16">
      <Suspense fallback={null}>
        <AdminLoginForm />
      </Suspense>
    </div>
  );
}
