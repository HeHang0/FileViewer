﻿using System;
using System.Text;

namespace FileViewer.FileHelper.IcnsParser
{
  public class IcnsType
  {
    private readonly int type;
    private readonly int width;
    private readonly int height;
    private readonly int bitsPerPixel;
    private readonly TypeDetails details;

    // https://en.wikipedia.org/wiki/Apple_Icon_Image_format
    public static readonly IcnsType[] ALL_TYPES = new IcnsType[]
    {
      // 16x12
      new IcnsType("icm#", 16, 12, 1, TypeDetails.HasMask),
      new IcnsType("icm4", 16, 12, 4, TypeDetails.None),
      new IcnsType("icm8", 16, 12, 8, TypeDetails.None),
      // 16x16
      new IcnsType("ics#", 16, 16, 1, TypeDetails.Mask),
      new IcnsType("ics4", 16, 16, 4, TypeDetails.None),
      new IcnsType("ics8", 16, 16, 8, TypeDetails.None),
      new IcnsType("is32", 16, 16, 32, TypeDetails.None),
      new IcnsType("s8mk", 16, 16, 8, TypeDetails.Mask),
      new IcnsType("icp4", 16, 16, 32, TypeDetails.Compressed),
      new IcnsType("ic04", 16, 16, 32, TypeDetails.ARGB),
      // 18x18
      new IcnsType("icsb", 18, 18, 32, TypeDetails.ARGB), // not tested
      // 32x32
      new IcnsType("ICON", 32, 32, 1, TypeDetails.None),
      new IcnsType("ICN#", 32, 32, 1, TypeDetails.HasMask),
      new IcnsType("icl4", 32, 32, 4, TypeDetails.None),
      new IcnsType("icl8", 32, 32, 8, TypeDetails.None),
      new IcnsType("il32", 32, 32, 32, TypeDetails.None),
      new IcnsType("l8mk", 32, 32, 8, TypeDetails.Mask),
      new IcnsType("icp5", 32, 32, 32, TypeDetails.Compressed),
      new IcnsType("ic11", 32, 32, 32, TypeDetails.Retina),
      new IcnsType("ic05", 32, 32, 32, TypeDetails.ARGB),
      // 36x36      
      new IcnsType("icsB", 36, 36, 32, TypeDetails.ARGB), // not tested
      // 48x48
      new IcnsType("ich#", 48, 48, 1, TypeDetails.Mask),
      new IcnsType("ich4", 48, 48, 4, TypeDetails.None),
      new IcnsType("ich8", 48, 48, 8, TypeDetails.None),
      new IcnsType("ih32", 48, 48, 32, TypeDetails.None),
      new IcnsType("h8mk", 48, 48, 8, TypeDetails.Mask),
      // 64x64      
      new IcnsType("icp6", 64, 64, 32, TypeDetails.Compressed),
      new IcnsType("ic12", 64, 64, 32, TypeDetails.Retina),
      // 128x128
      new IcnsType("it32", 128, 128, 32, TypeDetails.None),
      new IcnsType("t8mk", 128, 128, 8, TypeDetails.Mask),
      new IcnsType("ic07", 128, 128, 32, TypeDetails.Compressed),
      // other
      new IcnsType("ic08", 256, 256, 32, TypeDetails.Compressed),
      new IcnsType("ic13", 256, 256, 32, TypeDetails.Retina),
      new IcnsType("ic09", 512, 512, 32, TypeDetails.Compressed),
      new IcnsType("ic14", 512, 512, 32, TypeDetails.Retina),
      new IcnsType("ic10", 1024, 1024, 32, TypeDetails.Retina),
    };

    private IcnsType(string type, int width, int height, int bitsPerPixel, TypeDetails details)
    {
      this.type = TypeAsInt(type);
      this.width = width;
      this.height = height;
      this.bitsPerPixel = bitsPerPixel;
      this.details = details;
    }

    public int Type
    {
      get { return type; }
    }

    public int Width
    {
      get { return width; }
    }

    public int Height
    {
      get { return height; }
    }

    public int BitsPerPixel
    {
      get { return bitsPerPixel; }
    }

    public TypeDetails Details
    {
      get { return details; }
    }    

    public static IcnsType FindType(int type, TypeDetails ignoreDetails)
    {
      for (int i = 0; i < ALL_TYPES.Length; i++)
      {
        if (ALL_TYPES[i].type != type)
          continue;

        if (ignoreDetails != 0 && ALL_TYPES[i].Details == ignoreDetails)
          continue;

        return ALL_TYPES[i];
      }
      return null;
    }

    public static IcnsType FindType(int width, int height, int bpp, TypeDetails details)
    {
      for (int i = 0; i < ALL_TYPES.Length; i++)
      {
        IcnsType type = ALL_TYPES[i];
        if (type.width == width &&
            type.height == height &&
            type.bitsPerPixel == bpp &&
            type.details == details)
          return type;
      }

      return null;
    }

    public static int TypeAsInt(string type)
    {
      byte[] bytes = Encoding.ASCII.GetBytes(type);

      if (bytes.Length != 4)
        throw new Exception("Invalid ICNS type");

      return ((0xff & bytes[0]) << 24) |
             ((0xff & bytes[1]) << 16) |
             ((0xff & bytes[2]) << 8) |
             (0xff & bytes[3]);
    }

    public static string DescribeType(int type)
    {
      byte[] bytes = new byte[4];
      bytes[0] = (byte)(0xff & (type >> 24));
      bytes[1] = (byte)(0xff & (type >> 16));
      bytes[2] = (byte)(0xff & (type >> 8));
      bytes[3] = (byte)(0xff & type);

      return Encoding.ASCII.GetString(bytes);
    }

    public enum TypeDetails
    {
      /// <summary>
      /// The default image with no detils.
      /// </summary>
      None,
      /// <summary>
      /// The image is alpha mask.
      /// </summary>
      Mask,
      /// <summary>
      /// Has alpha mask.
      /// </summary>
      HasMask,
      /// <summary>
      /// Whole 4 channels are used.
      /// </summary>
      ARGB,
      /// <summary>
      /// Compressed, j2k or PNG codec is used.
      /// </summary>
      Compressed,
      /// <summary>
      /// Retina (2x) image. j2k or PNG is used.
      /// </summary>
      Retina,
    }
  }
}
