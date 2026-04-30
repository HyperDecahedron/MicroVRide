import socket

# Create UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind to all interfaces on port 1234
sock.bind(("0.0.0.0", 1234))

print("Listening for UDP packets on port 1234...")

while True:
    data, addr = sock.recvfrom(4096)  # buffer size is 4096 bytes
    print(f"Received from {addr}: {data}")