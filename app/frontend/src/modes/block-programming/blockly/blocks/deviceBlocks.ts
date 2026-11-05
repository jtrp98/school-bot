import {
  ANALOG_PINS,
  LED_PINS,
  PWM_PINS,
  choice,
  command,
  pin,
  reporter,
  symbols,
  value,
  type BlockJson,
} from "./builders";
import { COLOR_FIELD } from "../colorField";
import { NAMED_COLORS } from "../colorValue";

// Button codes of the 17-key NEC remote that ships with the kit; the generator compares the
// received code with the chosen one.
const REMOTE_BUTTONS = [
  ["1", "0xFFA25D"],
  ["2", "0xFF629D"],
  ["3", "0xFFE21D"],
  ["4", "0xFF22DD"],
  ["5", "0xFF02FD"],
  ["6", "0xFFC23D"],
  ["7", "0xFFE01F"],
  ["8", "0xFFA857"],
  ["9", "0xFF906F"],
  ["*", "0xFF6897"],
  ["0", "0xFF9867"],
  ["#", "0xFFB04F"],
  ["▲", "0xFF18E7"],
  ["◀", "0xFF10EF"],
  ["OK", "0xFF38C7"],
  ["▶", "0xFF5AA5"],
  ["▼", "0xFF4AB5"],
] as const;

const onOff = () => choice("STATE", "state", ["HIGH", "LOW"]);
const readMode = () => choice("MODE", "mode", ["DIGITAL", "ANALOG"]);
const numberField = (name: string, initial: number, min: number, max: number) => ({
  type: "field_number",
  name,
  value: initial,
  min,
  max,
  precision: 1,
});

const digitalSensor = (type: string) => reporter(type, "sensor", [pin()], "Boolean");
const analogSensor = (type: string) => reporter(type, "sensor", [pin("PIN", ANALOG_PINS)], "Number");
const dualSensor = (type: string) => reporter(type, "sensor", [pin(), readMode()], "Number");

// Fields and inputs match engines/mixly/src/school-bot-*.js (table in engines/mixly/docs/API.md).
export const DEVICE_BLOCKS: readonly BlockJson[] = [
  digitalSensor("sb_button"),
  digitalSensor("sb_touch"),
  digitalSensor("sb_reed_switch"),
  digitalSensor("sb_sound"),
  digitalSensor("sb_light"),
  digitalSensor("sb_line_tracker"),
  analogSensor("sb_potentiometer"),
  analogSensor("sb_soil_moisture"),
  analogSensor("sb_water_level"),
  analogSensor("sb_grayscale"),
  dualSensor("sb_ir_obstacle"),
  dualSensor("sb_tilt"),
  dualSensor("sb_pir"),
  dualSensor("sb_mq7"),
  dualSensor("sb_mq3"),

  command("sb_active_buzzer", "sensor", [pin(), onOff()]),
  command("sb_relay", "sensor", [pin(), onOff()]),
  command("sb_fan", "sensor", [
    pin("PIN_A", PWM_PINS),
    pin("PIN_B"),
    choice("DIRECTION", "fan", ["FORWARD", "REVERSE"]),
    value("SPEED", "Number"),
  ]),
  command("sb_fan_stop", "sensor", [pin("PIN_A", PWM_PINS), pin("PIN_B")]),
  command("sb_buzzer_tone", "sensor", [pin(), value("FREQUENCY", "Number")]),
  command("sb_buzzer_tone_for", "sensor", [pin(), value("FREQUENCY", "Number"), value("DURATION", "Number")]),
  command("sb_buzzer_stop", "sensor", [pin()]),
  command("sb_rgb_setup", "sensor", [
    pin(),
    numberField("COUNT", 1, 1, 60),
    numberField("BRIGHTNESS", 50, 0, 100),
  ]),
  command("sb_rgb_set", "sensor", [pin(), value("PIXEL", "Number"), value("COLOR", "Colour")]),
  reporter("sb_rgb_color", "sensor", [
    value("RED", "Number"),
    value("GREEN", "Number"),
    value("BLUE", "Number"),
  ], "Colour"),
  reporter("sb_rgb_named_color", "sensor", [
    { ...choice("COLOR", "color", Object.keys(NAMED_COLORS)), type: COLOR_FIELD },
  ], "Colour"),
  reporter("sb_ultrasonic", "sensor", [pin("TRIG"), pin("ECHO")], "Number"),
  reporter("sb_mpu6050", "sensor", [
    choice("VALUE", "mpu", ["ACCEL_X", "ACCEL_Y", "ACCEL_Z", "GYRO_X", "GYRO_Y", "GYRO_Z"]),
  ], "Number"),
  reporter("sb_dht11", "sensor", [pin(), choice("VALUE", "dht", ["TEMPERATURE", "HUMIDITY"])], "Number"),
  reporter("sb_gesture_is", "sensor", [
    choice("DIRECTION", "gesture", ["DIR_LEFT", "DIR_RIGHT", "DIR_UP", "DIR_DOWN", "DIR_NEAR", "DIR_FAR"]),
  ], "Boolean"),
  reporter("sb_gesture_light", "sensor", [choice("CHANNEL", "light", ["RED", "GREEN", "BLUE", "AMBIENT"])], "Number"),
  reporter("sb_color_sensor", "sensor", [choice("CHANNEL", "light", ["RED", "GREEN", "BLUE", "CLEAR"])], "Number"),

  command("sb_servo", "motor", [pin(), value("ANGLE", "Number")]),

  command("sb_oled_print", "display", [symbols("LINE", ["1", "2", "3", "4"].map(line => [line, line])), value("TEXT")]),
  command("sb_oled_clear", "display"),
  command("sb_led", "display", [pin("PIN", LED_PINS), onOff()]),
  command("sb_led_brightness", "display", [pin("PIN", PWM_PINS), value("BRIGHTNESS", "Number")]),

  reporter("sb_ir_received", "remote", [pin()], "Boolean"),
  reporter("sb_ir_button", "remote", [symbols("BUTTON", REMOTE_BUTTONS)], "Boolean"),
];
