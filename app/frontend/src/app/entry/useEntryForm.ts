import { useEffect, useRef, useState } from "react";
import { allowsEntry } from "./types";
import type { EntryScreenProps } from "./EntryScreen";

export function useEntryForm(props: EntryScreenProps) {
  const { status, busy, loginAvailable } = props;
  const [key, setKey] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [activating, setActivating] = useState(false);
  const heading = useRef<HTMLHeadingElement>(null);
  const valid = allowsEntry(status);
  const checking = status.status === "checking";
  const unavailable = status.status === "unavailable";
  const firstRun = status.status === "unactivated" || activating;
  const validKey = key.length > 0 && key.length <= 256 && !/\s/.test(key);
  const disabled = busy || checking || unavailable;

  useEffect(() => {
    heading.current?.focus();
    setKey("");
    setPassword("");
    if (valid) setActivating(false);
  }, [status.status, valid]);

  async function activate() {
    if (disabled || !validKey) return;
    const submitted = key;
    setKey("");
    await props.onActivate(submitted);
  }
  async function login() {
    if (busy || !username.trim() || !password || !loginAvailable) return;
    const submitted = password;
    setPassword("");
    await props.onLogin(username, submitted);
  }

  return {
    key, setKey, username, setUsername, password, setPassword, setActivating, heading,
    valid, checking, unavailable, firstRun, validKey, disabled, activate, login,
  };
}
