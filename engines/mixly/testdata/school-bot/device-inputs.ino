void setup(){
  Serial.begin(9600);
  pinMode(2, INPUT);
  pinMode(4, INPUT);
}

void loop(){
  Serial.println(digitalRead(2));
  Serial.println(analogRead(A1));
  Serial.println(digitalRead(4));
  Serial.println(analogRead(A0));

}