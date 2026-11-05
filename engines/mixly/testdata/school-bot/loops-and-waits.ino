void setup(){
  Serial.begin(9600);
}

void loop(){
  for (int i = 0; i < 3; i++) {
    for (int i2 = 0; i2 < 2; i2++) {
      Serial.println("hi");
    }
  }
  while (!(random(1, 7) > 3)) {
  }
  while (true) {
    delay(500);
  }

}