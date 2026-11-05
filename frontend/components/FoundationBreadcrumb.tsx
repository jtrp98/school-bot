import Link from "next/link";

interface BreadcrumbItem {
  label: string;
  href?: string;
}

export default function FoundationBreadcrumb({
  items,
}: {
  items: BreadcrumbItem[];
}) {
  return (
    <nav aria-label="Breadcrumb" className="mb-4 flex flex-wrap items-center gap-2 text-sm">
      {items.map((item, index) => {
        const isLast = index === items.length - 1;

        return (
          <div key={`${item.label}-${index}`} className="flex items-center gap-2">
            {item.href && !isLast ? (
              <Link
                href={item.href}
                className="text-ink/55 transition-colors hover:text-sky-dark"
              >
                {item.label}
              </Link>
            ) : (
              <span className={isLast ? "font-medium text-ink" : "text-ink/55"}>
                {item.label}
              </span>
            )}
            {!isLast ? <span className="text-ink/25">›</span> : null}
          </div>
        );
      })}
    </nav>
  );
}
