import type { WorkspaceViewContext } from "./workspaceView";
import { buildJointsView } from "./jointsView";
import { buildTilesView } from "./tilesView";
import { buildRowsView } from "./rowsView";
import { buildHeaderFields } from "./workspaceHeaderFields";
import { buildActionFields } from "./workspaceActionFields";
import { buildDialogFields } from "./workspaceDialogFields";
import { buildMenuFields } from "./workspaceMenuFields";
import { buildToolbarFields } from "./workspaceToolbarFields";
import { buildPlaybackFields } from "./workspacePlaybackFields";
import { buildSequenceFields } from "./workspaceSequenceFields";
import { buildPlaybackBarView } from "./playbackBar";

export type WorkspaceFieldsContext = WorkspaceViewContext & {
 joints: ReturnType<typeof buildJointsView>;
 tiles: ReturnType<typeof buildTilesView>;
 rows: ReturnType<typeof buildRowsView>;
};

export function buildWorkspaceFields(context: WorkspaceViewContext) {
 const fieldsContext = { ...context, joints: buildJointsView(context), tiles: buildTilesView(context), rows: buildRowsView(context) };
 return {
  ...buildHeaderFields(fieldsContext),
  ...buildActionFields(fieldsContext),
  ...buildDialogFields(fieldsContext),
  ...buildMenuFields(fieldsContext),
  ...buildToolbarFields(fieldsContext),
  ...buildPlaybackFields(fieldsContext),
  ...buildSequenceFields(fieldsContext),
  playbackBar: buildPlaybackBarView(context),
  playbackActions: context.screen.playbackActions,
 };
}
