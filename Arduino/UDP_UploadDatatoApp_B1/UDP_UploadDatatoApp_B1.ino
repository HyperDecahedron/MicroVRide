#include <WiFi.h>
#include <WiFiUdp.h>

const char* ssid = "ASUS_VR";
const char* password = "MicroVRide";
const char* udpHost = "192.168.1.16";  // CHANGE THIS Unity headset IP
const int udpPort = 1234;

WiFiUDP udp;
const int fsrPins[4] = {32, 33, 34, 35};
int baselineFSR[4] = {0};
int maxFSR[4] = {1, 1, 1, 1};
const int samples = 30;

const char* boardName = "left";
const char* labels[4] = {"left_mid_l", "left_mid_r", "left_heel", "left_toe"};

bool calibrated = false;
bool ready = false;

void sendStatus(const char* status) {
  char buf[64];
  snprintf(buf, sizeof(buf), "status=%s&board=%s", status, boardName);
  udp.beginPacket(udpHost, udpPort);
  udp.print(buf);
  udp.endPacket();
}


/*void setup() {
  Serial.begin(115200);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) delay(500);
  udp.begin(udpPort);
  sendStatus("waiting_start");
  ready = true;
}*/

void setup() {
  Serial.begin(115200);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) delay(500);

  Serial.print("ESP32 IP address: ");
  Serial.println(WiFi.localIP());

  udp.begin(udpPort);
  sendStatus("waiting_start");
  ready = true;
}

void loop() {
  if (!calibrated) {
    int sz = udp.parsePacket();
    if (sz > 0) {
      char buf[64];
      udp.read(buf, sz);
      buf[sz] = 0;
      if (strcmp(buf, "start_prep") == 0) {
        sendStatus("prep_start");
      } else if (strcmp(buf, "start_baseline") == 0) {
        doBaselineCalibration();
      } else if (strcmp(buf, "start_max") == 0) {
        doMaxCalibration();
        calibrated = true;
      }
    }
    delay(10);
    return;
  }

  // Send sensor data
  int raw[4], corr[4];
  float norm[4];
  for (int i = 0; i < 4; i++) {
    raw[i] = analogRead(fsrPins[i]);
    corr[i] = max(raw[i] - baselineFSR[i], 0);
    norm[i] = (float)corr[i] / maxFSR[i];
  }

  char data[128];
  snprintf(data, sizeof(data),
    "fs-board=%s&%s_norm=%.3f&%s_norm=%.3f&%s_norm=%.3f&%s_norm=%.3f",
    boardName,
    labels[0], norm[0],
    labels[1], norm[1],
    labels[2], norm[2],
    labels[3], norm[3]
  );
  udp.beginPacket(udpHost, udpPort);
  udp.print(data);
  udp.endPacket();
  Serial.println("📤 Sent fs-board packet");
  delay(50);
}

void doBaselineCalibration() {
  sendStatus("baseline_start");
  memset(baselineFSR, 0, sizeof(baselineFSR));
  for (int i = 0; i < samples; i++) {
    for (int j = 0; j < 4; j++)
      baselineFSR[j] += analogRead(fsrPins[j]);
    delay(200);
  }
  for (int j = 0; j < 4; j++)
    baselineFSR[j] /= samples;
  sendStatus("baseline_done");
}

void doMaxCalibration() {
  sendStatus("max_start");
  memset(maxFSR, 0, sizeof(maxFSR));
  for (int i = 0; i < samples; i++) {
    for (int j = 0; j < 4; j++)
      maxFSR[j] += max(analogRead(fsrPins[j]) - baselineFSR[j], 0);
    delay(200);
  }
  for (int j = 0; j < 4; j++)
    maxFSR[j] = max(maxFSR[j] / samples, 1);
  sendStatus("max_done");
}
