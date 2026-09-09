using moju.repository;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using moju.domain;

namespace moju.service
{
    internal class UserService
    {
        public static UserService Instance { get; } = new UserService();
        private UserService()
        {

        }

        public async Task<bool> SignUp(string username, string passwordPlainText, string realName, int status)
        {
            string salt = "";
            string hashPassword = ";";

            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                byte[] saltBytes = new byte[16];
                randomNumberGenerator.GetBytes(saltBytes);
                salt = Convert.ToBase64String(saltBytes);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordPlainText, 16, 10000))
            {
                hashPassword = Convert.ToBase64String(pbkdf2.GetBytes(32));
                salt = Convert.ToBase64String(pbkdf2.Salt);
            }

            await UserRepository.Instance.Insert(new User(username, hashPassword, salt, realName, status));

            return true;
        }

        public async Task<bool> login(string username, string passwordPlainText)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordPlainText))
            {
                return false;
            }

            User user = await UserRepository.Instance.SelectUserByUsername(username);
            if(user == null || string.IsNullOrWhiteSpace(user.salt) )
            {
                return false;
            }
            byte[] saltBytes = Convert.FromBase64String(user.salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordPlainText, saltBytes, 10000))
            {
                string passwordEncoded = Convert.ToBase64String(pbkdf2.GetBytes(32));
                return passwordEncoded != null && passwordEncoded.Equals(user.password);
            }
        }

    }
}
