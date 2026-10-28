import { useLayoutEffect, useRef, useState } from "react";

export function useLessonContentFit(contentKey?: string) {
  const viewportRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [scale, setScale] = useState(1);

  useLayoutEffect(() => {
    const viewport = viewportRef.current;
    const content = contentRef.current;
    if (!viewport || !content) return;

    function fitContent() {
      if (!viewport || !content || !content.offsetWidth || !content.offsetHeight) return;
      // Transforms leave these dimensions unchanged, so fitting never feeds back into measurement.
      setScale(Math.min(
        1,
        viewport.clientWidth / content.offsetWidth,
        viewport.clientHeight / content.offsetHeight,
      ));
    }

    const observer = new ResizeObserver(fitContent);
    observer.observe(viewport);
    observer.observe(content);
    fitContent();
    return () => observer.disconnect();
  }, [contentKey]);

  return { viewportRef, contentRef, scale };
}
