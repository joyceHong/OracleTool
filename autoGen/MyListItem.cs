using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace OracleGen
{
    public class MyListItem
    {
        private string itemText;
        private DataTable dtCols;
        private List<ProcHeader> procHeaders = new List<ProcHeader>();

        public MyListItem(string itemText)
        {
            this.itemText = itemText;
        }

        public string ItemText
        {
            get { return itemText; }
            set { itemText = value; }
        }
        public DataTable DtCols
        {
            get { return dtCols; }
            set { dtCols = value; }
        }
        public List<ProcHeader> ProcHeaders
        {
            get { return procHeaders; }
            set { procHeaders = value; }
        }
        public override string ToString()
        {
            return itemText;
        }
    }
}
