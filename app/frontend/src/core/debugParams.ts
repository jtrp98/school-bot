export const param = (name: string): string | undefined =>
  new URLSearchParams(location.search).get(name) ?? undefined;
