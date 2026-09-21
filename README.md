# MicroVride — VR Micromobility & Cybersickness Simulator

This project contains MicroVride, a VR micromobility simulator, that has been further developed to investigate **cybersickness mitigation techniques**.

The project supports four micromobility vehicles:

* Electric scooter
* Segway
* Electric unicycle
* One-wheel skateboard

Each vehicle can be tested under four cybersickness-mitigation conditions:

| Condition                | Code     |
| ------------------------ | -------- |
| No mitigation            | `none`   |
| Airflow                  | `af`     |
| Virtual helmet           | `hel`    |
| Airflow + virtual helmet | `af_hel` |

The simulator also integrates physical sensors and actuators through **ESP32/Arduino hardware and UDP communication**, including IMU, throttle, foot-pressure sensors, and an airflow device.

---

## Project Overview

The simulator is designed around a VR experimental workflow:

1. Start the application.
2. Immediatly after starting the app, calibrate the physical foot-pressure sensors.
3. Select a cybersickness-mitigation condition.
4. Select one of the four vehicles.
5. Complete the corresponding training/simulation experience by reaching the finish line.
6. Collect movement, coin/pickup, and session telemetry. Saved automatically when reaching the finish line.

---

## Vehicles

The vehicles are already defined in the project, but they can be easily customized by their correspondent component in the inspector. 

### Electric Scooter

Controlled primarily using:

* IMU data for steering.
* Thumb-throttle data for acceleration.

The scooter controller includes configurable:

* Maximum speed
* Maximum steering angle
* Steering deadzone
* Steering sensitivity
* Steering inversion
* Throttle deadzone
* Expo curves
* Filtering
* Slew-rate limiting
* Speed-dependent steering
* Collision recovery
* Debug speed override

The repository also contains an optional keyboard-debug mode using **W/A/S/D**.

### Segway

The Segway uses a combination of:

* Foot-pressure/FSR sensors for forward and backward movement.
* IMU yaw for steering.

Forward/backward intent is derived from the difference between normalized toe and heel pressure.

The controller includes:

* Forward/backward deadzones
* Hysteresis
* Speed shaping
* Steering sensitivity
* Steering filtering
* Speed-dependent steering
* Collision detection and recovery
* Telemetry logging

### Electric Unicycle

The electric unicycle uses the IMU for vehicle control.

By default:

* IMU pitch → forward/backward speed
* IMU yaw → steering

The relevant mappings are configurable in `ElectricUnicycleController`.

### One-Wheel Skateboard

The one-wheel skateboard uses the IMU for vehicle control.

By default:

* IMU pitch → forward/backward movement
* IMU yaw → steering/carving

The controller additionally contains asymmetric forward/reverse speed mappings and separate forward/reverse acceleration parameters.

---

# Cybersickness Mitigation

The project implements four experimental conditions.

## 1. No mitigation

Code:

```text
none
```

Neither the airflow device nor the virtual helmet is enabled.

## 2. Airflow

Code:

```text
af
```

The physical airflow system is enabled.

`FanController.cs` calculates the required airflow direction and intensity based on vehicle movement and turning, then communicates with an external ESP32 through UDP.

## 3. Virtual helmet

Code:

```text
hel
```

The virtual helmet GameObject is enabled inside the simulator.

## 4. Airflow + virtual helmet

Code:

```text
af_hel
```

Both the virtual helmet and physical airflow system are enabled.

The mitigation condition is selected in the start scene by `StartCSManager.cs` and stored in:

```csharp
SessionState.CSTechnique
```

The available values are:

```text
none
af
hel
af_hel
```

`TechniquesEnabler.cs` then enables or disables the corresponding effects when the simulation scene starts.

---


# Unity Version

The repository specifies:

```text
Unity 2021.3.35f1
```

with revision:

```text
157b46ce122a
```

Using the same Unity version is recommended to minimize package and serialization compatibility problems.

---

# Running the Deployed Application

This section describes how to operate the **deployed MicroVride application directly in the VR headset** during an experimental session.

## 1. Prepare the vehicle

Prepare the physical simulator for the vehicle that will be tested.

