import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { NumberedTips } from "./NumberedTips";
import type { ConnectScreenState } from "./useConnectScreen";

type SearchView = { mark: string; warn: boolean; title: string; body: string };

// The blue step box of "find ours by unplugging"; it turns yellow when the child has to act
// differently (two cards vanished, or the board has not come back yet).
export function UnplugSearchBox({ screen }: { screen: ConnectScreenState }) {
  const { t } = useTranslation();
  const search = screen.search.state;
  if (!search) return null;
  let view: SearchView;
  if (search.step === "unplug") {
    view = {
      mark: "1",
      warn: false,
      title: t("blockLab.connect.search.unplug.title"),
      body: t("blockLab.connect.search.unplug.body"),
    };
  } else if (search.step === "plugBack" && screen.search.slow) {
    view = {
      mark: "?",
      warn: true,
      title: t("blockLab.connect.search.slow.title"),
      body: t("blockLab.connect.search.slow.body"),
    };
  } else if (search.step === "plugBack") {
    view = {
      mark: "2",
      warn: false,
      title: t("blockLab.connect.search.plugBack.title"),
      body: t("blockLab.connect.search.plugBack.body"),
    };
  } else if (search.step === "tooMany") {
    view = {
      mark: "!",
      warn: true,
      title: t("blockLab.connect.search.tooMany.title", { cardCount: search.vanished }),
      body: t("blockLab.connect.search.tooMany.body"),
    };
  } else {
    view = {
      mark: "3",
      warn: false,
      title: search.portChanged
        ? t("blockLab.connect.search.found.titleMoved", { port: search.port })
        : t("blockLab.connect.search.found.title"),
      body: t("blockLab.connect.search.found.body"),
    };
  }
  const slowTips = [
    t("blockLab.connect.cableTips.pushIn"),
    t("blockLab.connect.cableTips.otherSocketRescan"),
    t("blockLab.connect.cableTips.light"),
  ];
  return (
    <>
      <div
        role="status"
        className={[
          "flex items-center gap-[16px] rounded-[16px] border-2 border-solid px-[20px] py-[12px]",
          view.warn ? "border-[var(--warn)] bg-[var(--warn-tint)]" : "border-blue bg-blue-tint",
        ].join(" ")}
      >
        <span
          className={[
            "box-border flex h-[36px] w-[36px] shrink-0 items-center justify-center rounded-full text-[18px] font-bold",
            view.warn ? "border-2 border-solid border-[var(--warn)] bg-surface text-ink" : "bg-blue text-white",
          ].join(" ")}
        >
          {view.mark}
        </span>
        <div className="flex flex-1 flex-col">
          <span className="text-[18px] font-bold">{view.title}</span>
          <span className="text-[16px]">{view.body}</span>
        </div>
        {search.step === "tooMany" && (
          <Button variant="secondary" size="md" onClick={screen.startSearch}>
            {t("common.messages.startOver")}
          </Button>
        )}
        {search.step === "plugBack" && screen.search.slow && (
          <Button variant="secondary" size="md" aria-expanded={screen.search.tips} onClick={screen.search.toggleTips}>
            {t(screen.search.tips ? "blockLab.connect.tips.hide" : "blockLab.connect.tips.show")}
          </Button>
        )}
        <Button variant="secondary" size="md" onClick={screen.closeSearch}>
          {t(search.step === "found" ? "common.messages.close" : "common.cancel")}
        </Button>
      </div>
      {search.step === "plugBack" && screen.search.slow && screen.search.tips && (
        <NumberedTips tips={slowTips} look="card" />
      )}
    </>
  );
}
