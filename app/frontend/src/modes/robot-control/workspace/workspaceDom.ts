type ElementRef = (element: HTMLElement | null) => void;

// Elements the workspace logic scrolls, focuses or measures. Components register them through callback refs so the
// logic does not depend on ids or selectors written in the markup.
export class WorkspaceDom {
  root: HTMLElement | null = null;
  cardsTrack: HTMLElement | null = null;
  tableScroll: HTMLElement | null = null;
  private readonly tiles = new Map<string, HTMLElement>();
  private readonly tileRefs = new Map<string, ElementRef>();
  private readonly jointCards = new Map<number, HTMLElement>();
  private readonly jointCardRefs = new Map<number, ElementRef>();
  private readonly cells = new Map<string, HTMLElement>();
  private readonly cellRefs = new Map<string, ElementRef>();

  readonly setRoot: ElementRef = (element) => {
    this.root = element;
  };

  readonly setCardsTrack: ElementRef = (element) => {
    this.cardsTrack = element;
  };

  readonly setTableScroll: ElementRef = (element) => {
    this.tableScroll = element;
  };

  // One stable ref per pose key: a new function every render would make React detach and re-attach the element.
  tileRef(key: string): ElementRef {
    const known = this.tileRefs.get(key);
    if (known) return known;
    const ref: ElementRef = (element) => {
      if (element) {
        this.tiles.set(key, element);
        return;
      }
      this.tiles.delete(key);
      this.tileRefs.delete(key);
    };
    this.tileRefs.set(key, ref);
    return ref;
  }

  jointCardRef(index: number): ElementRef {
    const known = this.jointCardRefs.get(index);
    if (known) return known;
    const ref: ElementRef = (element) => {
      if (element) this.jointCards.set(index, element);
      else this.jointCards.delete(index);
    };
    this.jointCardRefs.set(index, ref);
    return ref;
  }

  cellRef(poseKey: string, joint: number): ElementRef {
    const id = `${poseKey}:${joint}`;
    const known = this.cellRefs.get(id);
    if (known) return known;
    const ref: ElementRef = (element) => {
      if (element) {
        this.cells.set(id, element);
        return;
      }
      this.cells.delete(id);
      this.cellRefs.delete(id);
    };
    this.cellRefs.set(id, ref);
    return ref;
  }

  focusCell(poseKey: string, joint: number): void {
    const cell = this.cells.get(`${poseKey}:${joint}`) as HTMLInputElement | undefined;
    if (!cell) return;
    cell.focus();
    cell.select();
  }

  focusJointCard(index: number): void {
    this.jointCards.get(index)?.focus();
  }

  tile(key: string): HTMLElement | null {
    return this.tiles.get(key) ?? null;
  }

  tileInput(key: string): HTMLInputElement | null {
    const element = this.tile(key);
    return element && element.tagName === "INPUT" ? (element as HTMLInputElement) : null;
  }

  query<E extends HTMLElement = HTMLElement>(selector: string): E | null {
    return this.root ? this.root.querySelector<E>(selector) : null;
  }

  queryAll(selector: string): HTMLElement[] {
    return this.root ? Array.from(this.root.querySelectorAll<HTMLElement>(selector)) : [];
  }

  contains(container: HTMLElement | null, target: EventTarget | null): boolean {
    return !!container && !!target && container.contains(target as Node);
  }

  // Popup menus are positioned in workspace-root coordinates because they render inside the root.
  pointInRoot(clientX: number, clientY: number): { x: number; y: number } {
    if (!this.root) return { x: 0, y: 0 };
    const rect = this.root.getBoundingClientRect();
    return { x: clientX - rect.left, y: clientY - rect.top };
  }

  scrollTileIntoView(key: string, behavior: ScrollBehavior = "smooth"): void {
    this.tile(key)?.scrollIntoView({ behavior, block: "nearest", inline: "nearest" });
  }

  focusTileInput(key: string): void {
    const input = this.tileInput(key);
    if (!input) return;
    input.focus();
    input.select();
  }

  selectCardRenameInput(): void {
    this.cardsTrack?.querySelector<HTMLInputElement>("input.aw-edit")?.select();
  }

  scrollSequenceToEnd(): void {
    if (this.cardsTrack) this.cardsTrack.scrollLeft = this.cardsTrack.scrollWidth;
    if (this.tableScroll) this.tableScroll.scrollTop = this.tableScroll.scrollHeight;
  }
}
