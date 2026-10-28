import type { ReactElement } from "react";
import { Trans } from "react-i18next";

type RichTextProps = {
  i18nKey: string;
  values?: Record<string, unknown>;
  components: Record<string, ReactElement>;
};

// Values are escaped and then unescaped after parsing so a user-typed name containing
// tag-like text cannot be mistaken for one of the sentence's own tags.
export function RichText({ i18nKey, values, components }: RichTextProps) {
  return (
    <Trans
      i18nKey={i18nKey}
      values={values}
      components={components}
      shouldUnescape
      tOptions={{ interpolation: { escapeValue: true } }}
    />
  );
}
