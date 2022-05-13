using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Common
{
    public class Cryptopher
    {
        public string AppKey { get; set; } = "EmployeeEvaluation!@#123";
        public string AppKeySalt { get; set; } = "123456Aa@";
        private readonly string AppKeySaltDefault = "123456Aa@";

        public string PasswordHash(string password)
        {
            var key = AppKey + GetSalt2();
            byte[] data = Encoding.UTF8.GetBytes(password + key);
            var hashAlgoritm = SHA256.Create();
            var passwordData = hashAlgoritm.ComputeHash(data);
            return ToHexString(Encoding.UTF8.GetString(passwordData));
        }

        private string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.UTF8.GetBytes(str);

            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        private string FromHexString(string hexStr)
        {
            var bytes = new byte[hexStr.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public string Encrypt(string toEncrypt)
        {
            try
            {
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
                byte[] resultArray = Encrypt(toEncryptArray);
                return ToHexString(Convert.ToBase64String(resultArray, 0, resultArray.Length));
            }
            catch
            {
                return null;
            }
        }

        public string Decrypt(string toDecrypt)
        {
            try
            {
                byte[] toDecryptArray = Convert.FromBase64String(FromHexString(toDecrypt));
                byte[] resultArray = Decrypt(toDecryptArray);
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch
            {
                return null;
            }
        }

        private void SetupTdes(TripleDESCryptoServiceProvider provider)
        {
            var key = AppKey + GetSalt2();

            var hashmd5 = new MD5CryptoServiceProvider();
            var keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();

            provider.Key = keyArray;
            provider.Mode = CipherMode.ECB;
            provider.Padding = PaddingMode.PKCS7;
        }

        private byte[] Encrypt(byte[] toEncrypt)
        {
            using (var tdesProvider = new TripleDESCryptoServiceProvider())
            {
                SetupTdes(tdesProvider);

                byte[] resultArray = tdesProvider.CreateEncryptor().TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
                tdesProvider.Clear();
                return resultArray;
            }
        }

        private byte[] Decrypt(byte[] toDecrypt)
        {
            using (var tdesProvider = new TripleDESCryptoServiceProvider())
            {
                SetupTdes(tdesProvider);

                byte[] resultArray = tdesProvider.CreateDecryptor().TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);

                tdesProvider.Clear();
                return resultArray;
            }
        }

        private string GetSalt2()
        {
            return AppKeySalt.Length > 0 ? AppKeySalt : AppKeySaltDefault;
        }
    }
}
