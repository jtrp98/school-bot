import { t } from "../../i18n";
import type { Stage } from "../../modes/coding-adventure/LearnMap";

export const shortOf = (s: Stage) => t(s.short ?? s.t);
