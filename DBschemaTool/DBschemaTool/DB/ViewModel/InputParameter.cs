using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBschemaTool.DB.ViewModel
{
    public class InputParameter
    {
        /// <summary>
        /// 參數名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 資料型態
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 參數型別(in/out)
        /// </summary>
        public string ParamentType { get; set; }

        /// <summary>
        /// 欄位長度限制
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// 小數點數
        /// </summary>
        public int? Point { get; set; }

        /// <summary>
        /// 是否空值:YN 針對table/view 有效
        /// </summary>
        public String NullAble { get; set; }

    }
}
