import { t } from "../../i18n";
import { useEffect, useRef } from "react";
import Phaser from "phaser";
import { useTranslation } from "react-i18next";
import type { Command } from "../../core/types";

const gameHostClass = [
  "game-host w-full [aspect-ratio:768/416] overflow-hidden rounded-control bg-blue-tint",
  "[&_canvas]:block [&_canvas]:max-w-full",
].join(" ");

export default function GameCanvas({
  commands,
  runID,
}: {
  commands: Command[];
  runID: number;
}) {
  const host = useRef<HTMLDivElement>(null);
  const { i18n } = useTranslation();
  const labels = useRef<{ node: Phaser.GameObjects.Text; translationKey: string }[]>([]);
  useEffect(() => {
    for (const { node, translationKey } of labels.current) if (node.active) node.setText(t(translationKey));
  }, [i18n.resolvedLanguage]);
  useEffect(() => {
    let cancelled = false;
    class Playground extends Phaser.Scene {
      create() {
        this.cameras.main.setBackgroundColor("#eef5ed");
        const graphics = this.add.graphics();
        graphics.fillStyle(0xc9ddd0);
        graphics.fillCircle(110, 260, 135);
        graphics.fillStyle(0xd8e7d4);
        graphics.fillCircle(440, 275, 175);
        graphics.fillStyle(0xe4eddc);
        graphics.fillCircle(670, 280, 145);
        for (let i = 0; i < 24; i++) {
          this.add.rectangle(i * 32 + 16, 335, 32, 34, 0x47775f);
          this.add.rectangle(i * 32 + 16, 316, 32, 8, 0x7ba77c);
        }
        const heading = this.add.text(24, 24, t("adventure.messages.theFirstLittleAdventure"), {
          fontFamily: "monospace",
          fontSize: "12px",
          color: "#587365",
        });
        const subtitle = this.add.text(24, 48, t("adventure.messages.moveJumpFindYourWay"), {
          fontFamily: "sans-serif",
          fontSize: "22px",
          color: "#24483f",
        });
        labels.current.push(
          { node: heading, translationKey: "adventure.messages.theFirstLittleAdventure" },
          { node: subtitle, translationKey: "adventure.messages.moveJumpFindYourWay" },
        );
        this.add.rectangle(652, 275, 4, 80, 0x426a5e);
        this.add.triangle(672, 250, 0, 0, 34, 12, 0, 24, 0xe99063);
        const texture = this.make.graphics({ x: 0, y: 0 });
        texture.fillStyle(0xe99063);
        texture.fillRoundedRect(0, 0, 26, 34, 6);
        texture.fillStyle(0x24483f);
        texture.fillRect(15, 8, 4, 4);
        texture.generateTexture("hero", 26, 34);
        texture.destroy();
        const floor = this.physics.add
          .staticImage(384, 334, "hero")
          .setVisible(false);
        floor.setDisplaySize(768, 34).refreshBody();
        const hero = this.physics.add
          .sprite(64, 298, "hero")
          .setCollideWorldBounds(true);
        this.physics.add.collider(hero, floor);
        const pause = (milliseconds: number) =>
          new Promise<void>((resolve) =>
            this.time.delayedCall(milliseconds, resolve),
          );
        void (async () => {
          await pause(200);
          for (const command of commands) {
            if (cancelled || !hero.active) return;
            if (command.op === "jump") {
              hero.setVelocityY(-240);
              await pause(800);
            } else {
              const direction = command.op === "move_right" ? 1 : -1;
              hero.setFlipX(direction < 0);
              hero.setVelocityX(direction * 96);
              await pause((((command.value ?? 1) * 32) / 96) * 1000);
              if (cancelled || !hero.active) return;
              hero.setVelocityX(0);
            }
          }
          if (!cancelled) {
            const key = commands.length
              ? "common.messages.playbackCompleteTryAnotherRoute"
              : "common.messages.writeAFewCommandsToBegin";
            const caption = this.add.text(
              24,
              370,
              t(key),
              { fontFamily: "sans-serif", fontSize: "15px", color: "#426a5e" },
            );
            labels.current.push({ node: caption, translationKey: key });
          }
        })();
      }
    }
    const game = new Phaser.Game({
      type: Phaser.AUTO,
      parent: host.current!,
      width: 768,
      height: 416,
      backgroundColor: "#eef5ed",
      scene: Playground,
      physics: {
        default: "arcade",
        arcade: { gravity: { x: 0, y: 650 }, debug: false },
      },
      fps: { target: 30 },
      scale: { mode: Phaser.Scale.FIT, autoCenter: Phaser.Scale.CENTER_BOTH },
      audio: { noAudio: true },
    });
    return () => {
      cancelled = true;
      labels.current = [];
      game.destroy(true);
    };
  }, [commands, runID]);
  return (
    <div
      className={gameHostClass}
      ref={host}
      aria-label={t("adventure.messages.gameCommandPlaybackPreview")}
    />
  );
}
