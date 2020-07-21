using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OracleGen.Properties;
using System.IO;
using System.Configuration;
using DBschemaTool.Factory;

namespace OracleGen
{
    public partial class Form1 : Form
    {
        private StringFormat _stringFormat = new StringFormat(StringFormatFlags.NoWrap);
        public string _returnValue;
        DBFactory _dbFactory;
        string _configConnectionName = "OracleConnectionString";

        public Form1()
        {
            InitializeComponent();


            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            _stringFormat.LineAlignment = StringAlignment.Center;
            _stringFormat.Trimming = StringTrimming.EllipsisCharacter;
        }

        void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void setConnectionStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectDialog dlg = new ConnectDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                ShowTables(cmbDBType.SelectedValue.ToString());
            }
            dlg.Dispose();
        }

        private void ShowTables(string dbType)
        {

            DataTable dt = _dbFactory.FugoRepository.GetDT($@"select OBJECT_NAME from all_objects
                        where owner = 'FUGO21'
                        and status = 'VALID'
                        and object_type in ( '{dbType}')
                        and OBJECT_NAME not like 'PBC%'
                        and OBJECT_NAME not like 'TOAD_%'
                        and OBJECT_NAME not like 'PLAN_%'
                        order by object_type, object_name");

            lsTables.BeginUpdate();
            lsTables.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                lsTables.Items.Add(new MyListItem(row["OBJECT_NAME"].ToString()));
            }
            lsTables.EndUpdate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
        }

        private void lsTables_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (lsTables.Items.Count == 0 || e.Index < 0)
            {
                return;
            }
            Brush fontBrush = Brushes.Black; ;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(Brushes.Green, e.Bounds);
                fontBrush = Brushes.White;
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            }

            e.Graphics.DrawString(lsTables.Items[e.Index].ToString(), e.Font, fontBrush, e.Bounds, _stringFormat);

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                e.DrawFocusRectangle();
            }
        }

        private void lsTables_SizeChanged(object sender, EventArgs e)
        {
            lsTables.BeginUpdate();
            lsTables.Refresh();
            lsTables.EndUpdate();
        }

        private void lsTables_DoubleClick(object sender, EventArgs e)
        {
            if (lsTables.SelectedItem == null)
            {
                return;
            }
            GenerateCode(lsTables.SelectedItem.ToString(), txtOutput.Text, txtNameSpace.Text);
        }

        private void GenerateCode(string tableName, string outputPath, string nameSpace)
        {
            CodeOptionsDialog dlg = new CodeOptionsDialog();
            dlg._configConnectionName = _configConnectionName;
            dlg._tableName = tableName;
            dlg._nameSpace = nameSpace;
            dlg._ouputPath = outputPath;
            dlg._oracleType = cmbDBType.SelectedValue.ToString();

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                List<string> returnModel = dlg.returnValue;
                txtCode.Text = string.Join("\r\n", dlg.returnValue);
            }
            dlg.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            cmbDBType.DataSource = Enum.GetNames(typeof(DBType));
            cmbDBType.SelectedIndex = 0;
            ReadConnection();
        }

        private void ReadConnection()
        {
            _dbFactory = new DBFactory(_configConnectionName);
            ShowTables(cmbDBType.SelectedValue.ToString());
        }

        private void lsTables_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnShowCode_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            txtOutput.Text = path.SelectedPath;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxSpxName.Text))
            {
                return;
            }

            GenerateCode(tbxSpxName.Text, txtOutput.Text, txtNameSpace.Text);
        }

        private void cmbDBType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowTables(cmbDBType.SelectedValue.ToString());
        }
    }
}