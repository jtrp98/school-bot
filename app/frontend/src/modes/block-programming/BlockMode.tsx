import { lazy, Suspense } from "react";
import { param } from "../../core/debugParams";
import BlockLabShell from "./shell/BlockLabShell";
import "./blockLab.css";

// The design check sheet is a development aid, so production builds leave it out.
const BlockSheet = import.meta.env.DEV ? lazy(() => import("./BlockSheet")) : null;

export default function BlockMode() {
  return (
    <div data-theme="block-lab">
      {BlockSheet && param("scenario") === "block-sheet" ? (
        <Suspense fallback={null}>
          <BlockSheet />
        </Suspense>
      ) : (
        <BlockLabShell />
      )}
    </div>
  );
}
