using Dapper;
using DBschemaTool.DB.ViewModel;
using DBschemaTool.Repository.Common.Interface;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace DBschemaTool.Repository
{
    /// <summary>
    /// Pchome資料Repository
    /// </summary>
    public class FugoRepository : IDisposable
    {
        private IDatabaseConnectionHelper _DatabaseConnection { get; }

        internal FugoRepository(IDatabaseConnectionHelper databaseConnectionHelper)
        {
            this._DatabaseConnection = databaseConnectionHelper;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    return;
                }
                disposedValue = true;
            }
        }
        ~FugoRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        public List<string> GetSpDto(string des, List<OutParameter> outParameters)
        {
            // var rtnCode = needCode ? "AL_RTNCODE number ;" : "";
            // var rtnMsg = needCode ? "AS_RTNMSG varchar2(2000) ;" : "";

            string ourParaments = string.Empty;

            foreach (OutParameter outValue in outParameters)
            {
                ourParaments += $"{outValue.Name} {outValue.OutType};";
            }

            var getset = " { get; set; }";
            string sqlCommand = $@"declare
    cur number;
    cnt number;
    cols_desc dbms_sql.desc_tab;
    v_col_type varchar2(100);
    v_col_scale number;
    v_col_precision number;    
    v_csharp_col_type varchar2(100);
   {ourParaments}      
begin
    
    {des};    
    cur := dbms_sql.to_cursor_number(acur_cur); 
    
  
    -- common  
    dbms_sql.describe_columns(cur, cnt, cols_desc );
    
    
    FOR i IN 1..cols_desc.COUNT
    LOOP
      v_col_scale := cols_desc(i).col_scale;
      v_col_precision := cols_desc(i).col_precision;
    
      v_col_type := case when cols_desc(i).col_type = 1 then 'NVARCHAR2' 
                      when cols_desc(i).col_type = 2 then      'NUMBER' 
                      when cols_desc(i).col_type = 12 then     'DATE'  
                      when cols_desc(i).col_type = 96 then     'NCHAR'  
                      when cols_desc(i).col_type = 112 then    'NCLOB'  
                      when cols_desc(i).col_type = 113 then    'BLOB'  
                      when cols_desc(i).col_type = 114 then    'BFILE'                                             
                      end;
       if v_col_type = 'NUMBER' then
         if v_col_scale>0 then
            v_csharp_col_type := 'double';
         elsif v_col_scale = -127 then
            v_csharp_col_type := 'decimal';          
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,7) -1) then
            v_csharp_col_type := 'short'; --'byte';         
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,15) -1) then
            v_csharp_col_type := 'short';
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,31) -1) then
            v_csharp_col_type := 'int'; 
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,63) -1) then
            v_csharp_col_type := 'long';                                     
         end if;
       elsif v_col_type = 'NVARCHAR2' or v_col_type = 'NCHAR' or v_col_type = 'NCLOB' then
             v_csharp_col_type := 'string';  
       elsif v_col_type = 'DATE' then
             v_csharp_col_type := 'DateTime';  
       elsif v_col_type = 'BLOB' or v_col_type = 'BFILE' then
             v_csharp_col_type := 'byte[]';                            
       end if;
         
       if cols_desc(i).col_null_ok = true and (v_col_type = 'NUMBER' or v_col_type = 'DATE') then
            v_csharp_col_type :=v_csharp_col_type||'?'; 
       end if;
                
       dbms_output.put_line(
           v_csharp_col_type  
          ||' ' || cols_desc(i).col_name
          ||' '
          
          );
       
       v_csharp_col_type := '';
       
    END LOOP;


    dbms_sql.close_cursor(cur);
end;



