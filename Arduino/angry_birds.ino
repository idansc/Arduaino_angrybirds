#include <SPI.h>
#include <Ethernet.h>
#include <Servo.h>
#define ANGRY_BIRDS_VERSION "v0.1"
#define FORWARD 0
#define BACKWARD 1

// Constants

// TODO(matan): Get the shield's actual mac address
static byte MAC_ADDRESS[] = {
  0xDE, 0xBD, 0xBE, 0xEF, 0xBA, 0xBE
};

// Pin constants
static const byte PIG1_PIN = 9;
static const byte PIG2_PIN = 7;
static const byte PIG3_PIN = 8;
static const byte RAIL_DIR_PIN = 6;
static const byte RAIL_STEP_PIN = 5;
static const byte CAT_ENABLE_PIN = 2;
static const byte CAT_MOT1_PIN = 3;
static const byte CAT_MOT2_PIN = 4;

// other constants
static const long RAIL_STEP_INTERVAL = 10;
static const int RAIL_STEP_MAX = 450;
static const int SOLENOID_STEP_AMOUNT = 100;
static const int FIRE_ON_INTERVAL_HARD = 180;
static const int FIRE_ON_INTERVAL = 120;
static const int FIRE_DELAY_INTERVAL = 2000;

// Global vars
int g_step_state = LOW;
int g_dir_state = HIGH;
int g_rail_target = 0;
int g_rail_current = 0;
int g_trigger_state = 0;
int g_fire_interval = FIRE_ON_INTERVAL;
byte g_pig_ff[3] = {0,};

// Objects
EthernetServer server(80);

void setup() {
  // Ethernet setup
  Serial.begin(9600);
  while (!Serial);
  Serial.println("BOOT");
  Serial.println("Getting IP via DHCP...");
  if (Ethernet.begin(MAC_ADDRESS) == 0) {
    Serial.println("DHCP failed");
    for (;;);
  }
  
  Serial.println("Angry Birds " ANGRY_BIRDS_VERSION " says Hello!");
  Serial.print("Ethernet server is at ");
  for (byte i; i < 4; ++i) {
    Serial.print(Ethernet.localIP()[i], DEC);
    Serial.print(".");
  }
  Serial.println();
  server.begin();
  Serial.println("Server begun.");
  pinMode(RAIL_DIR_PIN, OUTPUT);
  pinMode(RAIL_STEP_PIN, OUTPUT);
  pinMode(PIG1_PIN, INPUT);
  pinMode(PIG2_PIN, INPUT);
  pinMode(PIG3_PIN, INPUT);
  pinMode(CAT_ENABLE_PIN, OUTPUT);
  pinMode(CAT_MOT1_PIN, OUTPUT);
  pinMode(CAT_MOT2_PIN, OUTPUT);
  digitalWrite(RAIL_DIR_PIN, g_dir_state);
  digitalWrite(RAIL_STEP_PIN, g_step_state);
  Serial.println("Done setup!");
}

int toggle_state(int prev_state) {
  if (prev_state == LOW)
    return HIGH;
  return LOW;
}

void process_command() {
  EthernetClient client = server.available();
  if (!client)
    return;
  Serial.println("Got client.");
  while (!client.available());
  Serial.println("Got input!");
  char input[5] = {0,};
  for (byte i = 0; i < sizeof(input); ++i) {
    input[i] = client.read();
    if (input[i] == '\n')
      continue;
  }
  input[4] = '\0';
  Serial.print("Input is: ");
  Serial.println(input);
  int motion = 0;
  char output[4];
  output[3] = '\0';
  switch (input[0]) {
    case 'r':
      Serial.println("Got read cmd.");
      output[0] = g_pig_ff[0] + '0';
      output[1] = g_pig_ff[1] + '0';
      output[2] = g_pig_ff[2] + '0';
      output[0] =  + '0';
      output[1] = (!digitalRead(PIG2_PIN)) + '0';
      output[2] = (!digitalRead(PIG3_PIN)) + '0';
      Serial.print("Sent: ");
      Serial.println(output);
      client.write(output);
      break;
    case 'm':
      Serial.println("Got move cmd.");
      motion = atoi(input + 1);
      if (motion < 0) motion = 0;
      if (motion > RAIL_STEP_MAX) motion = RAIL_STEP_MAX;
      g_rail_target = motion;
      Serial.print("Move to: ");
      Serial.println(motion);
      break;
    case 'f':
      Serial.println("Got FIRE cmd!");
      g_trigger_state = 1;
      break;
    default:
      break;
  }
}

