using System.Security.Cryptography;
using System.Text;

namespace FileViewer.Tools
{
    public static class String
    {
        public static string CalculateMD5(string input)
        {
            // 创建MD5实例
            using (MD5 md5 = MD5.Create())
            {
                // 计算字符串的字节表示的哈希值
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

                // 将字节转换为十六进制字符串
                StringBuilder result = new StringBuilder(hash.Length * 2);

                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("X2"));
                }

                return result.ToString();
            }
        }
    }
}
