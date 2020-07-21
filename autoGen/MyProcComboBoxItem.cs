using System;
using System.Collections.Generic;
using System.Text;

namespace OracleGen
{
    class MyProcComboBoxItem
    {
        private string name;
        private string declaration;
        private bool isProc;
        private string package;

        public MyProcComboBoxItem(string declaration, string package)
        {
            this.declaration = declaration.Trim().ToUpper();
            this.package = package;
            Parse();
        }

        private void Parse()
        {
            string prefix = "PROCEDURE";

            if (!declaration.StartsWith(prefix))
            {
                isProc = false;
                return;
            }

            int procIx = declaration.IndexOf(prefix) + prefix.Length + 1;
            int openingBracketIx = declaration.IndexOf("(");

            name = declaration.Substring(procIx, openingBracketIx - procIx).Trim();

            isProc = true;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Declaration
        {
            get { return declaration; }
            set { declaration = value; }
        }
        public bool IsProc
        {
            get { return isProc; }
            set { isProc = value; }
        }
        public string Package
        {
            get { return package; }
            set { package = value; }
        }

        public override string ToString()
        {
            return name;
        }
    }
}