";

            List<string> classStruct = new List<string>();
            using (var conn = _DatabaseConnection.Create())
            {
                conn.Execute("dbms_output.enable",
                   commandType: CommandType.StoredProcedure);
                DynamicParameters p = new DynamicParameters();
                p.Add("line", dbType: DbType.String,
                    direction: ParameterDirection.Output, size: 4000);
                p.Add("status", dbType: DbType.Int32,
                    direction: ParameterDirection.Output);
                conn.Execute(sqlCommand);
                int status;
                do
                {
                    conn.Execute("dbms_output.get_line", p,
                        commandType: CommandType.StoredProcedure);

                    var tempString = p.Get<string>("line");
                    if (!string.IsNullOrEmpty(tempString))
                    {
                        classStruct.Add("public " + p.Get<string>("line") + getset);
                        // Console.WriteLine(p.Get<string>("line"));

                    }
                    status = p.Get<int>("status");

                } while (status == 0);
                return classStruct;
            }

        }

        public List<string> GetSpDto_copy(string des, bool needCode, bool needMsg)
        {
            var rtnCode = needCode ? "AL_RTNCODE number ;" : "";
            var rtnMsg = needCode ? "AS_RTNMSG varchar2(2000) ;" : "";
            var getset = " { get; set; }";
            string sqlCommand = $@"declare
    cur number;
    cnt number;
    cols_desc dbms_sql.desc_tab;
    acur_cur sys_refcursor;
    v_col_type varchar2(100);
    v_col_scale number;
    v_col_precision number;    
    v_csharp_col_type varchar2(100);
   {rtnCode} 
   {rtnMsg}   
    
begin
    
    {des};    
    cur := dbms_sql.to_cursor_number(acur_cur); 
    
  
    -- common  
    dbms_sql.describe_columns(cur, cnt, cols_desc );
    
    
    FOR i IN 1..cols_desc.COUNT
    LOOP
      v_col_scale := cols_desc(i).col_scale;
      v_col_precision := cols_desc(i).col_precision;
    
      v_col_type := case when cols_desc(i).col_type = 1 then 'NVARCHAR2' 
                      when cols_desc(i).col_type = 2 then      'NUMBER' 
                      when cols_desc(i).col_type = 12 then     'DATE'  
                      when cols_desc(i).col_type = 96 then     'NCHAR'  
                      when cols_desc(i).col_type = 112 then    'NCLOB'  
                      when cols_desc(i).col_type = 113 then    'BLOB'  
                      when cols_desc(i).col_type = 114 then    'BFILE'                                             
                      end;
       if v_col_type = 'NUMBER' then
         if v_col_scale>0 then
            v_csharp_col_type := 'double';
         elsif v_col_scale = -127 then
            v_csharp_col_type := 'decimal';          
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,7) -1) then
            v_csharp_col_type := 'short'; --'byte';         
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,15) -1) then
            v_csharp_col_type := 'short';
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,31) -1) then
            v_csharp_col_type := 'int'; 
         elsif v_col_scale=0 and (power(10,v_col_precision) -1) <= (power(2,63) -1) then
            v_csharp_col_type := 'long';                                     
         end if;
       elsif v_col_type = 'NVARCHAR2' or v_col_type = 'NCHAR' or v_col_type = 'NCLOB' then
             v_csharp_col_type := 'string';  
       elsif v_col_type = 'DATE' then
             v_csharp_col_type := 'DateTime';  
       elsif v_col_type = 'BLOB' or v_col_type = 'BFILE' then
             v_csharp_col_type := 'byte[]';                            
       end if;
         
       if cols_desc(i).col_null_ok = true and (v_col_type = 'NUMBER' or v_col_type = 'DATE') then
            v_csharp_col_type :=v_csharp_col_type||'?'; 
       end if;
                
       dbms_output.put_line(
           v_csharp_col_type  
          ||' ' || cols_desc(i).col_name
          ||' '
          
          );
       
       v_csharp_col_type := '';
       
    END LOOP;


    dbms_sql.close_cursor(cur);
end;



