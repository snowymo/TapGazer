/*
  Button

  Turns on and off a light emitting diode(LED) connected to digital pin 13,
  when pressing a pushbutton attached to pin 2.

  The circuit:
  - LED attached from pin 13 to ground
  - pushbutton attached to pin 2 from +5V
  - 10K resistor attached to pin 2 from ground

  - Note: on most Arduinos there is already an LED on the board
    attached to pin 13.

  created 2005
  by DojoDave <http://www.0j0.org>
  modified 30 Aug 2011
  by Tom Igoe

  This example code is in the public domain.

  http://www.arduino.cc/en/Tutorial/Button
*/

// constants won't change. They're used here to set pin numbers:
const int Rthumb = 7;     // the number of
const int Rindex = 8;     // the number of
const int Rmiddle = 9;     // the number of
const int Rring = 10;     // the number of
const int Rpinky = 11;     // the number of

const int LT = 6;     // the number of
const int LI = 5;     // the number of
const int LM = 4;     // the number of
const int LR = 3;     // the number of
const int LP = 2;     // the number of
const int ledPin =  13;      // the number of the LED pin

// variables will change:
int RthumbState = 0;         // variable for reading the pushbutton status
int RindexState = 0;         //
int RmiddleState = 0;         //
int RringState = 0;         //
int RpinkyState = 0;         //

int LTState = 0;         //
int LIState = 0;         //
int LMState = 0;         //
int LRState = 0;         //
int LPState = 0;         //

void setup() {
  //start serial connection
  Serial.begin(9600);//115200
  // initialize the LED pin as an output:
  pinMode(LED_BUILTIN, OUTPUT);
  // initialize the pushbutton pin as an input:
  pinMode(Rthumb, INPUT);
  pinMode(Rindex, INPUT);
  pinMode(Rmiddle, INPUT);
  pinMode(Rring, INPUT);
  pinMode(Rpinky, INPUT);

  pinMode(LT, INPUT);
  pinMode(LI, INPUT);
  pinMode(LM, INPUT);
  pinMode(LR, INPUT);
  pinMode(LP, INPUT);
}

void loop() {
  // read the state of the pushbutton value:
  RthumbState = digitalRead(Rthumb);
  RindexState = digitalRead(Rindex);
  RmiddleState = digitalRead(Rmiddle);
  RringState = digitalRead(Rring);
  RpinkyState = digitalRead(Rpinky);

  LTState = digitalRead(LT);
  LIState = digitalRead(LI);
  LMState = digitalRead(LM);
  LRState = digitalRead(LR);
  LPState = digitalRead(LP);
  Serial.flush();
  int LH = (LPState << 4) + (LRState << 3) + (LMState << 2) + (LIState << 1) + LTState;
  int RH = (RthumbState << 4) + (RindexState << 3) + (RmiddleState << 2) + (RringState << 1) + RpinkyState;
  int ret = (1 << 10) + (LH << 5) + RH;
  Serial.println(ret, BIN);
  //Serial.print(thumbState);Serial.print(",");Serial.print(indexState);Serial.print(",");Serial.print(middleState);Serial.print(",");Serial.print(ringState);Serial.print(",");Serial.print(pinkyState);Serial.print("\t");
  //Serial.print(RTState);Serial.print(",");Serial.print(RIState);Serial.print(",");Serial.print(RMState);Serial.print(",");Serial.print(RRState);Serial.print(",");Serial.print(RPState);Serial.print("\n");
  // check if the pushbutton is pressed. If it is, the buttonState is HIGH:
  //  if (indexState == HIGH || middleState == HIGH) {
  //    // turn LED on:
  //    digitalWrite(ledPin, HIGH);
  //  } else {
  //    // turn LED off:
  //    digitalWrite(ledPin, LOW);
  //  }

  //delay(1);        // delay in between reads for stability
}
