using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Common
{
    public static class Utils
    {
        public static string EncryptedPassword(string password, string salt = null)
        {
            var cryptoher = new Cryptopher();
            if (!string.IsNullOrEmpty(salt))
                cryptoher.AppKeySalt = salt;
            return cryptoher.PasswordHash(password);
        }
    }
}
