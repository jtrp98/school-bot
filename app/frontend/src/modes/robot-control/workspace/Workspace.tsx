import { WorkspaceLogic } from "./WorkspaceLogic";
import { Template } from "./template";

export class Workspace extends WorkspaceLogic {
  render() {
    return <Template v={this.renderVals()} />;
  }
}
