using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace OracleGen
{
    public class CodeGenerator
    {
        private DataTable _dtDataTypeMappings;
        private string _methodName;
        private DataTable _dtParams;
        private string _outParamName;
        private string _packageName;
        private DataTable _dtCols;

        public CodeGenerator(DataTable dtDataTypeMappings, string packageName, DataTable dtCols)
        {
            this._dtDataTypeMappings = dtDataTypeMappings;
            this._packageName = packageName;
            this._dtCols = dtCols;

            _dtParams = new DataTable();
            _dtParams.Columns.Add("NAME");
            _dtParams.Columns.Add("DATA_TYPE");
            _dtParams.Columns.Add("IS_OUT", typeof(bool));
            _dtParams.Columns.Add("PARAM_DATA_TYPE");
            _dtParams.Columns.Add("METHOD_PARAM_DATA_TYPE");
            _dtParams.Columns.Add("NEEDS_LENGTH", typeof(bool));
        }
        private void AddParam(string name, string dataType, bool isOut)
        {
            DataRow row = _dtParams.NewRow();
            row["NAME"] = name;
            row["DATA_TYPE"] = dataType;
            row["IS_OUT"] = isOut;

            DataRow[] rows = _dtDataTypeMappings.Select(
               string.Format("SourceType='{0}'", dataType));
            if (rows.Length > 0)
            {
                row["PARAM_DATA_TYPE"] = rows[0]["TargetType"];
                row["METHOD_PARAM_DATA_TYPE"] = rows[0]["TargetMethodType"];
                row["NEEDS_LENGTH"] = rows[0]["IncludeLength"];
            }
            else
            {
                row["PARAM_DATA_TYPE"] = "NOT_FOUND";
                row["METHOD_PARAM_DATA_TYPE"] = "NOT_FOUND";
                row["NEEDS_LENGTH"] = false;
            }
            _dtParams.Rows.Add(row);
        }
        public string GenerateDataTableMethod(string procText)
        {
            ParseProcText(procText);

            StringBuilder bld = new StringBuilder();
            bld.AppendFormat("public static DataTable {0}(", ConvertName(_methodName, true));
            bld.Append(GetMethodParams()).Append(")\r\n");
            bld.Append("{\r\n");
            bld.Append("using(OracleConnection cn=new OracleConnection(DBHelper.ConnectionString)){\r\n");
            bld.AppendFormat("OracleCommand cmd=new OracleCommand(\"{0}{1}{2}\", cn);\r\n", _packageName, (_packageName.Length>0 ? "." : ""), _methodName);
            bld.Append("cmd.CommandType = CommandType.StoredProcedure;\r\n");
            AddParams(bld);
            bld.Append("OracleDataAdapter da = new OracleDataAdapter(cmd);\r\n");
            bld.Append("DataTable d=new DataTable();\r\n");
            bld.Append("da.Fill(d);\r\n");
            bld.Append("return d;\r\n");
            bld.Append("}\r\n}\r\n");


            return bld.ToString();
        }
        public string GenerateIntMethod(string procText)
        {
            ParseProcText(procText);

            StringBuilder bld = new StringBuilder();
            bld.AppendFormat("public static int {0}(", ConvertName(_methodName, true));
            bld.Append(GetMethodParams()).Append(")\r\n");
            bld.Append("{\r\n");
            bld.Append("using(OracleConnection cn=new OracleConnection(DBHelper.ConnectionString)){\r\n");
            bld.AppendFormat("OracleCommand cmd=new OracleCommand(\"{0}{1}{2}\", cn);\r\n", _packageName, (_packageName.Length > 0 ? "." : ""), _methodName);
            bld.Append("cmd.CommandType = CommandType.StoredProcedure;\r\n");
            AddParams(bld);
            bld.Append("cn.Open();\r\n");
            bld.Append("cmd.ExecuteNonQuery();\r\n");
            bld.AppendFormat("return int.Parse(cmd.Parameters[\"{0}\"].Value.ToString());\r\n",
                _outParamName);
            bld.Append("}\r\n}\r\n");


            return bld.ToString();
        }
        public string GenerateVoidMethod(string procText)
        {
            ParseProcText(procText);

            StringBuilder bld = new StringBuilder();
            bld.AppendFormat("public static void {0}(", ConvertName(_methodName, true));
            bld.Append(GetMethodParams()).Append(")\r\n");
            bld.Append("{\r\n");
            bld.Append("using(OracleConnection cn=new OracleConnection(DBHelper.ConnectionString)){\r\n");
            bld.AppendFormat("OracleCommand cmd=new OracleCommand(\"{0}{1}{2}\", cn);\r\n", _packageName, (_packageName.Length > 0 ? "." : ""), _methodName);
            bld.Append("cmd.CommandType = CommandType.StoredProcedure;\r\n");
            AddParams(bld);
            bld.Append("cn.Open();\r\n");
            bld.Append("cmd.ExecuteNonQuery();\r\n");
            bld.Append("}\r\n}\r\n");


            return bld.ToString();
        }
        private string AddParams(StringBuilder bld)
        {
            foreach (DataRow row in _dtParams.Rows)
            {
                if ((bool)row["IS_OUT"])
                {
                    _outParamName = row["NAME"].ToString();
                    bld.AppendFormat("cmd.Parameters.Add(\"{0}\", OracleType.{1}).Direction = ParameterDirection.Output;\r\n", row["NAME"], row["PARAM_DATA_TYPE"]);
                }
                else
                {
                    if ((bool)row["NEEDS_LENGTH"])
                    {
                        bld.AppendFormat("cmd.Parameters.Add(\"{0}\", OracleType.{1}, {2}).Value = {3};\r\n", row["NAME"], row["PARAM_DATA_TYPE"], GetColumnLength(row["NAME"].ToString().Substring(2)), ConvertName(row["NAME"].ToString(), false));
                    }
                    else
                    {
                        bld.AppendFormat("cmd.Parameters.Add(\"{0}\", OracleType.{1}).Value = {2};\r\n", row["NAME"], row["PARAM_DATA_TYPE"], ConvertName(row["NAME"].ToString(), false));
                    }
                }
            }

            return bld.ToString();
        }
        private string GetMethodParams()
        {
            StringBuilder bld = new StringBuilder();
            foreach (DataRow row in _dtParams.Rows)
            {
                if ((bool)row["IS_OUT"])
                {
                    continue;
                }
                bld.AppendFormat("{0} {1},", row["METHOD_PARAM_DATA_TYPE"], ConvertName(row["NAME"].ToString(), false));
            }

            if (bld.Length > 0)
            {
                return bld.ToString().Substring(0, bld.Length - 1);
            }
            else
            {
                return "";
            }
        }
        private void ParseProcText(string procText)
        {
            string prefix = "PROCEDURE";
            int procIx = procText.IndexOf(prefix) + prefix.Length + 1;
            int openingBracketIx = procText.IndexOf("(");
            int closingBracketIx = procText.IndexOf(")");

            _methodName = procText.Substring(procIx, openingBracketIx - procIx).Trim();

            Console.WriteLine("method name: " + _methodName);

            string procParams = procText.Substring(openingBracketIx + 1, closingBracketIx - openingBracketIx - 1);

            Console.WriteLine("params: " + procParams);

            string[] paramParts = procParams.Trim().Split(',');

            foreach (string paramPart in paramParts)
            {
                if (paramPart == null || paramPart.Trim().Length == 0)
                {
                    continue;
                }

                string[] paramNameAndType = paramPart.Trim().Split(' ');
                if (paramNameAndType.Length == 3) //out param
                {
                    AddParam(paramNameAndType[0], paramNameAndType[2], true);
                }
                else if (paramNameAndType.Length == 2)
                {
                    AddParam(paramNameAndType[0], paramNameAndType[1], false);
                }

            }

        }
        private string GetColumnLength(string colName)
        {
            string query=string.Format("COLUMN_NAME='{0}'", colName);
            DataRow[] rows = _dtCols.Select(query);
            return rows[0]["CHAR_LENGTH"].ToString();
        }
        private string ConvertName(string name, bool isMethodName)
        {
            if (!isMethodName && name.StartsWith("V_"))
            {
                name = name.Substring(2);
            }
            StringBuilder bld = new StringBuilder();
            bool lastDash = false;
            int i = 0;
            foreach (char c in name)
            {
                if (c == '_')
                {
                    lastDash = true;
                }
                else
                {
                    if (lastDash)
                    {
                        bld.Append(c);
                    }
                    else
                    {
                        if (isMethodName && i == 0)
                        {
                            bld.Append(c);
                        }
                        else
                        {
                            bld.Append(char.ToLower(c));
                        }
                    }
                    lastDash = false;
                }
                i++;
            }

            return bld.ToString();
        }
    }
}
