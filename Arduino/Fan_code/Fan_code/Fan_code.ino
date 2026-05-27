#include <WiFi.h>
#include <WiFiUdp.h>
#include <ESP32Servo.h>
#include <Arduino.h>

// WIFI and UDP settings
//const char* ssid = "DemonNet";
//const char* password = "1234tongue";
const char* ssid = "XIAOYAN iPhone";
const char* password = "leahFan2328391";
const int UDP_PORT = 5052;
WiFiUDP udp;
char incomingPacket[255];

// Servo
const int servoPin = 12; // D14
Servo fanServo;

// Fan MOSFET control
const int fanPin = 14;       
const int fanFreq = 25000; // frequency of the PWM signal
const int fanResolution = 8; // 0-255 PWM range

// States
int target_angle = 90; 
int current_angle = 90;
unsigned long updateIntervalMs = 100; // change the servo angle 1 degree every update_time ms 

unsigned long lastServoUpdate = 0;

void UpdateServoAngle() {
  unsigned long now = millis();

  if (target_angle != current_angle && (now - lastServoUpdate >= updateIntervalMs)) {

    if (target_angle > current_angle) {
      current_angle++;
      fanServo.write(current_angle);
    }
    else if (target_angle < current_angle) {
      current_angle--;
      fanServo.write(current_angle);
    }

    lastServoUpdate = now;
  }
}

void setup() {
  Serial.begin(115200);

  // Connect to WiFi
  Serial.println("Connecting to WiFi...");
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("\nConnected!");
  Serial.print("ESP32 IP address: ");
  Serial.println(WiFi.localIP());

  // Start UDP
  udp.begin(UDP_PORT);
  Serial.printf("Listening for UDP packets on port %d\n", UDP_PORT);

  // Servo setup
  ESP32PWM::allocateTimer(0);
  fanServo.setPeriodHertz(50);
  fanServo.attach(servoPin, 500, 2400);
  fanServo.write(90);

  // Fan PWM setup
  ledcAttach(fanPin, fanFreq, fanResolution);
  ledcWrite(fanPin, 0);
}

void loop() {

  UpdateServoAngle();

  int packetSize = udp.parsePacket();
  if (packetSize) {

    int len = udp.read(incomingPacket, 254);
    if (len > 0) {
      incomingPacket[len] = 0;
    }

    Serial.print("Received: ");
    Serial.println(incomingPacket);

    float fan_angle;
    float fan_speed;

    if (sscanf(incomingPacket, "%f,%f", &fan_angle, &fan_speed) == 2) {
      
      // the udp sender states the target angle
      target_angle = constrain((int)fan_angle, 10, 170); 

      // Speed 
      fan_speed = constrain(fan_speed, 0.0, 1.0);
      int pwmValue = (int)(fan_speed * 255.0);
      ledcWrite(fanPin, pwmValue);
    }
  }
}