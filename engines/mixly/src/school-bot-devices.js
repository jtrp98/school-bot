'use strict';
// School Bot device blocks for the Arduino UNO (Block Lab sensor, display and output kits).
// Field values come from the page, so each is checked against the UNO's pins and fixed
// choices before it lands in C++. This keeps programs on pins that work; it is not the
// security boundary for compiling, which the host must enforce before routing compile.
// The checks also refuse pin combinations that fail silently on the board (see claim).
// Each generation runs in a fresh VM, so the module state below covers one program.
(function () {
  const A = Blockly.Arduino;
  const ANALOG_PINS = ['A0', 'A1', 'A2', 'A3', 'A4', 'A5'];
  // 0 and 1 carry upload and Serial; 13 flashes during every reset and upload, so only the
  // on-board LED block may use it.
  const IO_PINS = ['2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12'].concat(ANALOG_PINS);
  const LED_PINS = IO_PINS.concat(['13']);
  // Servo takes Timer1 (PWM on 9, 10) and tone/IR take Timer2 (PWM on 3, 11), which leaves a
  // dead PWM pin stuck low. 5 and 6 run on Timer0 and keep working beside them.
  const PWM_PINS = ['5', '6'];
  const isInteger = text => /^-?\d+$/.test(text);
  const claims = {};
  let timer2Owner = '';
  // Wiring that compiles but fails on the board, one entry per cause. Generation goes on so
  // that every cause is found; generator.go then refuses the program and project.validate
  // hands the list to Block Lab, which words it for children.
  const problems = [];

  function pin(block, name, allowed) {
    const value = block.getFieldValue(name);
    if (!allowed.includes(value)) throw new Error(block.type + ': pin ' + value + ' is not allowed');
    return value;
  }

  function choice(block, name, allowed) {
    const value = block.getFieldValue(name);
    if (!allowed.includes(value)) throw new Error(block.type + ': ' + name + ' ' + value + ' is not allowed');
    return value;
  }

  // A pin serves one kind of use: a plain input, a plain or PWM output (one LED may be
  // switched and dimmed), or one driver such as NeoPixel, Servo, tone or the I2C bus.
  function claim(value, role) {
    if (!claims[value]) {
      claims[value] = role;
      return;
    }
    if (claims[value] === role) return;
    const shared = problems.find(problem => problem.code === 'PIN_SHARED' && problem.pin === value);
    if (!shared) problems.push({ code: 'PIN_SHARED', pin: value, uses: [claims[value], role] });
    else if (!shared.uses.includes(role)) shared.uses.push(role);
  }

  // tone() reprograms Timer2, after which the IR receiver stops decoding (and the reverse).
  function useTimer2(owner) {
    if (!timer2Owner) {
      timer2Owner = owner;
      return;
    }
    if (timer2Owner === owner || problems.some(problem => problem.code === 'TIMER2_SHARED')) return;
    problems.push({ code: 'TIMER2_SHARED', uses: [timer2Owner, owner] });
  }

  // Block Lab shows percentages; Arduino PWM and NeoPixel brightness take 0-255.
  function percentToByte(value) {
    if (isInteger(value)) return String(Math.round(Math.min(100, Math.max(0, Number(value))) * 255 / 100));
    return 'map(constrain(' + value + ', 0, 100), 0, 100, 0, 255)';
  }

  function inputPin(block) {
    const value = pin(block, 'PIN', IO_PINS);
    claim(value, 'input');
    A.setups_['sb_pin_' + value] = 'pinMode(' + value + ', INPUT);';
    return value;
  }

  function analogPin(block) {
    const value = pin(block, 'PIN', ANALOG_PINS);
    claim(value, 'input');
    return value;
  }

  function outputPin(block, name, allowed, role) {
    const value = pin(block, name, allowed || IO_PINS);
    claim(value, role || 'output');
    A.setups_['sb_pin_' + value] = 'pinMode(' + value + ', OUTPUT);';
    return value;
  }

  function useI2c() {
    claim('A4', 'i2c');
    claim('A5', 'i2c');
  }

  function define(types, generator) {
    for (const type of types) A[type] = generator;
  }

  // Inputs. Each device has its own block so the toolbox can name it; the code is shared.
  define(['sb_button', 'sb_touch', 'sb_reed_switch', 'sb_sound', 'sb_light', 'sb_line_tracker',
    'sb_collision', 'sb_vibration'], function () {
    return ['digitalRead(' + inputPin(this) + ')', A.ORDER_ATOMIC];
  });

  define(['sb_potentiometer', 'sb_soil_moisture', 'sb_water_level', 'sb_grayscale', 'sb_thermistor'], function () {
    return ['analogRead(' + analogPin(this) + ')', A.ORDER_ATOMIC];
  });

  define(['sb_ir_obstacle', 'sb_tilt', 'sb_pir', 'sb_mq7', 'sb_mq3', 'sb_flame'], function () {
    if (choice(this, 'MODE', ['DIGITAL', 'ANALOG']) === 'ANALOG') {
      return ['analogRead(' + analogPin(this) + ')', A.ORDER_ATOMIC];
    }
    return ['digitalRead(' + inputPin(this) + ')', A.ORDER_ATOMIC];
  });

  // On/off outputs.
  define(['sb_active_buzzer', 'sb_relay'], function () {
    const value = outputPin(this, 'PIN');
    return 'digitalWrite(' + value + ', ' + choice(this, 'STATE', ['HIGH', 'LOW']) + ');\n';
  });

  A.sb_led = function () {
    const value = outputPin(this, 'PIN', LED_PINS);
    return 'digitalWrite(' + value + ', ' + choice(this, 'STATE', ['HIGH', 'LOW']) + ');\n';
  };

  A.sb_led_brightness = function () {
    const value = outputPin(this, 'PIN', PWM_PINS);
    const level = A.valueToCode(this, 'BRIGHTNESS', A.ORDER_NONE) || '0';
    return 'analogWrite(' + value + ', ' + percentToByte(level) + ');\n';
  };

  // The L9110 fan driver takes PWM speed on INA and direction on INB; with INB high the
  // duty cycle inverts, so reverse writes 255 minus the speed.
  A.sb_fan = function () {
    const ina = outputPin(this, 'PIN_A', PWM_PINS);
    const inb = outputPin(this, 'PIN_B');
    const speed = percentToByte(A.valueToCode(this, 'SPEED', A.ORDER_NONE) || '0');
    if (choice(this, 'DIRECTION', ['FORWARD', 'REVERSE']) === 'FORWARD') {
      return 'analogWrite(' + ina + ', ' + speed + ');\ndigitalWrite(' + inb + ', LOW);\n';
    }
    const inverted = isInteger(speed) ? String(255 - Number(speed)) : '255 - ' + speed;
    return 'analogWrite(' + ina + ', ' + inverted + ');\ndigitalWrite(' + inb + ', HIGH);\n';
  };

  A.sb_fan_stop = function () {
    const ina = outputPin(this, 'PIN_A', PWM_PINS);
    const inb = outputPin(this, 'PIN_B');
    return 'digitalWrite(' + ina + ', LOW);\ndigitalWrite(' + inb + ', LOW);\n';
  };

  A.sb_buzzer_tone = function () {
    const value = outputPin(this, 'PIN', IO_PINS, 'tone');
    const frequency = A.valueToCode(this, 'FREQUENCY', A.ORDER_NONE) || '0';
    useTimer2('tone');
    return 'tone(' + value + ', ' + frequency + ');\n';
  };

  A.sb_buzzer_tone_for = function () {
    const value = outputPin(this, 'PIN', IO_PINS, 'tone');
    const frequency = A.valueToCode(this, 'FREQUENCY', A.ORDER_NONE) || '0';
    const duration = A.valueToCode(this, 'DURATION', A.ORDER_NONE) || '0';
    useTimer2('tone');
    return 'tone(' + value + ', ' + frequency + ', ' + duration + ');\n';
  };

  A.sb_buzzer_stop = function () {
    useTimer2('tone');
    return 'noTone(' + outputPin(this, 'PIN', IO_PINS, 'tone') + ');\n';
  };

  A.sb_servo = function () {
    const value = pin(this, 'PIN', IO_PINS);
    claim(value, 'servo');
    const angle = A.valueToCode(this, 'ANGLE', A.ORDER_NONE) || '90';
    A.definitions_['include_Servo'] = '#include <Servo.h>';
    A.definitions_['var_declare_servo_' + value] = 'Servo servo_' + value + ';';
    A.setups_['setup_servo_' + value] = 'servo_' + value + '.attach(' + value + ');';
    return 'servo_' + value + '.write(' + angle + ');\n';
  };

  // 3-channel RGB LED module (NeoPixel). The setup block fixes the LED count at compile time;
  // 3 bytes of the UNO's 2 KB RAM per LED and USB current both cap it. Brightness lives here
  // because NeoPixel brightness scales the whole strip and loses colour precision each time
  // it changes, so setting it per pixel would dim pixels set earlier.
  const neoPixel = value => 'rgb_display_' + value;

  A.sb_rgb_setup = function () {
    const value = pin(this, 'PIN', IO_PINS);
    claim(value, 'neopixel');
    const count = this.getFieldValue('COUNT');
    if (!/^[1-9]\d?$/.test(count) || Number(count) > 60) throw new Error('sb_rgb_setup: LED count must be 1-60');
    const brightness = this.getFieldValue('BRIGHTNESS');
    if (!/^\d{1,3}$/.test(brightness) || Number(brightness) > 100) {
      throw new Error('sb_rgb_setup: brightness must be 0-100');
    }
    A.definitions_['include_Adafruit_NeoPixel'] = '#include <Adafruit_NeoPixel.h>';
    A.definitions_['var_declare_rgb_display_' + value] = 'Adafruit_NeoPixel ' + neoPixel(value) +
      ' = Adafruit_NeoPixel(' + count + ',' + value + ',NEO_GRB + NEO_KHZ800);';
    A.setups_['setup_rgb_display_begin_' + value] = neoPixel(value) + '.begin();\n  ' +
      neoPixel(value) + '.setBrightness(' + percentToByte(brightness) + ');';
    return '';
  };

  // Pixels are counted from 1 in Block Lab, like every other position the children see.
  A.sb_rgb_set = function () {
    const value = pin(this, 'PIN', IO_PINS);
    claim(value, 'neopixel');
    const pixel = A.valueToCode(this, 'PIXEL', A.ORDER_ADDITIVE) || '1';
    const colour = A.valueToCode(this, 'COLOR', A.ORDER_NONE) || '0';
    const index = isInteger(pixel) ? String(Number(pixel) - 1) : pixel + ' - 1';
    return neoPixel(value) + '.setPixelColor(' + index + ', ' + colour + ');\n' +
      neoPixel(value) + '.show();\n';
  };

  A.sb_rgb_color = function () {
    const red = A.valueToCode(this, 'RED', A.ORDER_BITWISE_AND) || '0';
    const green = A.valueToCode(this, 'GREEN', A.ORDER_BITWISE_AND) || '0';
    const blue = A.valueToCode(this, 'BLUE', A.ORDER_BITWISE_OR) || '0';
    return ['(((' + red + ' & 0xffffff) << 16) | ((' + green + ' & 0xffffff) << 8) | ' + blue + ')',
      A.ORDER_ATOMIC];
  };

  const NAMED_COLOURS = {
    RED: '0xFF0000', GREEN: '0x00FF00', BLUE: '0x0000FF', YELLOW: '0xFFFF00',
    CYAN: '0x00FFFF', MAGENTA: '0xFF00FF', WHITE: '0xFFFFFF', OFF: '0x000000',
  };

  // A colour the child mixed on the wheel or by numbers is kept as #RRGGBB. It is pasted into
  // C++ as a hex literal, so nothing but six capital hex digits passes.
  const MIXED_COLOUR = /^#[0-9A-F]{6}$/;

  A.sb_rgb_named_color = function () {
    const mixed = this.getFieldValue('COLOR');
    if (MIXED_COLOUR.test(mixed)) return ['0x' + mixed.slice(1), A.ORDER_ATOMIC];
    return [NAMED_COLOURS[choice(this, 'COLOR', Object.keys(NAMED_COLOURS))], A.ORDER_ATOMIC];
  };

  // school-bot-modules.js loads next and must share these checks and their per-program state.
  A.schoolBotPins = { IO_PINS, pin, choice, claim, useTimer2, useI2c };
  A.schoolBotProblems = problems;
})();
