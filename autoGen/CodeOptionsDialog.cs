using DBschemaTool.DB.ViewModel;
using DBschemaTool.Factory;
using DBschemaTool.Helper;
using DBschemaTool.Repository;
using DBschemaTool.Repository.Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OracleGen
{
    public partial class CodeOptionsDialog : Form
    {
        public string _tableName;
        public string _nameSpace;
        public string _ouputPath;
        public DataTable _dtCols;
        public string _configConnectionName;
        public string _oracleType; // storePrecudre/view/table
        public List<string> returnValue;
        DBFactory _dbFactory;
        public CodeOptionsDialog()
        {
            InitializeComponent();

        }

        private void CodeOptionsDialog_Load(object sender, EventArgs e)
        {
            this.Text = "Options - " + _tableName;
            _dbFactory = new DBFactory(_configConnectionName);
            ShowColumns();
        }

        private void ShowColumns()
        {
            txtSeq.Text = _tableName;
            DataTable dt = _dbFactory.FugoRepository.GetColumns(_tableName, _oracleType);
            _dtCols = dt;
            foreach (DataRow row in dt.Rows)
            {
                dataGridView1.Rows.Add(new string[]{
                        row["Name"].ToString(),
                        row["DataType"].ToString(),
                        row["ParamentType"].ToString(),
                        row["Length"].ToString(),
                        row["Nullable"].ToString(),
                        row["Point"].ToString()
                    });
            }
        }

        private void btnSearchInputModel_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder s = new StringBuilder();

                string columnName = string.Empty;
                string dataType = string.Empty;
                List<string> attrs = new List<string>();
                string paramentType;
                int? point;
                int? length;
                List<InputParameter> inputParameters = new List<InputParameter>();
                string nullAble;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    paramentType = dataGridView1.Rows[i].Cells["ParamentType"].Value?.ToString().Trim();
                    point = (string.IsNullOrWhiteSpace(dataGridView1.Rows[i].Cells["Point"].Value?.ToString())) ? null : (int?)Convert.ToInt32(dataGridView1.Rows[i].Cells["Point"].Value.ToString());
                    columnName = dataGridView1.Rows[i].Cells["Name"].Value?.ToString();
                    dataType = dataGridView1.Rows[i].Cells["DataType"].Value?.ToString();
                    nullAble = dataGridView1.Rows[i].Cells["Nullable"].Value?.ToString();
                    length = (string.IsNullOrWhiteSpace(dataGridView1.Rows[i].Cells["Length"].Value?.ToString())) ? null : (int?)Convert.ToInt32(dataGridView1.Rows[i].Cells["Length"].Value.ToString());
                    inputParameters.Add(new InputParameter()
                    {
                        Name = columnName,
                        DataType = dataType,
                        ParamentType = paramentType,
                        Point = point,
                        NullAble = nullAble,
                        Length = length
                    });
                }

                List<string> dataResult = FileHelper.CleanString(inputParameters, _tableName, _nameSpace, _oracleType);
                this.returnValue = dataResult;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查詢model失敗{ex.Message}");

            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                (List<string> paraments, List<OutParameter> outParaNames) = AutoGenParaments();
                string spxCall = $"{_tableName}({string.Join(",", paraments)})";
                if (_oracleType == "PROCEDURE" && paraments.Contains("ACUR_CUR"))
                {
                    var outputModel = new DBFactory(_configConnectionName).FugoRepository.GetSpDto(spxCall, outParaNames);
                    outputModel = FileHelper.FillOutputModel(outputModel, _tableName, _nameSpace);
                    FileHelper.CreateCsFile(outputModel, "Output", _tableName, _ouputPath);
                }
                FileHelper.CreateCsFile(FileHelper.CleanString(new DBFactory(_configConnectionName).FugoRepository.GetSpInputDto(_tableName, _oracleType), _tableName, _nameSpace, _oracleType), "Input", _tableName, _ouputPath);
                MessageBox.Show("InputModel產生完畢");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匯出檔案失敗{ex.Message}");
            }
        }

        private void btnAcurSearch_Click(object sender, EventArgs e)
        {

            string inputValue = string.Empty;
            string dataType = string.Empty;
            string spxWithArugements = string.Empty;
            string paramentType = string.Empty;
            (List<string> paraments, List<OutParameter> outParaNames) = AutoGenParaments();
            spxWithArugements = $"{txtSeq.Text}({string.Join(",", paraments)})";

            if (!paraments.Contains("ACUR_CUR"))
            {
                MessageBox.Show("無法支援無acur_cur的查詢");
                return;
            }

            try
            {
                var outputModel = new DBFactory("OracleConnectionString").FugoRepository.GetSpDto(spxWithArugements, outParaNames);
                this.returnValue = outputModel;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private (List<string> paraments, List<OutParameter> outParaNames) AutoGenParaments()
        {
            List<string> paraments = new List<string>();
            List<OutParameter> outParaNames = new List<OutParameter>();
            string paramentType;
            string dataType;
            string codeType; //轉換成model(c#)的型別
            string inputValue;
            int? point;
            string columnName;
            string nullAble;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                paramentType = dataGridView1.Rows[i].Cells["ParamentType"].Value?.ToString().Trim();
                point = (string.IsNullOrWhiteSpace(dataGridView1.Rows[i].Cells["Point"].Value.ToString())) ? null : (int?)Convert.ToInt32(dataGridView1.Rows[i].Cells["Point"].Value.ToString());
                dataType = dataGridView1.Rows[i].Cells["DataType"].Value.ToString();
                nullAble = dataGridView1.Rows[i].Cells["Nullable"]?.Value.ToString();
                codeType = FileHelper.ConvertType(paramentType, dataType, point, nullAble, _tableName, _nameSpace, _oracleType);
                columnName = dataGridView1.Rows[i].Cells["Name"].Value.ToString();
                inputValue = dataGridView1.Rows[i].Cells["Value"].Value?.ToString();
                inputValue = _dbFactory.FugoRepository.GetSpxParamentValue(codeType, paramentType, inputValue, columnName);
                paraments.Add(inputValue);
                if (paramentType.ToUpper() == "OUT")
                {
                    var OutParameter = _dbFactory.FugoRepository.DeclareSPXRtnColumnName(dataType, columnName);
                    if (OutParameter != null)
                    {
                        outParaNames.Add(OutParameter);
                    }
                }
            }
            return (paraments, outParaNames);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}