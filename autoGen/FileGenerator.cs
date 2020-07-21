using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleGen
{
    public class FileGenerator
    {

        public static void writeToFile(string fileName, string writeLine)
        {
            string strPath = System.IO.Directory.GetCurrentDirectory();
            string[] lines = { writeLine };
            System.IO.File.WriteAllLines($"{strPath}\\{fileName}_input.txt", lines);
        }

        public static void runExe(string cParams)
        {
            string strPath = $"{System.IO.Directory.GetCurrentDirectory()}\\dbTool";
            string filename = Path.Combine(strPath, "DBschemaTool.exe");
            var proc = System.Diagnostics.Process.Start(filename, cParams);

        }
    }
}
