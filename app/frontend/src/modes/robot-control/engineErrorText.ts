import { t } from "../../i18n";
import { EngineError } from "../../core/engineError";
import { AdapterError, errorCode } from "./engineAdapter";

// Full sentences per code: word order differs by language, so the code picks the whole message.
export const ENGINE_ERROR_KEYS: Record<string, string> = {
  ENGINE_BUSY: "robot.errors.engineBusy",
  ENGINE_CANCELLED: "robot.errors.engineCancelled",
  ENGINE_TIMEOUT: "robot.errors.engineTimeout",
  ENGINE_FAILED: "robot.errors.engineFailed",
  TRANSPORT_ERROR: "robot.errors.transportError",
  DEVICE_TIMEOUT: "robot.errors.deviceTimeout",
  SAFETY_LIMIT: "robot.errors.safetyLimit",
  INVALID_INPUT: "robot.errors.invalidInput",
  PROFILE_INVALID: "robot.errors.profileInvalid",
  SERVO_CALIBRATION_INVALID: "robot.errors.servoCalibrationInvalid",
  JOINT_VALUE_OUT_OF_RANGE: "robot.errors.jointValueOutOfRange",
  POSE_INVALID: "robot.errors.poseInvalid",
  POSITION_UNAVAILABLE: "robot.errors.positionUnavailable",
  POSITION_NOT_REPORTED: "robot.errors.positionNotReported",
  POSITION_OUT_OF_PROFILE: "robot.errors.positionOutOfProfile",
  SAMPLE_MODE: "robot.errors.sampleMode",
  ROBOT_STOPPING: "robot.errors.robotStopping",
  DEVICE_NOT_CONFIRMED: "robot.errors.deviceNotConfirmed",
  ROBOT_NOT_SELECTED: "robot.errors.robotNotSelected",
  CONNECTION_SUPERSEDED: "robot.errors.connectionSuperseded",
  DEVICE_NOT_FOUND: "robot.errors.deviceNotFound",
};
export const UNEXPECTED_ENGINE_ERROR_KEY = "robot.errors.unexpected";

/** The localized text for an error shown to the user. The engine's own diagnostic is kept verbatim as `detail`. */
export function engineErrorText(error: unknown): string {
  const message = error instanceof Error ? error.message : String(error);
  const code = errorCode(error);
  if (!code) return message;
  const key = ENGINE_ERROR_KEYS[code];
  if (key && error instanceof AdapterError) return t(key, error.values);
  const detail = error instanceof EngineError ? error.detail : message;
  if (key) return t(key, { detail });
  return error instanceof EngineError ? t(UNEXPECTED_ENGINE_ERROR_KEY, { detail: `${code}: ${detail}` }) : message;
}
