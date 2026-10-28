import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import th from "./locales/th.json";
import en from "./locales/en.json";
import zh from "./locales/zh.json";

export type Language = "th" | "en" | "zh";
export const languages: { value: Language; label: string }[] = [
  { value: "th", label: "ไทย" },
  { value: "en", label: "EN" },
  { value: "zh", label: "中文" },
];
export const isLanguage = (value: unknown): value is Language => value === "th" || value === "en" || value === "zh";

function initialLanguage(): Language {
  try {
    const saved = localStorage.getItem("school-bot:lang");
    if (isLanguage(saved)) return saved;
  } catch {
    // Embedded webviews can disable local storage; language switching must still work.
  }
  return "th";
}

void i18n.use(initReactI18next).init({
  resources: { th: { translation: th }, en: { translation: en }, zh: { translation: zh } },
  lng: initialLanguage(),
  supportedLngs: ["th", "en", "zh"],
  fallbackLng: "th",
  load: "languageOnly",
  initAsync: false,
  nsSeparator: false,
  interpolation: { escapeValue: false },
  react: { useSuspense: false },
});

i18n.on("languageChanged", (language: string) => {
  if (typeof document !== "undefined") document.documentElement.lang = language;
  try { localStorage.setItem("school-bot:lang", language); } catch { /* Storage is optional. */ }
});

export function t(key: string | null | undefined, values?: Record<string, unknown>): string {
  if (!key) return "";
  const trimmed = key.trim();
  if (!trimmed) return key;
  return key.slice(0, key.indexOf(trimmed)) + i18n.t(trimmed, values) + key.slice(key.indexOf(trimmed) + trimmed.length);
}
export const speechLanguage = (): string => ({ th: "th-TH", en: "en-US", zh: "zh-CN" })[i18n.language as Language] ?? "th-TH";
export default i18n;
