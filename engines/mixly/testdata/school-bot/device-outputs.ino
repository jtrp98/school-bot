
#include <Adafruit_NeoPixel.h>
#include <Servo.h>

Adafruit_NeoPixel rgb_display_2 = Adafruit_NeoPixel(4,2,NEO_GRB + NEO_KHZ800);
Servo servo_10;

void setup(){
  rgb_display_2.begin();
  rgb_display_2.setBrightness(51);
  pinMode(8, OUTPUT);
  pinMode(6, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(7, OUTPUT);
  pinMode(4, OUTPUT);
  servo_10.attach(10);
}

void loop(){
  digitalWrite(8, HIGH);
  analogWrite(6, 128);
  analogWrite(5, 51);
  digitalWrite(7, HIGH);
  tone(4, 262, 500);
  servo_10.write(90);
  rgb_display_2.setPixelColor(0, (((255 & 0xffffff) << 16) | ((128 & 0xffffff) << 8) | 0));
  rgb_display_2.show();
  rgb_display_2.setPixelColor(1, 0xFF0000);
  rgb_display_2.show();
  digitalWrite(5, LOW);
  digitalWrite(7, LOW);

}