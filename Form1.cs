using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Xml;
using System.Net;

namespace AlphaIMS
{
	public partial class Form1 : Form
	{
        static int SelectHeaderRowIndex = -1;
        static int SelectHeaderColumnIndex = -1;
        List<TABLECOLUMN> tableColList = new List<TABLECOLUMN>();

		public Form1()
		{
			InitializeComponent();

            this.Width = 800;
            this.Height = 720;
            this.Text = "AlphaIMS-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.BackColor = Color.FromArgb(61, 64, 77);
            monthCalendar1.MaxSelectionCount = 1;
            controlPanel.BackColor = Color.FromArgb(45, 57, 83);
            toolTip2.SetToolTip(deleteBtn, "删除表格");
            monthCalendar1.BringToFront();
            userLabel.Text = "";

            readBtn.Location = new Point(readBtn.Location.X, measureGroupBox.Location.Y + measureGroupBox.Height + 30);
            saveBtn.Location = new Point(readBtn.Location.X, readBtn.Location.Y + readBtn.Height + 10);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
            TableXml.Init();

            string xmlFile = Application.StartupPath + "\\Setting.xml";
            XmlDocument doc = new XmlDocument();
            if (File.Exists(xmlFile))
            {
                doc.Load(xmlFile);

                XmlNode node = doc.DocumentElement.SelectSingleNode("/Setting/FilePath");
                if (node != null)
                {
                    filepathTextBox.Text = node.InnerText;
                    folderBrowserDialog1.SelectedPath = filepathTextBox.Text;
                }

                int size;
                node = doc.DocumentElement.SelectSingleNode("/Setting/Font");
                if (node != null)
                    if (Int32.TryParse(node.InnerText, out size))
                        DataGridView1.DefaultCellStyle.Font = new Font("Microsoft JhengHei", size);
            }

            try
            {
                Directory.CreateDirectory(folderBrowserDialog1.SelectedPath);
            }
            catch (Exception ex)
            {
                EventLog.Write(ex.Message);
            }
		}

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tableColList.Clear();
            TableXml.Deinit();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            controlPanel.Width = this.Width;

            DataGridView1.Width = this.Width - DataGridView1.Location.X - 20;
            DataGridView1.Height = this.Height - DataGridView1.Location.Y - 50;

            minimizeBtn.Location = new Point(controlPanel.Width - 90, minimizeBtn.Location.Y);
            maximizeBtn.Location = new Point(controlPanel.Width - 65, maximizeBtn.Location.Y);
            closeBtn.Location = new Point(controlPanel.Width - 40, closeBtn.Location.Y);

            deleteBtn.Location = new Point(controlPanel.Width - 120, minimizeBtn.Location.Y + 25);
            settingBtn.Location = new Point(deleteBtn.Location.X - settingBtn.Width - 10, deleteBtn.Location.Y);
            settingPanel.Location = new Point(settingBtn.Location.X + settingBtn.Width - settingPanel.Width, settingBtn.Location.Y + settingBtn.Height + 10);

            if (this.WindowState == FormWindowState.Maximized)
            {
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.normal;
            }
            else
            {
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.maximize;
            }
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void closeBtn_MouseEnter(object sender, EventArgs e)
        {
            closeBtn.BackgroundImage = AlphaIMS.Properties.Resources.close_dark;
        }

