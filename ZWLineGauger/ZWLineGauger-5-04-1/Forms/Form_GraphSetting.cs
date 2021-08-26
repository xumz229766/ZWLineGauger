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

namespace ZWLineGauger
{
    public partial class Form_GraphSetting : Form
    {
        MainUI form_parent;

        public Form_GraphSetting(MainUI parent)
        {
            this.form_parent = parent;
            InitializeComponent();

            combo_GraphZoomRatio.Items.Add("10000");
            combo_GraphZoomRatio.Items.Add("20000");
            combo_GraphZoomRatio.Items.Add("40000");
            combo_GraphZoomRatio.Items.Add("80000");
            combo_GraphZoomRatio.Items.Add("180000");
            
            bool bHasMatch = false;
            for (int n = 0; n < combo_GraphZoomRatio.Items.Count; n++)
            {
                //string msg = string.Format("222222 {0}: {1}, {2}", n+1, combo_GraphZoomRatio.Items[n].ToString(), form_parent.m_nGraphZoomRatio.ToString());
                //Debugger.Log(0, null, msg);
                if (combo_GraphZoomRatio.Items[n].ToString() == MainUI.m_nGraphZoomRatio.ToString())
                {
                    bHasMatch = true;
                    combo_GraphZoomRatio.SelectedIndex = n;
                    break;
                }
            }
            if (false == bHasMatch)
                combo_GraphZoomRatio.SelectedIndex = 0;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            form_parent.SaveAppParams();
            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void combo_GraphZoomRatio_SelectChange(object sender, EventArgs e)
        {
            MainUI.m_nGraphZoomRatio = Convert.ToInt32(combo_GraphZoomRatio.Items[combo_GraphZoomRatio.SelectedIndex].ToString());
        }

        private void Form_GraphSetting_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
    }
}
