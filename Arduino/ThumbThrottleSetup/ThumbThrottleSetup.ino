#include <WiFi.h>
#include <WiFiUdp.h>

const char* ssid = "MasterVR";
const char* password = "11112222";
const char* udpAddress = "192.168.0.188";  // CHANGE THIS, UNITY HEADSET IP
const int udpPort = 4210;

WiFiUDP udp;

const int throttlePin = 33;
const float voltageMin = 1.25;
const float voltageMax = 3.30;

void setup() {
  Serial.begin(115200);
  delay(500);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\n✅ Connected to Wi-Fi.");
  Serial.println(WiFi.localIP());
  udp.begin(udpPort);
}

void loop() {
  int rawADC = analogRead(throttlePin);
  float voltage = (rawADC / 4095.0) * 3.3;
  Serial.print("Raw ADC: ");
  Serial.print(rawADC);
  Serial.print(" | Voltage: ");
  Serial.print(voltage, 4);

  float normalized = (voltage - voltageMin) / (voltageMax - voltageMin);
  normalized = constrain(normalized, 0.0, 1.0);

  // Debug output
  
  Serial.print(" | Normalized: ");
  Serial.println(normalized, 3);


  // Format and send
  char buffer[32];
  snprintf(buffer, sizeof(buffer), "%.3f", normalized);
  udp.beginPacket(udpAddress, udpPort);
  udp.write((const uint8_t*)buffer, strlen(buffer));
  udp.endPacket();

  
  delay(200);  // 10 Hz
}
