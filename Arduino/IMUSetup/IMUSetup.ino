#include <Wire.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>

// Wi-Fi credentials
//const char* ssid = "ASUS_VR";
//const char* password = "MicroVRide";
const char* ssid = "MasterVR";
const char* password = "11112222";

// UDP target
const char* udpHost = "192.168.0.188";  // CHANGE THIS, UNITY HEADSET IP
const int udpPort = 1235;

WiFiUDP udp;
Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x28);  // Default I2C address

// Variables to store offset
imu::Vector<3> baseEuler;

void setup() {
  Serial.begin(115200);
  Wire.begin(21, 22);  // SDA = 21, SCL = 22 (for ESP32)

  // === Initialize BNO055 ===
  if (!bno.begin()) {
    Serial.println("❌ BNO055 not detected. Check wiring!");
    while (1);
  }
  delay(1000);
  bno.setExtCrystalUse(true);
  Serial.println("✅ BNO055 initialized.");

  // === Wait for device to settle flat ===
  Serial.println("🕐 Zeroing IMU — Please keep it flat and still...");
  delay(3000);
  baseEuler = bno.getVector(Adafruit_BNO055::VECTOR_EULER);
  Serial.print("✅ IMU zeroed to: ");
  Serial.print("Roll: "); Serial.print(baseEuler.x());
  Serial.print(" | Pitch: "); Serial.print(baseEuler.y());
  Serial.print(" | Yaw: "); Serial.println(baseEuler.z());

  // === Connect Wi-Fi ===
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\n✅ Wi-Fi connected.");
  Serial.print("IP: "); Serial.println(WiFi.localIP());

  udp.begin(udpPort);
}

void loop() {
  // Read raw Euler angles
  imu::Vector<3> euler = bno.getVector(Adafruit_BNO055::VECTOR_EULER);

  // Subtract baseline orientation
  float roll = euler.x() - baseEuler.x();
  float pitch = euler.y() - baseEuler.y();
  float yaw = euler.z() - baseEuler.z();

  // Normalize to [-180, 180]
  if (roll > 180) roll -= 360;
  if (pitch > 180) pitch -= 360;
  if (yaw > 180) yaw -= 360;
  if (roll < -180) roll += 360;
  if (pitch < -180) pitch += 360;
  if (yaw < -180) yaw += 360;

  // Format UDP message
  char buf[128];
  snprintf(buf, sizeof(buf),
           "imu-roll=%.2f&imu-pitch=%.2f&imu-yaw=%.2f",
           roll, pitch, yaw);

  // Send UDP
  udp.beginPacket(udpHost, udpPort);
  udp.print(buf);
  udp.endPacket();

  Serial.println(buf);
  delay(50);  // 20 Hz
}
