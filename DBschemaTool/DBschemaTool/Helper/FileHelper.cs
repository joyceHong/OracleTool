using DBschemaTool.DB.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBschemaTool.Helper
{
    public class FileHelper
    {

        public static void CreateCsFile(ICollection<string> sl, string v, string spname, string outputPath)
        {
            if (v.ToUpper() == "INPUT") { CreateCsFileInput(sl, spname, outputPath); };
            if (v.ToUpper() == "OUTPUT") { CreateCsFileOutput(sl, spname, outputPath); };
        }

        private static void CreateCsFileInput(ICollection<string> sl, string spname, string outputPath)
        {
            var filePath = Path.GetFullPath(System.IO.Path.Combine(outputPath, $"Input/{spname}.cs"));

            var directPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directPath))
            {
                Directory.CreateDirectory(directPath);
            }
            FlashFile(sl, filePath);
        }

        private static void CreateCsFileOutput(ICollection<string> sl, string spname, string outputPath)
        {
            var filePath = Path.GetFullPath(System.IO.Path.Combine(outputPath, $"Output/{spname}.cs"));

            var directPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directPath))
            {
                Directory.CreateDirectory(directPath);
            }
            FlashFile(sl, filePath);
        }

        private static void FlashFile(ICollection<string> sl, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.AppendAllLines(filePath, sl);
        }

        public static List<string> FillOutputModel(List<string> outputModel, string spname, string nameSpace)
        {
            outputModel.Insert(0, $"using System; ");
            outputModel.Insert(1, $"using System.Collections.Generic;");
            outputModel.Insert(2, $"using System.ComponentModel.DataAnnotations;");
            outputModel.Insert(3, $"using System.ComponentModel.DataAnnotations.Schema;");

            outputModel.Insert(4, $"namespace {nameSpace}.Output");
            outputModel.Insert(5, "{  ");

            outputModel.Insert(6, $"public class {spname} ");
            outputModel.Insert(7, "{  ");
            outputModel.Add("}");
            outputModel.Add("}");
            return outputModel;
        }

        public static List<string> CleanString(IEnumerable<InputParameter> columns, string spname, string nameSpace, string oracleType)
        {
            List<string> returnLs = new List<string>();
            returnLs.Add($"using System; ");
            returnLs.Add($"using System.Data;");
            returnLs.Add($"using EUMALL.DataAccess.Attributes;");
            returnLs.Add($"using System.Collections.Generic;");
            returnLs.Add($"using System.ComponentModel.DataAnnotations;");
            returnLs.Add($"using System.ComponentModel.DataAnnotations.Schema;");
            // 如果是其他類型view/table 就不需要顯示output
            if (oracleType.ToUpper() == "PROCEDURE")
            {
                returnLs.Add($"using {nameSpace}.Output;");
            }

            returnLs.Add($"namespace {nameSpace}.Input");
            returnLs.Add("{");
            returnLs.Add($"    public class {spname}");
            returnLs.Add("    {");
            foreach (var t in columns)
            {
                if (oracleType.ToUpper() == "PROCEDURE" && t.ParamentType?.ToUpper() == "OUT")
                {
                    returnLs.Add($"        [ParameterDirection(Direction = ParameterDirection.Output)]");
                }

                // 無法得知 return msg 長度 暫時寫死        
                if (oracleType.ToUpper() == "PROCEDURE" && t.ParamentType?.ToUpper() == "OUT" && t.DataType.ToUpper() == "VARCHAR2")
                {
                    returnLs.Add($"        [MaxLength(2000)]");
                }
                else if (oracleType.ToUpper() == "PROCEDURE" && t.ParamentType?.ToUpper() == "OUT" && t.DataType.ToUpper() == "NUMBER" && t.Length != null)
                {
                    returnLs.Add($"        [MaxLength({t.Length})]");
                }


                returnLs.Add($"        public {ConvertType(t.ParamentType, t.DataType, t.Point, t.NullAble, spname, nameSpace, oracleType)}  {t.Name}  " + " { get; set; }"); //input Model 
            }
            returnLs.Add("    }");
            returnLs.Add("}");
            return returnLs;
        }


        public static string ConvertType(string paramentInOut, string dataType, int? point, string nullAble, string spname, string nameSpace, string dbType)
        {
            string returnStr = string.Empty;
            string isNull = string.Empty;
            if (dbType.ToUpper() == "PROCEDURE" && paramentInOut?.ToUpper() == "IN")
            {
                isNull = "?";
            }
            else if (dbType.ToUpper() == "TABLE" || dbType.ToUpper() == "VIEW")
            {
                isNull = (nullAble.ToUpper() == "Y") ? "?" : string.Empty;
            }

            switch (dataType.ToUpper())
            {
                case "NUMBER":
                    if (point == null)
                    {
                        returnStr = $"int{isNull}";
                    }
                    else
                    {
                        if (point > 0)
                            returnStr = $"decimal{isNull}";
                        else
                            returnStr = $"int{isNull}";
                    }
                    break;
                case "NVARCHAR2":
                case "NCHAR":
                case "NCLOB":
                    returnStr = "string";
                    break;
                case "DATE":
                    returnStr = $"DateTime{isNull}";
                    break;
                case "BLOB":
                case "BFILE":
                    returnStr = $"byte[]{isNull}";
                    break;
                case "REF CURSOR":
                case "SYS_REFCURSOR":
                    returnStr = $"List<{nameSpace}.Output.{spname}>";
                    break;

                default:
                    returnStr = "string";
                    break;
            }
            return returnStr;

        }
    }
}
