import socket

ESP32_IP = "10.125.93.23" # esp ip address
ESP32_PORT = 5052

key_map = {
    '0': (0, 0), # (Fan position, fan speed) in degrees, central position is 0 degrees. 
    '1': (10, 0.2),
    '2': (20, 0.2),
    '3': (30, 0.5),
    '4': (40, 1),
    '5': (50, 1),
    '6': (60, 1),
    '7': (70, 1),
    '8': (80, 1),
    '9': (90, 1)
}

# Create UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print("Press 1 or 2 to send data to ESP32. Type 'esc' to quit.")

try:
    while True:
        key = input("Key: ").strip()

        if key.lower() == 'esc':
            print("Exiting...")
            break

        if key in key_map:
            angle, value = key_map[key]
            message = f"{angle},{value}"

            try:
                sock.sendto(message.encode('utf-8'), (ESP32_IP, ESP32_PORT))
                print(f"Sent: {message}")
            except Exception as e:
                print(f"Error sending UDP message: {e}")
        else:
            print("Invalid key. Use 1, 2 or 'esc'.")

except KeyboardInterrupt:
    print("\nStopped by user.")

sock.close()