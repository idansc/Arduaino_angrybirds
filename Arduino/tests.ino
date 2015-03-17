#include "Servo.h"

// Pin constants
static const int PIG1_PIN = 9;
static const int PIG2_PIN = 7;
static const int PIG3_PIN = 8;
static const int RAIL_DIR_PIN = 6;
static const int RAIL_STEP_PIN = 5;
static const int SERVO_PIN = 9;

// other constants
static const long RAIL_STEP_INTERVAL = 10;
static const int RAIL_STEP_AMOUNT = 450;
static const int SOLENOID_STEP_AMOUNT = 100;
static const int SERVO_SET = 15;
static const int SERVO_FIRE = 100;

// Global vars
int g_step_state = LOW;
int g_dir_state = HIGH;
long g_prev_millis = 0;
int g_step_counter = 0;
int g_solenoid_counter = 0;
int g_pig_debug_counter = 0;

// Objects
//Servo trigger;

void setup() {
  pinMode(RAIL_DIR_PIN, OUTPUT);
  pinMode(RAIL_STEP_PIN, OUTPUT);
  pinMode(SERVO_PIN, OUTPUT);
  pinMode(PIG1_PIN, INPUT);
  pinMode(PIG2_PIN, INPUT);
  pinMode(PIG3_PIN, INPUT);
  digitalWrite(RAIL_DIR_PIN, g_dir_state);
  digitalWrite(RAIL_STEP_PIN, g_step_state);
  //trigger.attach(SERVO_PIN);
  //trigger.write(SERVO_SET);
  Serial.begin(9600);
  Serial.println("Done setup!");
}

int toggle_state(int prev_state) {
  if (prev_state == LOW)
    return HIGH;
  return LOW;
}

void pig_debug_pin(int pig_pin) {
  Serial.print("Pig pin number ");
  Serial.print(pig_pin);
  Serial.print(" is:");
  if (digitalRead(pig_pin) == HIGH)
    Serial.println("HIGH");
  else
    Serial.println("LOW");
}
void pig_debug() {
  pig_debug_pin(PIG1_PIN);
  pig_debug_pin(PIG2_PIN);
  pig_debug_pin(PIG3_PIN); 
}

void loop() {
  long cur_millis = millis();

  if (cur_millis - g_prev_millis > RAIL_STEP_INTERVAL) {
    //Serial.println("Start step");
    g_prev_millis = cur_millis;
    g_step_state = toggle_state(g_step_state);
    digitalWrite(RAIL_STEP_PIN, g_step_state);

    ++g_step_counter;
    if (g_solenoid_counter > 0) {
      --g_solenoid_counter;
      //trigger.write(SERVO_FIRE);
    }
    
    if (g_pig_debug_counter == 0) {
      g_pig_debug_counter = 40;
      pig_debug(); 
    }
    --g_pig_debug_counter;
    
    if (g_step_counter >= RAIL_STEP_AMOUNT) {
      g_step_counter = 0;
      g_dir_state = toggle_state(g_dir_state);
      digitalWrite(RAIL_DIR_PIN, g_dir_state);
      if (g_dir_state == LOW) {
        g_solenoid_counter = SOLENOID_STEP_AMOUNT;
      }
    }
    
    if (g_solenoid_counter <= 0) {
      //trigger.write(SERVO_SET);
    }
  }
}
