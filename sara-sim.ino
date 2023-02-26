#include <ArduinoJson.h>
#include <Wire.h>
#include <Adafruit_PWMServoDriver.h>
#include <EEPROM.h>
#include <StreamUtils.h>

Adafruit_PWMServoDriver pwm = Adafruit_PWMServoDriver();

#define BAUD_RATE 9600

#define PWM_FREQ 60

#define STATE_EEPROM_SIZE 256 /* size of eeprom size */

#define SERVO_PIN_0 "0" /* Base     */
#define SERVO_PIN_1 "1" /* Shoulder */
#define SERVO_PIN_2 "2" /* Elbow    */
#define SERVO_PIN_3 "3" /* Forearm  */
#define SERVO_PIN_4 "4" /* Wrist    */
#define SERVO_PIN_5 "5" /* Claw     */

#define SERVO_TYPE_270 1
#define SERVO_TYPE_180 0

/* JSON keys for EEPROM state */
#define JSON_KEY_SERVO_ARRAY F("srv")
  #define SERVO_IDX_MOTOR_TYPE 0
  #define SERVO_IDX_LIMIT_MIN 1
  #define SERVO_IDX_LIMIT_MAX 2
  #define SERVO_IDX_RESET_ANGLE 3
#define JSON_KEY_SERVO_PWM_MIN F("pmi")
#define JSON_KEY_SERVO_PWM_MAX F("pma")

/* JSON keys for command request packet */
#define JSON_KEY_COMMAND_REQUEST_TYPE "cmd"
#define JSON_KEY_COMMAND_REQUEST_DATA "data"

/* JSON keys for command response packet */
#define JSON_KEY_COMMAND_RESPONSE_STATUS F("status")
#define JSON_KEY_COMMAND_RESPONSE_MESSAGE F("msg")
#define JSON_KEY_COMMAND_RESPONSE_DATA F("data")

/* REQUEST for command init_servo */
#define COMMAND_INIT_SERVO 0
#define JSON_KEY_INIT_SERVPIN_O F("pin")
#define JSON_KEY_INIT_SERVO_MOTOR_TYPE F("mot")
#define JSON_KEY_INIT_SERVO_LIMIT_MIN F("lmi")
#define JSON_KEY_INIT_SERVO_LIMIT_MAX F("lma")
#define JSON_KEY_INIT_SERVO_RESET_ANGLE F("rsa")

/* REQUEST for command: set_servo_angle */
#define COMMAND_SET_SERVO_ANGLE 1
#define JSON_KEY_SET_SERVPIN_O F("pin")
#define JSON_KEY_SET_SERVO_ANGLE F("deg")

/* RESPONSE for command: get_servo_status */
#define COMMAND_GET_SERVO_STATUS 2
#define JSON_KEY_GET_SERVO_STATUPIN_S F("pin")
#define JSON_KEY_GET_SERVO_STATUS_MOTOR_TYPE F("mot")
#define JSON_KEY_GET_SERVO_STATUS_LIMIT_MIN F("lmi")
#define JSON_KEY_GET_SERVO_STATUS_LIMIT_MAX F("lma")
#define JSON_KEY_GET_SERVO_STATUS_RESET_ANGLE F("rsa")

/* REQUEST for command: return_to_base */
#define COMMAND_RTB 3

/* REQUEST for command: return_to_base */
#define COMMAND_GET_EEPROM_STATE 4

/* JSON endpoint handlers */
JsonDocument &handle_set_servo_angle(JsonDocument &doc, JsonDocument &response);
JsonDocument &handle_init_servo(JsonDocument &doc, JsonDocument &response);
JsonDocument &handle_get_servo_state(JsonDocument &doc, JsonDocument &response);
JsonDocument &handle_return_to_base(JsonDocument &response);
JsonDocument &handle_get_eeprom_state(JsonDocument &doc);

char eeprom_write_state(JsonDocument &doc);
char eeprom_read_state(JsonDocument &doc);
char eeprom_read_state(JsonDocument &doc, DeserializationOption::Filter filter);
char eeprom_memset(const char c = '\0');

void init_state(JsonDocument &root);
void init_servo(JsonArray &root, uint8_t pin, uint8_t motor_type, int16_t limit_min, int16_t limit_max, int16_t reset_angle);

void get_command();

uint8_t hw_set_servo_angle(int16_t angle, uint8_t pin, int16_t limit_min, int16_t limit_max, uint8_t motor_type, int16_t pulse_width_min, int16_t pulse_width_max, int16_t reset_angle);

