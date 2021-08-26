using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ZWLineGauger.Gaugers;

namespace ZWLineGauger.Forms
{
    public partial class Form_Calibration : Form
    {
        MainUI parent;

        public double m_calib_result = 1;
        public double m_physical_length = 100;

        int   m_prev_ratio = 0;

        public Form_Calibration(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();
            
            this.StartPosition = FormStartPosition.Manual;
            this.Location = (Point)new Size(parent.ui_MainImage.Location.X + parent.ui_MainImage.Width + 30,
                parent.ui_MainImage.Location.Y + 80);
            
            comboBox_LenRatio.Enabled = parent.comboBox_Len.Enabled;

            string[] ratios = new string[] { "50X", "100X", "200X", "500X", "1000X", "1500X" };
            if ((true == comboBox_LenRatio.Enabled) || (true == parent.m_bOfflineMode))
            {
                for (int n = 0; n < ratios.Length; n++)
                    comboBox_LenRatio.Items.Add(ratios[n]);
                comboBox_LenRatio.SelectedIndex = parent.comboBox_Len.SelectedIndex;
            }

            textBox_PhysicalLength.Text = Convert.ToString(m_physical_length);
            textBox_CalibResult.Text = Convert.ToString(m_calib_result);

            gridview_MeasureResults.RowHeadersVisible = false;
            gridview_MeasureResults.ReadOnly = true;
            gridview_MeasureResults.ColumnCount = 3;
            gridview_MeasureResults.ColumnHeadersVisible = true;
            gridview_MeasureResults.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.Columns[0].Width = 60;
            gridview_MeasureResults.Columns[1].Width = 105;
            gridview_MeasureResults.Columns[2].Width = 105;
            gridview_MeasureResults.Columns[0].Name = "序号";
            gridview_MeasureResults.Columns[1].Name = "像素长度(px)";
            gridview_MeasureResults.Columns[2].Name = "物理长度(um)";

            gridview_RatiosAndResults.RowHeadersVisible = false;
            gridview_RatiosAndResults.ReadOnly = true;
            gridview_RatiosAndResults.ColumnCount = 2;
            gridview_RatiosAndResults.ColumnHeadersVisible = true;
            gridview_RatiosAndResults.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_RatiosAndResults.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_RatiosAndResults.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_RatiosAndResults.Columns[0].Width = 66;
            gridview_RatiosAndResults.Columns[1].Width = 200;
            gridview_RatiosAndResults.Columns[0].Name = "倍率";
            gridview_RatiosAndResults.Columns[1].Name = "标定结果(像素/微米)";
            for (int n = 0; n < 6; n++)
            {
                String str = string.Format("{0:0.000}  (或 {1:0.000} um/pixel)", parent.m_calib_data[n], 1 / parent.m_calib_data[n]);
                String[] row = new String[2] { ratios[n], str };

                gridview_RatiosAndResults.Rows.Add(row);
               
            }
            try
            {
                string value = gridview_RatiosAndResults[1, comboBox_LenRatio.SelectedIndex].Value.ToString().Substring(0, 5);
                nudmmpixcel.Value = (decimal)Convert.ToDouble(value);
                this.gridview_RatiosAndResults.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                this.gridview_RatiosAndResults.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            catch { }
          
            this.ui_btn_13LineWidth.Image = parent.m_images_for_line_width_13[0];
        }
        
        // 按钮：保存
        private void btn_Save_Click(object sender, EventArgs e)
        {
            if ((true == comboBox_LenRatio.Enabled) || (true == parent.m_bOfflineMode))
            {
                for (int n = 0; n < 6; n++)
                {
                    string value = gridview_RatiosAndResults[1, n].Value.ToString().Substring(0, 5);
                    parent.m_calib_data[n] = Convert.ToDouble(value);
                }

                try
                {
                    string value = gridview_RatiosAndResults[1, comboBox_LenRatio.SelectedIndex].Value.ToString().Substring(0, 5);
                    nudmmpixcel.Value = (decimal)Convert.ToDouble(value);
                }
                catch { }
                //parent.SaveCalibData(parent.m_strCalibDataPath, parent.m_calib_data);

                MessageBox.Show(this, "保存成功", "提示", MessageBoxButtons.OK);
            }
            
            Close();
        }

        // 按钮：取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        // 添加标定结果
        public void add_calib_result(double line_width)
        {
            if (((true == comboBox_LenRatio.Enabled) && (gridview_MeasureResults.RowCount <= 10)) || (true == parent.m_bOfflineMode))
            {
                String str1 = string.Format("{0}", gridview_MeasureResults.RowCount);
                String str2 = string.Format("{0:0.000}", line_width);
                String str3 = string.Format("{0:0.000}", textBox_PhysicalLength.Text);
                String[] row = new String[3] { str1, str2, str3 };

                gridview_MeasureResults.Rows.Add(row);
                
                double dbCalibResult = 0;
                for (int n = 1; n < gridview_MeasureResults.RowCount; n++)
                {
                    double len1 = Convert.ToDouble(gridview_MeasureResults[1, n - 1].Value);
                    double len2 = Convert.ToDouble(textBox_PhysicalLength.Text);
                    dbCalibResult += (len1 / len2);
                }
                m_calib_result = dbCalibResult / (gridview_MeasureResults.RowCount - 1);
                
                textBox_CalibResult.Text = string.Format("{0:0.000}", m_calib_result);

                gridview_RatiosAndResults[1, comboBox_LenRatio.SelectedIndex].Value = string.Format("{0:0.000}  (或 {1:0.000} um/pixel)", m_calib_result, 1 / m_calib_result);
            }
        }
        
        // 按钮：清空
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            gridview_MeasureResults.Rows.Clear();
        }

