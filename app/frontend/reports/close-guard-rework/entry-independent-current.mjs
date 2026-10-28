import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createHash } from "node:crypto";
import { JSDOM } from "jsdom";
import { createServer } from "vite";

const dom = new JSDOM("<!doctype html><html><body></body></html>", {
  url: "http://localhost",
  pretendToBeVisual: true,
});
for (const key of ["window", "document", "HTMLElement", "Element", "Node", "Event",
  "MouseEvent", "KeyboardEvent", "SubmitEvent", "location", "history", "localStorage"]) {
  globalThis[key] = dom.window[key];
}
globalThis.IS_REACT_ACT_ENVIRONMENT = true;
globalThis.ResizeObserver = class {
  observe() {}
  disconnect() {}
};
const clientCalls = [];
let fullscreen = true;
globalThis.__entryCheckClient = new Proxy({
  info: async () => ({ platform: "isolated-fixture", version: "0.0.0" }),
  onCloseRequested: () => () => {},
  isFullscreen: async () => fullscreen,
  setFullscreen: async value => {
    clientCalls.push(["fullscreen", value]);
    fullscreen = value;
  },
  minimizeWindow: async () => {
    clientCalls.push(["minimize"]);
  },
  cancel: async () => {
    clientCalls.push(["cancel"]);
  },
}, {
  get(target, key) {
    if (key in target) return target[key];
    return async () => {
      throw Error("Unexpected host call: " + String(key));
    };
  },
});
const React = await import("react");
const { createRoot } = await import("react-dom/client");
const { act, createElement: h } = React;
const server = await createServer({
  configFile: false,
  root: process.cwd(),
  optimizeDeps: { noDiscovery: true, include: [] },
  server: { middlewareMode: true, hmr: false, watch: null },
  appType: "custom",
  plugins: [{
    name: "independent-entry-check",
    enforce: "pre",
    load(id) {
      const path = id.replaceAll("\\", "/");
      if (path.endsWith("/src/core/client.ts")) {
        return "export const client = globalThis.__entryCheckClient; export const preview = false;";
      }
      if (/\/src\/modes\/(robot-control\/RobotMode|coding-adventure\/(LessonsMode|CodeLabMode)|block-programming\/BlockMode)\.tsx$/.test(path)) {
        return "import React from 'react'; export default function Fixture(){return React.createElement('div',{'data-lab-fixture':'true'},'isolated Lab fixture');}";
      }
    },
  }],
});
let mounted;
let group = "shared";
let passed = 0;
let failed = 0;
const results = [];
const errors = [];
const originalError = console.error;
console.error = (...args) => {
  errors.push(args.map(String).join(" "));
};
const expiry = "2026-10-28T10:00:00Z";
const valid = { status: "valid", expiresAt: expiry };
const offline = { status: "offline-valid", expiresAt: expiry };
const deferred = () => {
  let resolve, reject;
  const promise = new Promise((yes, no) => {
    resolve = yes;
    reject = no;
  });
  return { promise, resolve, reject };
};
function host(status = valid, overrides = {}) {
  let listener;
  const calls = [];
  const adapter = {
    readStatus: async () => status,
    subscribe: callback => {
      listener = callback;
      return () => {
        listener = undefined;
      };
    },
    activate: async key => {
      calls.push(["activate", key]);
    },
    validate: async () => {
      calls.push(["validate"]);
    },
    guest: async () => {
      calls.push(["guest"]);
    },
    ...overrides,
  };
  return { adapter, calls, emit: next => listener?.(next) };
}
async function cleanup() {
  if (mounted) {
    await act(async () => mounted.unmount());
    mounted = undefined;
  }
  document.body.replaceChildren();
}
async function render(element) {
  await cleanup();
  const container = document.createElement("div");
  document.body.append(container);
  mounted = createRoot(container);
  await act(async () => {
    mounted.render(element);
  });
  return container;
}
async function run(name, test) {
  try {
    await cleanup();
    await test();
    passed++;
    results.push({ group, name, result: "PASS" });
  } catch (error) {
    failed++;
    results.push({ group, name, result: "FAIL", actual: error.message });
    console.log("FAIL", name, error.message);
  } finally {
    await cleanup();
  }
}
const input = async (selector, value) => {
  const node = document.querySelector(selector);
  assert.ok(node, selector);
  await act(async () => {
    Object.getOwnPropertyDescriptor(
      dom.window.HTMLInputElement.prototype,
      "value",
    ).set.call(node, value);
    node.dispatchEvent(new Event("input", { bubbles: true }));
  });
};
const click = async node => {
  assert.ok(node, "button exists");
  await act(async () => {
    node.click();
  });
};
const button = key => [...document.querySelectorAll("button")]
  .find(node => node.textContent === i18n.t(key));
