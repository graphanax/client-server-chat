## Сlient-server chat on the TCP with RSA encryption

The software solution consists of a client application (WPF application with the MVVM pattern), 
a server application (console application), and an RSA encryption library.

The client and server are launched on automatically defined ports and local IP addresses, but 
they are also launched on user settings (via command-line launch). Each client is identified by 
their username within a single session. For correct messaging, clients must exchange public keys, 
and then they must connect to the server. The server logs the activity of clients, but it does not 
have the ability to encrypt / decrypt messages transmitted between clients.
