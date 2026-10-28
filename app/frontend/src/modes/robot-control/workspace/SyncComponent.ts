import { Component } from "react";

// Handlers and timers read state before React processes its update queue, so updates must be visible immediately.
export class SyncComponent<P = object> extends Component<P, any> {

  declare state: any;

  override setState(patch: any, callback?: () => void): void {
    const next = typeof patch === "function" ? patch(this.state, this.props) : patch;
    if (next == null) return;
    this.state = { ...this.state, ...next };
    // React rebuilds instance.state from its queue, so it must receive the same merged state.
    super.setState(this.state, callback);
  }
}
