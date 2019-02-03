using System;
using System.IO;
using System.Security.Cryptography;

namespace StickFightTheGameTrainer.Common
{
    internal static class AesUtility
    {
        internal static byte[] EncryptStringToBytes(string plainText, string key, string iv)
        {
            return EncryptStringToBytes(plainText, Convert.FromBase64String(key), Convert.FromBase64String(iv));
        }

        internal static string EncryptStringToBase64String(string plainText, string key, string iv)
        {
            return Convert.ToBase64String(EncryptStringToBytes(plainText, Convert.FromBase64String(key), Convert.FromBase64String(iv)));
        }

        internal static string DecryptStringFromBytes(byte[] cipherText, string key, string iv)
        {
            return DecryptStringFromBytes(cipherText, Convert.FromBase64String(key), Convert.FromBase64String(iv));
        }

        internal static string DecryptStringFromBase64String(string cipherText, string key, string iv)
        {
            return DecryptStringFromBytes(Convert.FromBase64String(cipherText), Convert.FromBase64String(key), Convert.FromBase64String(iv));
        }

        internal static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            byte[] encrypted;

            using (var aesAlgo = new AesCryptoServiceProvider())
            {
                aesAlgo.Key = key;
                aesAlgo.IV = iv;
                aesAlgo.Padding = PaddingMode.PKCS7;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlgo.CreateEncryptor(aesAlgo.Key, aesAlgo.IV);
                
                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        internal static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            string plaintext;

            using (var aesAlgo = new AesCryptoServiceProvider())
            {
                aesAlgo.Key = key;
                aesAlgo.IV = iv;
                aesAlgo.Padding = PaddingMode.PKCS7;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlgo.CreateDecryptor(aesAlgo.Key, aesAlgo.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }
    }
}
