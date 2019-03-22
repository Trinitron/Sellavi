using System;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Security
{
    /// <summary>
    ///     Generates a 16 byte Unique Identification code of a computer
    ///     Example: 4876-8DB5-EE85-69D3-FE52-8CF7-395D-2EA9
    /// </summary>
    public static class MachineGuid
    {
        private static string fingerPrint = string.Empty;

        public static string Value()
        {
            if (string.IsNullOrEmpty(fingerPrint))
                fingerPrint =
                    GetHash("CPU >> " + cpuId() + "\nBIOS >> " + biosId() + "\nBASE >> " + baseId() + "\nUNAME >>" +
                            Environment.UserName);

            return fingerPrint;
        }

        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            var enc = new ASCIIEncoding();
            var bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }

        private static string GetHexString(byte[] bt)
        {
            var s = string.Empty;
            for (var i = 0; i < bt.Length; i++)
            {
                var b = bt[i];
                int n, n1, n2;
                n = b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char) (n2 - 10 + 'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char) (n1 - 10 + 'A')).ToString();
                else
                    s += n1.ToString();
                if (i + 1 != bt.Length && (i + 1) % 2 == 0) s += "-";
            }

            return s;
        }

        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.Read(buffer, 0, 2048);
            }

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        #region Original Device ID Getting Code

        //Return a hardware identifier
        private static string identifier
            (string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            var result = "";
            var mc =
                new ManagementClass(wmiClass);
            var moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
                if (mo[wmiMustBeTrue].ToString() == "True")
                    if (result == "")
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                        }

            return result;
        }

        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty)
        {
            var result = "";
            var mc =
                new ManagementClass(wmiClass);
            var moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
                //Only get the first one
                if (result == "" && mo[wmiProperty] != null)
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }

            return result;
        }

        private static string cpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            var retVal = identifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = identifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = identifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                        retVal = identifier("Win32_Processor", "Manufacturer");

                    //Add clock speed for extra security
                    retVal += identifier("Win32_Processor", "MaxClockSpeed");
                }
            }

            return retVal;
        }

        //BIOS Identifier
        private static string biosId()
        {
            return identifier("Win32_BIOS", "Manufacturer")
                   + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
                   + identifier("Win32_BIOS", "IdentificationCode")
                   + identifier("Win32_BIOS", "SerialNumber")
                   + identifier("Win32_BIOS", "ReleaseDate")
                   + identifier("Win32_BIOS", "Version");
        }

        //Main physical hard drive ID
        private static string diskId()
        {
            return identifier("Win32_DiskDrive", "Model")
                   + identifier("Win32_DiskDrive", "Manufacturer")
                   + identifier("Win32_DiskDrive", "Signature")
                   + identifier("Win32_DiskDrive", "TotalHeads");
        }

        //Motherboard ID
        private static string baseId()
        {
            return identifier("Win32_BaseBoard", "Model")
                   + identifier("Win32_BaseBoard", "Manufacturer")
                   + identifier("Win32_BaseBoard", "Name")
                   + identifier("Win32_BaseBoard", "SerialNumber");
        }

        //Primary video controller ID
        private static string videoId()
        {
            return identifier("Win32_VideoController", "DriverVersion")
                   + identifier("Win32_VideoController", "Name");
        }

        //First enabled network card ID
        private static string macId()
        {
            return identifier("Win32_NetworkAdapterConfiguration",
                "MACAddress", "IPEnabled");
        }

        #endregion
    }
}