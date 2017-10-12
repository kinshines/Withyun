using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Domain.Helper
{
    public class Security
    {
        /// <summary>
        /// 创建验证码字符
        /// </summary>
        /// <param name="length">字符长度</param>
        /// <returns>验证码字符</returns>
        public static string CreateVerificationText(int length)
        {
            char[] verification = new char[length];
            char[] dictionary = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            Random random = new Random();
            for (int i = 0; i < length; i++) { verification[i] = dictionary[random.Next(dictionary.Length - 1)]; }
            return new string(verification);
        }


        /// <summary>
        /// 256位散列加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>密文</returns>
        public static string Sha256(string plainText)
        {
            SHA256Managed sha256 = new SHA256Managed();
            byte[] cipherText = sha256.ComputeHash(Encoding.Default.GetBytes(plainText));
            return Convert.ToBase64String(cipherText);
        }

        public static string GenerateUserToken(string purpose, int userId, string stamp)
        {
            var ms = new MemoryStream();
            using (var writer = ms.CreateWriter())
            {
                writer.Write(DateTimeOffset.UtcNow);
                writer.Write(Convert.ToString(userId, CultureInfo.InvariantCulture));
                writer.Write(purpose ?? "");
                writer.Write(stamp ?? "");
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static bool ValidateUserToken(string purpose, string token, int userId, string stamp)
        {
            try
            {
                var data = Convert.FromBase64String(token);
                var ms = new MemoryStream(data);
                using (var reader = ms.CreateReader())
                {
                    var creationTime = reader.ReadDateTimeOffset();
                    var expirationTime = creationTime + TimeSpan.FromDays(1);
                    if (expirationTime < DateTimeOffset.UtcNow)
                    {
                        return false;
                    }

                    var readUserId = reader.ReadString();
                    if (!String.Equals(readUserId, Convert.ToString(userId, CultureInfo.InvariantCulture)))
                    {
                        return false;
                    }
                    var purp = reader.ReadString();
                    if (!String.Equals(purp, purpose))
                    {
                        return false;
                    }
                    var readStamp = reader.ReadString();
                    return readStamp == stamp;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
    }

    internal static class StreamExtensions
    {
        internal static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        public static BinaryReader CreateReader(this Stream stream)
        {
            return new BinaryReader(stream, DefaultEncoding, true);
        }

        public static BinaryWriter CreateWriter(this Stream stream)
        {
            return new BinaryWriter(stream, DefaultEncoding, true);
        }

        public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader)
        {
            return new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
        }

        public static void Write(this BinaryWriter writer, DateTimeOffset value)
        {
            writer.Write(value.UtcTicks);
        }
    }
}