        // 倍率组合框选项变更事件
        private void comboBox_LenRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            parent.comboBox_Len.SelectedIndex = comboBox_LenRatio.SelectedIndex;

            if (comboBox_LenRatio.SelectedIndex != m_prev_ratio)
                gridview_MeasureResults.Rows.Clear();

            m_prev_ratio = comboBox_LenRatio.SelectedIndex;

            try
            {
                string value = gridview_RatiosAndResults[1, comboBox_LenRatio.SelectedIndex].Value.ToString().Substring(0, 5);
                nudmmpixcel.Value = (decimal)Convert.ToDouble(value);
            }
            catch { }
        }

        private void Form_Calibration_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }

        private void ui_btn_13LineWidth_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_13 != parent.m_current_measure_type)
            {
                parent.set_gauger(MEASURE_TYPE.LINE_WIDTH_13);
            }
            this.ui_btn_13LineWidth.Image = parent.m_images_for_line_width_13[2];
        }

        private void ui_btn_13LineWidth_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_13 != parent.m_current_measure_type)
                this.ui_btn_13LineWidth.Image = parent.m_images_for_line_width_13[1];

            parent.m_tooltip_for_measure_item.ShowAlways = true;
            parent.m_tooltip_for_measure_item.SetToolTip(this.ui_btn_13LineWidth, "13线宽");
        }

        private void ui_btn_13LineWidth_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_13 != parent.m_current_measure_type)
                this.ui_btn_13LineWidth.Image = parent.m_images_for_line_width_13[0];
        }

        private void btnUpdata_Click(object sender, EventArgs e)
        {
            if (DialogResult.No == MessageBox.Show("是否手动更新标定结果", "提示", MessageBoxButtons.YesNo)) return;
            try
            {
                gridview_RatiosAndResults[1, comboBox_LenRatio.SelectedIndex].Value = string.Format("{0:0.000}  (或 {1:0.000} um/pixel)", nudmmpixcel.Value, 1 / nudmmpixcel.Value);
                MessageBox.Show("更新成功");
            }
            catch { }
        }
    }
}
