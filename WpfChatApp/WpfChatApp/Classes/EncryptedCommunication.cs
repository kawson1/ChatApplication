using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace WpfChatApp.Classes
{
    [Serializable]
    internal class EncryptedMessage
    {
        public byte[] Key { get; set; }
        public byte[] Message { get; set; }
    }

    internal class EncryptedCommunication
    {
        readonly byte[] receiverPublicKey;
        public bool ECB = false;

        public EncryptedCommunication(byte[] receiverPublicKey)
        {
            this.receiverPublicKey = receiverPublicKey;
        }

        public EncryptedMessage EncryptMessage(string message)
        {
            var encryptedMessage = new EncryptedMessage();
            byte[] key;

            // Encrypt message with AES algorithm and save key
            encryptedMessage.Message = Encryption.aesEncrypt(Encoding.ASCII.GetBytes(message), out key, ECB);

            // Encrypt AES key with receiver public key
            byte[] encryptedAESKey = Encryption.rsaEncryptSHA1(key, receiverPublicKey);
            encryptedMessage.Key = encryptedAESKey;

            return encryptedMessage;
        }
        
        public EncryptedMessage EncryptMessage(string message, out byte[] AESKey)
        {
            var encryptedMessage = new EncryptedMessage();
            byte[] key;

            // Encrypt message with AES algorithm and save key
            encryptedMessage.Message = Encryption.aesEncrypt(Encoding.ASCII.GetBytes(message), out key, ECB);

            AESKey = key;

            // Encrypt AES key with receiver public key
            byte[] encryptedAESKey = Encryption.rsaEncryptSHA1(key, receiverPublicKey);
            encryptedMessage.Key = encryptedAESKey;

            return encryptedMessage;
        }
        
        public EncryptedMessage EncryptData(byte[] data)
        {
            var encryptedMessage = new EncryptedMessage();
            byte[] key;

            // Encrypt message with AES algorithm and save key
            encryptedMessage.Message = Encryption.aesEncrypt(data, out key, ECB);

            // Encrypt AES key with receiver public key
            byte[] encryptedAESKey = Encryption.rsaEncryptSHA1(key, receiverPublicKey);
            encryptedMessage.Key = encryptedAESKey;

            return encryptedMessage;
        }

        public string DecryptMessage(EncryptedMessage encryptedMessage, string password, string privKeyPath)
        {
            try
            {
                byte[] decryptedPrivKey = DecryptPrivateKey(password, privKeyPath);

                byte[] decryptedAesKey = Encryption.rsaDecryptSHA1(encryptedMessage.Key, decryptedPrivKey);

                byte[] decryptedMessage = Encryption.aesDecrypt(encryptedMessage.Message, decryptedAesKey, ECB);
                return Encoding.ASCII.GetString(decryptedMessage);
            }
            catch
            {
                return "Zle haslo !";
            }
        } 
        
        public byte[] DecryptPrivateKey(string password, string privKeyPath)
        {
            byte[] encryptedPrivKey = File.ReadAllBytes(privKeyPath);
            return Encryption.aesDecrypt(encryptedPrivKey, Encoding.ASCII.GetBytes(password));
        } 
        
        public byte[] DecryptData(EncryptedMessage encryptedMessage, string password, string privKeyPath)
        {
            byte[] encryptedPrivKey = File.ReadAllBytes(privKeyPath);
            byte[] decryptedPrivKey = Encryption.aesDecrypt(encryptedPrivKey, Encoding.ASCII.GetBytes(password));

            byte[] decryptedAesKey = Encryption.rsaDecryptSHA1(encryptedMessage.Key, decryptedPrivKey);

            byte[] decryptedMessage = Encryption.aesDecrypt(encryptedMessage.Message, decryptedAesKey, ECB);
            return decryptedMessage;
        }

        public static void GenerateRSAKeyPair(string publicKeyDestination, string privateKeyDestination, string password, bool ECB = false)
        {
            using (RSA rsaAlgorithm = RSA.Create())
            {
                var pubKey = Encoding.ASCII.GetBytes(rsaAlgorithm.ToXmlString(false));
                var privKey = Encoding.ASCII.GetBytes(rsaAlgorithm.ToXmlString(true));

                // Encrypt private key with AES 256
                byte[] encryptedPrivKey = Encryption.aesEncrypt(privKey, Encoding.ASCII.GetBytes(password), ECB);
                File.WriteAllBytes(publicKeyDestination, pubKey);
                File.WriteAllBytes(privateKeyDestination, encryptedPrivKey);
            }
        }

        public static byte[] GetRSAPublicKey(string pubKeyPath)
        {
            return File.ReadAllBytes(pubKeyPath);
        }
    }

    internal class Serializer
    {
        public static byte[] ToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
    }
}
