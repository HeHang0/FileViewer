using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace FileViewer.Tools
{
    public static class File
    {
        public static bool ProcessStart(string fileName, string arguments = "")
        {
            try
            {
                Process.Start(new ProcessStartInfo(fileName, arguments) { UseShellExecute = true });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Delete(string fileName)
        {
            try
            {
                System.IO.File.Delete(fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string CalculateFileMD5(string filePath)
        {
            // 创建一个MD5实例
            using MD5 md5 = MD5.Create();
            // 读取文件的字节并计算哈希值
            using FileStream stream = System.IO.File.OpenRead(filePath);
            byte[] hash = md5.ComputeHash(stream);

            // 将字节转换为十六进制字符串
            StringBuilder result = new(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public static double FileSize(string filePath)
        {
            var f = new FileInfo(filePath);
            if (f.Exists)
            {
                return f.Length * 1.0 / 1048576;
            }
            return 0;
        }

        //private static IEnumerable<int> FindChildProcessIds(int parentId, string? childName)
        //{
        //    List<int> result = new();
        //    try
        //    {
        //        var processes = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={parentId}").Get();
        //        if (processes != null)
        //        {
        //            foreach (var item in processes)
        //            {
        //                var name = ((string)item.Properties["Name"].Value).ToLower() ?? string.Empty;// 
        //                if (string.IsNullOrEmpty(childName) || name == childName.ToLower())
        //                {
        //                    result.Add((int)item.Properties["ProcessId"].Value);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return result;
        //}

        //private static void KillProcesses(IEnumerable<int> processIds)
        //{
        //    foreach (var item in processIds)
        //    {
        //        try
        //        {
        //            Process.GetProcessById(item)?.Kill();
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //}
    }
}
