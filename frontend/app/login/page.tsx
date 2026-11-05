import { Suspense } from "react";
import LoginForm from "./LoginForm";

export const metadata = {
  title: "เข้าสู่ระบบ — School Bot",
};

export default function LoginPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-white px-6 py-16">
      <Suspense fallback={null}>
        <LoginForm />
      </Suspense>
    </div>
  );
}
