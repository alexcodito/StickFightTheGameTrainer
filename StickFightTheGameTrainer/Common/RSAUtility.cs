using System;
using System.Security.Cryptography;
using System.Text;

namespace StickFightTheGameTrainer.Common
{
    public static class RsaUtility
    {
        public static string Encrypt(string data, string publicKey)
        {
            //publicKey = "<RSAKeyValue><Modulus>qr9TWSzlmEGqIAsA2Q4WCwLjTnH2MdFlIFAjHKuWixYGzH0Nc5TrtOeQ6r810Wx8m01f+ZGEP0HRnNBz/Dnr2+FQ0+djfI6gpfyhLa6vDFCMfF18I9cBdKk9TYFzTnnLHE7ViUTE4qRZE33fKkBQNNjS5cJVpBWlLgYdj4NBIeU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            var testData = Encoding.UTF8.GetBytes(data);

            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    rsa.FromXmlString(publicKey);

                    var encryptedData = rsa.Encrypt(testData, true);

                    var base64Encrypted = Convert.ToBase64String(encryptedData);

                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static String GenerateKeys()
        {
            // Generate a public/private key pair.  
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);

            // Save the public key information to an RSAParameters structure.  
            //RSAParameters RSAKeyInfo = RSA.ExportParameters(false);

            return rsa.ToXmlString(true);
        }

        public static string Decrypt(string data, string privateKey)
        {
            //privateKey = "<RSAKeyValue><Modulus>qr9TWSzlmEGqIAsA2Q4WCwLjTnH2MdFlIFAjHKuWixYGzH0Nc5TrtOeQ6r810Wx8m01f+ZGEP0HRnNBz/Dnr2+FQ0+djfI6gpfyhLa6vDFCMfF18I9cBdKk9TYFzTnnLHE7ViUTE4qRZE33fKkBQNNjS5cJVpBWlLgYdj4NBIeU=</Modulus><Exponent>AQAB</Exponent><P>4lt3U8QO8tSt/NoN+14E5dtxVNJiJOS9HDcZjWuJCmbW6VnzbGIAd8o2XbW+OYHYgMWEZj4Oijt5NgFYir+5xw==</P><Q>wRuPVlgRVcrn97XFKUNhoFRMhaXCe/0JC9dTgxWno6JqPBdXVFp4ElfAN3tdd801pK/7cQmUgfZ+NA895s3m8w==</Q><DP>LNMUFEB0/V1kfvfnYOnDaolELhnjWY76bAX1R24OG0M2N8uaStI1aYNftLryyoyOBSIYD+8mDfWtESa/E+rXrQ==</DP><DQ>v4vF12DN+Sqmg7hi1HZI7U8RBHSSgIhSo9M3vEwLmC2vcOG6Nyrx9Ufjm2UcJoyADFafc9WL4IoPTzZbMYjF7w==</DQ><InverseQ>sVB60zzwVrnGFscKhHvsCCrP16mgDMI36hZEXuD7W54ad4tYCOhksrT9EjGWv4LB3h419kLWd6Pqoyexptd9qw==</InverseQ><D>otBljbb+Bz02F+/mExMXedB7JyYL+F0O7Mz9bi1AE8ghG4Ry+y+Zwpq82NsTWmr0NrKFPdjAHgGI+trafjxM5YHs2YQBM7+c68iFo6vYb6qS3SbY/NaDR0bviByrAjZ016KUb/g+fc4Zl3tX3ZIHKvwFp1WHuP1l20DaLfXvENE=</D></RSAKeyValue>";

            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    var base64Encrypted = data;

                    rsa.FromXmlString(privateKey);

                    var resultBytes = Convert.FromBase64String(base64Encrypted);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
    }
}
