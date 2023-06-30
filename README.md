# ChatApplication

WPF application in C# that allows to encrypted communication between two persons, using sockets and TCP protocol.

Cryptosystem methods for safe data transfer:
    * RSA
    * AES
    * SHA-256
    * MD5

Application allows to send text message and files (any size).

Initial window
![Alt text](image.png)

Scanning state, waiting for incomming connetions
![Alt text](image-1.png)

Chatting
![Alt text](image-2.png)

Incoming file acceptation dialog
![Alt text](image-3.png)

Downloading file progress
![Alt text](image-4.png)



Literature
[1] https://learn.microsoft.com/pl-pl/dotnet/api/system.net.sockets.socket?view=net-7.0
[2] https://pl.wikipedia.org/wiki/Gniazdo_(telekomunikacja)