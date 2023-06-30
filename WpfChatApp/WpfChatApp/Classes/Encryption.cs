using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WpfChatApp.Classes
{
    internal class Encryption
    {
        public static byte[] rsaEncryptSHA1(byte[] input, byte[] publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(System.Text.Encoding.ASCII.GetString(publicKey));
                return rsa.Encrypt(input, RSAEncryptionPadding.OaepSHA1);
            }
        }

        public static byte[] rsaDecryptSHA1(byte[] input, byte[] privateKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(System.Text.Encoding.UTF8.GetString(privateKey));
                return rsa.Decrypt(input, RSAEncryptionPadding.OaepSHA1);
            }
        }

        public static byte[] Sha256encrypt(byte[] input)
        {
            return SHA256.Create().ComputeHash(input);
        }
        
        public static byte[] MD5encrypt(byte[] input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                return md5.ComputeHash(input);
        }

        public static byte[] aesEncrypt(byte[] inputBytes, out byte[] key, bool ECB = false)
        {
            byte[] encryptedInput;

            using (Aes aesAlgorithm = new AesManaged())
            {
                // SHA256 because AES uses 256 bit key and 128 IV
                var IVhash = MD5encrypt(aesAlgorithm.Key);
                key = aesAlgorithm.Key;
                aesAlgorithm.Key = Sha256encrypt(aesAlgorithm.Key);

                aesAlgorithm.IV = IVhash;
                aesAlgorithm.Padding = PaddingMode.PKCS7;
                if(ECB)
                    aesAlgorithm.Mode = CipherMode.ECB;
                else
                    aesAlgorithm.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor();
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(inputBytes, 0, inputBytes.Length);
                    }
                    encryptedInput = msEncrypt.ToArray();
                }
            }
            return encryptedInput;
        }

        public static byte[] aesEncrypt(byte[] inputBytes, byte[] key, bool ECB = false)
        {
            byte[] encryptedInput;

            using(Aes aesAlgorithm = new AesManaged())
            {
                // SHA256 because AES uses 256 bit key and 128 IV
                var hashedKey = Sha256encrypt(key);
                var IVhash = MD5encrypt(key);

                aesAlgorithm.Key = hashedKey;
                aesAlgorithm.IV = IVhash;
                aesAlgorithm.Padding = PaddingMode.PKCS7;
                if (ECB)
                    aesAlgorithm.Mode = CipherMode.ECB;
                else
                    aesAlgorithm.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor();
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(inputBytes, 0, inputBytes.Length);
                    }
                    encryptedInput = msEncrypt.ToArray();
                }
            }
            return encryptedInput;
        }
       
        public static byte[] aesDecrypt(byte[] inputBytes, byte[] key, bool ECB = false)
        {
            byte[] decryptedInput;

            var hashedPassword = Sha256encrypt(key);
            var IVhash = MD5encrypt(key);

            using (Aes aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.Padding = PaddingMode.PKCS7;
                if (ECB)
                    aesAlgorithm.Mode = CipherMode.ECB;
                else
                    aesAlgorithm.Mode = CipherMode.CBC;
                aesAlgorithm.Key = hashedPassword;
                aesAlgorithm.IV = IVhash;

                ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor();

                try
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(inputBytes, 0, inputBytes.Length);

                        }
                        decryptedInput = msEncrypt.ToArray();
                    }
                }
                catch
                {
                    int x = 50;
                    return new byte[] { 1, 2 };
                }
            }
            return decryptedInput;
        }
    }
}
