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
        static FileExtension GetFileType(string filePath)
        {
            if (File.Exists(filePath))
            {
                if(Enum.TryParse(Path.GetExtension(filePath).TrimStart('.').ToUpper(), out FileExtension ext))
                {
                    return ext;
                }
            }
            else if (Directory.Exists(filePath))
            {
                return FileExtension.Folder;
            }
            return FileExtension.None;
        }

        public static (FileViewType Type, FileExtension Ext) GetFileViewType(string filePath)
        {
            var ext = GetFileType(filePath);
            var type = FileViewType.None;
            switch (ext)
            {
                case FileExtension.JPG:
                case FileExtension.BMP:
                case FileExtension.PNG:
                case FileExtension.GIF:
                case FileExtension.ICO:
                case FileExtension.SVG:
                    type = FileViewType.Image;
                    break;
                case FileExtension.TXT:
                case FileExtension.CS:
                case FileExtension.GO:
                case FileExtension.JS:
                case FileExtension.JSON:
                case FileExtension.VUE:
                case FileExtension.SQL:
                case FileExtension.HTML:
                case FileExtension.BAT:
                case FileExtension.CSS:
                case FileExtension.MD:
                case FileExtension.BASH:
                case FileExtension.GITIGNORE:
                case FileExtension.GITATTRIBUTE:
                case FileExtension.RC:
                case FileExtension.XML:
                case FileExtension.LOG:
                case FileExtension.PY:
                    type = FileViewType.Code;
                    break;
                case FileExtension.MP3:
                case FileExtension.WMA:
                case FileExtension.M4A:
                case FileExtension.WAV:
                    type = FileViewType.Music;
                    break;
                case FileExtension.MP4:
                case FileExtension.RMVB:
                case FileExtension.RM:
                case FileExtension.MKV:
                case FileExtension.TS:
                    type = FileViewType.Video;
                    break;
                case FileExtension.PDF:
                    type = FileViewType.Pdf;
                    break;
                case FileExtension.XLSX:
                case FileExtension.XLS:
                    type = FileViewType.Excel;
                    break;
                case FileExtension.DOC:
                case FileExtension.DOCX:
                    type = FileViewType.Word;
                    break;
                case FileExtension.PPT:
                case FileExtension.PPTX:
                    type = FileViewType.PowerPoint;
                    break;
                case FileExtension.Folder:
                    type = FileViewType.Folder;
                    break;
            }
            return (type, ext);
        }
    }

    public enum FileViewType
    {
        Image, Code, Txt, Music, Video, Word, Excel, PowerPoint, Pdf, Folder, None
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
        MD = 3532,
        BASH = 3533,
        GITIGNORE = 3534,
        GITATTRIBUTE = 3535,
        HTML = 6033,
        ASPX = 239187,
        CS = 117115,
        CSS = 64102,
        JS = 119105,
        JSON = 119106,
        VUE = 119107,
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
        None = 0,
        Folder,
        PY,
        ICO,
        SVG,
        RC,
        MP3,
        WAV,
        M4A,
        WMA,
        MP4,
        RMVB,
        RM,
        MKV,
        TS,
        DOC,
        XLS,
        PPT,
        DOCX,
        XLSX,
        PPTX
    }
}
