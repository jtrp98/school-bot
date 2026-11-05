volatile int score;

void setup(){
  score = 0;
  Serial.begin(9600);
}

void loop(){
  score = score + 2;
  if (String("hello").indexOf(String("ell")) != -1) {
    Serial.println(String("hello").charAt(0));
    Serial.println(String("hello").charAt(score - 1));

  }

}