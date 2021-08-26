using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZWLineGauger.Gaugers;

namespace ZWLineGauger.Forms
{
    public partial class Form_ModifyTask : Form
    {
        MainUI m_parent;

        bool bInit = false;

        public static bool m_bChooseAdd = false;                                  // 记录用户是否点击了 “确定” 按钮

        static double   m_default_upper_delta_percent = 7;
        static double   m_default_lower_delta_percent = 7;
        static double   m_default_upper_delta2_percent = 7;
        static double   m_default_lower_delta2_percent = 7;
        
        public Form_ModifyTask(MainUI parent)
        {
            this.m_parent = parent;
            InitializeComponent();

            m_bChooseAdd = false;

            comboBox_AllowanceModifyMode.Items.Add("绝对值");
            comboBox_AllowanceModifyMode.Items.Add("百分比");
            comboBox_AllowanceModifyMode.SelectedIndex = (true == parent.m_bAbsoluteAllowance) ? 0 : 1;

            label_unit1.Text = parent.comboBox_Unit.Text;
            label_unit2.Text = parent.comboBox_Unit.Text;

            int nStartIdx = parent.m_vec_task_gridview_selected_data_indices[0];
            textBox_Name.Text = parent.m_current_task_data[nStartIdx].m_name;
            textBox_Name.Select(textBox_Name.TextLength, 0);                         // 光标定位到文本最后

            switch (parent.m_current_task_data[nStartIdx].m_mes_type)
            {
                case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                case MEASURE_TYPE.HAND_PICK_CIRCLE:
                    textBox_StandardValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_radius[0], parent.m_nUnitType));
                    textBox_UpperValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_radius_upper[0], parent.m_nUnitType));
                    textBox_LowerValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_radius_lower[0], parent.m_nUnitType));
                    break;
                    
                case MEASURE_TYPE.LINE_WIDTH_14:
                case MEASURE_TYPE.LINE_WIDTH_13:
                case MEASURE_TYPE.LINE_WIDTH_23:
                case MEASURE_TYPE.LINE_SPACE:
                case MEASURE_TYPE.ARC_LINE_SPACE:
                case MEASURE_TYPE.HAND_PICK_LINE:
                case MEASURE_TYPE.LINE:
                case MEASURE_TYPE.ARC_LINE_WIDTH:
                    textBox_StandardValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width[0], parent.m_nUnitType));
                    textBox_UpperValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width_upper[0], parent.m_nUnitType));
                    textBox_LowerValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width_lower[0], parent.m_nUnitType));
                    break;

                case MEASURE_TYPE.LINE_WIDTH_1234:
                    textBox_StandardValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width[0], parent.m_nUnitType));
                    textBox_UpperValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width_upper[0], parent.m_nUnitType));
                    textBox_LowerValue.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width_lower[0], parent.m_nUnitType));
                    textBox_StandardValue2.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width[1], parent.m_nUnitType));
                    textBox_UpperValue2.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width_upper[1], parent.m_nUnitType));
                    textBox_LowerValue2.Text = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(parent.m_current_task_data[nStartIdx].m_metric_line_width_lower[1], parent.m_nUnitType));
                    break;
            }

            // 隐藏
            if (MEASURE_TYPE.LINE_WIDTH_1234 != parent.m_current_task_data[nStartIdx].m_mes_type)
            {
                textBox_StandardValue2.Visible = false;
                textBox_UpperValue2.Visible = false;
                textBox_LowerValue2.Visible = false;
                textBox_UpperDelta2.Visible = false;
                textBox_LowerDelta2.Visible = false;
                label_UpperPercent2.Visible = false;
                label_LowerPercent2.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                label7.Visible = false;
                label_unit2.Visible = false;
            }

            bInit = true;
        }

        // 按钮：确定
        private void btn_OK_Click(object sender, EventArgs e)
        {
            string new_name = textBox_Name.Text;
            double new_standard_value = Convert.ToDouble(textBox_StandardValue.Text);
            double new_upper_value = Convert.ToDouble(textBox_UpperValue.Text);
            double new_lower_value = Convert.ToDouble(textBox_LowerValue.Text);
            double new_standard_value2 = ("" == textBox_StandardValue2.Text) ? 0 : Convert.ToDouble(textBox_StandardValue2.Text);
            double new_upper_value2 = ("" == textBox_UpperValue2.Text) ? 0 : Convert.ToDouble(textBox_UpperValue2.Text);
            double new_lower_value2 = ("" == textBox_LowerValue2.Text) ? 0 : Convert.ToDouble(textBox_LowerValue2.Text);

            switch (m_parent.m_nUnitType)
            {
                case 0:
                    new_standard_value = GeneralUtils.convert_mm_value_by_unit(new_standard_value, 1);
                    new_upper_value = GeneralUtils.convert_mm_value_by_unit(new_upper_value, 1);
                    new_lower_value = GeneralUtils.convert_mm_value_by_unit(new_lower_value, 1);
                    new_standard_value2 = GeneralUtils.convert_mm_value_by_unit(new_standard_value2, 1);
                    new_upper_value2 = GeneralUtils.convert_mm_value_by_unit(new_upper_value2, 1);
                    new_lower_value2 = GeneralUtils.convert_mm_value_by_unit(new_lower_value2, 1);
                    break;

                case 1:
                    break;

                case 2:
                    new_standard_value = GeneralUtils.convert_mil_value_by_unit(new_standard_value, 1);
                    new_upper_value = GeneralUtils.convert_mil_value_by_unit(new_upper_value, 1);
                    new_lower_value = GeneralUtils.convert_mil_value_by_unit(new_lower_value, 1);
                    new_standard_value2 = GeneralUtils.convert_mil_value_by_unit(new_standard_value2, 1);
                    new_upper_value2 = GeneralUtils.convert_mil_value_by_unit(new_upper_value2, 1);
                    new_lower_value2 = GeneralUtils.convert_mil_value_by_unit(new_lower_value2, 1);
                    break;
            }
            
            // 修改当前任务的数据
            for (int n = 0; n < m_parent.m_vec_task_gridview_selected_data_indices.Count; n++)
            {
                int idx = m_parent.m_vec_task_gridview_selected_data_indices[n];
                switch (m_parent.m_current_task_data[idx].m_mes_type)
                {
                    case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                    case MEASURE_TYPE.HAND_PICK_CIRCLE:
                        m_parent.m_current_task_data[idx].m_metric_radius[0] = new_standard_value;
                        m_parent.m_current_task_data[idx].m_metric_radius_upper[0] = new_upper_value;
                        m_parent.m_current_task_data[idx].m_metric_radius_lower[0] = new_lower_value;
                        break;
                    //GeneralUtils.convert_value_by_unit(, parent.m_nUnitType)
                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.HAND_PICK_LINE:
                    case MEASURE_TYPE.LINE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        m_parent.m_current_task_data[idx].m_metric_line_width[0] = new_standard_value;
                        m_parent.m_current_task_data[idx].m_metric_line_width_upper[0] = new_upper_value;
                        m_parent.m_current_task_data[idx].m_metric_line_width_lower[0] = new_lower_value;
                        break;

                    case MEASURE_TYPE.LINE_WIDTH_1234:
                        m_parent.m_current_task_data[idx].m_metric_line_width[0] = new_standard_value;
                        m_parent.m_current_task_data[idx].m_metric_line_width_upper[0] = new_upper_value;
                        m_parent.m_current_task_data[idx].m_metric_line_width_lower[0] = new_lower_value;
                        m_parent.m_current_task_data[idx].m_metric_line_width[1] = new_standard_value2;
                        m_parent.m_current_task_data[idx].m_metric_line_width_upper[1] = new_upper_value2;
                        m_parent.m_current_task_data[idx].m_metric_line_width_lower[1] = new_lower_value2;
                        break;
                }

                m_parent.m_current_task_data[idx].m_name = new_name;

                m_bChooseAdd = true;

                if ((true == m_parent.m_bIsMeasuringInArrayMode) && (Math.Abs(m_parent.m_current_task_data[idx].m_center_x_in_metric) > 0.00001))
                {
                    if (DialogResult.Yes == MessageBox.Show(this, "当前处于阵列测量模式，是否将本次设置的标准值和名称自动应用于阵列其它units的同一个测量点?", "提示", MessageBoxButtons.YesNo))
                    {
                        m_parent.m_bApplySameParamsToOtherUnits = true;
                        m_parent.m_base_data_for_other_units = m_parent.m_current_task_data[idx];
                    }
                }
            }

            // 由于show_task_on_gridview()函数会清空原表格内容，故需要保存原表格选中行号，待表格刷新内容后自动选中对应的行
            List<int> vec_selected_rows_indices = new List<int>();
            for (int n = 0; n < m_parent.gridview_MeasureTask.SelectedRows.Count; n++)
                vec_selected_rows_indices.Add(m_parent.gridview_MeasureTask.SelectedRows[n].Index);

            // 刷新表格内容
            //parent.show_task_on_gridview(parent.gridview_MeasureTask, parent.m_current_task_data);
            for (int n = 0; n < m_parent.m_vec_task_gridview_selected_data_indices.Count; n++)
            {
                int idx = m_parent.m_vec_task_gridview_selected_data_indices[n];
                m_parent.update_row_on_gridview(m_parent.gridview_MeasureTask, m_parent.m_current_task_data, idx);
            }

            // 重新选中原来那些行
            m_parent.gridview_MeasureTask.Rows[0].Selected = false;
            for (int n = 0; n < vec_selected_rows_indices.Count; n++)
                m_parent.gridview_MeasureTask.Rows[vec_selected_rows_indices[n]].Selected = true;

            this.Close();
        }

        // 按钮：取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 组合框：上下限修改方式
        private void comboBox_AllowanceModifyMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (0 == comboBox_AllowanceModifyMode.SelectedIndex)
            {
                m_parent.m_bAbsoluteAllowance = true;
                label_UpperPercent.Text = "";
                label_LowerPercent.Text = "";
                label_UpperPercent2.Text = "";
                label_LowerPercent2.Text = "";

                textBox_UpperDelta.Text = "";
                textBox_LowerDelta.Text = "";
                textBox_UpperDelta2.Text = "";
                textBox_LowerDelta2.Text = "";
            }
            else
            {
                m_parent.m_bAbsoluteAllowance = false;
                label_UpperPercent.Text = "%";
                label_LowerPercent.Text = "%";
                label_UpperPercent2.Text = "%";
                label_LowerPercent2.Text = "%";

                textBox_UpperDelta.Text = Convert.ToString(m_default_upper_delta_percent);
                textBox_LowerDelta.Text = Convert.ToString(m_default_lower_delta_percent);
                textBox_UpperDelta2.Text = Convert.ToString(m_default_upper_delta2_percent);
                textBox_LowerDelta2.Text = Convert.ToString(m_default_lower_delta2_percent);
            }
        }

        // 文本框：上限值变更
        private void textBox_UpperDelta_TextChanged(object sender, EventArgs e)
        {
            if ((textBox_UpperDelta.Text.Length > 0) && (textBox_StandardValue.Text.Length > 0))
            {
                m_default_upper_delta_percent = Convert.ToDouble(textBox_UpperDelta.Text);
                double value = m_default_upper_delta_percent;
                if (true == m_parent.m_bAbsoluteAllowance)
                    textBox_UpperValue.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue.Text) + value);
                else
                    textBox_UpperValue.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue.Text) * (100 + value) / 100);
            }
        }

        // 文本框：下限值变更
        private void textBox_LowerDelta_TextChanged(object sender, EventArgs e)
        {
            if ((textBox_LowerDelta.Text.Length > 0) && (textBox_StandardValue.Text.Length > 0))
            {
                m_default_lower_delta_percent = Convert.ToDouble(textBox_LowerDelta.Text);
                double value = m_default_lower_delta_percent;
                if (true == m_parent.m_bAbsoluteAllowance)
                    textBox_LowerValue.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue.Text) - value);
                else
                    textBox_LowerValue.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue.Text) * (100 - value) / 100);
            }
        }

        private void textBox_UpperDelta2_TextChanged(object sender, EventArgs e)
        {
            if ((textBox_UpperDelta2.Text.Length > 0) && (textBox_StandardValue2.Text.Length > 0))
            {
                m_default_upper_delta2_percent = Convert.ToDouble(textBox_UpperDelta2.Text);
                double value = m_default_upper_delta2_percent;
                if (true == m_parent.m_bAbsoluteAllowance)
                    textBox_UpperValue2.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue2.Text) + value);
                else
                    textBox_UpperValue2.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue2.Text) * (100 + value) / 100);
            }
        }

        private void textBox_LowerDelta2_TextChanged(object sender, EventArgs e)
        {
            if ((textBox_LowerDelta2.Text.Length > 0) && (textBox_StandardValue2.Text.Length > 0))
            {
                m_default_lower_delta2_percent = Convert.ToDouble(textBox_LowerDelta2.Text);
                double value = m_default_lower_delta2_percent;
                if (true == m_parent.m_bAbsoluteAllowance)
                    textBox_LowerValue2.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue2.Text) - value);
                else
                    textBox_LowerValue2.Text = Convert.ToString(Convert.ToDouble(textBox_StandardValue2.Text) * (100 - value) / 100);
            }
        }

        // 文本框：标准值变更
        private void textBox_StandardValue_TextChanged(object sender, EventArgs e)
        {
            if (true == bInit)
            {
                textBox_UpperDelta_TextChanged(new object(), new EventArgs());
                textBox_LowerDelta_TextChanged(new object(), new EventArgs());
            }
        }

        private void textBox_StandardValue2_TextChanged(object sender, EventArgs e)
        {
            if (true == bInit)
            {
                textBox_UpperDelta2_TextChanged(new object(), new EventArgs());
                textBox_LowerDelta2_TextChanged(new object(), new EventArgs());
            }
        }

        private void Form_ModifyTask_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
    }
}
