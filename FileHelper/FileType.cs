using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileViewer.FileHelper
{
    public class FileType
    {
        public static FileExtension GetFileType(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        try
                        {
                            var b1 = br.ReadByte().ToString();
                            var b2 = br.ReadByte().ToString();
                            int.TryParse(b1 + b2, out int typeInt);
                            return (FileExtension)typeInt;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            else if (Directory.Exists(filePath))
            {
                return FileExtension.Folder;
            }
            return FileExtension.None;
        }


    }
    public enum FileExtension
    {
        JPG = 255216,
        GIF = 7173,
        BMP = 6677,
        PNG = 13780,
        COM = 7790,
        EXE = 7790,
        DLL = 7790,
        RAR = 8297,
        ZIP = 8075,
        XML = 6063,
        HTML = 6033,
        ASPX = 239187,
        CS = 117115,
        JS = 119105,
        TXT = 210187,
        SQL = 255254,
        GO = 11297,
        BAT = 64101,
        BTSEED = 10056,
        RDP = 255254,
        PSD = 5666,
        PDF = 3780,
        CHM = 7384,
        LOG = 70105,
        REG = 8269,
        HLP = 6395,
        DOC = 208207,
        XLS = 208207,
        DOCX = 208207,
        XLSX = 208207,
        Folder = 1,
        None = 0,
    }
}
