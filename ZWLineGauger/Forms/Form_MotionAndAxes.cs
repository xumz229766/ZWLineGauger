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

namespace ZWLineGauger.Forms
{
    public partial class Form_MotionAndAxes : Form
    {
        MainUI form_parent;

        public Form_MotionAndAxes(MainUI parent)
        {
            this.form_parent = parent;
            InitializeComponent();

            comboBox_DIR_X.Items.Add("正");
            comboBox_DIR_X.Items.Add("负");
            comboBox_DIR_Y.Items.Add("正");
            comboBox_DIR_Y.Items.Add("负");
            comboBox_DIR_Z.Items.Add("正");
            comboBox_DIR_Z.Items.Add("负");

            int X = MotionOps.AXIS_X - 1;
            int Y = MotionOps.AXIS_Y - 1;
            int Z = MotionOps.AXIS_Z - 1;

            textBox_MotionUnitScale.Text = Convert.ToString(MainUI.m_motion.m_axes[X].motion_unit_scale);
            textBox_ShortRangeAcc.Text = Convert.ToString(MainUI.m_motion.m_axes[X].acc_for_short_range);
            textBox_ShortRangeDec.Text = Convert.ToString(MainUI.m_motion.m_axes[X].dec_for_short_range);
            textBox_ShortRangeVel.Text = Convert.ToString(MainUI.m_motion.m_axes[X].vel_for_short_range);
            textBox_LongRangeAcc.Text = Convert.ToString(MainUI.m_motion.m_axes[X].acc_for_long_range);
            textBox_LongRangeDec.Text = Convert.ToString(MainUI.m_motion.m_axes[X].dec_for_long_range);
            textBox_LongRangeVel.Text = Convert.ToString(MainUI.m_motion.m_axes[X].vel_for_long_range);
            textBox_NegativeLimit.Text = Convert.ToString(MainUI.m_motion.m_axes[X].negative_limit);
            textBox_PositiveLimit.Text = Convert.ToString(MainUI.m_motion.m_axes[X].positive_limit);
            textBox_HomeSpeed.Text = Convert.ToString(MainUI.m_motion.m_axes[X].home_speed);
            textBox_HomeSpeedSecond.Text = Convert.ToString(MainUI.m_motion.m_axes[X].home_speed_second);
            if (1 == MainUI.m_motion.m_axes[X].nDir)
                comboBox_DIR_X.SelectedIndex = 0;
            else
                comboBox_DIR_X.SelectedIndex = 1;

            textBox_MotionUnitScaleY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].motion_unit_scale);
            textBox_ShortRangeAccY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].acc_for_short_range);
            textBox_ShortRangeDecY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].dec_for_short_range);
            textBox_ShortRangeVelY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].vel_for_short_range);
            textBox_LongRangeAccY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].acc_for_long_range);
            textBox_LongRangeDecY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].dec_for_long_range);
            textBox_LongRangeVelY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].vel_for_long_range);
            textBox_NegativeLimitY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].negative_limit);
            textBox_PositiveLimitY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].positive_limit);
            textBox_HomeSpeedY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].home_speed);
            textBox_HomeSpeedSecondY.Text = Convert.ToString(MainUI.m_motion.m_axes[Y].home_speed_second);
            if (1 == MainUI.m_motion.m_axes[Y].nDir)
                comboBox_DIR_Y.SelectedIndex = 0;
            else
                comboBox_DIR_Y.SelectedIndex = 1;

            textBox_MotionUnitScaleZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].motion_unit_scale);
            textBox_ShortRangeAccZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].acc_for_short_range);
            textBox_ShortRangeDecZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].dec_for_short_range);
            textBox_ShortRangeVelZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].vel_for_short_range);
            textBox_LongRangeAccZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].acc_for_long_range);
            textBox_LongRangeDecZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].dec_for_long_range);
            textBox_LongRangeVelZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].vel_for_long_range);
            textBox_NegativeLimitZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].negative_limit);
            textBox_PositiveLimitZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].positive_limit);
            textBox_HomeSpeedZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].home_speed);
            textBox_HomeSpeedSecondZ.Text = Convert.ToString(MainUI.m_motion.m_axes[Z].home_speed_second);
            if (1 == MainUI.m_motion.m_axes[Z].nDir)
                comboBox_DIR_Z.SelectedIndex = 0;
            else
                comboBox_DIR_Z.SelectedIndex = 1;

            textBox_PadLeftBottomCrdX.Text = Convert.ToString(MainUI.m_motion.m_pad_leftbottom_crd.x);
            textBox_PadLeftBottomCrdY.Text = Convert.ToString(MainUI.m_motion.m_pad_leftbottom_crd.y);
            textBox_PadRightTopCrdX.Text = Convert.ToString(MainUI.m_motion.m_pad_righttop_crd.x);
            textBox_PadRightTopCrdY.Text = Convert.ToString(MainUI.m_motion.m_pad_righttop_crd.y);

            textBox_PCBLeftBottomCrdX.Text = Convert.ToString(MainUI.m_motion.m_PCB_leftbottom_crd.x);
            textBox_PCBLeftBottomCrdY.Text = Convert.ToString(MainUI.m_motion.m_PCB_leftbottom_crd.y);
            textBox_PCBLeftBottomCrdZ.Text = Convert.ToString(MainUI.m_motion.m_PCB_leftbottom_crd.z);

            textBox_AutoFocus_Upper.Text = Convert.ToString(MainUI.m_motion.m_autofocus_upper_pos);
            textBox_AutoFocus_Lower.Text = Convert.ToString(MainUI.m_motion.m_autofocus_lower_pos);
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            Axis axisX = new Axis();
            if (0 == comboBox_DIR_X.SelectedIndex)
                axisX.nDir = 1;
            else
                axisX.nDir = -1;
            axisX.motion_unit_scale = Convert.ToDouble(textBox_MotionUnitScale.Text);
            axisX.acc_for_short_range = Convert.ToDouble(textBox_ShortRangeAcc.Text);
            axisX.dec_for_short_range = Convert.ToDouble(textBox_ShortRangeDec.Text);
            axisX.vel_for_short_range = Convert.ToDouble(textBox_ShortRangeVel.Text);
            axisX.acc_for_long_range = Convert.ToDouble(textBox_LongRangeAcc.Text);
            axisX.dec_for_long_range = Convert.ToDouble(textBox_LongRangeDec.Text);
            axisX.vel_for_long_range = Convert.ToDouble(textBox_LongRangeVel.Text);
            axisX.negative_limit = Convert.ToDouble(textBox_NegativeLimit.Text);
            axisX.positive_limit = Convert.ToDouble(textBox_PositiveLimit.Text);
            axisX.home_speed = Convert.ToDouble(textBox_HomeSpeed.Text);
            axisX.home_speed_second = Convert.ToDouble(textBox_HomeSpeedSecond.Text);
            if (false == MainUI.m_motion.check_axis_validness(axisX))
            {
                MessageBox.Show(this, "X轴参数有误，请检查修改再尝试保存。", "提示", MessageBoxButtons.OK);
                return;
            }

            Axis axisY = new Axis();
            if (0 == comboBox_DIR_Y.SelectedIndex)
                axisY.nDir = 1;
            else
                axisY.nDir = -1;
            axisY.motion_unit_scale = Convert.ToDouble(textBox_MotionUnitScaleY.Text);
            axisY.acc_for_short_range = Convert.ToDouble(textBox_ShortRangeAccY.Text);
            axisY.dec_for_short_range = Convert.ToDouble(textBox_ShortRangeDecY.Text);
            axisY.vel_for_short_range = Convert.ToDouble(textBox_ShortRangeVelY.Text);
            axisY.acc_for_long_range = Convert.ToDouble(textBox_LongRangeAccY.Text);
            axisY.dec_for_long_range = Convert.ToDouble(textBox_LongRangeDecY.Text);
            axisY.vel_for_long_range = Convert.ToDouble(textBox_LongRangeVelY.Text);
            axisY.negative_limit = Convert.ToDouble(textBox_NegativeLimitY.Text);
            axisY.positive_limit = Convert.ToDouble(textBox_PositiveLimitY.Text);
            axisY.home_speed = Convert.ToDouble(textBox_HomeSpeedY.Text);
            axisY.home_speed_second = Convert.ToDouble(textBox_HomeSpeedSecondY.Text);
            if (false == MainUI.m_motion.check_axis_validness(axisY))
            {
                MessageBox.Show(this, "Y轴参数有误，请检查修改再尝试保存。", "提示", MessageBoxButtons.OK);
                return;
            }

            Axis axisZ = new Axis();
            if (0 == comboBox_DIR_Z.SelectedIndex)
                axisZ.nDir = 1;
            else
                axisZ.nDir = -1;
            axisZ.motion_unit_scale = Convert.ToDouble(textBox_MotionUnitScaleZ.Text);
            axisZ.acc_for_short_range = Convert.ToDouble(textBox_ShortRangeAccZ.Text);
            axisZ.dec_for_short_range = Convert.ToDouble(textBox_ShortRangeDecZ.Text);
            axisZ.vel_for_short_range = Convert.ToDouble(textBox_ShortRangeVelZ.Text);
            axisZ.acc_for_long_range = Convert.ToDouble(textBox_LongRangeAccZ.Text);
            axisZ.dec_for_long_range = Convert.ToDouble(textBox_LongRangeDecZ.Text);
            axisZ.vel_for_long_range = Convert.ToDouble(textBox_LongRangeVelZ.Text);
            axisZ.negative_limit = Convert.ToDouble(textBox_NegativeLimitZ.Text);
            axisZ.positive_limit = Convert.ToDouble(textBox_PositiveLimitZ.Text);
            axisZ.home_speed = Convert.ToDouble(textBox_HomeSpeedZ.Text);
            axisZ.home_speed_second = Convert.ToDouble(textBox_HomeSpeedSecondZ.Text);
            if (false == MainUI.m_motion.check_axis_validness(axisZ))
            {
                MessageBox.Show(this, "Z轴参数有误，请检查修改再尝试保存。", "提示", MessageBoxButtons.OK);
                return;
            }
            
            MainUI.m_motion.m_axes[MotionOps.AXIS_X - 1] = axisX;
            MainUI.m_motion.m_axes[MotionOps.AXIS_Y - 1] = axisY;
            MainUI.m_motion.m_axes[MotionOps.AXIS_Z - 1] = axisZ;

            MainUI.m_motion.m_pad_leftbottom_crd.x = Convert.ToDouble(textBox_PadLeftBottomCrdX.Text);
            MainUI.m_motion.m_pad_leftbottom_crd.y = Convert.ToDouble(textBox_PadLeftBottomCrdY.Text);
            MainUI.m_motion.m_pad_righttop_crd.x = Convert.ToDouble(textBox_PadRightTopCrdX.Text);
            MainUI.m_motion.m_pad_righttop_crd.y = Convert.ToDouble(textBox_PadRightTopCrdY.Text);
            MainUI.m_motion.m_PCB_leftbottom_crd.x = Convert.ToDouble(textBox_PCBLeftBottomCrdX.Text);
            MainUI.m_motion.m_PCB_leftbottom_crd.y = Convert.ToDouble(textBox_PCBLeftBottomCrdY.Text);
            MainUI.m_motion.m_PCB_leftbottom_crd.z = Convert.ToDouble(textBox_PCBLeftBottomCrdZ.Text);

            MainUI.m_motion.m_autofocus_upper_pos = Convert.ToDouble(textBox_AutoFocus_Upper.Text);
            MainUI.m_motion.m_autofocus_lower_pos = Convert.ToDouble(textBox_AutoFocus_Lower.Text);

            if (true == MainUI.m_motion.save_params())
            {
                MessageBox.Show(this, "保存成功", "提示", MessageBoxButtons.OK);
                Close();
            }
            else
            {
                MessageBox.Show(this, "运动控制参数有误，请检查修改再保存。", "提示", MessageBoxButtons.OK);
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        // 获取小地图左下角坐标
        private void btn_GetPadLeftBottomCrd_Click(object sender, EventArgs e)
        {
            Point3d crd = new Point3d(0, 0, 0);
            if (true == MainUI.m_motion.get_xyz_crds(ref crd))
            {
                textBox_PadLeftBottomCrdX.Text = Convert.ToString(crd.x);
                textBox_PadLeftBottomCrdY.Text = Convert.ToString(crd.y);
            }
        }

        // 获取小地图右上角坐标
        private void btn_GetPadRightTopCrd_Click(object sender, EventArgs e)
        {
            Point3d crd = new Point3d(0, 0, 0);
            if (true == MainUI.m_motion.get_xyz_crds(ref crd))
            {
                textBox_PadRightTopCrdX.Text = Convert.ToString(crd.x);
                textBox_PadRightTopCrdY.Text = Convert.ToString(crd.y);
            }
        }

        // 获取PCB左下角快捷到达坐标
        private void btn_GetPCBLeftBottomCrd_Click(object sender, EventArgs e)
        {
            Point3d crd = new Point3d(0, 0, 0);
            if (true == MainUI.m_motion.get_xyz_crds(ref crd))
            {
                textBox_PCBLeftBottomCrdX.Text = Convert.ToString(crd.x);
                textBox_PCBLeftBottomCrdY.Text = Convert.ToString(crd.y);
                textBox_PCBLeftBottomCrdZ.Text = Convert.ToString(crd.z);
                //string msg2 = string.Format("222222 crd [{0:0.000},{1:0.000},{2:0.000}]", crd.x, crd.y, crd.z);
                //Debugger.Log(0, null, msg2);
            }
        }

        private void Form_MotionAndAxes_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
    }
}