const submit = async selector => {
  await act(async () => document.querySelector(selector).dispatchEvent(
    new SubmitEvent("submit", { bubbles: true, cancelable: true }),
  ));
};
let i18n;
try {
  ({ default: i18n } = await server.ssrLoadModule("/src/i18n/index.ts"));
  const { useEntry } = await server.ssrLoadModule("/src/app/entry/useEntry.ts");
  const { unavailableEntryAdapter } =
    await server.ssrLoadModule("/src/app/entry/types.ts");
  const { EntryScreen } =
    await server.ssrLoadModule("/src/app/entry/EntryScreen.tsx");
  const { useAppScreen } = await server.ssrLoadModule("/src/App.tsx");
  const { AppView } = await server.ssrLoadModule("/src/app/AppView.tsx");
  const { registerWindowCloseGuard, prepareWindowClose } =
    await server.ssrLoadModule("/src/core/windowLifecycle.ts");

  async function entryFixture(a) {
    let entry;
    function Fixture() {
      entry = useEntry(a);
      return h(EntryScreen, {
        status: entry.status,
        busy: entry.busy,
        recovering: entry.recovering,
        error: entry.error,
        loginAvailable: !!a.login,
        offlineAvailable: !!a.useOffline,
        onActivate: entry.activate,
        onValidate: entry.validate,
        onOffline: entry.useOffline,
        onGuest: entry.guest,
        onLogin: entry.login,
        onRetryRecovery: entry.retryRecovery,
      });
    }
    await render(h(Fixture));
    return () => entry;
  }

  async function appFixture(a = unavailableEntryAdapter, actions = {
    requestStop: async () => {},
    close: async () => {},
  }) {
    let screen;
    function Fixture() {
      screen = useAppScreen(a, actions);
      return h(AppView, { v: screen });
    }
    await render(h(Fixture));
    return () => screen;
  }

  const shellHidden = () => {
    const shell = document.querySelector(".app-shell");
    return !shell || (
      shell.hasAttribute("inert") && shell.classList.contains("hidden")
    );
  };

  for (const path of [
    "/src/modes/robot-control/RobotMode.tsx",
    "/src/modes/coding-adventure/CodeLabMode.tsx",
    "/src/modes/block-programming/BlockMode.tsx",
  ]) {
    await server.ssrLoadModule(path);
  }
  const settleLab = async () => {
    for (
      let i = 0;
      i < 100 && !document.querySelector("[data-lab-fixture]");
      i++
    ) {
      await act(async () => {
        await new Promise(resolve => setImmediate(resolve));
      });
    }
  };

  const manifest = readFileSync("reports/entry-files.sha256");
  await run("manifest ผูก source ล่าสุดครบ 15 ไฟล์", async () => {
    const lines = manifest.toString().trim().split(/\r?\n/);
    assert.equal(lines.length, 15);
    for (const line of lines) {
      const [hash, path] = line.split(/  /);
      assert.equal(
        createHash("sha256").update(readFileSync(path)).digest("hex"),
        hash,
        path,
      );
    }
    assert.equal(
      createHash("sha256").update(manifest).digest("hex"),
      "ab984fe746ba8b671b702ba4cc01627d81cb56bdf2ab701add3d061235dbaf4b",
    );
  });

  group = "entry";
  for (const state of [
    "unactivated", "checking", "expired", "denied",
    "clock_untrusted", "locked", "unavailable",
  ]) {
    await run(state + " ปฏิเสธ Guest/Login และซ่อน shell ทั้งสาม Labs", async () => {
      const a = host({ status: state, expiresAt: expiry }, {
        login: async () => {
          a.calls.push(["login"]);
          return "success";
        },
      });
      const get = await appFixture(a.adapter);
      await act(async () => {
        await get().entry.guest();
        await get().entry.login("u", "p");
      });
      assert.equal(get().entry.entered, false);
      assert.equal(a.calls.length, 0);
      assert.ok(shellHidden());
      assert.equal(document.querySelectorAll("[data-lab-fixture]").length, 0);
    });
  }

  await run("missing adapter fail closed", async () => {
    const get = await appFixture();
    assert.equal(get().entry.status.status, "unavailable");
    await act(async () => get().entry.guest());
    assert.ok(shellHidden());
  });

  for (const badExpiry of [undefined, "", "invalid-date"]) {
    await run("valid ไม่มี expiry ที่อ่านได้ไม่เปิด E2: " + String(badExpiry), async () => {
      const get = await entryFixture(
        host({ status: "valid", expiresAt: badExpiry }).adapter,
      );
      await act(async () => get().guest());
      assert.equal(get().entered, false);
    });
  }

  for (const key of ["", " ", "A B", "A\nB", "A".repeat(257)]) {
    await run("Key ผิดขอบเขตไม่เรียก host: " + JSON.stringify(key).slice(0, 28), async () => {
      const a = host({ status: "unactivated" });
      const get = await entryFixture(a.adapter);
      await act(async () => get().activate(key));
      assert.equal(a.calls.length, 0);
    });
  }

  for (const size of [1, 256]) {
    await run("Key ยาว " + size + " ส่ง host และล้างช่องทันที", async () => {
      const a = host({ status: "unactivated" });
      await entryFixture(a.adapter);
      await input("#entry-license-key", "K".repeat(size));
      await submit("form");
      assert.deepEqual(a.calls, [["activate", "K".repeat(size)]]);
      assert.equal(document.querySelector("#entry-license-key").value, "");
    });
  }

  await run("activation สำเร็จเฉพาะ callback ไม่ปลดล็อกเอง", async () => {
    const a = host({ status: "unactivated" });
    const get = await entryFixture(a.adapter);
    await act(async () => get().activate("K"));
    assert.equal(get().status.status, "unactivated");
    assert.equal(get().entered, false);
  });

  await run("activation loading กัน duplicate และ retry ได้หลัง failure", async () => {
    const wait = deferred();
    let attempts = 0;
    const a = host({ status: "unactivated" }, {
      activate: async () => {
        attempts++;
        await wait.promise;
      },
    });
    const get = await entryFixture(a.adapter);
    let pending;
    await act(async () => {
      pending = get().activate("K");
      void get().activate("K");
    });
    assert.equal(attempts, 1);
    assert.equal(get().busy, true);
    await act(async () => {
      wait.reject(Error("fixture failure"));
      await pending;
    });
    assert.equal(get().error, "operation_failed");
    await act(async () => get().activate("K"));
    assert.equal(attempts, 2);
    assert.equal(get().entered, false);
  });

  await run("E1 empty/maxLength/heading focus", async () => {
    await entryFixture(host({ status: "unactivated" }).adapter);
    assert.equal(document.querySelector("input").maxLength, 256);
    assert.equal(button("entry.activation.submit").disabled, true);
    assert.equal(
      button("entry.activation.submit").getAttribute("aria-disabled"),
      "true",
    );
    assert.equal(document.activeElement, document.querySelector("h1"));
  });

  for (const state of [valid, offline]) {
    await run(state.status + " ต้องเลือก E2 ก่อนและ Guest จึงเปิด shell", async () => {
      const a = host(state);
      const get = await appFixture(a.adapter);
      assert.ok(shellHidden());
      await click(button("entry.choice.guest"));
      assert.equal(get().entry.entered, true);
      assert.equal(document.querySelector(".app-shell").hasAttribute("inert"), false);
      assert.ok(document.body.textContent.includes(i18n.t("entry.choice.guestTitle")));
    });
  }

  await run("เปิดใหม่ถาม E2 ซ้ำไม่ auto-login", async () => {
    const a = host();
    let get = await appFixture(a.adapter);
    await click(button("entry.choice.guest"));
    get = await appFixture(a.adapter);
    assert.equal(get().entry.entered, false);
    assert.ok(button("entry.choice.guest"));
  });

  await run("E2 แสดงพื้นที่ร่วม ไม่มี PIN และไม่มีช่อง PIN", async () => {
    await entryFixture(host().adapter);
    assert.ok(document.body.textContent.includes(i18n.t("entry.choice.guestDescription")));
    assert.equal(document.querySelectorAll("input").length, 2);
    assert.equal(document.querySelectorAll('[id*="pin"]').length, 0);
  });

  await run("Login unavailable ไม่จำลอง success", async () => {
    const get = await entryFixture(host().adapter);
    assert.equal(document.querySelector("#entry-password").disabled, true);
    await act(async () => get().login("u", "p"));
    assert.equal(get().error, "unavailable");
    assert.equal(get().entered, false);
  });

  for (const result of ["invalid_credentials", "network", "unavailable", "success"]) {
    await run("Login " + result + " ล้าง password คง username", async () => {
      const a = host(valid, { login: async () => result });
      const get = await entryFixture(a.adapter);
      await input("#entry-username", "student");
      await input("#entry-password", "secret-fixture-only");
      await submit("form");
      assert.equal(document.querySelector("#entry-password").value, "");
      assert.equal(document.querySelector("#entry-username").value, "student");
      assert.equal(get().entered, result === "success");
      if (result !== "success") assert.equal(get().error, result);
      assert.ok(!JSON.stringify(localStorage).includes("secret-fixture-only"));
    });
  }

  await run("Login loading กัน submit ซ้ำ", async () => {
    const wait = deferred();
    let calls = 0;
    const a = host(valid, {
      login: async () => {
        calls++;
        return wait.promise;
      },
    });
    const get = await entryFixture(a.adapter);
    await input("#entry-username", "u");
    await input("#entry-password", "p");
    await submit("form");
    await submit("form");
    assert.equal(calls, 1);
    assert.equal(get().busy, true);
    assert.equal(document.querySelector("#entry-password").value, "");
    await act(async () => wait.resolve("invalid_credentials"));
  });

  await run("ผล Login เก่าหลัง denied ไม่เปิด shell", async () => {
    const wait = deferred();
    const a = host(valid, { login: async () => wait.promise });
    const get = await entryFixture(a.adapter);
    await act(async () => {
      void get().login("u", "p");
    });
    await act(async () => a.emit({ status: "denied", expiresAt: expiry }));
    await act(async () => wait.resolve("success"));
    assert.equal(get().entered, false);
  });

  await run("subscription denial ชนะ stale initial readStatus", async () => {
    const wait = deferred();
    const a = host(valid, { readStatus: () => wait.promise });
    const get = await entryFixture(a.adapter);
    await act(async () => a.emit({ status: "denied" }));
    await act(async () => wait.resolve(valid));
    assert.equal(get().status.status, "denied");
  });

  await run("readStatus failure fail closed", async () => {
    const get = await entryFixture(host(valid, {
      readStatus: async () => {
        throw Error("offline");
      },
    }).adapter);
    assert.equal(get().status.status, "unavailable");
    assert.equal(get().entered, false);
  });

  await run("offline host expiry คงเดิมหลังเข้า Guest/validate/remount", async () => {
    const a = host(offline);
    let get = await appFixture(a.adapter);
    await click(button("entry.choice.guest"));
    await act(async () => get().entry.validate());
    assert.equal(get().entry.status.expiresAt, expiry);
    get = await appFixture(a.adapter);
    assert.equal(get().entry.status.expiresAt, expiry);
    assert.equal(get().entry.entered, false);
  });

  for (const state of ["expired", "denied", "clock_untrusted", "locked"]) {
    await run("ระหว่างใช้เปลี่ยนเป็น " + state + " ล็อก shell และขอ recovery", async () => {
      let recovery = 0;
      const a = host(valid, {
        saveAndRequestStop: async () => {
          recovery++;
        },
      });
      const get = await appFixture(a.adapter);
      await click(button("entry.choice.guest"));
      await act(async () => a.emit({ status: state, expiresAt: expiry }));
      assert.equal(get().entry.entered, false);
      assert.equal(recovery, 1);
      assert.ok(shellHidden());
      assert.ok(document.querySelector('[role="alert"]'));
    });
  }

  await run("recovery ล้มแสดง error และ retry ผ่าน callback จริง", async () => {
    let attempts = 0;
    const a = host(valid, {
      saveAndRequestStop: async () => {
        if (++attempts === 1) throw Error("stop failed");
      },
    });
    const get = await appFixture(a.adapter);
    await click(button("entry.choice.guest"));
    await act(async () => a.emit({ status: "expired" }));
    assert.equal(get().entry.error, "recovery_failed");
    await click(button("entry.recovery.retry"));
    assert.equal(attempts, 2);
    assert.equal(get().entry.entered, false);
  });

  await run("offline bypass เฉพาะ checking และ host ระบุพร้อม", async () => {
    let calls = 0;
    const a = host({
      status: "checking",
      canUseOffline: true,
      expiresAt: expiry,
    }, {
      useOffline: async () => {
        calls++;
      },
    });
    const get = await entryFixture(a.adapter);
    await click(button("entry.lock.useOffline"));
    assert.equal(calls, 1);
    assert.equal(get().entered, false);
    await act(async () => a.emit({
      status: "locked",
      canUseOffline: true,
      expiresAt: expiry,
    }));
    await act(async () => get().useOffline());
    assert.equal(calls, 1);
    assert.equal(button("entry.lock.useOffline"), undefined);
  });

  await run("host expiry/status เท่านั้น ไม่เผย token/key/device identity ใน DOM", async () => {
    const a = host({
      ...offline,
      entitlement: "secret-token",
      key: "secret-key",
      binding_id: "secret-binding",
    });
    await appFixture(a.adapter);
    await click(button("entry.choice.guest"));
    for (const value of ["secret-token", "secret-key", "secret-binding"]) {
      assert.ok(!document.body.innerHTML.includes(value));
      assert.ok(!JSON.stringify(localStorage).includes(value));
    }
    assert.ok(document.querySelector('[aria-live="polite"]'));
  });

  for (const language of ["th", "en", "zh"]) {
    await run(language + " E1/E2/E3/error ไม่มี untranslated key/interpolation", async () => {
      await act(async () => i18n.changeLanguage(language));
      for (const status of [
        { status: "unactivated" },
        { ...offline, expiringSoon: true },
        { status: "locked", error: "UNKNOWN_FORBIDDEN" },
      ]) {
        await entryFixture(host(status).adapter);
        assert.ok(!/entry\.[A-Za-z]|\{\{/.test(document.body.textContent));
        for (const field of document.querySelectorAll("input")) {
          assert.ok(document.querySelector('label[for="' + field.id + '"]'));
        }
      }
      assert.equal(localStorage.getItem("school-bot:lang"), language);
    });
  }
  await act(async () => i18n.changeLanguage("th"));

  for (const error of [
    "network", "DEVICE_LIMIT", "DEVICE_NOT_ACTIVATED", "DEVICE_PROOF_INVALID",
    "LICENSE_INVALID", "LICENSE_INACTIVE", "BINDING_REVOKED", "INVALID_ENTITLEMENT",
    "UNKNOWN_FORBIDDEN", "CREDENTIAL_STORE_FAILED", "CHALLENGE_EXPIRED",
  ]) {
    await run(error + " แสดง alert และกรอก Key ใหม่ได้", async () => {
      const get = await entryFixture(host({ status: "locked", error }).adapter);
      assert.ok([...document.querySelectorAll('[role="alert"]')].some(node =>
        node.textContent.includes(i18n.t(
          error === "network"
            ? "entry.errors.onlineCheckFailed"
            : "entry.errors." + error,
        )),
      ));
      await click(button("entry.lock.reactivate"));
      assert.ok(document.querySelector("#entry-license-key"));
      assert.equal(get().entered, false);
    });
  }

  await run("ชื่อบัญชีแยกจาก status License", async () => {
    const get = await appFixture(host(valid, {
      login: async () => "success",
    }).adapter);
    await act(async () => get().entry.login("student-name", "p"));
    const license = document.querySelector('[aria-live="polite"]');
    assert.ok(license);
    assert.ok(!license.textContent.includes("student-name"));
    assert.ok(document.body.textContent.includes(
      i18n.t("entry.choice.account", { username: "student-name" }),
    ));
  });

  group = "shell";
  await run("สลับสาม Labs จาก shell ผ่าน actual navigation seam", async () => {
    const get = await appFixture(host().adapter);
    await click(button("entry.choice.guest"));
    for (const view of ["code-lab", "block-programming", "robot-control"]) {
      await act(async () => get().go(
        view,
        view === "robot-control" ? "editor" : undefined,
        true,
      ));
      assert.equal(get().view, view);
      await settleLab();
      assert.ok(document.querySelector("[data-lab-fixture]"), view);
    }
  });

  await run("เปลี่ยน mode แล้วยกเลิกคงหน้าเดิม", async () => {
    const get = await appFixture(host().adapter);
    await click(button("entry.choice.guest"));
    await act(async () => get().go("code-lab", undefined, true));
    await act(async () => get().go("block-programming"));
    assert.equal(get().view, "code-lab");
    assert.ok(document.querySelector('[role="dialog"]'));
    await click(button("modal.leave.stay"));
    assert.equal(get().view, "code-lab");
    assert.equal(get().pending, undefined);
  });

  await run("สลับภาษาไม่ remount shell และคง mode", async () => {
    const get = await appFixture(host().adapter);
    await click(button("entry.choice.guest"));
    await act(async () => get().go("code-lab", undefined, true));
    const shell = document.querySelector(".app-shell");
    await settleLab();
    const lab = document.querySelector("[data-lab-fixture]");
    assert.ok(lab);
    for (const language of ["en", "zh", "th"]) {
      await act(async () => get().shell.setLang(language));
      assert.equal(document.querySelector(".app-shell"), shell);
      assert.equal(document.querySelector("[data-lab-fixture]"), lab);
      assert.equal(get().view, "code-lab");
    }
  });

  for (const phase of ["saving", "stopping", "closing"]) {
    await run(phase + " ล้มเหลวไม่ข้ามขั้น/หน้าต่างยังเปิดและ retry ได้", async () => {
      const calls = [];
      let fail = true;
      const action = name => async () => {
        calls.push(name);
        if (fail && name === phase) throw Error("fixture " + name);
      };
      const get = await appFixture(host().adapter, {
        save: action("saving"),
        requestStop: action("stopping"),
        close: action("closing"),
      });
      await act(async () => get().setCloseRequested(true));
      await click(button("entry.close.save"));
      assert.equal(get().closeRequested, true);
      assert.equal(get().closeError, "entry.close.errors." + phase);
      assert.deepEqual(
        calls,
        ["saving", "stopping", "closing"].slice(
          0,
          ["saving", "stopping", "closing"].indexOf(phase) + 1,
        ),
      );
      assert.ok(document.querySelector('[role="alert"]').textContent.includes(
        i18n.t(get().closeError),
      ));
      fail = false;
      calls.length = 0;
      await click(button("entry.close.retry"));
      assert.deepEqual(calls, ["saving", "stopping", "closing"]);
      assert.equal(get().closeRequested, false);
    });
  }

  await run("close cancel ไม่เรียก save/stop/close และคืน focus", async () => {
    let calls = 0;
    const get = await appFixture(host().adapter, {
      save: async () => {
        calls++;
      },
      requestStop: async () => {
        calls++;
      },
      close: async () => {
        calls++;
      },
    });
    const trigger = document.querySelector("button.window-caption-close");
    trigger.focus();
    await act(async () => get().setCloseRequested(true));
    assert.ok(document.querySelector('[role="dialog"]').contains(document.activeElement));
    await click(button("entry.close.cancel"));
    assert.equal(get().closeRequested, false);
    assert.equal(calls, 0);
    assert.equal(document.activeElement, trigger);
  });

  await run("close Escape ยกเลิกผ่าน actual App lifecycle", async () => {
    const get = await appFixture(host().adapter);
    await act(async () => get().setCloseRequested(true));
    await act(async () => window.dispatchEvent(new KeyboardEvent("keydown", {
      key: "Escape",
      bubbles: true,
      cancelable: true,
    })));
    assert.equal(get().closeRequested, false);
  });

  await run("close Tab/ShiftTab handlers วน focus ใน dialog", async () => {
    const get = await appFixture(host().adapter, {
      save: async () => {},
      requestStop: async () => {},
      close: async () => {},
    });
    await act(async () => get().setCloseRequested(true));
    const buttons = [...document.querySelector('[role="dialog"]')
      .querySelectorAll("button:not(:disabled)")];
    buttons.at(-1).focus();
    await act(async () => buttons.at(-1).dispatchEvent(new KeyboardEvent("keydown", {
      key: "Tab",
      bubbles: true,
      cancelable: true,
    })));
    assert.equal(document.activeElement, buttons[0]);
    await act(async () => buttons[0].dispatchEvent(new KeyboardEvent("keydown", {
      key: "Tab",
      shiftKey: true,
      bubbles: true,
      cancelable: true,
    })));
    assert.equal(document.activeElement, buttons.at(-1));
  });

  await run("close loading กัน duplicate และ Escape ไม่ปิด dialog", async () => {
    const wait = deferred();
    let stops = 0, closes = 0;
    const get = await appFixture(host().adapter, {
      requestStop: async () => {
        stops++;
        await wait.promise;
      },
      close: async () => {
        closes++;
      },
    });
    await act(async () => get().setCloseRequested(true));
    await click(button("entry.close.withoutSave"));
    await act(async () => {
      void get().closeWindow();
      window.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape" }));
    });
    assert.equal(stops, 1);
    assert.equal(closes, 0);
    assert.equal(get().closeRequested, true);
    assert.ok(document.querySelector('[role="status"][aria-live="polite"]'));
    assert.ok(!document.body.textContent.includes("หุ่นหยุดแล้ว"));
    await act(async () => wait.reject(Error("stop failed")));
  });

  await run("ไม่มี save callback ปิดปุ่ม save และเรียกตรงไม่ข้ามไป stop", async () => {
    let stops = 0;
    const get = await appFixture(host().adapter, {
      requestStop: async () => {
        stops++;
      },
      close: async () => {},
    });
    await act(async () => get().setCloseRequested(true));
    assert.equal(button("entry.close.save").disabled, true);
    await act(async () => get().closeWindow(true));
    assert.equal(stops, 0);
    assert.equal(get().closeRequested, true);
    assert.equal(get().closeError, "entry.close.errors.saving");
  });

  await run("close guard lifecycle เรียกครบก่อนรอและ failure คงหน้าต่าง", async () => {
    const calls = [];
    const wait = deferred();
    let closes = 0;
    const offA = registerWindowCloseGuard(() => {
      calls.push("a");
      return wait.promise;
    });
    const offB = registerWindowCloseGuard(() => {
      calls.push("b");
      throw Error("guard failed");
    });
    try {
      const get = await appFixture(host().adapter, {
        requestStop: prepareWindowClose,
        close: async () => {
          closes++;
        },
      });
      await act(async () => get().setCloseRequested(true));
      await click(button("entry.close.withoutSave"));
      assert.deepEqual(calls, ["a", "b"]);
      assert.equal(closes, 0);
      await act(async () => wait.resolve());
      assert.equal(get().closeRequested, true);
      assert.equal(get().closeError, "entry.close.errors.stopping");
      offA();
      offB();
      await click(button("entry.close.retry"));
      assert.equal(closes, 1);
    } finally {
      offA();
      offB();
    }
  });

  await run("window controls ใช้ fake callbacks พร้อม aria-label ครบ", async () => {
    clientCalls.length = 0;
    await appFixture(host().adapter);
    const controls = [...document.querySelectorAll(".window-caption-controls button")];
    assert.equal(controls.length, 3);
    for (const control of controls) assert.ok(control.getAttribute("aria-label"));
    await click(controls[0]);
    await click(controls[1]);
    assert.ok(clientCalls.some(([name]) => name === "minimize"));
    assert.ok(clientCalls.some(([name]) => name === "fullscreen"));
  });

  await run("close guard แสดงสาเหตุที่รับมา ตาม signed §7", async () => {
    const reason = "อุปกรณ์ทดสอบไม่ตอบสนอง";
    const off = registerWindowCloseGuard(() => {
      throw Error(reason);
    });
    try {
      const get = await appFixture(host().adapter, {
        requestStop: prepareWindowClose,
        close: async () => {},
      });
      await act(async () => get().setCloseRequested(true));
      await click(button("entry.close.withoutSave"));
      const actual = document.querySelector('[role="alert"]')?.textContent;
      console.log(JSON.stringify({
        case: "close-guard-reason",
        expected: reason,
        actual,
      }));
      assert.ok(actual?.includes(reason), "ข้อความไม่แสดงสาเหตุจาก close guard");
    } finally {
      off();
    }
  });

  group = "entry";
  await run("unmount ถอด subscription และผล Login ค้างไม่กระทบ instance ใหม่", async () => {
    const wait = deferred();
    let subscribed = 0;
    const a = host(valid, {
      subscribe: () => {
        subscribed++;
        return () => {
          subscribed--;
        };
      },
      login: async () => wait.promise,
    });
    let get = await entryFixture(a.adapter);
    assert.equal(subscribed, 1);
    await act(async () => {
      void get().login("old-account", "old-password");
    });
    await cleanup();
    assert.equal(subscribed, 0);
    get = await entryFixture(host({ status: "unactivated" }).adapter);
    await act(async () => wait.resolve("success"));
    assert.equal(get().entered, false);
  });

  await run("React StrictMode ยังคง gate และ cleanup subscription", async () => {
    let subscribed = 0;
    const a = host(valid, {
      subscribe: () => {
        subscribed++;
        return () => {
          subscribed--;
        };
      },
    });
    await render(h(React.StrictMode, null, h(function Fixture() {
      const entry = useEntry(a.adapter);
      return h("output", null, entry.entered ? "entered" : "choice");
    })));
    assert.equal(document.querySelector("output").textContent, "choice");
    assert.equal(subscribed, 1);
    await cleanup();
    assert.equal(subscribed, 0);
  });

  await run("valid expiry เป็น host presentation ไม่คำนวณ renew เองจากเวลาของ UI", async () => {
    const get = await entryFixture(host({
      status: "valid",
      expiresAt: "2000-01-01T00:00:00Z",
    }).adapter);
    assert.equal(get().status.expiresAt, "2000-01-01T00:00:00Z");
  });

  await run("Key ไม่ถูก persist หลัง activation ล้มเหลว", async () => {
    const a = host({ status: "unactivated" }, {
      activate: async () => {
        throw Error("fixture");
      },
    });
    const get = await entryFixture(a.adapter);
    await input("#entry-license-key", "key-fixture-only");
    await submit("form");
    assert.equal(document.querySelector("#entry-license-key").value, "");
    assert.equal(get().entered, false);
    assert.ok(!JSON.stringify(localStorage).includes("key-fixture-only"));
  });

  await run("expiry เด่นและ aria-live เมื่อ host แจ้ง expiringSoon", async () => {
    await entryFixture(host({ ...offline, expiringSoon: true }).adapter);
    const strong = document.querySelector('[aria-live="polite"] strong');
    assert.equal(strong?.textContent, i18n.t("entry.license.expiringSoon"));
  });

  group = "shared";
  await run("entry keys และ interpolation parameters ตรงครบสามภาษา", async () => {
    function flatten(value, prefix = "", out = {}) {
      for (const [key, child] of Object.entries(value)) {
        const path = prefix ? prefix + "." + key : key;
        if (typeof child === "string") out[path] = child;
        else flatten(child, path, out);
      }
      return out;
    }
    const locales = ["th", "en", "zh"].map(lang =>
      flatten(JSON.parse(readFileSync(
        "src/i18n/locales/" + lang + ".json",
        "utf8",
      )).entry),
    );
    const keys = Object.keys(locales[0]).sort();
    for (const locale of locales) {
      assert.deepEqual(Object.keys(locale).sort(), keys);
      for (const key of keys) {
        const params = text => [...text.matchAll(/\{\{\s*([^} ]+)\s*\}\}/g)]
          .map(match => match[1]).sort();
        assert.ok(locale[key].length > 0, key);
        assert.deepEqual(params(locale[key]), params(locales[0][key]), key);
      }
    }
    console.log(JSON.stringify({ entryKeys: keys.length, locales: 3 }));
  });

  group = "entry";
  await run("E1 Activate รอ host status ก่อนเปลี่ยน E2", async () => {
    const a = host({ status: "unactivated" });
    const get = await entryFixture(a.adapter);
    await input("#entry-license-key", "fixture-key");
    await submit("form");
    assert.equal(get().entered, false);
    assert.ok(document.querySelector("#entry-license-key"));
    await act(async () => a.emit(valid));
    assert.ok(button("entry.choice.guest"));
    assert.equal(document.querySelector("#entry-license-key"), null);
    assert.equal(get().entered, false);
  });

  await run("E1 offline แสดง first activation network error", async () => {
    const get = await entryFixture(host({
      status: "unactivated",
      error: "network",
    }).adapter);
    assert.ok(document.body.textContent.includes(i18n.t("entry.errors.network")));
    await act(async () => get().guest());
    assert.equal(get().entered, false);
  });

  await run("E3 denied สามารถกรอกใหม่และ host valid จึงกลับ E2", async () => {
    const a = host({ status: "denied", error: "LICENSE_INACTIVE" });
    const get = await entryFixture(a.adapter);
    await click(button("entry.lock.reactivate"));
    await input("#entry-license-key", "replacement-fixture");
    await submit("form");
    assert.deepEqual(a.calls, [["activate", "replacement-fixture"]]);
    assert.equal(get().entered, false);
    await act(async () => a.emit(valid));
    assert.ok(button("entry.choice.guest"));
    assert.equal(get().entered, false);
  });

  await run("E1 Escape ไม่มีทางข้าม gate", async () => {
    const get = await appFixture(host({ status: "unactivated" }).adapter);
    await act(async () => window.dispatchEvent(new KeyboardEvent("keydown", {
      key: "Escape",
      bubbles: true,
    })));
    assert.equal(get().entry.entered, false);
    assert.ok(shellHidden());
  });

  group = "shared";
  await run("ไม่มี React render/effect error", async () => assert.deepEqual(errors, []));

  const groups = Object.fromEntries(["entry", "shell", "shared"].map(key => [key, {
    passed: results.filter(item => item.group === key && item.result === "PASS").length,
    failed: results.filter(item => item.group === key && item.result === "FAIL").length,
  }]));
  console.log(JSON.stringify({
    passed,
    failed,
    cases: results.length,
    groups,
    failures: results.filter(test => test.result === "FAIL"),
  }, null, 2));
  process.exitCode = failed ? 1 : 0;
} finally {
  await cleanup();
  console.error = originalError;
  await server.close();
  dom.window.close();
}