* Mount the appropriate physical interface:

  * **Handlebar** for vehicles that use the handlebar setup (e-scooter, segway).
  * **Hemispherical balance base** for vehicles that use the balance-base setup (electric unicycle, one-wheel skateboard).
* Connect the required sensors:

  * Throttle
  * IMU
  * Foot-pressure sensors
* Make sure all sensors and hardware are securely connected before starting the application.

## 2. Prepare the airflow system

* Place the airflow accessory electronics inside the **fanny bag** (it is better to have this prepared beforehand).
* Wear the fanny bag securely.
* Make sure the fan is properly mounted on the VR headset.
* Turn on the fan battery.

## 3. Connect the headset to the laboratory Wi-Fi

Connect the VR headset to the laboratory Wi-Fi network:

```text
MasterVR
```

## 4. Open the application

Launch the deployed **MicroVride** application from the VR headset.

## 5. Center the VR view

Immediately after opening the application, recenter the headset view.

To do this:

**Long-click the Meta button** on the VR controller.

Center looking forward for Segway and Electric Unicycle. Center looking to your right for E-Scooter and One-wheel Skateboard.

## 6. Calibrate the foot sensors — Segway only

If the **Segway** is being tested, calibrate the foot-pressure sensors immediately after opening the application.

When the application displays:

> **Please stand on the board**

align your feet naturally on the foot insoles and follow the calibration procedure shown by the application.


## 7. Select the cybersickness mitigation technique

From the application menu, select **one** of the available cybersickness mitigation conditions.

## 8. Select the vehicle

Select the vehicle that is going to be tested.

After selecting a vehicle, the application will load the corresponding **training scene**.

## 9. Start the game

Remain in the training scene until you are comfortable with the vehicle and its controls.

When ready, **right-click/up-right click the button labeled `Start Game`** to begin the simulation.

## 10. Complete the game

Complete the simulated course and cross the **finish line**.

After crossing the finish line, the application will automatically return to the **vehicle selection menu**.

## 11. Airflow operation

When the airflow system is correctly connected, airflow is controlled automatically by the application.

The airflow system:

* Starts and stops automatically when entering and leaving the relevant scenes.
* Provides airflow **only while the vehicle is moving forward**.
* Changes its direction according to the direction in which the vehicle is moving/turning.

No manual activation of the airflow system is required during normal operation.

---

# Build Scenes

The project's active Unity build configuration contains the following scenes:

```text
Assets/MicroVride Scenes/Start.unity

Assets/MicroVride Scenes/EscooterTraining.unity
Assets/MicroVride Scenes/SegwayTraining.unity
Assets/MicroVride Scenes/UnicycleTraining.unity
Assets/MicroVride Scenes/SkateboardTraining.unity

Assets/MicroVride Scenes/EscooterSimulator.unity
Assets/MicroVride Scenes/SegwaySimulator.unity
Assets/MicroVride Scenes/UnicycleSimulator.unity
Assets/MicroVride Scenes/SkateboardSimulator.unity
```

There are also several deprecated/old simulator scenes that are disabled in the build settings.

---

# Application Flow

The main entry point is:

```text
Start.unity
```

The start scene provides the vehicle and cybersickness-condition selection interface.

The main flow is:

```text
Start
  │
  ├── Calibrate foot sensors
  │
  ├── Select cybersickness condition
  │       ├── none
  │       ├── airflow
  │       ├── virtual helmet
  │       └── airflow + virtual helmet
  │
  └── Select vehicle
          │
          ├── Electric Scooter
          │      ├── Training
          │      └── Simulator
          │
          ├── Segway
          │      ├── Training
          │      └── Simulator
          │
          ├── Electric Unicycle
          │      ├── Training
          │      └── Simulator
          │
          └── One-Wheel Skateboard
                 ├── Training
                 └── Simulator
```

Vehicle selection is managed by:

```text
VehicleSelectionManager.cs
```

The selected vehicle and cybersickness condition are stored in the static `SessionState` class.

---

# Sensor Architecture

The simulator communicates with external hardware primarily through **UDP**.

The main Unity receiver is:

```text
VehicleDataReceiver.cs
```

It listens on three ports:

