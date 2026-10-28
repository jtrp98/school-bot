import { t } from "../../i18n";
import { useState } from "react";
import { client } from "../../core/client";
import type { Command } from "../../core/types";

export function useCodeLab() {
  const [source, setSource] = useState(
    "hero.move_right(4)\nhero.jump()\nhero.move_right(3)",
  );
  const [commands, setCommands] = useState<Command[]>([]);
  const [runID, setRunID] = useState(0);
  const [busy, setBusy] = useState(false);
  const [steps, setSteps] = useState(0);
  const [status, setStatus] = useState(
    "common.messages.writeASequenceThenBringItToLife",
  );
  async function run() {
    setBusy(true);
    try {
      const response = await client.run("coding-adventure", "program.run", {
        source,
      });
      setCommands(response.data?.commands ?? []);
      setRunID((value) => value + 1);
      setSteps(response.data?.steps ?? 0);
      setStatus(
        "adventure.status.accepted",
      );
    } catch (error) {
      setStatus(String(error));
    } finally {
      setBusy(false);
    }
  }
  async function save() {
    try {
      await client.save("coding-adventure", { schema: 1, source });
      setStatus("adventure.status.saved");
    } catch (error) {
      setStatus(String(error));
    }
  }
  async function load() {
    try {
      const value = (await client.load("coding-adventure")) as {
        schema?: number;
        source?: string;
      } | null;
      if (!value) {
        setStatus("common.messages.noSavedProgramYet");
        return;
      }
      if (
        value.schema !== 1 ||
        typeof value.source !== "string" ||
        value.source.length > 16384
      )
        throw new Error("Invalid saved program");
      setSource(value.source);
      setStatus("common.messages.savedProgramLoaded");
    } catch (error) {
      setStatus(String(error));
    }
  }
  function reset() {
    setCommands([]);
    setRunID(value => value + 1);
  }

  return {
    source,
    setSource,
    commands,
    runID,
    busy,
    status: t(status, { count: steps }),
    run,
    save,
    load,
    reset,
  };
}
