#include <WiFi.h>
#include <WiFiUdp.h>
#include <ESP32Servo.h>
#include <Arduino.h>

const char* ssid = "DemonNet";
const char* password = "1234tongue";

const int UDP_PORT = 5052;

WiFiUDP udp;

const int servoPin = 14; // D14
Servo fanServo;

// Fan MOSFET control
const int fanPin = 26;       
const int fanFreq = 25000; // frequency of the PWM signal
const int fanResolution = 8; // 0-255 PWM range

char incomingPacket[255];

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

      Serial.print("Fan angle: ");
      Serial.println(fan_angle);

      Serial.print("Fan speed: ");
      Serial.println(fan_speed);

      // Servo angle
      fanServo.write((int)fan_angle);

      // Speed 
      fan_speed = constrain(fan_speed, 0.0, 1.0);
      int pwmValue = fan_speed * 255;
      ledcWrite(fanPin, pwmValue);
    }
  }
}