| Data         | UDP Port |
| ------------ | -------: |
| Throttle     |   `4210` |
| IMU          |   `1235` |
| Foot sensors |   `1234` |

The receivers are designed to persist between Unity scene changes.

---

# IMU

The repository contains:

```text
Arduino/IMUSetup/IMUSetup.ino
```

The firmware uses an Adafruit BNO055 IMU and sends Euler-angle measurements to Unity.

The transmitted message format is:

```text
imu-roll=<value>&imu-pitch=<value>&imu-yaw=<value>
```

The Unity receiver exposes:

```text
imuPitch
imuRoll
imuYaw
```

The IMU is used primarily for steering on the scooter/Segway and for both speed and steering on the electric unicycle and one-wheel skateboard.

The IMU firmware currently sends data at approximately:

```text
20 Hz
```

based on its `50 ms` loop delay.

---

# Thumb Throttle

The repository contains:

```text
Arduino/ThumbThrottleSetup/ThumbThrottleSetup.ino
```

The throttle reads an analog input and converts the voltage to a normalized range:

```text
0.0 → 1.0
```

The normalized value is sent over UDP on port:

```text
4210
```

The Unity receiver exposes this as:

```csharp
VehicleDataReceiver.throttle
```

The exact physical throttle model is not specified in the repository.

---

# Foot-Pressure Sensors

The repository contains firmware for two sensor boards:

```text
Arduino/UDP_UploadDatatoApp_B1/
Arduino/UDP_UploadDatatoApp_B2/
```

The boards correspond to:

```text
B1 → left foot
B2 → right foot
```

Each board reads four force-sensitive resistor channels.

The resulting measurements include:

```text
left_heel
left_toe
left_mid_l
left_mid_r

right_heel
right_toe
right_mid_l
right_mid_r
```

The normalized data is transmitted using messages such as:

```text
fs-board=left&left_mid_l_norm=...&left_mid_r_norm=...&left_heel_norm=...&left_toe_norm=...
```

and corresponding right-foot data.

---

# Airflow Hardware

The airflow mitigation system is controlled by:

```text
Assets/Cybersickness/Scripts/FanController.cs
```

and Arduino firmware located in:

```text
Arduino/Fan_code/
```

The Unity application sends UDP commands containing:

```text
fan angle
fan speed
```

to the external ESP32 fan controller.

The fan controller operates:

* A servo for airflow direction.
* PWM-controlled fan hardware for airflow intensity.

The communication port is:

```text
5052
```

The fan controller uses the vehicle's turning behavior to determine whether airflow should be directed left, right, or centrally.

---

# Data Logging

The project contains an experimental data-logging system:

```text
Assets/StudyLogger.cs
```

The logger records information associated with each ride/session.

A session contains information such as:

* Session ID
* Participant ID
* Vehicle
* Start/end timestamps
* Ride duration
* Planned coins
* Collected coins
* Cybersickness mitigation technique

The mitigation condition is stored in the session summary as:

```text
csTechnique
```

---

# Logged Files

For each ride, the logger creates files including:

```text
events_<timestamp>.jsonl
summary_<timestamp>.csv
telemetry_<timestamp>.csv
coins_<timestamp>.csv
```

On Android, the study logger is configured to use the device's Documents directory:

```text
/storage/emulated/0/Documents/
```

A separate folder is created for each ride/session.

The exact Android filesystem behavior may depend on the Unity/Android version and device configuration.

---

# Latency Measurement

Each of the main vehicle controllers contains an update-interval measurement system.

The controllers measure the time between consecutive Unity `Update()` calls for a configurable period.

By default, the measurement duration is:

```text
10 seconds
```

The resulting CSV contains:

```text
sample_index
update_interval_ms
```

Files use vehicle-specific prefixes such as:

```text
ESCOOTER_SENSOR_LATENCY_<timestamp>.csv
UNICYCLE_SENSOR_LATENCY_<timestamp>.csv
SKATEBOARD_SENSOR_LATENCY_<timestamp>.csv
```

The Segway controller also implements the same mechanism.

These files are written to:

```text
Application.persistentDataPath
```

The exact location depends on the target platform.

---

# Coins and Experimental Events