        private void minimizeBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void maximizeBtn_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.maximize;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.normal;
            }
        }

        private void closeBtn_MouseLeave(object sender, EventArgs e)
        {
            closeBtn.BackgroundImage = AlphaIMS.Properties.Resources.close;
        }

        private void minimizeBtn_MouseEnter(object sender, EventArgs e)
        {
            minimizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.minimize_dark;
        }

        private void minimizeBtn_MouseLeave(object sender, EventArgs e)
        {
            minimizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.minimize;
        }

        private void maximizeBtn_MouseEnter(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.normal_dark;
            }
            else
            {
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.maximize_dark;
            }
        }

        private void maximizeBtn_MouseLeave(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.normal;
            }
            else
            {
                maximizeBtn.BackgroundImage = AlphaIMS.Properties.Resources.maximize;
            }
        }


        //-------------------------------------Product Setting-------------------------------------
        private void stationComboBox_GotFocus(object sender, EventArgs e)
        {
            stationComboBox.Items.Clear();

            List<string> stations = new List<string>();
            TableXml.GetStations(ref stations);
            for (int i = 0; i < stations.Count; i++ )
                stationComboBox.Items.Add(stations[i]);

            stations.Clear();
            stations = null;
        }

        private void stationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataGridView1.Rows.Clear();
            DataGridView1.Columns.Clear();
            modelComboBox.Items.Clear();
            modelComboBox.Text = "";
            customerComboBox.Items.Clear();
            customerComboBox.Text = "";

            readBtn.Enabled = false;
        }

        private void stationComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!LoginForm.Login)
                e.Handled = true;
        }

        private void modelComboBox_GotFocus(object sender, EventArgs e)
        {
            modelComboBox.Items.Clear();
            if (stationComboBox.Text != "")
            {
                List<string> models = new List<string>();
                TableXml.GetModel(stationComboBox.Text, ref models);

                for (int i = 0; i < models.Count; i++)
                    modelComboBox.Items.Add(models[i]);

                models.Clear();
                models = null;
            }
        }

        private void modelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataGridView1.Rows.Clear();
            DataGridView1.Columns.Clear();
            customerComboBox.Items.Clear();
            customerComboBox.Text = "";

            readBtn.Enabled = false;
        }

        private void modelComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!LoginForm.Login)
                e.Handled = true;
        }

        private void customerComboBox_GotFocus(object sender, EventArgs e)
        {
            customerComboBox.Items.Clear();

            if (stationComboBox.Text != "" && modelComboBox.Text != "")
            {
                List<string> customers = new List<string>();

                TableXml.GetCustomers(stationComboBox.Text, modelComboBox.Text, ref customers);

                for (int i = 0; i < customers.Count; i++)
                    customerComboBox.Items.Add(customers[i]);

                customers.Clear();
                customers = null;
            }
        }

        private void customerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (stationComboBox.Text != "" && modelComboBox.Text != "")
            {
                int rows = 0;
                int columns = 0;
                TABLECOLUMN tableCol;

                tableColList.Clear();
                DataGridView1.Rows.Clear();
                DataGridView1.Columns.Clear();

                TableXml.GetProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, ref rows, ref columns, ref tableColList);

                DataGridView1.ColumnCount = columns;
                for (int i = 0; i < columns; i++) 
                {
                    if (i < tableColList.Count)
                    {
                        tableCol = tableColList[i];
                    }
                    else
                    {
                        tableCol = new TABLECOLUMN();
                        tableColList.Add(tableCol);
                    }

                    if (tableCol.Readvalue == 4)
                        DataGridView1.Columns[i].Width = 100;
                    else
                        DataGridView1.Columns[i].Width = 60;

                    DataGridView1.Columns[i].Name = tableCol.Name;
                    DataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                for (int i = 0; i < rows; i++)
                {
                    string[] row = new string[columns];
                    DataGridView1.Rows.Add(row);

                    DataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
                }

                readBtn.Enabled = true;

                this.ActiveControl = DataGridView1;
            }
        }

        private void customerComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TABLECOLUMN tableCol;
            if (e.KeyChar == (Char)13)
            {
                if (stationComboBox.Text != "" && modelComboBox.Text != "" && customerComboBox.Text != "")
                {
                    if (TableXml.SelectProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text) == 1)
                        return;

                    tableColList.Clear();
                    DataGridView1.Rows.Clear();
                    DataGridView1.Columns.Clear();

                    DataGridView1.ColumnCount = 10;

                    for (int i = 0; i < DataGridView1.ColumnCount; i++)
                    {
                        DataGridView1.Columns[i].Width = 60;
                        DataGridView1.Columns[i].Name = Convert.ToChar(i + 65).ToString();
                        DataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                        tableCol = new TABLECOLUMN();
                        tableCol.Name = DataGridView1.Columns[i].Name;

                        tableColList.Add(tableCol);
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        DataGridView1.Rows.Add("");
                        DataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
                    }

                    DataGridView1.CurrentCell.Selected = false;

                    int rows = DataGridView1.Rows.Count;
                    int columns = DataGridView1.Columns.Count;

                    TableXml.InsertProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, rows, columns);
                    for (int i = 0; i < tableColList.Count; i++)
                    {
                        tableCol = tableColList[i];
                        TableXml.InsertColumnInfo(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, i, tableCol);
                    }

                    this.ActiveControl = DataGridView1;
                }
            }
            else
            {
                if (!LoginForm.Login)
                    e.Handled = true;
            }
        }

        //-------------------------------------Measure Setting-------------------------------------
        private void measureTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)13)
            {
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void lotnumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)13)
            {
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void dateTextBox_Enter(object sender, EventArgs e)
        {
            monthCalendar1.Visible = true;
        }

        private void dateTextBox_Leave(object sender, EventArgs e)
        {
            if (!monthCalendar1.Focused)
                monthCalendar1.Visible = false;
        }

        private void dateTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            var monthCalendar = sender as MonthCalendar;
            dateTextBox.Text = string.Format("{0:yyyyMMdd}", monthCalendar.SelectionStart);
            monthCalendar.Visible = false;
            this.SelectNextControl((Control)sender, true, true, true, true);
        }

        private void monthCalendar1_Leave(object sender, EventArgs e)
        {
            var monthCalendar = sender as MonthCalendar;
            monthCalendar.Visible = false;
        }


        //-------------------------------------DataGridView Event-------------------------------------
        private void DataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SelectHeaderRowIndex = -1;
                if (DataGridView1.Rows[e.RowIndex].HeaderCell.Value != null)
                {
                    SelectHeaderRowIndex = e.RowIndex;
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SelectHeaderColumnIndex = e.ColumnIndex;
                contextMenuStrip2.Show(Cursor.Position);
            }
        }

        private void DataGridView1_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (!LoginForm.Login)
                return;

            var cellRectangle = DataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            
            TABLECOLUMN tableCol = tableColList[e.ColumnIndex];
            DataGridView1.Columns[e.ColumnIndex].HeaderCell.Style.BackColor = Color.FromArgb(72, 180, 225);

            int locationX = this.Location.X + DataGridView1.Location.X + cellRectangle.Location.X + 5;
            int locationY = this.Location.Y + DataGridView1.Location.Y + cellRectangle.Location.Y + cellRectangle.Height + 30;

            ColumnForm form = new ColumnForm(e.ColumnIndex, locationX, locationY, tableCol);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                tableCol.Name = form.ReturnName;
                tableCol.Formula = form.ReturnFormula;
                tableCol.Min = form.ReturnMin;
                tableCol.Max = form.ReturnMax;
                tableCol.Readvalue = form.ReturnReadvalue;
                tableColList[e.ColumnIndex] = tableCol;

                TableXml.SaveColumnInfo(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, e.ColumnIndex, tableCol);

                DataGridView1.Columns[e.ColumnIndex].Name = tableCol.Name;

                if(tableCol.Readvalue == 4)
                    DataGridView1.Columns[e.ColumnIndex].Width = 100;
                else
                    DataGridView1.Columns[e.ColumnIndex].Width = 60;
            }

            DataGridView1.Columns[e.ColumnIndex].HeaderCell.Style.BackColor = SystemColors.Control;
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int column = DataGridView1.CurrentCell.ColumnIndex;
                int row = DataGridView1.CurrentCell.RowIndex;
                string value = GetValue(column, row);
                DataGridView1.CurrentCell.Value = value;
                DataGridView1.CurrentCell.Selected = false;

                RefreshFormula(row);

                NextValue(ref row, ref column);
                DataGridView1.CurrentCell = DataGridView1[column, row];
                DataGridView1.CurrentCell.Selected = true;
            }
            e.Handled = true;
        }

        private void addRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectHeaderRowIndex > -1)
            {
                DataGridView1.Rows.Insert(SelectHeaderRowIndex);

                for (int i = 0; i < DataGridView1.RowCount; i++)
                    DataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();

                SelectHeaderRowIndex = -1;

                TableXml.SaveProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, DataGridView1.RowCount, DataGridView1.ColumnCount);
            }
        }

        private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectHeaderRowIndex > -1)
            {
                if (stationComboBox.Text != "" && modelComboBox.Text != "" && customerComboBox.Text != "")
                {
                    DataGridView1.Rows.RemoveAt(SelectHeaderRowIndex);

                    for (int i = 0; i < DataGridView1.RowCount; i++)
                        DataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();

                    TableXml.SaveProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, DataGridView1.RowCount, DataGridView1.ColumnCount);
                }
                SelectHeaderRowIndex = -1;
             }
        }

        private void addColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectHeaderColumnIndex > -1)
            {
                if (stationComboBox.Text != "" && modelComboBox.Text != "" && customerComboBox.Text != "")
                {
                    TABLECOLUMN tableCol = new TABLECOLUMN();
                    tableCol.Name = "New";

                    DataGridViewColumn dtCol = new DataGridViewColumn();
                    dtCol.Name = "New";
                    dtCol.Width = 60;
                    dtCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                    dtCol.CellTemplate = new DataGridViewTextBoxCell();

                    tableColList.Insert(SelectHeaderColumnIndex, tableCol);
                    DataGridView1.Columns.Insert(SelectHeaderColumnIndex, dtCol);
                    TableXml.InsertColumnInfo(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, SelectHeaderColumnIndex, tableCol);
                    TableXml.SaveProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, DataGridView1.RowCount, DataGridView1.ColumnCount);
                }
                    
                SelectHeaderColumnIndex = -1;
            }
        }

        private void deleteColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectHeaderColumnIndex > -1)
            {
                if (stationComboBox.Text != "" && modelComboBox.Text != "" && customerComboBox.Text != "")
                {
                    tableColList.RemoveAt(SelectHeaderColumnIndex);
                    DataGridView1.Columns.RemoveAt(SelectHeaderColumnIndex);

                    TableXml.DeleteColumnInfo(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, SelectHeaderColumnIndex);
                    TableXml.SaveProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, DataGridView1.RowCount, DataGridView1.ColumnCount);
                }

                SelectHeaderColumnIndex = -1;  
            }
        }


        //-------------------------------------Button Event-------------------------------------
        private void HorizontalRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.ActiveControl = DataGridView1;
        }

        private void VerticalRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.ActiveControl = DataGridView1;
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            if (stationComboBox.Text == "")
            {
                MessageBox.Show("请输入站別", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (modelComboBox.Text == "")
            {
                MessageBox.Show("请输入产品型号", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (customerComboBox.Text == "")
            {
                MessageBox.Show("请输入客戶別", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定删除表格啊?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                TableXml.DeletProduct(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text);

                DataGridView1.Rows.Clear();
                DataGridView1.Columns.Clear();
                stationComboBox.Text = "";
                modelComboBox.Text = "";
                customerComboBox.Text = "";
            }
        }

        private void settingBtn_Click(object sender, EventArgs e)
        {
            settingPanel.Visible = !settingPanel.Visible;

            if (settingPanel.Visible)
            {
                fontComboBox.Text = Convert.ToInt32(DataGridView1.DefaultCellStyle.Font.Size).ToString();
            }
            else
            {
                int oldSize = Convert.ToInt32(DataGridView1.DefaultCellStyle.Font.Size);
                int newSize;

                if (fontComboBox.Text != "")
                {
                    if (Int32.TryParse(fontComboBox.Text, out newSize))
                        DataGridView1.DefaultCellStyle.Font = new Font("Microsoft JhengHei", newSize);
                    else
                        newSize = oldSize;

                    int columns = DataGridView1.Columns.Count;
                    int rows = DataGridView1.Rows.Count;

                    for (int i = 0; i < columns; i++)
                        DataGridView1.Columns[i].Width = (DataGridView1.Columns[i].Width / oldSize) * newSize;

                    for (int j = 0; j < rows; j++)
                    {
                        DataGridView1.Rows[j].Height = newSize + 15;
                    }
                }

                string xmlFile = Application.StartupPath + "\\Setting.xml";
                if (File.Exists(xmlFile))
                {
                    XmlDocument doc;
                    XmlNode fontNode;
                    XmlNode pathNode;

                    doc = new XmlDocument();
                    doc.Load(xmlFile);

                    fontNode = doc.DocumentElement.SelectSingleNode("/Setting/Font");
                    if (fontNode != null)
                        fontNode.InnerText = Convert.ToInt32(DataGridView1.DefaultCellStyle.Font.Size).ToString();

                    pathNode = doc.DocumentElement.SelectSingleNode("/Setting/FilePath");
                    if (pathNode != null)
                        pathNode.InnerText = folderBrowserDialog1.SelectedPath;

                    doc.Save(xmlFile);
                }
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (stationComboBox.Text == "")
            {
                MessageBox.Show("请输入站別", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (modelComboBox.Text == "")
            {
                MessageBox.Show("请输入产品型号", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (customerComboBox.Text == "")
            {
                MessageBox.Show("请输入客戶別", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lotnumberTextBox.Text == "")
            {
                MessageBox.Show("请输入批号", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (machineIdTextBox.Text == "")
            {
                MessageBox.Show("请输入机台编号", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int locationX = saveBtn.Location.X + saveBtn.Width + 10;
            int locationY = saveBtn.Location.Y;
            InAdditionForm form = new InAdditionForm(locationX, locationY);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                Application.UseWaitCursor = true;
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents(); //<-- IMPORTANT 

                //saveDataToDB();

                string filename = filepathTextBox.Text + String.Format("\\{0}_{1}_{2}_{3}_{4}_{5:yyyyMMddHHmm}.xls", lotnumberTextBox.Text, machineIdTextBox.Text, stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, DateTime.Now);
                if (Directory.Exists(filepathTextBox.Text))
                    saveFileToExcel(filename);

                if (form.ReturnMailList != "")
                {
                    SendMail(form.ReturnMailList, filename);
                }

                for (int i = 0; i < DataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < DataGridView1.Columns.Count; j++)
                    {
                        DataGridView1[j, i].Value = "";
                    }
                }

                Application.UseWaitCursor = false;
                this.Cursor = Cursors.Default;
                Application.DoEvents(); //<-- IMPORTANT
            }
        }

        private void readBtn_Click(object sender, EventArgs e)
        {
            if (DataGridView1.CurrentCell == null)
            {
                MessageBox.Show("请选择表格", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int column = DataGridView1.CurrentCell.ColumnIndex;
            int row = DataGridView1.CurrentCell.RowIndex;
            string value = GetValue(column, row);
            DataGridView1.CurrentCell.Value = value;
            DataGridView1.CurrentCell.Selected = false;

            RefreshFormula(row);

            NextValue(ref row, ref column);
            DataGridView1.CurrentCell = DataGridView1[column, row];
            DataGridView1.CurrentCell.Selected = true; 
        }

        private void securityBtn_Click(object sender, EventArgs e)
        {
            int locationX = securityBtn.Location.X;
            int locationY = controlPanel.Location.Y + controlPanel.Height;
            LoginForm form = new LoginForm(locationX, locationY);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                if (LoginForm.Login)
                {
                    userLabel.Text = form.ReturnUser;
                    securityBtn.BackgroundImage = AlphaIMS.Properties.Resources.security_logined;
                    deleteBtn.Enabled = true;
                }
                else
                {
                    userLabel.Text = "";
                    securityBtn.BackgroundImage = AlphaIMS.Properties.Resources.security;
                    deleteBtn.Enabled = false;
                }
            }
        }

        private void securityBtn_MouseEnter(object sender, EventArgs e)
        {
            if (!LoginForm.Login)
                securityBtn.BackgroundImage = AlphaIMS.Properties.Resources.security_dark;
        }

        private void securityBtn_MouseLeave(object sender, EventArgs e)
        {
            if (!LoginForm.Login)
                securityBtn.BackgroundImage = AlphaIMS.Properties.Resources.security;
        }

        private void filePathBtn_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                filepathTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void filepathTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        //-------------------------------------Other-------------------------------------
        private string GetValue(int column, int row)
        {
            double value = 0.0;
            TABLECOLUMN tableCol = tableColList[column];

            if (tableCol == null)
            {
                EventLog.Write("Not found TableCol");
                return value.ToString("f3");
            }

            try
            {
                Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(printscreen as Image);

                int w = printscreen.Size.Width;
                int h = printscreen.Size.Height;
                graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
                printscreen.Save("D:\\PrintScreen.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c microscope.exe D:\\PrintScreen.bmp");
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                proc.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                EventLog.Write(ex.Message);
            }
            finally
            {
                if (File.Exists("D:\\PrintScreen.bmp"))
                    File.Delete("D:\\PrintScreen.bmp");

                if (File.Exists("D:\\PrintScreen.txt"))
                {
                    string line;
                    int index = 0;
                    StreamReader file = new StreamReader("D:\\PrintScreen.txt");
                    while ((line = file.ReadLine()) != null)
                    {
                        if (tableCol.Readvalue == index)
                        {
                            Double.TryParse(line.Trim('-'), out value);
                            break;
                        }
                        index++;
                    }

                    file.Close();
                    File.Delete("D:\\PrintScreen.txt");
                }
            }

            if (value < tableCol.Min || value > tableCol.Max)
                DataGridView1[column, row].Style.ForeColor = Color.Red;
            else
                DataGridView1[column, row].Style.ForeColor = Color.Black;

            return value.ToString("f3");
        }

        private void NextValue(ref int row, ref int column)
        {
            if (row == DataGridView1.Rows.Count - 1 && column == DataGridView1.Columns.Count - 1)
                return;

            TABLECOLUMN tableCol;

            if (VerticalRadioButton.Checked)
            {
                if (row >= DataGridView1.Rows.Count - 1)
                {
                    while ((column + 1) <= DataGridView1.Columns.Count - 1)
                    {
                        column++;
                        row = 0;
                        tableCol = tableColList[column];

                        if (tableCol.Formula == "")
                        {
                            break;
                        }
                        else
                        {
                            row = DataGridView1.Rows.Count - 1;
                        }
                    }       
                }
                else
                {
                    row++;
                }
            }
            else
            {
                if (column >= DataGridView1.Columns.Count - 1)
                {
                    if (row < DataGridView1.Rows.Count - 1)
                    {
                        row++;
                        column = 0;
                    }
                }
                else
                {
                    column++;
                }

                tableCol = tableColList[column];

                if (tableCol.Formula != "")
                {
                    if (column >= DataGridView1.Columns.Count - 1)
                    {
                        if (row < DataGridView1.Rows.Count - 1)
                        {
                            row++;
                            column = 0;
                        }
                    }
                    else
                    {
                        column++;
                    }
                }
            }
        }

        private void RefreshFormula(int row)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameter = new CompilerParameters();
            parameter.ReferencedAssemblies.Add("System.dll");
            parameter.GenerateExecutable = false;
            parameter.GenerateInMemory = true;
            CompilerResults result;

            TABLECOLUMN tableCol;
            string line;
            double value;

            for (int i = 0; i < tableColList.Count; i++ )
            {
                tableCol = tableColList[i];

                if (tableCol.Formula == "")
                    continue;

                value = 0.00;
                line = "";
                for (int j = 0; j < tableCol.Formula.Length; j++)
                {
                    int asciie = Convert.ToInt32(tableCol.Formula[j]) - 65;
                    if (asciie >= 0 && asciie <= 25 && asciie < DataGridView1.Columns.Count)
                    {
                        if (DataGridView1[asciie, row].FormattedValue.ToString() == "")
                            line += "0.0";
                        else
                            line += DataGridView1[asciie, row].FormattedValue.ToString();
                    }
                    else
                    {
                        line += tableCol.Formula[j];
                    }
                }

                result = provider.CompileAssemblyFromSource(parameter, CreateCode(line));

                if (result.Errors.Count == 0)
                {
                    Assembly assembly = result.CompiledAssembly;
                    Type AType = assembly.GetType("ANameSpace.AClass");
                    MethodInfo method = AType.GetMethod("AFunc");

                    Double.TryParse(method.Invoke(null, null).ToString(), out value);
                }

                DataGridView1[i, row].Value = value.ToString("f3");

                if (value < tableCol.Min || value > tableCol.Max)
                {
                    DataGridView1[i, row].Style.ForeColor = Color.Red;
                }
                else
                {
                    DataGridView1[i, row].Style.ForeColor = Color.Black;
                }
            }
        }

        private string CreateCode(string para)
        {
            return "using System; namespace ANameSpace{static class AClass{public static object AFunc(){return " + para + ";}}}";
        }

        private void saveDataToDB()
        {
            string result = "";
            for (int i = 0; i < DataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < DataGridView1.Columns.Count; j++)
                {
                    result += DataGridView1[j, i].FormattedValue.ToString();

                    if (j == DataGridView1.Columns.Count - 1)
                        result += "\r\n";
                    else
                        result += ",";
                }
            }

            TableXml.SaveData(stationComboBox.Text, modelComboBox.Text, customerComboBox.Text, result, string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); 
        }

        private void saveFileToExcel(string filename)
        {
            try
            {
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Add(true);

                Excel.Worksheet xlWorksheet = (Excel.Worksheet)xlApp.ActiveSheet;
                Excel.Range xlRange = xlWorksheet.UsedRange;

                for (int i = 0; i < DataGridView1.ColumnCount; i++)
                {
                    xlRange = (Excel.Range)xlWorksheet.Cells[1, i + 1];
                    xlRange.Value2 = DataGridView1.Columns[i].HeaderText;

                    xlRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                for (int j = 0; j < DataGridView1.RowCount; j++)
                {
                    for (int i = 0; i < DataGridView1.ColumnCount; i++)
                    {
                        xlRange = (Excel.Range)xlWorksheet.Cells[j + 2, i + 1];

                        if (DataGridView1[i, j].Value == null)
                        {
                            xlRange.Value2 = "0.0";
                        }
                        else
                        {
                            xlRange.Value2 = DataGridView1[i, j].Value.ToString();
                            xlRange.Font.Color = System.Drawing.ColorTranslator.ToOle(DataGridView1[i, j].Style.ForeColor);
                            DataGridView1[i, j].Value = "";
                        }

                        xlRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    }
                }

                xlApp.DisplayAlerts = false;
                xlWorkbook.SaveAs(filename, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                //if (printer)
                //xlWorkbook.PrintOut(1, 1, 1, false, true, false, true, filename);

                if (xlApp != null)
                {
                    xlWorkbook.Close(Type.Missing, filename, Type.Missing);
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlWorksheet);
                    Marshal.ReleaseComObject(xlWorkbook);
                    Marshal.ReleaseComObject(xlApp);
                    xlWorksheet = null;
                    xlWorkbook = null;
                    xlApp = null;
                }

                GC.Collect();

                MessageBox.Show("储存到" + filename, "资讯", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnableControls(bool enable)
        {
            readBtn.Text = (enable) ? "开始" : "結束";

            //DataGridView1.Enabled = enable;
            stationComboBox.Enabled = enable;
            modelComboBox.Enabled = enable;
            customerComboBox.Enabled = enable;
            VerticalRadioButton.Enabled = enable;
            HorizontalRadioButton.Enabled = enable;
            measureTextBox.Enabled = enable;
            lotnumberTextBox.Enabled = enable;
            dateTextBox.Enabled = enable;
            machineIdTextBox.Enabled = enable;
            saveBtn.Enabled = enable;
        }

        private void SendMail(string toMail, string filename)
        {
            string subject = "QM3000";
            string fromMail = "system@zktech.com.cn";

            MailMessage message = new MailMessage(fromMail, toMail);//MailMessage(寄信者, 收信者)
            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = subject;//E-mail主旨
            message.Body = "";//E-mail內容
            message.Priority = MailPriority.High;
            message.Attachments.Add(new Attachment(filename));

            SmtpClient smtp = new SmtpClient("mailgate.zktech.com.cn");
            //smtp.EnableSsl = true;

            try
            {
                smtp.Send(message);
                MessageBox.Show("邮件已发送", "资讯", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
	}    
}