";

            List<string> classStruct = new List<string>();
            using (var conn = _DatabaseConnection.Create())
            {
                conn.Execute("dbms_output.enable",
                   commandType: CommandType.StoredProcedure);
                DynamicParameters p = new DynamicParameters();
                p.Add("line", dbType: DbType.String,
                    direction: ParameterDirection.Output, size: 4000);
                p.Add("status", dbType: DbType.Int32,
                    direction: ParameterDirection.Output);
                conn.Execute(sqlCommand);
                int status;
                do
                {
                    conn.Execute("dbms_output.get_line", p,
                        commandType: CommandType.StoredProcedure);

                    var tempString = p.Get<string>("line");
                    if (!string.IsNullOrEmpty(tempString))
                    {
                        classStruct.Add("public " + p.Get<string>("line") + getset);
                        // Console.WriteLine(p.Get<string>("line"));

                    }
                    status = p.Get<int>("status");

                } while (status == 0);
                return classStruct;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table">SPX/view/table的名稱</param>
        /// <param name="oragleType">procedure/view/table 種類</param>
        /// <returns></returns>
        public IEnumerable<InputParameter> GetSpInputDto(string table, string oragleType)
        {
            string sqlCommand = string.Empty;
            if (oragleType == "PROCEDURE")
            {
                sqlCommand = this.GetSPXColumns(table);
            }
            else
            {
                sqlCommand = this.GetViewColumns(table);
            }


            using (var conn = _DatabaseConnection.Create())
            {
                var result = conn.Query<InputParameter>(sqlCommand);

                return result;
            }
        }

        public DataTable GetDT(string sql)
        {
            using (var conn = _DatabaseConnection.Create())
            {
                DataTable table = new DataTable("MyTable");
                var reader = conn.ExecuteReader(sql);
                table.Load(reader);
                return table;
            }
        }

        public DataTable GetColumns(string tableName, string dbType)
        {
            string sql = string.Empty;
            if (dbType == "PROCEDURE")
            {
                sql = this.GetSPXColumns(tableName);
            }
            else
            {
                sql = this.GetViewColumns(tableName);
            }

            DataTable dt = this.GetDT(sql);
            return dt;
        }

        private string GetViewColumns(string table)
        {
            return $"SELECT column_name as Name,data_type as DataType,'' as ParamentType,data_length as Length,nullable as Nullable,data_scale as point from all_tab_columns where  table_name ='{table}'";
        }

        private string GetSPXColumns(string spxName)
        {
            return $"SELECT argument_name as Name ,data_type as DataType,in_out as ParamentType,data_length as Length,defaulted as Nullable,data_scale as point FROM SYS.ALL_ARGUMENTS where object_name = '{spxName}' order by SEQUENCE";
        }

        /// <summary>
        /// 呼叫spx時需要傳入的值
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="inOutType">參數的型態 in/out</param>
        /// <param name="inputValue"></param>
        /// <param name="columName"></param>
        /// <returns></returns>
        public string GetSpxParamentValue(string dataType, string inOutType, string inputValue, string columName)
        {
            if (string.IsNullOrWhiteSpace(inputValue) && inOutType != "OUT")
            {
                return "null";
            }
            else if (inOutType == "OUT")
            {
                return columName;
            }

            switch (dataType)
            {
                case "string":
                    return $"'{inputValue}'";
                case "int":
                case "int?":
                    return $"{inputValue}";
                case "decimal":
                case "decimal?":
                    return $"{inputValue}";
                case "datetime":
                case "datetime?":
                    return null;
                //case "acur_cur":
                //    return "acur_cur";
                //case "al_rtncode":
                //    return "al_rtncode"; //傳回cursor                    
                //case "as_rtnmsg":
                //    return "as_rtnmsg"; //傳回cursor                    
                default:
                    return null;
            }
        }

        /// <summary>
        /// 呼叫spx動態宣告out 變數
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public OutParameter DeclareSPXRtnColumnName(string dbType, string columnName)
        {
            switch (dbType.ToUpper())
            {
                case "REF CURSOR":
                    return new OutParameter()
                    {
                        Name = columnName,
                        OutType = "sys_refcursor"
                    };
                case "NUMBER":
                    return new OutParameter()
                    {
                        Name = columnName,
                        OutType = dbType
                    };
                case "VARCHAR2":
                case "VARCHAR":
                    return new OutParameter()
                    {
                        Name = columnName,
                        OutType = dbType + " (2000)"
                    };
                default:
                    return null;
            }
        }
    }
}
