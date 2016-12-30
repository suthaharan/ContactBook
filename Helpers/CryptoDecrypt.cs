using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;


namespace Contact2015.Helpers
{
    public class CryptoDecrypt
    {

        private static string Key = "ABC123DEF456GH78";
        private static byte[] GetByte(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static byte[] EncryptString(string data)
        {
            byte[] byteData = GetByte(data);
            SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
            algo.Key = GetByte(Key);
            algo.GenerateIV();

            MemoryStream mStream = new MemoryStream();
            mStream.Write(algo.IV, 0, algo.IV.Length);

            CryptoStream myCrypto = new CryptoStream(mStream, algo.CreateEncryptor(), CryptoStreamMode.Write);
            myCrypto.Write(byteData, 0, byteData.Length);
            myCrypto.FlushFinalBlock();

            return mStream.ToArray();
        }

        public static string DecryptString(byte[] data)
        {
            SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
            algo.Key = GetByte(Key);
            MemoryStream mStream = new MemoryStream();

            byte[] byteData = new byte[algo.IV.Length];
            Array.Copy(data, byteData, byteData.Length);
            algo.IV = byteData;
            int readFrom = 0;
            readFrom += algo.IV.Length;

            CryptoStream myCrypto = new CryptoStream(mStream, algo.CreateDecryptor(), CryptoStreamMode.Write);
            myCrypto.Write(data, readFrom, data.Length - readFrom);
            myCrypto.FlushFinalBlock();

            return Encoding.UTF8.GetString(mStream.ToArray());
        }

        public static string GetEncryptedQueryString(string data)
        {
            return Convert.ToBase64String(EncryptString(data));
        }

        public static string GetDecryptedQueryString(string data)
        {
            byte[] byteData = Convert.FromBase64String(data.Replace(" ", "+"));
            return DecryptString(byteData);
        }


        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }


        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }


        public static string AES_256_WebKey_Encrypt(string dbField)
        {
            string webpass = WebConfigurationManager.AppSettings["AESPASS"];

            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(dbField);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(webpass);
            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(bytesEncrypted);

        }

        public static string AES_256_WebKey_Decrypt(string dbField)
        {
            string webpass = WebConfigurationManager.AppSettings["AESPASS"];

            byte[] bytesToBeDecrypted = Convert.FromBase64String(dbField);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(webpass);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;

        }

        public static string SHA256Pass(string password) 
        {
            // Get the bytes of the string
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            return Convert.ToBase64String(passwordBytes);
        }


    }
}