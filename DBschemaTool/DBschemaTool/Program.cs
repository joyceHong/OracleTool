using DBschemaTool.Factory;
using DBschemaTool.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBschemaTool
{
    class Program
    {
        public static string filePath = "";
        static string NameSpace = "SPMTool.Model";
        static string StoreProcedure = "";
        static string ConfigConnectionName = "OracleConnectionString";
        static bool boolReturnCode = true;
        static bool boolReturnMsg = true;

        static void Main(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("缺乏必要參數"); return; }
            var logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                SetInitArgs(args);

                if (!string.IsNullOrEmpty(StoreProcedure))
                {
                    Console.WriteLine($"NameSpace : {NameSpace}");
                    Console.WriteLine($"filePath : {filePath}");
                    Console.WriteLine($"StoreProcedure : {StoreProcedure}");
                    var spname = StoreProcedure.Split('(')[0].Trim();

                    var outputModel = new DBFactory(ConfigConnectionName).FugoRepository.GetSpDto_copy(StoreProcedure, boolReturnCode, boolReturnMsg);
                    outputModel = FileHelper.FillOutputModel(outputModel, spname, Program.filePath);
                    FileHelper.CreateCsFile(outputModel, "Output", spname, Program.filePath);
                    FileHelper.CreateCsFile(FileHelper.CleanString(new DBFactory(ConfigConnectionName).FugoRepository.GetSpInputDto(spname, "PROCEDURE"), spname, Program.NameSpace, "PROCEDURE"), "Input", spname, Program.filePath);
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                logger.Error(ex, $"{StoreProcedure} Error: {ex.Message}");
            }

        }

        private static void SetInitArgs(string[] args)
        {
            for (int i = 0; i < args.Count(); ++i)
            {
                if (args[i].Trim().ToLower() == "-h")
                {


                    break;
                    throw new Exception(@"
-h | Help 
-p | Folder Path 
-n | Name Space 
-sp | Store Procedure Name
-c | ConfigConnectionName ");
                }
                if (args[i].Trim().ToLower() == "-n")
                {
                    NameSpace = args[i + 1];
                }
                if (args[i].Trim().ToLower() == "-p")
                {
                    filePath = args[i + 1];
                }
                if (args[i].Trim().ToLower() == "-sp")
                {
                    StoreProcedure = args[i + 1];
                }
                if (args[i].Trim().ToLower() == "-c")
                {
                    ConfigConnectionName = args[i + 1];
                }
                if (args[i].Trim().ToLower() == "-rc")
                {
                    boolReturnCode = Convert.ToBoolean(Convert.ToInt32(args[i + 1]));
                }

                if (args[i].Trim().ToLower() == "-rm")
                {
                    boolReturnMsg = Convert.ToBoolean(Convert.ToInt32(args[i + 1]));
                }
            }
        }

    }
}