int16_t CurrentPulseWidths[6] = { 0 };

void setup() {
  Serial.begin(BAUD_RATE);
  // Serial.println(F("SARAsim initiated."));

  pwm.begin();
  pwm.setPWMFreq(60);
  // Serial.println(F("SARAsim PWM initialized @ 60 HZ."));

  // temp: reset eeprom and initialize
  eeprom_memset();
  {
    StaticJsonDocument<330> json;
    init_state(json);
    eeprom_write_state(json);
  }
  {
    StaticJsonDocument<0> null;
    handle_return_to_base(null);
  }
}

void loop() {
  get_command();

  /* maintain pwm signal for all pins inside loop */
  for (int i = 0; i < 6; ++i) {
    hw_set_pwm(i, CurrentPulseWidths[i]);
  }

  delay(10);
}

/*
 * Store Json document in MessagePack format on Uno's EEPROM.
 */
char eeprom_write_state(JsonDocument &doc) {
  EepromStream eepromStream(0, STATE_EEPROM_SIZE);
  return serializeMsgPack(doc, eepromStream);
}

/*
 * Retrieve MessagePack entry from Uno's EEPROM and deserialize.
 */
char eeprom_read_state(JsonDocument &doc) {
  EepromStream eepromStream(0, EEPROM.length());
  DeserializationError error = deserializeMsgPack(doc, eepromStream);

  if (error)
    return -1;

  return 0;
}

char eeprom_read_state(JsonDocument &doc, DeserializationOption::Filter filter) {
  EepromStream eepromStream(0, EEPROM.length());
  DeserializationError error = deserializeMsgPack(doc, eepromStream, filter);

  if (error)
    return -1;

  return 0;
}

char eeprom_memset(const char c = '\0') {
  for (short i = 0; i < EEPROM.length(); ++i) {
    EEPROM.write(i, c);
  }
}

inline void init_servo(JsonObject &root, const char *pin, uint8_t motor_type, int16_t limit_min, int16_t limit_max, int16_t reset_angle) {
  JsonArray servo = root.createNestedArray(pin);
  servo.add(motor_type);
  servo.add(limit_min);
  servo.add(limit_max);
  servo.add(reset_angle);
}

inline void init_state(JsonDocument &root) {
  JsonObject servos = root.createNestedObject(JSON_KEY_SERVO_ARRAY);

  init_servo(servos, SERVO_PIN_0, /* servo motor angle limit: */ SERVO_TYPE_270, /* user-defined angle limits: */ 0, 270, /* reset angle: */ 135);
  init_servo(servos, SERVO_PIN_1, /* servo motor angle limit: */ SERVO_TYPE_270, /* user-defined angle limits: */ 0, 180, /* reset angle: */ 37);
  init_servo(servos, SERVO_PIN_2, /* servo motor angle limit: */ SERVO_TYPE_180, /* user-defined angle limits: */ 0, 180, /* reset angle: */ 100);
  init_servo(servos, SERVO_PIN_3, /* servo motor angle limit: */ SERVO_TYPE_180, /* user-defined angle limits: */ 0, 170, /* reset angle: */ 150);
  init_servo(servos, SERVO_PIN_4, /* servo motor angle limit: */ SERVO_TYPE_180, /* user-defined angle limits: */ 0, 180, /* reset angle: */ 90);
  init_servo(servos, SERVO_PIN_5, /* servo motor angle limit: */ SERVO_TYPE_180, /* user-defined angle limits: */ 0, 35, /* reset angle: */ 0);

  root[JSON_KEY_SERVO_PWM_MAX] = 600;
  root[JSON_KEY_SERVO_PWM_MIN] = 150;
}

void get_command() {
  if (Serial.available() > 0) {
    StaticJsonDocument<400> doc;
    DeserializationError error = deserializeJson(doc, Serial);
    JsonObject obj = doc.as<JsonObject>();

    if (error) {
      // Serial.print(F("deserializeJson() failed: "));
      // Serial.println(error.f_str());
      return;
    }

    uint8_t command_type = obj["cmd"];
    JsonObject command_data = obj["data"].as<JsonObject>();

    // Serial.print(F("Command switch handler for type: "));
    // Serial.println(command_type);

    StaticJsonDocument<330> response;
    switch (command_type) {
      case COMMAND_SET_SERVO_ANGLE:
        // Serial.println(F("Received set_servo_angle command"));
        handle_set_servo_angle(command_data, response);
        break;
      case COMMAND_INIT_SERVO:
        // Serial.println(F("Received init_servo command"));
        handle_init_servo(command_data, response);
        break;
      case COMMAND_GET_SERVO_STATUS:
        // Serial.println(F("Received get_servo_status command"));
        handle_get_servo_state(command_data, response);
        break;
      case COMMAND_RTB:
        // Serial.println(F("Received return_to_base command"));
        handle_return_to_base(response);
        break;
      case COMMAND_GET_EEPROM_STATE:
        // Serial.println(F("Received get_eeprom_state command"));
        handle_get_eeprom_state(response);
        break;
      default:
        // Serial.println(F("Received unknown command"));
        response[F("status")] = 0;
        break;
    }

    // encode response to JSON string
    // serializeJson(response, Serial);
  }
}

