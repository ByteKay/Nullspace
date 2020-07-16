using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class ConvertMain
    {
        public static void Main(string[] argvs)
        {
            HashSet<string> ignore = new HashSet<string>();
            string xlsxDir = ".";
            string outCshapDir = ".";
            string outXmlDir = ".";
            if (argvs.Length > 0)
            {
                xlsxDir = argvs[0];
                if (argvs.Length > 1)
                {
                    outCshapDir = argvs[1];
                }
                if(argvs.Length > 2)
                {
                    outXmlDir = argvs[2];
                }
                for (int i = 3; i < argvs.Length; ++i)
                {
                    ignore.Add(argvs[i]);
                }
            }
            try
            {
                XlsxConvert.Convert(xlsxDir, outCshapDir, outXmlDir, ignore);
                Console.WriteLine("successfully convert...");
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
            }
            System.Threading.Thread.Sleep(100000);
        }
    }
}
