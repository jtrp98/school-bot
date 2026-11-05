
#include <U8g2lib.h>
#include <Wire.h>
#include "dht.h"
#include <Adafruit_TCS34725.h>
#include <IRremote.h>
#include <APDS9960.h>

U8G2_SSD1306_128X64_NONAME_1_HW_I2C u8g2(U8G2_R0, U8X8_PIN_NONE);
String sbOledLines[4];
long accelX, accelY, accelZ;
float gForceX, gForceY, gForceZ;
long gyroX, gyroY, gyroZ;
float rotX, rotY, rotZ;
dht DHT_2;
Adafruit_TCS34725 tcs = Adafruit_TCS34725(TCS34725_INTEGRATIONTIME_24MS, TCS34725_GAIN_4X);
IRrecv irrecv_11(11);
decode_results results_11;
unsigned long irValue;
APDS9960 apds = APDS9960();

float checkdistance_A2_A3() {
  digitalWrite(A2, LOW);
  delayMicroseconds(2);
  digitalWrite(A2, HIGH);
  delayMicroseconds(10);
  digitalWrite(A2, LOW);
  float distance = pulseIn(A3, HIGH) / 58.00;
  return distance;
}
void sbOledShow() {
  u8g2.firstPage();
  do {
    u8g2.setFont(u8g2_font_timR08_tf);
    u8g2.setFontPosTop();
    for (int line = 0; line < 4; line++) {
      u8g2.setCursor(0, line * 16);
      u8g2.print(sbOledLines[line]);
    }
  } while (u8g2.nextPage());
}
void setupMPU() {
  Wire.beginTransmission(0x68);
  Wire.write(0x6B);
  Wire.write(0b00000000);
  Wire.endTransmission();
  Wire.beginTransmission(0x68);
  Wire.write(0x1B);
  Wire.write(0x00000000);
  Wire.endTransmission();
  Wire.beginTransmission(0x68);
  Wire.write(0x1C);
  Wire.write(0b00000000);
  Wire.endTransmission();
}
void recordAccelRegisters() {
  Wire.beginTransmission(0x68);
  Wire.write(0x3B);
  Wire.endTransmission();
  if (Wire.requestFrom(0x68, 6) != 6) return;
  accelX = Wire.read() << 8 | Wire.read();
  accelY = Wire.read() << 8 | Wire.read();
  accelZ = Wire.read() << 8 | Wire.read();
  gForceX = accelX / 16384.0 * 100;
  gForceY = accelY / 16384.0 * 100;
  gForceZ = accelZ / 16384.0 * 100;
}
void recordGyroRegisters() {
  Wire.beginTransmission(0x68);
  Wire.write(0x43);
  Wire.endTransmission();
  if (Wire.requestFrom(0x68, 6) != 6) return;
  gyroX = Wire.read() << 8 | Wire.read();
  gyroY = Wire.read() << 8 | Wire.read();
  gyroZ = Wire.read() << 8 | Wire.read();
  rotX = gyroX / 131.0;
  rotY = gyroY / 131.0;
  rotZ = gyroZ / 131.0;
}
float sbMpuGForceX() {
  recordAccelRegisters();
  return gForceX;
}
bool sbIrReceived_11() {
  if (irrecv_11.decode(&results_11)) {
    irValue = results_11.value;
    irrecv_11.resume();
    return true;
  }
  return false;
}

void setup(){
  pinMode(A2, OUTPUT);
  pinMode(A3, INPUT_PULLUP);
  u8g2.setI2CAddress(0x3C*2);
  u8g2.begin();
  u8g2.enableUTF8Print();
  Wire.begin();
  setupMPU();
  tcs.begin();
  irrecv_11.enableIRIn();
  apds.init();
  apds.enableGestureSensor(true);
  apds.enableLightSensor(true);
}

void loop(){
  sbOledLines[0] = String(checkdistance_A2_A3());
  sbOledShow();
  sbOledLines[1] = String(sbMpuGForceX());
  sbOledShow();
  sbOledLines[2] = String(DHT_2.readTemperature(2));
  sbOledShow();
  sbOledLines[3] = String(tcs.getR());
  sbOledShow();
  if (sbIrReceived_11() && irValue == 0xFFA25D) {
    for (int line = 0; line < 4; line++) {
      sbOledLines[line] = "";
    }
    sbOledShow();

  }
  if (apds.readGesture() == DIR_LEFT) {
    sbOledLines[0] = String(apds.readAmbientLight());
    sbOledShow();

  }

}