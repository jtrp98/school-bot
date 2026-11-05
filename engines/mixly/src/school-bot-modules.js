'use strict';
// School Bot module blocks for the Arduino UNO that need a driver library or the I2C bus.
// Pin checks and conflict tracking come from school-bot-devices.js, which loads first.
(function () {
  const A = Blockly.Arduino;
  const { IO_PINS, pin, choice, claim, useTimer2, useI2c } = A.schoolBotPins;

  // OLED 128x64 over I2C in U8g2 page mode (128 bytes of RAM instead of a 1 KB frame), so
  // every print redraws all four remembered lines.
  function oled() {
    useI2c();
    A.definitions_['include_U8g2lib'] = '#include <U8g2lib.h>';
    A.definitions_['include_Wire'] = '#include <Wire.h>';
    A.definitions_['var_declare_u8g2'] = 'U8G2_SSD1306_128X64_NONAME_1_HW_I2C u8g2(U8G2_R0, U8X8_PIN_NONE);';
    A.definitions_['var_declare_sb_oled_lines'] = 'String sbOledLines[4];';
    A.definitions_['function_sb_oled_show'] = 'void sbOledShow() {\n' +
      '  u8g2.firstPage();\n' +
      '  do {\n' +
      '    u8g2.setFont(u8g2_font_timR08_tf);\n' +
      '    u8g2.setFontPosTop();\n' +
      '    for (int line = 0; line < 4; line++) {\n' +
      '      u8g2.setCursor(0, line * 16);\n' +
      '      u8g2.print(sbOledLines[line]);\n' +
      '    }\n' +
      '  } while (u8g2.nextPage());\n' +
      '}';
    A.setups_['setup_u8g2'] = 'u8g2.setI2CAddress(0x3C*2);\n  u8g2.begin();\n  u8g2.enableUTF8Print();';
  }

  A.sb_oled_print = function () {
    const line = choice(this, 'LINE', ['1', '2', '3', '4']);
    const text = A.valueToCode(this, 'TEXT', A.ORDER_NONE) || '""';
    oled();
    return 'sbOledLines[' + (Number(line) - 1) + '] = String(' + text + ');\nsbOledShow();\n';
  };

  A.sb_oled_clear = function () {
    oled();
    return 'for (int line = 0; line < 4; line++) {\n  sbOledLines[line] = "";\n}\nsbOledShow();\n';
  };

  A.sb_ultrasonic = function () {
    const trig = pin(this, 'TRIG', IO_PINS);
    const echo = pin(this, 'ECHO', IO_PINS);
    claim(trig, 'ultrasonic');
    claim(echo, 'ultrasonic');
    const name = 'checkdistance_' + trig + '_' + echo;
    A.setups_['sb_pin_' + trig] = 'pinMode(' + trig + ', OUTPUT);';
    A.setups_['sb_pin_' + echo] = 'pinMode(' + echo + ', INPUT_PULLUP);';
    A.definitions_['function_' + name] = 'float ' + name + '() {\n' +
      '  digitalWrite(' + trig + ', LOW);\n' +
      '  delayMicroseconds(2);\n' +
      '  digitalWrite(' + trig + ', HIGH);\n' +
      '  delayMicroseconds(10);\n' +
      '  digitalWrite(' + trig + ', LOW);\n' +
      '  float distance = pulseIn(' + echo + ', HIGH) / 58.00;\n' +
      '  return distance;\n' +
      '}';
    return [name + '()', A.ORDER_UNARY_POSTFIX];
  };

  // MPU6050 read straight from its registers over I2C (address 0x68). Each value block
  // refreshes the registers first, so children never have to order a separate read block.
  // A missing sensor keeps the last values instead of waiting forever for six bytes, which
  // would freeze loop() with every output left as it was.
  function mpu() {
    useI2c();
    A.definitions_['include_Wire'] = '#include <Wire.h>';
    A.definitions_['var_declare_sb_mpu'] = 'long accelX, accelY, accelZ;\n' +
      'float gForceX, gForceY, gForceZ;\n' +
      'long gyroX, gyroY, gyroZ;\n' +
      'float rotX, rotY, rotZ;';
    A.definitions_['function_setupMPU'] = 'void setupMPU() {\n' +
      '  Wire.beginTransmission(0x68);\n  Wire.write(0x6B);\n  Wire.write(0b00000000);\n  Wire.endTransmission();\n' +
      '  Wire.beginTransmission(0x68);\n  Wire.write(0x1B);\n  Wire.write(0x00000000);\n  Wire.endTransmission();\n' +
      '  Wire.beginTransmission(0x68);\n  Wire.write(0x1C);\n  Wire.write(0b00000000);\n  Wire.endTransmission();\n' +
      '}';
    A.definitions_['function_recordAccelRegisters'] = 'void recordAccelRegisters() {\n' +
      '  Wire.beginTransmission(0x68);\n  Wire.write(0x3B);\n  Wire.endTransmission();\n' +
      '  if (Wire.requestFrom(0x68, 6) != 6) return;\n' +
      '  accelX = Wire.read() << 8 | Wire.read();\n' +
      '  accelY = Wire.read() << 8 | Wire.read();\n' +
      '  accelZ = Wire.read() << 8 | Wire.read();\n' +
      '  gForceX = accelX / 16384.0 * 100;\n' +
      '  gForceY = accelY / 16384.0 * 100;\n' +
      '  gForceZ = accelZ / 16384.0 * 100;\n' +
      '}';
    A.definitions_['function_recordGyroRegisters'] = 'void recordGyroRegisters() {\n' +
      '  Wire.beginTransmission(0x68);\n  Wire.write(0x43);\n  Wire.endTransmission();\n' +
      '  if (Wire.requestFrom(0x68, 6) != 6) return;\n' +
      '  gyroX = Wire.read() << 8 | Wire.read();\n' +
      '  gyroY = Wire.read() << 8 | Wire.read();\n' +
      '  gyroZ = Wire.read() << 8 | Wire.read();\n' +
      '  rotX = gyroX / 131.0;\n  rotY = gyroY / 131.0;\n  rotZ = gyroZ / 131.0;\n' +
      '}';
    A.setups_['setup_sb_mpu'] = 'Wire.begin();\n  setupMPU();';
  }

  const MPU_VALUES = {
    ACCEL_X: ['recordAccelRegisters', 'gForceX'], ACCEL_Y: ['recordAccelRegisters', 'gForceY'],
    ACCEL_Z: ['recordAccelRegisters', 'gForceZ'], GYRO_X: ['recordGyroRegisters', 'rotX'],
    GYRO_Y: ['recordGyroRegisters', 'rotY'], GYRO_Z: ['recordGyroRegisters', 'rotZ'],
  };

  A.sb_mpu6050 = function () {
    const [record, variable] = MPU_VALUES[choice(this, 'VALUE', Object.keys(MPU_VALUES))];
    const name = 'sbMpu' + variable.charAt(0).toUpperCase() + variable.slice(1);
    mpu();
    A.definitions_['function_' + name] = 'float ' + name + '() {\n  ' + record + '();\n  return ' + variable + ';\n}';
    return [name + '()', A.ORDER_UNARY_POSTFIX];
  };

  // Infrared receiver (IRremote 3.x legacy API). Reading a code consumes it, so the
  // "received" condition stores it for the button comparison that follows.
  function irReceiver(value) {
    A.definitions_['include_IRremote'] = '#include <IRremote.h>';
    A.definitions_['var_declare_irrecv_' + value] = 'IRrecv irrecv_' + value + '(' + value + ');\n' +
      'decode_results results_' + value + ';';
    A.definitions_['var_declare_irValue'] = 'unsigned long irValue;';
    A.setups_['setup_ir_recv_' + value] = 'irrecv_' + value + '.enableIRIn();';
  }

  A.sb_ir_received = function () {
    const value = pin(this, 'PIN', IO_PINS);
    claim(value, 'input');
    useTimer2('ir');
    const name = 'sbIrReceived_' + value;
    irReceiver(value);
    A.definitions_['function_' + name] = 'bool ' + name + '() {\n' +
      '  if (irrecv_' + value + '.decode(&results_' + value + ')) {\n' +
      '    irValue = results_' + value + '.value;\n' +
      '    irrecv_' + value + '.resume();\n' +
      '    return true;\n' +
      '  }\n' +
      '  return false;\n' +
      '}';
    return [name + '()', A.ORDER_UNARY_POSTFIX];
  };

  A.sb_ir_button = function () {
    const code = this.getFieldValue('BUTTON');
    if (!/^0x[0-9A-F]{1,8}$/.test(code)) throw new Error('sb_ir_button: invalid remote code');
    A.definitions_['var_declare_irValue'] = 'unsigned long irValue;';
    return ['irValue == ' + code, A.ORDER_EQUALITY];
  };

  // DHT11 through the ZLTech dht library, whose read functions take the pin each time.
  A.sb_dht11 = function () {
    const value = pin(this, 'PIN', IO_PINS);
    claim(value, 'input');
    const temperature = choice(this, 'VALUE', ['TEMPERATURE', 'HUMIDITY']) === 'TEMPERATURE';
    const read = temperature ? 'readTemperature' : 'readHumidity';
    A.definitions_['include_dht'] = '#include "dht.h"';
    A.definitions_['var_declare_DHT_' + value] = 'dht DHT_' + value + ';';
    return ['DHT_' + value + '.' + read + '(' + value + ')', A.ORDER_UNARY_POSTFIX];
  };

  // APDS-9960 gesture and light sensor; gesture and light are enabled together.
  function apds() {
    useI2c();
    A.definitions_['include_Wire'] = '#include <Wire.h>';
    A.definitions_['include_APDS9960'] = '#include <APDS9960.h>';
    A.definitions_['var_declare_apds'] = 'APDS9960 apds = APDS9960();';
    A.setups_['setup_apds'] = 'apds.init();\n  apds.enableGestureSensor(true);\n  apds.enableLightSensor(true);';
  }

  const GESTURES = ['DIR_LEFT', 'DIR_RIGHT', 'DIR_UP', 'DIR_DOWN', 'DIR_NEAR', 'DIR_FAR'];

  A.sb_gesture_is = function () {
    const direction = choice(this, 'DIRECTION', GESTURES);
    apds();
    return ['apds.readGesture() == ' + direction, A.ORDER_EQUALITY];
  };

  const LIGHT_CHANNELS = {
    RED: 'readRedLight', GREEN: 'readGreenLight', BLUE: 'readBlueLight', AMBIENT: 'readAmbientLight',
  };

  A.sb_gesture_light = function () {
    const read = LIGHT_CHANNELS[choice(this, 'CHANNEL', Object.keys(LIGHT_CHANNELS))];
    apds();
    return ['apds.' + read + '()', A.ORDER_UNARY_POSTFIX];
  };

  // TCS34725 colour sensor (ZLTech's Adafruit fork adds getR/getG/getB/getC).
  const COLOUR_CHANNELS = { RED: 'getR', GREEN: 'getG', BLUE: 'getB', CLEAR: 'getC' };

  A.sb_color_sensor = function () {
    const read = COLOUR_CHANNELS[choice(this, 'CHANNEL', Object.keys(COLOUR_CHANNELS))];
    useI2c();
    A.definitions_['include_Wire'] = '#include <Wire.h>';
    A.definitions_['include_Adafruit_TCS34725'] = '#include <Adafruit_TCS34725.h>';
    A.definitions_['var_declare_tcs'] = 'Adafruit_TCS34725 tcs = ' +
      'Adafruit_TCS34725(TCS34725_INTEGRATIONTIME_24MS, TCS34725_GAIN_4X);';
    A.setups_['setup_tcs'] = 'tcs.begin();';
    return ['tcs.' + read + '()', A.ORDER_UNARY_POSTFIX];
  };
})();