void do_move_linear_rail() {
  static long prev_millis = 0;
  long curr_millis = millis();
  
  if (curr_millis - prev_millis < RAIL_STEP_INTERVAL)
    return;
  prev_millis = curr_millis;
  
  if (g_rail_target == g_rail_current)
    return;
  if (g_rail_target > g_rail_current) {
    g_rail_current++;
    digitalWrite(RAIL_DIR_PIN, HIGH);  
  } else {
    g_rail_current--;
    digitalWrite(RAIL_DIR_PIN, LOW);
  }
  
  g_step_state = toggle_state(g_step_state);
  digitalWrite(RAIL_STEP_PIN, g_step_state);
}

void start_fire(int dir) {
  switch (dir) {
  case FORWARD:
    digitalWrite(CAT_MOT1_PIN, HIGH);
    digitalWrite(CAT_MOT2_PIN, LOW);
    break;
  case BACKWARD:
    digitalWrite(CAT_MOT1_PIN, LOW);
    digitalWrite(CAT_MOT2_PIN, HIGH);
    break;
  }
  digitalWrite(CAT_ENABLE_PIN, HIGH);
}
void stop_fire() {
  digitalWrite(CAT_ENABLE_PIN, LOW);
}

void do_fire() {
  static long prev_millis = 0;
  long curr_millis = millis();
  switch (g_trigger_state) {
  case 0: // STATE 0 - NO FIRE
    break;
  case 1: // STATE 1 - WAIT FOR RAIL
    if (g_rail_target == g_rail_current)
      g_trigger_state = 2;
    break;
  case 2: // STATE 2 - FIRE!
    g_pig_ff[0] = 0;
    g_pig_ff[1] = 0;
    g_pig_ff[2] = 0;
    start_fire(FORWARD);
    prev_millis = curr_millis;
    g_fire_interval = g_rail_current >> 1;
    g_trigger_state = 3;
    break;
  case 3: // STATE 3 - WAIT FIRE_ON_INTERVAL ms, THEN STOP
    if (curr_millis - prev_millis < g_fire_interval)
      break;
    stop_fire();
    prev_millis = curr_millis;
    g_trigger_state = 4;
    break;
  case 4: // STATE 4 - WAIT FIRE_DELAY_INTERVAL ms
    if (curr_millis - prev_millis < FIRE_DELAY_INTERVAL)
      break;
    prev_millis = curr_millis;
    g_trigger_state = 5;
    break;
  case 5: // STATE 5 - FIRE BACKWARDS! (return slingshot)
    start_fire(BACKWARD);
    prev_millis = curr_millis;
    g_trigger_state = 6;
  case 6: // STATE 6 - WAIT FIRE_ON_INTERVAL ms, THEN STOP & RESET
    if (curr_millis - prev_millis < g_fire_interval)
      break;
    stop_fire();
    g_trigger_state = 0;
    break;
  default:
    g_trigger_state = 0;
  }
}

void do_read_pigs() {
  byte reading[3];
  reading[0] = (!digitalRead(PIG1_PIN));
  reading[1] = (!digitalRead(PIG2_PIN));
  reading[2] = (!digitalRead(PIG3_PIN));
  for (int i = 0; i < 3; ++i) {
    if (0 == g_pig_ff[i]) {
      if (reading[i] == 1) {
        g_pig_ff[i] = 1;
      }
    } else if (1 == g_pig_ff[i]) {
      if (reading[i] == 0) {
        g_pig_ff[i] = 2;
      } 
    }
    
  } 
}

void do_actions() {
  process_command();
  do_move_linear_rail();
  do_fire();
  do_read_pigs();
}

void loop() {
  do_actions();
}