JsonDocument &handle_set_servo_angle(JsonObject &request_data, JsonDocument &response) {
  const char *pin = request_data[F("pin")];
  int16_t angle = request_data[F("deg")];

  // create filter for pin
  StaticJsonDocument<100> filter;
  filter[F("srv")][pin] = true;
  filter[F("srv")][pin] = true;
  filter[F("pma")] = true;
  filter[F("pmi")] = true;

  // grab the cache from EEPROM
  StaticJsonDocument<200> cache;
  eeprom_read_state(cache, DeserializationOption::Filter(filter));

  // update servo angle
  int16_t reset_angle = cache[F("srv")][pin][SERVO_IDX_RESET_ANGLE];
  int16_t limit_min = cache[F("srv")][pin][SERVO_IDX_LIMIT_MIN];
  int16_t limit_max = cache[F("srv")][pin][SERVO_IDX_LIMIT_MAX];
  int8_t motor_type = cache[F("srv")][pin][SERVO_IDX_MOTOR_TYPE];
  int16_t pulse_width_min = cache[F("pmi")];
  int16_t pulse_width_max = cache[F("pma")];
  hw_set_servo_angle(angle, atoi(pin), limit_min, limit_max, motor_type, pulse_width_min, pulse_width_max, reset_angle);

  // construct success response
  response[F("status")] = 1;
  return response;
}

JsonDocument &handle_get_servo_state(JsonObject &request_data, JsonDocument &response) {
  // stub
}

JsonDocument &handle_init_servo(JsonObject &request_data, JsonDocument &response) {
  // stub
}

JsonDocument &handle_return_to_base(JsonDocument &response) {
  // read cache
  StaticJsonDocument<330> cache;
  eeprom_read_state(cache);

  uint16_t pulse_width_min = cache[F("pmi")];
  uint16_t pulse_width_max = cache[F("pma")];

  // clamp angle against angle limits
  JsonObject servo_dict = cache[F("srv")].as<JsonObject>();
  for (JsonPair kv : servo_dict) {
    const char *pin = kv.key().c_str();
    JsonArray servo = kv.value().as<JsonArray>();

    int16_t reset_angle = servo[SERVO_IDX_RESET_ANGLE];
    int16_t limit_min = servo[SERVO_IDX_LIMIT_MIN];
    int16_t limit_max = servo[SERVO_IDX_LIMIT_MAX];
    int8_t motor_type = servo[SERVO_IDX_MOTOR_TYPE];

    hw_set_servo_angle(reset_angle, atoi(pin), limit_min, limit_max, motor_type, pulse_width_min, pulse_width_max, reset_angle);
  }
  response[F("status")] = 1;
}

JsonDocument &handle_get_eeprom_state(JsonDocument &response) {
  eeprom_read_state(response);
}

inline uint8_t hw_set_pwm(uint8_t pin, int16_t pulse_width) {
  pwm.setPWM(pin, 0, pulse_width);
}

/* Hardware interface - update servo's angle */
inline uint8_t hw_set_servo_angle(
  int16_t angle,
  uint8_t pin,
  int16_t limit_min,
  int16_t limit_max,
  uint8_t motor_type,
  int16_t pulse_width_min,
  int16_t pulse_width_max,
  int16_t reset_angle) {
  /* clamp angle against user defined angle limits */
  angle = constrain(angle, limit_min, limit_max);

  /* create a PWM signal */
  int16_t angle_max = motor_type ? 270 : 180;
  int16_t pulse_width = map(angle, 0, angle_max, pulse_width_min, pulse_width_max);

  /* update global state of current pulse widths */
  CurrentPulseWidths[pin] = pulse_width;

  /* send hardware update */
  hw_set_pwm(pin, pulse_width);
}