The simulator contains a coin/pickup system used as part of the riding task.

Relevant scripts include:

```text
Coin.cs
CoinMeta.cs
CoinLifecycleController.cs
CoinSpawnSettings.cs
DistanceCoinCollector.cs
TransitionCoinSpawner.cs
```

The study logger records events such as:

```text
ride_start
ride_finish
coin_collected
coin_missed
collision
```

Coin information can also be stored in:

```text
coins_<timestamp>.csv
```

---

# Project Structure

A simplified view of the repository is:

```text
.
├── Arduino/
│   ├── Fan_code/
│   ├── IMUSetup/
│   ├── ThumbThrottleSetup/
│   ├── UDP_UploadDatatoApp_B1/
│   ├── UDP_UploadDatatoApp_B2/
│   ├── UDP_fan_sender.py
│   └── receiver_udp_esp_ip.py
│
├── Assets/
│   ├── Cybersickness/
│   │   └── Scripts/
│   │
│   ├── MicroVride Scenes/
│   │   ├── Start.unity
│   │   ├── *Training.unity
│   │   ├── *Simulator.unity
│   │   └── old scenes/
│   │
│   ├── INMOTION_Electric_Unicycle/
│   ├── BEDRILL/
│   ├── HighlightPlus/
│   ├── QuickOutline/
│   ├── TextMesh Pro/
│   ├── Samples/
│   ├── EscooterController.cs
│   ├── SegwayController.cs
│   ├── ElectricUnicycleController.cs
│   ├── OneWheelSkateboardController.cs
│   ├── VehicleDataReceiver.cs
│   ├── FootSensorInput.cs
│   ├── VehicleSelectionManager.cs
│   ├── StudyLogger.cs
│   └── SessionState.cs
│
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
│
└── ProjectSettings/
    ├── ProjectVersion.txt
    ├── EditorBuildSettings.asset
    ├── ProjectSettings.asset
    └── ...
```

---

# Main Scripts

| Script                            | Purpose                                             |
| --------------------------------- | --------------------------------------------------- |
| `VehicleSelectionManager.cs`      | Selects the vehicle and loads the appropriate scene |
| `SessionState.cs`                 | Stores vehicle and cybersickness-condition state    |
| `StartCSManager.cs`               | Selects one of the four mitigation conditions       |
| `TechniquesEnabler.cs`            | Enables/disables airflow and virtual helmet         |
| `VehicleDataReceiver.cs`          | Receives throttle, IMU and foot-sensor UDP data     |
| `FootSensorInput.cs`              | Handles foot sensor calibration and processing      |
| `EscooterController.cs`           | Controls the electric scooter                       |
| `SegwayController.cs`             | Controls the Segway                                 |
| `ElectricUnicycleController.cs`   | Controls the electric unicycle                      |
| `OneWheelSkateboardController.cs` | Controls the one-wheel skateboard                   |
| `FanController.cs`                | Controls physical airflow                           |
| `YawStabilizer.cs`                | Stabilizes the VR rig against vehicle pitch/roll    |
| `StudyLogger.cs`                  | Records experimental/session data                   |
| `CoinLifecycleController.cs`      | Manages coin lifecycle                              |
| `VehicleSceneLoader.cs`           | Handles simulator scene loading                     |

---

# Debugging Without Hardware

Several vehicle controllers contain debug functionality.

For example, the electric scooter controller supports a keyboard-debug mode:

```text
W → forward
S → backward/braking
A → turn left
D → turn right
```

The controllers also contain configurable debug-speed overrides.

However, the repository does **not** provide a documented hardware-free procedure for reproducing the complete experimental setup. Some vehicle functionality depends on the external sensor and vehicle systems.

---

# Network Configuration

The Unity application and ESP32 devices communicate using UDP.

The main ports are:

|   Port | Function               |
| -----: | ---------------------- |
| `1234` | Foot-pressure sensors  |
| `1235` | IMU                    |
| `4210` | Thumb throttle         |
| `5052` | Airflow/fan controller |

The ESP32 firmware contains network configuration values for:

* Wi-Fi SSID/password
* Unity headset/device IP
* Fan controller IP

These values are currently hard-coded in the Arduino/Python files.