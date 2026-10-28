import { AdventurePlayground } from "../../components/coding-adventure/AdventurePlayground";
import { useCodeLab } from "./useCodeLab";

export default function CodeLabMode() {
  return <AdventurePlayground {...useCodeLab()} />;
}
