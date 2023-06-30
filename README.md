# ChatApplication

WPF application in C# that allows to encrypted communication between two persons, using sockets and TCP protocol.

Cryptosystem methods for safe data transfer:
<ul>
    <li>* RSA</li>
    <li>* AES</li>
    <li>* SHA-256</li>
    <li>* MD5</li>
</ul>
Application allows to send text message and files (any size). <br>

Initial window<br>
![Alt text](img/image.png)

Scanning state, waiting for incomming connetions<br>
![Alt text](img/image-1.png)

Chatting<br>
![Alt text](img/image-2.png)

Incoming file acceptation dialog<br>
![Alt text](img/image-3.png)

Downloading file progress<br>
![Alt text](img/image-4.png)



Literature<br>
[1] https://learn.microsoft.com/pl-pl/dotnet/api/system.net.sockets.socket?view=net-7.0<br>
[2] https://pl.wikipedia.org/wiki/Gniazdo_(telekomunikacja)