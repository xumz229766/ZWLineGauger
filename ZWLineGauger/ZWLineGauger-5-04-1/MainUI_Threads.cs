using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Ports;
using System.Data.SqlClient;
using PMSGigE = PMSAPI.PMSGigE;
using PMSSTATUS = PMSAPI.PMSSTATUS_CODES;
using PMSImage = PMSAPI.PMSImage;
using ZWLineGauger.Gaugers;
using ZWLineGauger.Hardwares;
using HalconDotNet;
using System.Collections;

namespace ZWLineGauger
{
    public partial class MainUI : Form
    {
        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_render_gerber_big_image(char[] path, int nOrientation, int nZoomRatio, int[] out_ints, double[] out_doubles);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool copy_graph_view(byte[] buf, int nBytesPerLine, int nWidth, int nHeight);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_render_ODB_big_image(char[] path, int nOrientation, int nZoomRatio, int[] pRetInts, double[] pRetDoubles);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_copy_ODB_big_image(byte[] buf, int nBytesPerLine, int nWidth, int nHeight);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_ODB_measure_items(int[] pRetInts, double[] pRetDoubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool get_transform_matrix(int[] out_ints, double[] in_theory_crds, double[] in_real_crds, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool find_shape_model_test();

        public bool m_bNeedToInitMotionSystem = true;

        bool m_bThreadIsRunning_MeasureOnceMore = false;

        static public int   m_nGraphType = 0;                      // 0为无图纸，1为gerber，2为ODB++
        static public int   m_nBytesPerLine = 0;
        static public int   m_nGraphOffsetX = 0;
        static public int   m_nGraphOffsetY = 0;
        static public int   m_nGraphWidth = 0;
        static public int   m_nGraphHeight = 0;
        static public double   m_pixels_per_mm;

        public Point3d   m_current_xyz = new Point3d(0, 0, 0);

        // 线程：图纸模式创建首件时，寻找和测量定位孔
        public void thread_locate_and_measure_mark_pt(object obj)
        {
            dl_message_sender send_message = CBD_SendMessage;
            send_message("刷新图纸测量项列表", false, true, null);
            
            MeasurePointData data = (MeasurePointData)obj;

            data.m_strStepsFileName = m_strCurrentProductStep;
            data.m_strLayerFileName = m_strCurrentProductLayer;

            Debugger.Log(0, null, string.Format("222222 m_pcb_alignment_pt_on_graph [{0:0.000},{1:0.000}], data [{2:0.000},{3:0.000}]",
                m_pcb_alignment_pt_on_graph.x, m_pcb_alignment_pt_on_graph.y, data.m_center_x_in_metric, data.m_center_y_in_metric));

            // 设置倍率
            if (false == m_bOfflineMode)
                send_message("设置倍率", false, data.m_len_ratio, null);
            
            // 设置测量类型
            send_message("设置测量类型", false, data.m_mes_type, null);

            m_bShowSmallSelectionFrame = false;
            m_bShowFrameDuringTaskCreation = false;
            m_bShowCoarseMark = false;
            m_bShowAccurateMark = false;

            // 设置光源和亮度
            if (false == m_bOfflineMode)
            {
                data.m_bIsTopLightOn = (0 == m_nLightTypeForGuideCamForMarkPt) ? true : false;
                data.m_bIsBottomLightOn = (1 == m_nLightTypeForGuideCamForMarkPt) ? true : false;
                data.m_nTopBrightness = m_top_light.m_nBrightness;
                data.m_nBottomBrightness = m_bottom_light.m_nBrightness;

                data.m_bIsTopLightOn = true;
                data.m_bIsBottomLightOn = false;
                send_message("设置光源和亮度", false, data, null);
            }
            
            // 先移动到减去对齐偏移后的位置
            if (false == m_bOfflineMode)
            {
                Point3d target_crd = new Point3d();
                Point3d current_crd = new Point3d();
                m_motion.get_xyz_crds(ref current_crd);

                if (0 == get_fiducial_mark_count(m_current_task_data))
                {
                    Debugger.Log(0, null, string.Format("222222 第1个定位孔"));

                    if (2 == m_nCreateTaskMode)
                        target_crd = current_crd;
                    else
                    {
                        target_crd.x = m_pcb_alignment_pt_on_machine.x + (data.m_center_x_in_metric - m_pcb_alignment_pt_on_graph.x);
                        target_crd.y = m_pcb_alignment_pt_on_machine.y + (data.m_center_y_in_metric - m_pcb_alignment_pt_on_graph.y);
                        target_crd.z = current_crd.z;
                    }
                }
                else if (1 == get_fiducial_mark_count(m_current_task_data))
                {
                    Debugger.Log(0, null, string.Format("222222 第2个定位孔"));

                    Debugger.Log(0, null, string.Format("222222 m_current_task_data[0] [{0:0.000},{1:0.000}], [{2:0.000},{3:0.000}]", m_current_task_data[0].m_theory_machine_crd.x,
                        m_current_task_data[0].m_theory_machine_crd.y, m_current_task_data[0].m_center_x_in_metric, m_current_task_data[0].m_center_y_in_metric));

                    if (2 == m_nCreateTaskMode)
                    {
                        Point2d offset = new Point2d(0, 0);
                        offset.x = m_current_task_data[0].m_theory_machine_crd.x - m_current_task_data[0].m_center_x_in_metric;
                        offset.y = m_current_task_data[0].m_theory_machine_crd.y - m_current_task_data[0].m_center_y_in_metric;

                        target_crd.x = data.m_center_x_in_metric + offset.x;
                        target_crd.y = data.m_center_y_in_metric + offset.y;
                        target_crd.z = current_crd.z;
                    }
                    else
                    {
                        Point2d offset = new Point2d(0, 0);
                        Point2d first_mark_target_crd = new Point2d();
                        first_mark_target_crd.x = m_pcb_alignment_pt_on_machine.x + (m_current_task_data[0].m_center_x_in_metric - m_pcb_alignment_pt_on_graph.x);
                        first_mark_target_crd.y = m_pcb_alignment_pt_on_machine.y + (m_current_task_data[0].m_center_y_in_metric - m_pcb_alignment_pt_on_graph.y);
                        offset.x = m_current_task_data[0].m_theory_machine_crd.x - first_mark_target_crd.x;
                        offset.y = m_current_task_data[0].m_theory_machine_crd.y - first_mark_target_crd.y;

                        //Debugger.Log(0, null, string.Format("222222 m_current_task_data[0] [{0:0.000},{1:0.000}], [{2:0.000},{3:0.000}]", m_current_task_data[0].m_theory_machine_crd.x, 
                        //    m_current_task_data[0].m_theory_machine_crd.y, first_mark_target_crd.x, first_mark_target_crd.y));

                        target_crd.x = m_pcb_alignment_pt_on_machine.x + (data.m_center_x_in_metric - m_pcb_alignment_pt_on_graph.x);
                        target_crd.y = m_pcb_alignment_pt_on_machine.y + (data.m_center_y_in_metric - m_pcb_alignment_pt_on_graph.y);
                        target_crd.z = current_crd.z;

                        target_crd.x += offset.x;
                        target_crd.y += offset.y;
                    }
                }
                else if (2 == get_fiducial_mark_count(m_current_task_data))
                {
                    //Debugger.Log(0, null, string.Format("222222 第3个定位孔"));
                    double[] out_doubles = new double[10];
                    double theory_angle = 0;
                    double real_angle = 0;

                    get_theta(m_current_task_data[0].m_center_x_in_metric, m_current_task_data[0].m_center_y_in_metric,
                        m_current_task_data[1].m_center_x_in_metric, m_current_task_data[1].m_center_y_in_metric, out_doubles);
                    theory_angle = out_doubles[0];

                    get_theta(m_current_task_data[0].m_real_machine_crd.x, m_current_task_data[0].m_real_machine_crd.y,
                        m_current_task_data[1].m_real_machine_crd.x, m_current_task_data[1].m_real_machine_crd.y, out_doubles);
                    real_angle = out_doubles[0];

                    double[] in_crds = new double[2];
                    double[] out_crds = new double[2];
                    in_crds[0] = data.m_center_x_in_metric - m_current_task_data[0].m_center_x_in_metric;
                    in_crds[1] = data.m_center_y_in_metric - m_current_task_data[0].m_center_y_in_metric;
                    rotate_crd(in_crds, out_crds, real_angle - theory_angle);

                    
                    Debugger.Log(0, null, string.Format("222222 real_angle {0:0.000}, theory_angle {1:0.000}", real_angle, theory_angle));
                    Debugger.Log(0, null, string.Format("222222 angle offset {0:0.000}, [{1:0.000},{2:0.000}], [{3:0.000},{4:0.000}]", 
                        real_angle - theory_angle, in_crds[0], in_crds[1], out_crds[0], out_crds[1]));
                    
                    if (2 == m_nCreateTaskMode)
                    {
                        Point2d new_crd = new Point2d(out_crds[0], out_crds[1]);
                        
                        target_crd.x = new_crd.x + m_current_task_data[0].m_theory_machine_crd.x;
                        target_crd.y = new_crd.y + m_current_task_data[0].m_theory_machine_crd.y;
                        target_crd.z = current_crd.z;

                        Debugger.Log(0, null, string.Format("222222 target_crd [{0:0.000},{1:0.000}]", target_crd.x, target_crd.y));
                    }
                    else
                    {
                        Point2d new_crd = new Point2d(out_crds[0], out_crds[1]);
                        new_crd.x += m_current_task_data[0].m_center_x_in_metric;
                        new_crd.y += m_current_task_data[0].m_center_y_in_metric;

                        target_crd.x = m_pcb_alignment_pt_on_machine.x + (new_crd.x - m_pcb_alignment_pt_on_graph.x);
                        target_crd.y = m_pcb_alignment_pt_on_machine.y + (new_crd.y - m_pcb_alignment_pt_on_graph.y);
                        target_crd.z = current_crd.z;
                    }
                }

                //Debugger.Log(0, null, string.Format("222222 坐标 [{0:0.000},{1:0.000}]", target_crd.x, target_crd.y));
                m_motion.linear_XYZ_wait_until_stop(target_crd.x, target_crd.y, target_crd.z, false);
            }

            // 通过高度传感器到达清晰平面
            #region
            if (0 == get_fiducial_mark_count(m_current_task_data))
            {
                if ((true == m_bUseHeightSensor) && (false == m_bOfflineMode))
                {
                    double vel = m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_long_range;

                    m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, m_dbStageTriggerHeight + 15, vel);

                    if (true == m_IO.is_height_sensor_activated())
                    {
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, m_dbStageTriggerHeight - 3, 7);

                        for (int m = 0; m < 1000; m++)
                        {
                            if (false == m_IO.is_height_sensor_activated())
                            {
                                Point3d crd = new Point3d(0, 0, 0);
                                m_motion.get_xyz_crds(ref crd);

                                m_dbClearPlanePosZ = crd.z - m_dbStageHeightGap;

                                Thread.Sleep(150);
                                m_motion.stop_axis(MotionOps.AXIS_Z);
                                Thread.Sleep(150);

                                break;
                            }

                            Thread.Sleep(5);
                        }
                    }

                    if (true == m_IO.is_height_sensor_activated())
                    {
                        MessageBox.Show(this, "传感器触发高度设置有问题，可能触发高度过大或过小，请检查。", "提示");

                        m_bIsMeasuringDuringCreation = false;
                        return;
                    }
                    else
                    {
                        vel = m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_short_range;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, m_dbClearPlanePosZ, vel);

                        //m_bIsMeasuringDuringCreation = false;
                        //return;
                    }
                }
            }
            #endregion

            // 自动调节光源，以达到指定图像亮度
            //if ((0 == get_fiducial_mark_count(m_current_task_data)) && (false == m_bOfflineMode))
            if (false == m_bOfflineMode)
            {
                if (true == data.m_bIsTopLightOn)
                    m_top_light.auto_adjust_brightness(m_guide_camera, m_nGuideCamLowerBrightness, m_nGuideCamUpperBrightness);
                else
                    m_bottom_light.auto_adjust_brightness(m_guide_camera, m_nGuideCamLowerBrightness, m_nGuideCamUpperBrightness);
            }
            
            // 确认已经完成变倍
            if (false == m_bOfflineMode)
            {
                for (int k = 0; k < 500; k++)
                {
                    if (false == m_len.m_bLenIsChangingRatio)
                        break;
                    Thread.Sleep(10);
                }
            }
            
            // 自动对焦
            if (false == m_bOfflineMode)
                m_image_operator.thread_auto_focus(new object());

            // 在导航图像中寻找定位孔
            #region
            if ((true == data.m_bAutoFindMarkCircleInGuideCam) && (false == m_bOfflineMode))
            {
                // 先在导航图像中寻找定位孔，定位效果较粗糙
                Point2d center = new Point2d(0, 0);
                Point2d offset = new Point2d(0, 0);
                if (true == m_image_operator.find_circle_in_cam(m_guide_camera, m_guide_cam_lock, 
                    m_guide_camera.m_pixels_per_um, data.m_metric_radius[0], ref center))
                {
                    offset.x = center.x - (double)(m_guide_camera.m_nCamWidth / 2);
                    offset.y = center.y - (double)(m_guide_camera.m_nCamHeight / 2);

                    offset.x = (offset.x / m_guide_camera.m_pixels_per_um) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                    offset.y = (offset.y / m_guide_camera.m_pixels_per_um) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;
                    
                    Point3d current_crd = new Point3d(0, 0, 0);
                    m_motion.get_xyz_crds(ref current_crd);
                    m_motion.linear_XYZ_wait_until_stop(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z, 50, 0.25, false);
                    Thread.Sleep(500);

                    // 在导航图像上高亮显示通过粗定位找到的定位孔
                    m_dbCoarseMarkRadius = data.m_metric_radius[0];
                    m_bShowCoarseMark = true;
                    
                    // 然后在主图像中进一步寻找定位孔，以提高定位精度
                    if (true)
                    {
                        // 设置光源和亮度
                        if (false == m_bOfflineMode)
                        {
                            data.m_bIsTopLightOn = (0 == m_nLightTypeForGuideCamForMarkPt) ? true : false;
                            data.m_bIsBottomLightOn = (1 == m_nLightTypeForGuideCamForMarkPt) ? true : false;

                            send_message("设置光源和亮度", false, data, null);
                            Thread.Sleep(800);
                        }

                        // 自动调节光源，以达到指定图像亮度
                        if ((0 == get_fiducial_mark_count(m_current_task_data)) && (false == m_bOfflineMode))
                        {
                            if (true == data.m_bIsTopLightOn)
                                m_top_light.auto_adjust_brightness(m_main_camera, m_nMainCamLowerBrightness, m_nMainCamUpperBrightness);
                            else
                                m_bottom_light.auto_adjust_brightness(m_main_camera, m_nMainCamLowerBrightness, m_nMainCamUpperBrightness);
                        }

                        // 绘制搜索框
                        m_ptSelectionFrameCenter.x = m_main_camera.m_nCamWidth / 2;
                        m_ptSelectionFrameCenter.y = m_main_camera.m_nCamHeight / 2;

                        if (true == send_message("图纸模式首件制作过程中测量定位孔", false, data, null))
                        {
                            Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 111"));
                            m_event_wait_for_confirm_during_creation.Reset();
                            if (false == send_message("图纸模式首件制作过程中找到定位孔", false, data, null))
                            {
                                Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 222"));
                                
                                m_event_wait_for_confirm_during_creation.WaitOne();
                                Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 333"));
                            }
                            Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 444"));
                        }
                        else
                        {
                            //if (2 == m_nCreateTaskMode)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    MessageBox.Show(this, "在主图像中寻找定位孔失败。", "提示");
                                }));

                                m_event_wait_for_confirm_during_creation.Reset();
                                m_event_wait_for_confirm_during_creation.WaitOne();
                            }
                        }
                    }
                }
                else
                {
                    Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程 在导航图像中寻找定位孔失败"));

                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show(this, "在导航图像中寻找定位孔失败。", "提示");
                    }));
                }
            }
            #endregion

            // 获取三角变换矩阵
            if (get_fiducial_mark_count(m_current_task_data) >= 3)
            {
                m_triangle_trans_matrix = new double[10];

                m_list_stage_graph_crd_pairs.Clear();
                for (int n = 0; n < 3; n++)
                {
                    StageGraphCrdPair pair = new StageGraphCrdPair();
                    pair.graph_crd.x = m_current_task_data[n].m_center_x_in_metric;
                    pair.graph_crd.y = m_current_task_data[n].m_center_y_in_metric;
                    pair.stage_crd.x = m_current_task_data[n].m_real_machine_crd.x;
                    pair.stage_crd.y = m_current_task_data[n].m_real_machine_crd.y;

                    m_list_stage_graph_crd_pairs.Add(pair);
                }
                
                generate_transform_matrix_by_three_pts(m_current_task_data, ref m_triangle_trans_matrix);

                Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 获取三角变换矩阵"));
            }

            m_bIsMeasuringDuringCreation = false;
        }

        // 线程：图纸模式创建首件时，以阵列方式添加测量点
        public void thread_locate_and_measure_item_in_array_mode(object obj)
        {
            dl_message_sender send_message = CBD_SendMessage;
            Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 111"));
            MeasurePointData base_data = (MeasurePointData)obj;

            base_data.m_strStepsFileName = m_strCurrentProductStep;
            base_data.m_strLayerFileName = m_strCurrentProductLayer;

            m_bCancelAndRemoveUnfinishedItems = false;
            m_bApplySameParamsToOtherUnits = false;

            bool bIsDrawnByHand = base_data.m_bIsDrawnByHand;

            List<List<rotated_array_rect>> array_rects = GraphView.m_ODB_thumbnail_array_rects;
            int off_x = MainUI.m_nGraphOffsetX;
            int off_y = MainUI.m_nGraphOffsetY;
            double pixels_per_mm = MainUI.m_pixels_per_mm;
            double dbBaseAngle = 0;
            double dbBaseOffsetX = 0;                               // 测量点到所在unit左上角的偏移X
            double dbBaseOffsetY = 0;                               // 测量点到所在unit左上角的偏移Y
            string strBaseUnitName = "";
            Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 aaa"));
            // 求原始测量点所在unit的名称
            #region
            for (int n = 0; n < array_rects.Count; n++)
            {
                for (int m = 0; m < array_rects[n].Count; m++)
                {
                    double left = array_rects[n][m].left * pixels_per_mm + off_x;
                    double top = array_rects[n][m].top * pixels_per_mm + off_y;
                    double right = left + array_rects[n][m].width * pixels_per_mm;
                    double bottom = top + array_rects[n][m].height * pixels_per_mm;
                    if ((base_data.m_center_x_on_graph > left) && (base_data.m_center_x_on_graph < right)
                        && (base_data.m_center_y_on_graph > top) && (base_data.m_center_y_on_graph < bottom))
                    {
                        strBaseUnitName = array_rects[n][m].strUnitName;
                        dbBaseAngle = array_rects[n][m].odb_angle;
                        dbBaseOffsetX = base_data.m_center_x_on_graph - left;
                        dbBaseOffsetY = base_data.m_center_y_on_graph - top;
                        //Debugger.Log(0, null, string.Format("222222 name = {0}, dbBaseOffsetX = [{1:0.},{2:0.}]",
                        //                strBaseUnitName, dbBaseOffsetX, dbBaseOffsetY));
                        break;
                    }
                }
            }
            #endregion
            Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 bbb"));
            if (strBaseUnitName.Length <= 0)
            {
                new Thread(thread_locate_and_measure_item).Start(base_data);
                for (int n = 0; n < 4; n++)
                    m_graph_view.m_drawn_line_pts[n] = new Point2d(0, 0);
                return;
            }
            Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 ccc"));
            List<rotated_array_rect> vec_qualified_rects = new List<rotated_array_rect>();
            for (int n = 0; n < array_rects.Count; n++)
            {
                for (int m = 0; m < array_rects[n].Count; m++)
                {
                    if ((true == array_rects[n][m].bSelected) && (strBaseUnitName == array_rects[n][m].strUnitName))
                    {
                        rotated_array_rect rect = array_rects[n][m].cloneClass();
                        vec_qualified_rects.Add(rect);
                    }
                }
            }
            Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 222"));
            if (vec_qualified_rects.Count > 0)
            {
                // 当所有unit的m_nSelectOrder等于-1的时候，表示阵列处于全选状态
                if (vec_qualified_rects[0].m_nSelectOrder >= 0)
                    vec_qualified_rects.Sort();

                for (int n = 0; n < vec_qualified_rects.Count; n++)
                    Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 n {0}: vec_qualified_rects[n].left = [{1:0.000},{2:0.000}]", 
                        n, vec_qualified_rects[n].left, vec_qualified_rects[n].top));

                if (false == bIsDrawnByHand)
                {
                bool bNeedConfirm = false;
                for (int n = 0; n < vec_qualified_rects.Count; n++)
                {
                    double left = vec_qualified_rects[n].left * pixels_per_mm + off_x;
                    double top = vec_qualified_rects[n].top * pixels_per_mm + off_y;

                    MeasurePointData data = new MeasurePointData();
                        if (false == add_measure_point(base_data.m_mes_type, (int)(left + dbBaseOffsetX), (int)(top + dbBaseOffsetY), ref data, false, bIsDrawnByHand))
                    {
                        bNeedConfirm = true;
                        break;
                    }
                }
                    Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 333"));
                if (true == bNeedConfirm)
                {
                    if (DialogResult.Yes == MessageBox.Show(this, "识别到的图形元素数量与选择的阵列Unit数目不一致，是否放弃本次操作?", "提示", MessageBoxButtons.YesNo))
                    {
                        m_bIsMeasuringDuringCreation = false;
                            for (int n = 0; n < 4; n++)
                                m_graph_view.m_drawn_line_pts[n] = new Point2d(0, 0);
                        return;
                    }
                }
                }
                
                int nNewAddedItems = 0;
                for (int n = 0; n < vec_qualified_rects.Count; n++)
                {
                    double left = vec_qualified_rects[n].left * pixels_per_mm + off_x;
                    double top = vec_qualified_rects[n].top * pixels_per_mm + off_y;
                    
                    MeasurePointData data = new MeasurePointData();
                    bool result = false;
                    if (true == bIsDrawnByHand)
                    {
                        double offx = (vec_qualified_rects[n].left - vec_qualified_rects[0].left) * pixels_per_mm;
                        double offy = (vec_qualified_rects[n].top - vec_qualified_rects[0].top) * pixels_per_mm;
                        Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 offx = [{0:0.000},{1:0.000}]", offx, offy));
                        result = add_measure_point(base_data.m_mes_type, (int)(offx), (int)(offy), ref data, false, bIsDrawnByHand);
                    }
                    else
                        result = add_measure_point(base_data.m_mes_type, (int)(left + dbBaseOffsetX), (int)(top + dbBaseOffsetY), ref data, false, bIsDrawnByHand);
                    
                    if (true == result)
                    {
                        data.m_nArrayOrderIdx = m_nCurrentArrayOrderIdx;
                        m_measure_items_on_graph.Add(data);
                        nNewAddedItems++;
                        //send_message("刷新图纸测量项列表", false, null, null);
                    }
                }
                send_message("刷新图纸测量项列表", false, null, null);
                
                Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 444"));
                int nStart = m_measure_items_on_graph.Count - nNewAddedItems;
                for (int n = nStart; n < m_measure_items_on_graph.Count; n++)
                {
                    if (false == m_bOfflineMode)
                    {
                        send_message("将gridview选中在第N行", false, n, null);
                        
                        MeasurePointData new_data = m_measure_items_on_graph[n].cloneClass();

                        m_bIsMeasuringInArrayMode = true;
                        m_current_measure_graph_item = new_data;
                        thread_locate_and_measure_item(new_data);
                        m_bIsMeasuringDuringCreation = true;
                        m_bIsMeasuringInArrayMode = false;

                        if (true == m_bCancelAndRemoveUnfinishedItems)
                        {
                            for (int k = m_measure_items_on_graph.Count - 1; k >= 0; k--)
                            {
                                if (m_measure_items_on_graph[k].m_nArrayOrderIdx == m_nCurrentArrayOrderIdx)
                                    m_measure_items_on_graph.RemoveAt(k);
                            }
                            send_message("刷新图纸测量项列表", false, null, null);

                            m_event_wait_for_manual_gauge.Set();
                            m_event_wait_for_confirm_during_creation.Set();

                            break;
                        }

                        //Thread.Sleep(1000);
                    }
                }

                m_nCurrentArrayOrderIdx++;
            }
            Debugger.Log(0, null, string.Format("222222 以阵列方式添加测量点 555"));
            m_bIsMeasuringDuringCreation = false;
            m_bCancelAndRemoveUnfinishedItems = false;
            m_bApplySameParamsToOtherUnits = false;

            for (int n = 0; n < 4; n++)
                m_graph_view.m_drawn_line_pts[n] = new Point2d(0, 0);
        }

        // 线程：图纸模式创建首件时，寻找和测量普通对象
        public void thread_locate_and_measure_item(object obj)
        {
            dl_message_sender send_message = CBD_SendMessage;
            send_message("刷新图纸测量项列表", false, true, null);

            MeasurePointData data = (MeasurePointData)obj;
            data.m_nAlgorithm = m_nAlgorithm;

            //Debugger.Log(0, null, string.Format("222222 m_pcb_alignment_pt_on_graph [{0:0.000},{1:0.000}], data [{2:0.000},{3:0.000}]",
            //    m_pcb_alignment_pt_on_graph.x, m_pcb_alignment_pt_on_graph.y, data.m_center_x_in_metric, data.m_center_y_in_metric));

            m_bShowSmallSelectionFrame = false;
            m_bShowFrameDuringTaskCreation = false;
            m_bShowCoarseMark = false;
            m_bShowAccurateMark = false;

            // 设置倍率
            int nPrevLenRatio = comboBox_Len.SelectedIndex;
            if (false == m_bOfflineMode)
                send_message("设置倍率", false, data.m_len_ratio, null);

            data.m_nTopBrightness = m_top_light.m_nBrightness;
            data.m_nBottomBrightness = m_bottom_light.m_nBrightness;
            switch (data.m_mes_type)
            {
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    if (0 == m_nLightTypeForBGA)
                    {
                        data.m_bIsTopLightOn = true;
                        data.m_bIsBottomLightOn = false;
                    }
                    else
                    {
                        data.m_bIsTopLightOn = false;
                        data.m_bIsBottomLightOn = true;
                    }
                    break;

                case MEASURE_TYPE.LINE_WIDTH_14:
                case MEASURE_TYPE.LINE_WIDTH_1234:
                case MEASURE_TYPE.LINE_WIDTH_23:
                case MEASURE_TYPE.LINE_WIDTH_13:
                case MEASURE_TYPE.LINE_WIDTH_AVR:
                case MEASURE_TYPE.ARC_LINE_WIDTH:
                    if (0 == m_nLightTypeFor14Line)
                    {
                        data.m_bIsTopLightOn = true;
                        data.m_bIsBottomLightOn = false;
                    }
                    else
                    {
                        data.m_bIsTopLightOn = false;
                        data.m_bIsBottomLightOn = true;
                    }
                    break;

                case MEASURE_TYPE.LINE_SPACE:
                case MEASURE_TYPE.ARC_LINE_SPACE:
                    if (0 == m_nLightTypeForLineSpace)
                    {
                        data.m_bIsTopLightOn = true;
                        data.m_bIsBottomLightOn = false;
                    }
                    else
                    {
                        data.m_bIsTopLightOn = false;
                        data.m_bIsBottomLightOn = true;
                    }
                    break;
            }

            // 自动调节光源
            #region
            if ((false == m_bOfflineMode) && (true == m_bAutoAdjustLightDuringTaskCreation))
            {
                if (true == data.m_bIsTopLightOn)
                {
                    if (false == m_top_light.m_bOn)
                    {
                        m_top_light.m_bOn = true;
                        m_top_light.open_light();
                    }
                    m_bottom_light.m_bOn = false;
                    m_bottom_light.close_light();
                }
                else
                {
                    if (false == m_bottom_light.m_bOn)
                    {
                        m_bottom_light.m_bOn = true;
                        m_bottom_light.open_light();
                    }
                    m_top_light.m_bOn = false;
                    m_top_light.close_light();
                }
                refresh_light_icons();
            }
            #endregion

            // 设置测量类型
            send_message("设置测量类型", false, data.m_mes_type, null);

            // 先移动到减去对齐偏移后的位置
            if (false == m_bOfflineMode)
            {
                Point3d target_crd = new Point3d();
                m_motion.get_xyz_crds(ref target_crd);

                if (true == data.m_bIsFromODBAttribute)
                {
                    double[] trans_matrix = new double[10];

                    // 计算与待测点距离最近的三个点，基于此三个点，生成一个仿射变换矩阵
                    Point2d current_pt = new Point2d(data.m_center_x_in_metric, data.m_center_y_in_metric);
                    double min_dist1 = 1000000;
                    double min_dist2 = 1000000;
                    double min_dist3 = 1000000;
                    int min_dist1_idx = -1;
                    int min_dist2_idx = -1;
                    int min_dist3_idx = -1;
                    for (int n = 0; n < m_list_stage_graph_crd_pairs.Count; n++)
                    {
                        double dist = GeneralUtils.get_distance(m_list_stage_graph_crd_pairs[n].graph_crd, current_pt);
                        if (dist < min_dist1)
                        {
                            min_dist1 = dist;
                            min_dist1_idx = n;
                        }
                        else if (dist < min_dist2)
                        {
                            min_dist2 = dist;
                            min_dist2_idx = n;
                        }
                        else if (dist < min_dist3)
                        {
                            min_dist3 = dist;
                            min_dist3_idx = n;
                        }
                    }

                    if ((min_dist1_idx >= 0) && (min_dist2_idx >= 0) && (min_dist3_idx >= 0))
                    {
                        List<StageGraphCrdPair> list_pairs = new List<StageGraphCrdPair>();
                        list_pairs.Add(m_list_stage_graph_crd_pairs[min_dist1_idx]);
                        list_pairs.Add(m_list_stage_graph_crd_pairs[min_dist2_idx]);
                        list_pairs.Add(m_list_stage_graph_crd_pairs[min_dist3_idx]);

                        generate_transform_matrix_by_three_pts(list_pairs, ref trans_matrix);

                        target_crd.x = trans_matrix[0] * data.m_center_x_in_metric + trans_matrix[1] * data.m_center_y_in_metric + trans_matrix[2];
                        target_crd.y = trans_matrix[3] * data.m_center_x_in_metric + trans_matrix[4] * data.m_center_y_in_metric + trans_matrix[5];
                    }
                    else
                    {
                        target_crd.x = m_triangle_trans_matrix[0] * data.m_center_x_in_metric + m_triangle_trans_matrix[1] * data.m_center_y_in_metric + m_triangle_trans_matrix[2];
                        target_crd.y = m_triangle_trans_matrix[3] * data.m_center_x_in_metric + m_triangle_trans_matrix[4] * data.m_center_y_in_metric + m_triangle_trans_matrix[5];
                    }
                }
                else
                {
                    target_crd.x = m_triangle_trans_matrix[0] * data.m_center_x_in_metric + m_triangle_trans_matrix[1] * data.m_center_y_in_metric + m_triangle_trans_matrix[2];
                    target_crd.y = m_triangle_trans_matrix[3] * data.m_center_x_in_metric + m_triangle_trans_matrix[4] * data.m_center_y_in_metric + m_triangle_trans_matrix[5];
                }
                Debugger.Log(0, null, string.Format("222222 target_crd = [{0:0.000},{1:0.000}]", target_crd.x, target_crd.y));

                if (data.m_len_ratio > 0)
                {
                    target_crd.x += m_len_ratios_offsets[data.m_len_ratio].x;
                    target_crd.y += m_len_ratios_offsets[data.m_len_ratio].y;
                }
                
                m_motion.linear_XYZ_wait_until_stop(target_crd.x, target_crd.y, target_crd.z, false);
                Thread.Sleep(100);
            }
            
            // 确认已经完成变倍
            if (false == m_bOfflineMode)
            {
                if ((data.m_len_ratio - nPrevLenRatio) > 3)
                    Thread.Sleep(100);
                for (int k = 0; k < 500; k++)
                {
                    if (false == m_len.m_bLenIsChangingRatio)
                        break;
                    Thread.Sleep(10);
                }
                if ((data.m_len_ratio - nPrevLenRatio) > 3)
                    Thread.Sleep(100);
            }
            
            // 自动调节光源，以达到指定图像亮度
            if (false == m_bOfflineMode)
            {
                //if (true == data.m_bIsTopLightOn)
                //    m_top_light.auto_adjust_brightness(m_main_camera, m_nMainCamLowerBrightness, m_nMainCamUpperBrightness);
                //else
                //    m_bottom_light.auto_adjust_brightness(m_main_camera, m_nMainCamLowerBrightness, m_nMainCamUpperBrightness);
                if (true == m_top_light.m_bOn)
                    m_top_light.auto_adjust_brightness(m_main_camera, m_nMainCamLowerBrightness, m_nMainCamUpperBrightness);
                else if (true == m_bottom_light.m_bOn)
                    m_bottom_light.auto_adjust_brightness(m_main_camera, m_nMainCamLowerBrightness, m_nMainCamUpperBrightness);
            }

            Thread.Sleep(500);
            
            // 自动对焦
            if (false == m_bOfflineMode)
            {
                switch (data.m_mes_type)
                {
                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        if (true == data.m_bIsNormalLine)
                            m_image_operator.m_bFastFocus = true;

                        m_image_operator.m_mes_data = data;
                        m_image_operator.thread_auto_focus(new object());
                        m_image_operator.m_bFastFocus = false;
                        Thread.Sleep(500);
                        break;

                    default:
                        m_image_operator.thread_auto_focus(new object());
                        break;
                }
            }

            if (m_dbDelaySecondsBeforeMeasure > 0)
                Thread.Sleep((int)(double)(m_dbDelaySecondsBeforeMeasure * 1000));

            // 测量
            if (false == m_bOfflineMode)
            {
                switch (data.m_mes_type)
                {
                    // BGA
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                        if (true == send_message("图纸模式首件制作过程中添加测量项", false, data, null))
                        {
                            m_event_wait_for_manual_gauge.Reset();
                            if (false == send_message("图纸模式首件制作过程中在主图像找到测量对象", false, data, null))
                            {
                                m_event_wait_for_manual_gauge.WaitOne();
                            }
                        }
                        else
                        {
                            m_event_wait_for_manual_gauge.Reset();
                            m_event_wait_for_manual_gauge.WaitOne();
                        }
                        break;

                    // 线宽
                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        if (true == send_message("图纸模式首件制作过程中添加测量项", false, data, null))
                        {
                            m_event_wait_for_manual_gauge.Reset();
                            //Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中在主图像找到测量对象 111"));
                            if (false == send_message("图纸模式首件制作过程中在主图像找到测量对象", false, data, null))
                            {
                                //Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中在主图像找到测量对象 222"));
                                m_event_wait_for_manual_gauge.WaitOne();
                            }
                            //Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中在主图像找到测量对象 333"));
                        }
                        else
                        {
                            m_event_wait_for_manual_gauge.Reset();
                            m_event_wait_for_manual_gauge.WaitOne();
                        }
                        break;

                    default:
                        break;
                }

                switch (data.m_mes_type)
                {
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        StageGraphCrdPair pair = new StageGraphCrdPair();
                        pair.graph_crd.x = data.m_center_x_in_metric;
                        pair.graph_crd.y = data.m_center_y_in_metric;
                        pair.stage_crd.x = data.m_real_machine_crd.x;
                        pair.stage_crd.y = data.m_real_machine_crd.y;

                        m_list_stage_graph_crd_pairs.Add(pair);
                        break;
                }
            }
            
            if (true == m_bDoNotConfirmMeasureItemAtCreation)
                Thread.Sleep(1000);

            m_bIsMeasuringDuringCreation = false;
        }

        // 线程：执行图纸模式制作的测量任务
        public void thread_run_CAD_task(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            Debugger.Log(0, null, string.Format("222222 执行图纸模式制作的测量任务"));

            //send_message("开启吸附", true, null, null);
            Thread.Sleep(m_nMeasureTaskDelayTime);

            m_nUnitTypeBeforeRunningTask = m_nUnitType;

            // 开绿灯，关黄灯
            m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_LOW);
            m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_HIGH);

            double dbHeightDiffSum = 0;
            double dbHeightCompensation = 0;

            #region
            while (true)
            {
                for (int n = 0; n < m_current_task_data.Count; n++)
                {
                    //Debugger.Log(0, null, string.Format("222222 n {0}: crd = [{1:0.000},{2:0.000},{3:0.000}]", n,
                    //    m_current_task_data[n].m_machine_crd.x, m_current_task_data[n].m_machine_crd.y,
                    //    m_current_task_data[n].m_machine_crd.z));

                    feed_dog();

                    if (m_bStopTask || m_bEmergencyExit)
                        break;

                    while (true == m_bPause)
                    {
                        feed_dog();
                        if (m_bStopTask || m_bEmergencyExit)
                            break;
                        if (m_bExitProgram)
                            return;
                        Thread.Sleep(50);
                    }

                    m_bShowSmallSelectionFrame = false;
                    m_bShowFrameDuringTaskCreation = false;
                    m_bShowCoarseMark = false;
                    m_bShowAccurateMark = false;

                    m_nCurrentMeasureItemIdx = n;

                    MeasurePointData data = m_current_task_data[n];

                    if ((true == m_bIsAddingNewItemsToExistingTask) && (n >= 3))
                    {
                        m_bIsCreatingTask = true;

                        m_list_measure_results.Clear();
                        m_measure_items_on_graph.Clear();
                        gridview_GraphMeasureItems.Rows.Clear();

                        //btn_LoadTask.Enabled = false;
                        //btn_UpdateTask.Enabled = false;

                        m_nCreateTaskMode = 1;

                        MessageBox.Show(this, "已经完成三个定位孔的位置确认。现在请手动移动相机至要测量的位置，添加新测量项。", "提示");

                        return;
                    }

                    // 设置倍率
                    send_message("设置倍率", false, data.m_len_ratio, null);

                    // 设置单位
                    send_message("设置单位", false, data.m_unit, null);

                    // 设置光源和亮度
                    send_message("设置光源和亮度", false, data, null);

                    // 设置测量类型
                    send_message("设置测量类型", false, data.m_mes_type, null);

                    if (false == m_bIsAddingNewItemsToExistingTask)
                        send_message(string.Format("准备测量 第{0}个点", n + 1), false, m_current_task_data[n], n);

                    // 移动到记录位置
                    Point3d pt = data.m_theory_machine_crd;
                    if (m_nCurrentMeasureItemIdx >= 3)
                        pt = data.m_real_machine_crd;
                    else
                    {
                        if (1 == n)
                        {
                            Point2d offset = new Point2d(0, 0);
                            offset.x = m_current_task_data[0].m_real_machine_crd.x - m_current_task_data[0].m_theory_machine_crd.x;
                            offset.y = m_current_task_data[0].m_real_machine_crd.y - m_current_task_data[0].m_theory_machine_crd.y;

                            pt.x += offset.x;
                            pt.y += offset.y;
                        }
                        else if (2 == n)
                        {
                            double[] out_doubles = new double[10];
                            double theory_angle = 0;
                            double real_angle = 0;

                            get_theta(m_current_task_data[0].m_theory_machine_crd.x, m_current_task_data[0].m_theory_machine_crd.y,
                                m_current_task_data[1].m_theory_machine_crd.x, m_current_task_data[1].m_theory_machine_crd.y, out_doubles);
                            theory_angle = out_doubles[0];

                            get_theta(m_current_task_data[0].m_real_machine_crd.x, m_current_task_data[0].m_real_machine_crd.y,
                                m_current_task_data[1].m_real_machine_crd.x, m_current_task_data[1].m_real_machine_crd.y, out_doubles);
                            real_angle = out_doubles[0];

                            double[] in_crds = new double[2];
                            double[] out_crds = new double[2];
                            in_crds[0] = data.m_theory_machine_crd.x - m_current_task_data[0].m_theory_machine_crd.x;
                            in_crds[1] = data.m_theory_machine_crd.y - m_current_task_data[0].m_theory_machine_crd.y;
                            rotate_crd(in_crds, out_crds, real_angle - theory_angle);

                            Point2d new_crd = new Point2d(out_crds[0], out_crds[1]);
                            new_crd.x += m_current_task_data[0].m_real_machine_crd.x;
                            new_crd.y += m_current_task_data[0].m_real_machine_crd.y;

                            pt.x = new_crd.x;
                            pt.y = new_crd.y;
                        }
                    }

                    //m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, pt.z, 600, 1.5, false);
                    if ((0 == n) && (true == m_bUseHeightSensor) && (true == m_bUseAutofocusWhenRunningTask))
                        m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, m_dbStageTriggerHeight + 15, false);
                    else
                    {
                        if (n > 0)
                            dbHeightCompensation = dbHeightDiffSum / (double)n;

                        if ((Math.Abs(pt.x - m_current_task_data[n - 1].m_real_machine_crd.x) > 0.001) || (Math.Abs(pt.y - m_current_task_data[n - 1].m_real_machine_crd.y) > 0.001))
                        {
                            if (data.m_len_ratio > 0)
                            {
                                pt.x += m_len_ratios_offsets[data.m_len_ratio].x;
                                pt.y += m_len_ratios_offsets[data.m_len_ratio].y;
                            }
                            
                            m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, pt.z + dbHeightCompensation, false);
                        }
                    }

                    // 通过高度传感器到达清晰平面
                    if ((0 == n) && (true == m_bUseAutofocusWhenRunningTask))
                    {
                        if (true == m_bUseHeightSensor)
                        {
                            double vel = m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_long_range;

                            m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, m_dbStageTriggerHeight + 15, vel);

                            if (true == m_IO.is_height_sensor_activated())
                            {
                                m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, m_dbStageTriggerHeight - 3, 7);

                                for (int m = 0; m < 1000; m++)
                                {
                                    if (false == m_IO.is_height_sensor_activated())
                                    {
                                        Point3d crd = new Point3d(0, 0, 0);
                                        m_motion.get_xyz_crds(ref crd);

                                        m_dbClearPlanePosZ = crd.z - m_dbStageHeightGap;

                                        Thread.Sleep(150);
                                        m_motion.stop_axis(MotionOps.AXIS_Z);
                                        Thread.Sleep(150);

                                        double offset = m_dbClearPlanePosZ - m_current_task_data[0].m_real_machine_crd.z;
                                        for (int k = 1; k < m_current_task_data.Count; k++)
                                        {
                                            m_current_task_data[k].m_real_machine_crd.z += offset;
                                            if (m_current_task_data[k].m_real_machine_crd.z <= m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit)
                                                m_current_task_data[k].m_real_machine_crd.z = m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit + 0.2;
                                        }

                                        break;
                                    }

                                    Thread.Sleep(5);
                                }
                            }

                            if (true == m_IO.is_height_sensor_activated())
                            {
                                MessageBox.Show(this, "传感器触发高度设置有问题，可能触发高度过大或过小，请检查。", "提示");

                                m_bIsMeasuringDuringCreation = false;
                                return;
                            }
                            else
                            {
                                vel = m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_short_range;
                                m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, m_dbClearPlanePosZ, vel);

                                //m_bIsMeasuringDuringCreation = false;
                                //return;
                            }
                        }
                    }

                    //Debugger.Log(0, null, string.Format("222222 crd [{0:0.000},{1:0.000},{2:0.000}], num = {3}", pt.x, pt.y, pt.z, m_current_task_data.Count));

                    Thread.Sleep(150);

                    // 确认已经完成变倍
                    for (int k = 0; k < 500; k++)
                    {
                        if (false == m_len.m_bLenIsChangingRatio)
                            break;
                        Thread.Sleep(10);
                    }

                    //Debugger.Log(0, null, string.Format("111111 111 data.m_sharpness_at_creation = {0:0.000}", data.m_sharpness_at_creation));

                    // 自动对焦
                    if (true == m_bUseAutofocusWhenRunningTask)
                    {
                        bool bNeedFocus = true;
                        if (true == m_bSelectivelySkipAutofocus)
                        {
                            double sharpness = 0;
                            if (true == Gaugers.ImgOperators.get_image_sharpness(m_main_camera.m_pImageBuf,
                                m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                            {
                                //Debugger.Log(0, null, string.Format("222222 sharpness = {0:0.000}, data.m_sharpness_at_creation = {1:0.000}",
                                //    sharpness, data.m_sharpness_at_creation));
                                double ratio = (sharpness / data.m_sharpness_at_creation) * (double)100;
                                if (ratio >= m_nThresForSkippingAutofocus)
                                    bNeedFocus = false;
                            }
                        }
                        if (true == bNeedFocus)
                        {
                            switch (data.m_mes_type)
                            {
                                case MEASURE_TYPE.LINE_WIDTH_14:
                                case MEASURE_TYPE.LINE_WIDTH_23:
                                case MEASURE_TYPE.LINE_WIDTH_13:
                                case MEASURE_TYPE.LINE_WIDTH_1234:
                                case MEASURE_TYPE.LINE_SPACE:
                                case MEASURE_TYPE.ARC_LINE_SPACE:
                                case MEASURE_TYPE.ARC_LINE_WIDTH:
                                    m_image_operator.m_bFastFocus = true;
                                    m_image_operator.m_mes_data = data;
                                    m_image_operator.thread_auto_focus(new object());
                                    m_image_operator.m_bFastFocus = false;
                                    Thread.Sleep(200);
                                    break;

                                default:
                                    m_image_operator.thread_auto_focus(new object());
                                    break;
                            }
                        }
                        else
                            //Thread.Sleep(200);
                            Thread.Sleep(20);
                    }

                    if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != data.m_mes_type)
                    {
                    if (m_dbDelaySecondsBeforeMeasure > 0)
                            Thread.Sleep((int)(double)(m_dbDelaySecondsBeforeMeasure * 1000));
                    }
                    
                    Point3d measure_crd = m_current_task_data[n].m_real_machine_crd;
                    m_motion.get_xyz_crds(ref measure_crd);

                    dbHeightDiffSum += measure_crd.z - m_current_task_data[n].m_theory_machine_crd.z;
                    m_current_task_data[n].m_real_machine_crd.z = measure_crd.z;

                    // 测量
                    bool bIsGaugeOK = false;
                    switch (data.m_mes_type)
                    {
                        // 定位孔
                        case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                            if (true == data.m_bAutoFindMarkCircleInGuideCam)
                            {
                                // 先在导航图像中寻找定位孔，定位效果较粗糙
                                Thread.Sleep(300);

                                Point2d center = new Point2d(0, 0);
                                Point2d offset = new Point2d(0, 0);
                                if (true == m_image_operator.find_circle_in_cam(m_guide_camera, m_guide_cam_lock,
                                    m_guide_camera.m_pixels_per_um, data.m_metric_radius[0], ref center))
                                {
                                    offset.x = center.x - (double)(m_guide_camera.m_nCamWidth / 2);
                                    offset.y = center.y - (double)(m_guide_camera.m_nCamHeight / 2);

                                    offset.x = (offset.x / m_guide_camera.m_pixels_per_um) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                    offset.y = (offset.y / m_guide_camera.m_pixels_per_um) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;
                                    //Debugger.Log(0, null, string.Format("222222 n {0}: 定位孔 222 offset = [{1:0.000},{2:0.000}]", n, offset.x, offset.y));

                                    Point3d current_crd = new Point3d(0, 0, 0);
                                    m_motion.get_xyz_crds(ref current_crd);
                                    m_motion.linear_XYZ_wait_until_stop(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z, 50, 0.25, false);

                                    if (m_dbDelaySecondsBeforeMeasure > 0)
                                        Thread.Sleep((int)(double)(m_dbDelaySecondsBeforeMeasure * 1000));

                                    // 在导航图像上高亮显示通过粗定位找到的定位孔
                                    m_dbCoarseMarkRadius = data.m_metric_radius[0];
                                    m_bShowCoarseMark = true;

                                    // 然后在主图像中进一步寻找定位孔，以提高定位精度
                                    if (true)
                                    {
                                        bIsGaugeOK = send_message(string.Format("执行测量 第{0}个 定位孔", n + 1), true, m_current_task_data[n], n);
                                        if (true == bIsGaugeOK)
                                        {
                                            center = m_gauger.m_gauged_circle_center;
                                            offset.x = center.x - (double)(m_main_camera.m_nCamWidth / 2);
                                            offset.y = center.y - (double)(m_main_camera.m_nCamHeight / 2);

                                            offset.x = (offset.x / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                            offset.y = (offset.y / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                            current_crd = new Point3d(0, 0, 0);
                                            m_motion.get_xyz_crds(ref current_crd);
                                            current_crd.x += offset.x;
                                            current_crd.y += offset.y;

                                            data.m_real_machine_crd = current_crd;
                                        }
                                    }
                                }
                            }
                            break;

                        // BGA
                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                            bIsGaugeOK = send_message(string.Format("执行测量 第{0}个 位置", n - 3 + 1), true, m_current_task_data[n], n);
                            if (true == bIsGaugeOK)
                            {

                            }
                            break;

                        // 线宽
                        case MEASURE_TYPE.LINE_WIDTH_14:
                        case MEASURE_TYPE.LINE_WIDTH_23:
                        case MEASURE_TYPE.LINE_WIDTH_13:
                        case MEASURE_TYPE.LINE_WIDTH_1234:
                        case MEASURE_TYPE.LINE_SPACE:
                        case MEASURE_TYPE.ARC_LINE_SPACE:
                        case MEASURE_TYPE.ARC_LINE_WIDTH:
                            bIsGaugeOK = send_message(string.Format("执行测量 第{0}个 位置", n - 3 + 1), true, m_current_task_data[n], n);
                            if (true == bIsGaugeOK)
                            {

                            }
                            break;

                        default:
                            break;
                    }

                    // 测量失败，需要用户进行手动测量
                    //bIsGaugeOK = false;
                    if (false == bIsGaugeOK)
                    {
                        switch (data.m_mes_type)
                        {
                            case MEASURE_TYPE.HAND_PICK_LINE:
                            case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                m_event_wait_for_manual_gauge.Reset();

                                send_message(string.Format("请手动操作"), false, m_current_task_data[n], n);
                                m_event_wait_for_manual_gauge.WaitOne();
                                break;

                            default:
                                m_event_wait_for_manual_gauge.Reset();

                                send_message(string.Format("第{0}个 位置 需要用户进行手动测量", n - 3 + 1), true, m_current_task_data[n], n);
                                m_event_wait_for_manual_gauge.WaitOne();
                                break;
                        }
                    }
                    else
                    {
                        // 判断是否需要确认定位结果
                        if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == data.m_mes_type)
                        {
                            if (true == m_bNeedConfirmFiducialMark)
                            {
                                m_event_wait_for_confirm_during_autorun.Reset();

                                send_message("等待用户确认定位孔", false, null, null);
                                m_event_wait_for_confirm_during_autorun.WaitOne();
                            }
                            else
                                Thread.Sleep(300);
                        }
                        else
                        {
                            if (true == m_bNeedConfirmMeasureResult)
                            {
                                m_event_wait_for_confirm_during_autorun.Reset();

                                send_message("等待用户确认测量结果", false, null, null);
                                m_event_wait_for_confirm_during_autorun.WaitOne();
                            }
                            else
                            {
                                m_bTriggerSaveGaugeImage = true;

                                Thread.Sleep(300);
                            }
                        }
                    }

                    // 三点仿射变换
                    if (2 == n)
                    {
                        int[] out_ints = new int[10];
                        int[] out_ints2 = new int[10];
                        double[] in_theory_crds = new double[10];
                        double[] in_theory_crds2 = new double[10];
                        double[] in_real_crds = new double[10];

                        m_triangle_trans_matrix = new double[10];
                        double[] matrix2 = new double[10];  // 变换矩阵

                        for (int k = 0; k < 3; k++)
                        {
                            in_theory_crds[k * 2] = m_current_task_data[k].m_center_x_in_metric;
                            in_theory_crds[k * 2 + 1] = m_current_task_data[k].m_center_y_in_metric;
                            in_theory_crds2[k * 2] = m_current_task_data[k].m_theory_machine_crd.x;
                            in_theory_crds2[k * 2 + 1] = m_current_task_data[k].m_theory_machine_crd.y;

                            in_real_crds[k * 2] = m_current_task_data[k].m_real_machine_crd.x;
                            in_real_crds[k * 2 + 1] = m_current_task_data[k].m_real_machine_crd.y;

                            //Debugger.Log(0, null, string.Format("222222 k {0}: [{1:0.000},{2:0.000}], [{1:0.000},{2:0.000}]", pt.x, pt.y, pt.z, m_current_task_data.Count));
                        }

                        out_ints[0] = 0;
                        get_transform_matrix(out_ints, in_theory_crds, in_real_crds, m_triangle_trans_matrix);

                        out_ints2[0] = 0;
                        get_transform_matrix(out_ints2, in_theory_crds2, in_real_crds, matrix2);

                        if (1 == out_ints[0])
                        {
                            for (int k = 3; k < m_current_task_data.Count; k++)
                            {
                                MeasurePointData item = m_current_task_data[k];

                                Debugger.Log(0, null, string.Format("222222 111 k {0}: item.m_real_machine_crd = [{1:0.000},{2:0.000}]", k,
                                        item.m_real_machine_crd.x, item.m_real_machine_crd.y));

                                if (Math.Abs(item.m_center_x_in_metric) < 0.01)           // 小于0.01表明做首件时该测量项是手动拉框测量的
                                {
                                    Point3d item_crd = new Point3d(item.m_theory_machine_crd.x, item.m_theory_machine_crd.y, item.m_theory_machine_crd.z);

                                    item.m_real_machine_crd.x = matrix2[0] * item_crd.x + matrix2[1] * item_crd.y + matrix2[2];
                                    item.m_real_machine_crd.y = matrix2[3] * item_crd.x + matrix2[4] * item_crd.y + matrix2[5];
                                    item.m_real_machine_crd.z = item.m_theory_machine_crd.z;
                                    Debugger.Log(0, null, string.Format("222222 222 k {0}: item.m_real_machine_crd = [{1:0.000},{2:0.000}]", k,
                                        item.m_real_machine_crd.x, item.m_real_machine_crd.y));
                                }
                                else
                                {
                                    Point3d item_crd = new Point3d(item.m_center_x_in_metric, item.m_center_y_in_metric, item.m_theory_machine_crd.z);

                                    item.m_real_machine_crd.x = m_triangle_trans_matrix[0] * item_crd.x + m_triangle_trans_matrix[1] * item_crd.y + m_triangle_trans_matrix[2];
                                    item.m_real_machine_crd.y = m_triangle_trans_matrix[3] * item_crd.x + m_triangle_trans_matrix[4] * item_crd.y + m_triangle_trans_matrix[5];
                                    item.m_real_machine_crd.z = item.m_theory_machine_crd.z;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    //Thread.Sleep(600);
                }

                if (m_bStopTask || m_bEmergencyExit)
                    break;

                if (false == m_bRepeatRunningTask)
                    break;
                else
                {
                    m_nNumOfTaskRepetition--;

                    send_message("设置文本框内容", false, textBox_NumOfTaskRepetitions, string.Format("{0}", m_nNumOfTaskRepetition));

                    if (m_nNumOfTaskRepetition <= 0)
                        break;
                }
            }
            #endregion

            // 关闭吸附
            //if (false == m_bOfflineMode)
            //    send_message("关闭吸附", true, null, null);

            bool bOK = m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, 0, 20);
            Thread.Sleep(200);
            bOK = m_motion.linear_XY_wait_until_stop(0, 0, 500, 1, false);

            // 保存报表
            if (true == m_bSaveGaugeResultExcelReport)
            {
                send_message("保存报表", true, null, null);
                thread_save_gauge_result_excel_report(new object());
                //Thread t = new Thread(thread_save_gauge_result_excel_report);
                //t.Start();
            }

            m_bShowSmallSelectionFrame = false;
            m_bShowFrameDuringTaskCreation = false;
            m_bShowCoarseMark = false;
            m_bShowAccurateMark = false;

            m_bIsRunningTask = false;

            m_gauger.clear_gauger_state();

            // 设置单位
            send_message("设置单位", false, m_nUnitTypeBeforeRunningTask, null);

            send_message("EnableButton(bool)", false, btn_RunTask, true);

            // 关绿灯，开黄灯
            m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_HIGH);
            m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_LOW);
        }

        // 线程：执行手动创建的测量任务
        public void thread_run_handmade_task(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            Debugger.Log(0, null, string.Format("222222 执行手动创建的测量任务"));

            m_nUnitTypeBeforeRunningTask = m_nUnitType;

            double dbHeightDiffSum = 0;
            double dbHeightCompensation = 0;

            #region
            if (true == m_bOfflineMode)
            {
                for (int n = 0; n < m_current_task_data.Count; n++)
                {
                    m_nCurrentMeasureItemIdx = n;

                    // 设置测量类型
                    send_message("设置测量类型", false, m_current_task_data[n].m_mes_type, null);

                    // 设置单位
                    send_message("设置单位", false, m_current_task_data[n].m_unit, null);

                    send_message(string.Format("准备测量 第{0}个点", n + 1), true, m_current_task_data[n], n);
                    
                    Thread.Sleep(100);
                    //if (m_current_task_data[n].m_mes_type == MEASURE_TYPE.LINE_WIDTH_14)
                        //Thread.Sleep(5000);
                    
                    bool bIsGaugeOK = send_message(string.Format("执行测量 第{0}个点", n + 1), true, m_current_task_data[n], n);

                    if (false == bIsGaugeOK)
                    {
                        switch (m_current_task_data[n].m_mes_type)
                        {
                            case MEASURE_TYPE.HAND_PICK_LINE:
                            case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                m_event_wait_for_manual_gauge.Reset();

                                send_message(string.Format("请手动操作"), false, m_current_task_data[n], n);
                                m_event_wait_for_manual_gauge.WaitOne();
                                break;
                        }
                    }

                    //Debugger.Log(0, null, string.Format("222222 n {0}: m_theory_machine_crd = [{1:0.000},{2:0.000}]", n,
                    //    m_current_task_data[n].m_theory_machine_crd.x, m_current_task_data[n].m_theory_machine_crd.y));

                    Thread.Sleep(1000);
                }

                // 保存报表
                if (true == m_bSaveGaugeResultExcelReport)
                {
                    send_message("保存报表", true, null, null);
                    thread_save_gauge_result_excel_report(new object());
                    //Thread t = new Thread(thread_save_gauge_result_excel_report);
                    //t.Start();
                }

                m_bIsRunningTask = false;

                // 设置单位
                send_message("设置单位", false, m_nUnitTypeBeforeRunningTask, null);

                send_message("EnableButton(bool)", false, btn_RunTask, true);
                return;
            }
            #endregion

            // 开绿灯，关黄灯
            m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_LOW);
            m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_HIGH);

            Thread.Sleep(m_nMeasureTaskDelayTime);

            for (int n = 0; n < m_current_task_data.Count; n++)
                m_current_task_data[n].m_real_machine_crd = m_current_task_data[n].m_theory_machine_crd;

            #region
            while (true)
            {
                for (int n = 0; n < m_current_task_data.Count; n++)
                {
                    Debugger.Log(0, null, string.Format("222222 n {0}: crd = [{1:0.000},{2:0.000},{3:0.000}]", n,
                        m_current_task_data[n].m_theory_machine_crd.x, m_current_task_data[n].m_theory_machine_crd.y, m_current_task_data[n].m_theory_machine_crd.z));

                    feed_dog();

                    if (m_bStopTask || m_bEmergencyExit)
                        break;

                    while (true == m_bPause)
                    {
                        feed_dog();
                        if (m_bStopTask || m_bEmergencyExit)
                            break;
                        if (m_bExitProgram)
                            return;
                        Thread.Sleep(50);
                    }

                    m_bShowSmallSelectionFrame = false;
                    m_bShowFrameDuringTaskCreation = false;
                    m_bShowCoarseMark = false;
                    m_bShowAccurateMark = false;

                    m_nCurrentMeasureItemIdx = n;

                    MeasurePointData data = m_current_task_data[n];

                    if ((true == m_bIsAddingNewItemsToExistingTask) && (n >= 3))
                    {
                        m_bIsCreatingTask = true;

                        m_list_measure_results.Clear();
                        m_measure_items_on_graph.Clear();
                        gridview_GraphMeasureItems.Rows.Clear();

                        //btn_LoadTask.Enabled = false;
                        //btn_UpdateTask.Enabled = false;

                        m_nCreateTaskMode = 1;

                        MessageBox.Show(this, "已经完成三个定位孔的位置确认。现在请手动移动相机至要测量的位置，添加新测量项。", "提示");

                        return;
                    }

                    // 设置倍率
                    send_message("设置倍率", false, data.m_len_ratio, null);

                    // 设置单位
                    send_message("设置单位", false, data.m_unit, null);

                    // 设置光源和亮度
                    send_message("设置光源和亮度", false, data, null);

                    // 设置测量类型
                    send_message("设置测量类型", false, data.m_mes_type, null);

                    if (false == m_bIsAddingNewItemsToExistingTask)
                        send_message(string.Format("准备测量 第{0}个点", n + 1), false, m_current_task_data[n], n);

                    // 移动到记录位置
                    Point3d pt = data.m_theory_machine_crd;
                    if (m_nCurrentMeasureItemIdx >= 3)
                        pt = data.m_real_machine_crd;
                    if ((0 == n) && (true == m_bUseHeightSensor) && (true == m_bUseAutofocusWhenRunningTask))
                        m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, m_dbStageTriggerHeight + 15, false);
                    else
                    {
                        if (data.m_len_ratio > 0)
                        {
                            pt.x += m_len_ratios_offsets[data.m_len_ratio].x;
                            pt.y += m_len_ratios_offsets[data.m_len_ratio].y;
                        }

                        if (n > 0)
                            dbHeightCompensation = dbHeightDiffSum / (double)n;

                        m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, pt.z + dbHeightCompensation, false);
                    }

                    //m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, pt.z, 200, 0.5, false);

                    // 通过高度传感器到达清晰平面
                    if (0 == n)
                    {
                        if (true == m_bUseHeightSensor)
                        {
                            double vel = m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_long_range;

                            m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, m_dbStageTriggerHeight + 15, vel);
                            Debugger.Log(0, null, string.Format("222222 n {0}: m_dbStageTriggerHeight = {1:0.000}, {2}", n, m_dbStageTriggerHeight + 15, m_IO.is_height_sensor_activated()));
                            if (true == m_IO.is_height_sensor_activated())
                            {
                                m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, m_dbStageTriggerHeight - 2, 7);
                                Debugger.Log(0, null, string.Format("222222 n {0}: m_dbStageTriggerHeight = {1:0.000}", n, m_dbStageTriggerHeight + 15));
                                for (int m = 0; m < 1000; m++)
                                {
                                    if (false == m_IO.is_height_sensor_activated())
                                    {
                                        Point3d crd = new Point3d(0, 0, 0);
                                        m_motion.get_xyz_crds(ref crd);

                                        m_dbClearPlanePosZ = crd.z - m_dbStageHeightGap;

                                        Thread.Sleep(150);
                                        m_motion.stop_axis(MotionOps.AXIS_Z);
                                        Thread.Sleep(150);

                                        //double offset = m_dbClearPlanePosZ - m_current_task_data[0].m_real_machine_crd.z;
                                        //for (int k = 1; k < m_current_task_data.Count; k++)
                                        //{
                                        //    m_current_task_data[k].m_real_machine_crd.z += offset;
                                        //    if (m_current_task_data[k].m_real_machine_crd.z <= m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit)
                                        //        m_current_task_data[k].m_real_machine_crd.z = m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit + 0.2;
                                        //}

                                        break;
                                    }

                                    Thread.Sleep(5);
                                }
                            }
                            Debugger.Log(0, null, string.Format("222222 n {0}: 222 m_dbStageTriggerHeight = {1:0.000}", n, m_dbStageTriggerHeight + 15));
                            if (true == m_IO.is_height_sensor_activated())
                            {
                                MessageBox.Show(this, "传感器触发高度设置有问题，可能触发高度过大或过小，请检查。", "提示");

                                m_bIsMeasuringDuringCreation = false;
                                return;
                            }
                            else
                            {
                                vel = m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_short_range;
                                m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, m_dbClearPlanePosZ, vel);

                                //m_bIsMeasuringDuringCreation = false;
                                //return;
                            }
                        }
                    }

                    Thread.Sleep(200);

                    // 确认已经完成变倍
                    for (int k = 0; k < 500; k++)
                    {
                        if (false == m_len.m_bLenIsChangingRatio)
                            break;
                        Thread.Sleep(10);
                    }

                    //Debugger.Log(0, null, string.Format("111111 222 data.m_sharpness_at_creation = {0:0.000}", data.m_sharpness_at_creation));

                    // 自动对焦
                    if (true == m_bUseAutofocusWhenRunningTask)
                    {
                        if (true == m_bSelectivelySkipAutofocus)
                        {
                            double sharpness = 0;
                            if (true == Gaugers.ImgOperators.get_image_sharpness(m_main_camera.m_pImageBuf,
                                m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                            {
                                double ratio = (sharpness / data.m_sharpness_at_creation) * (double)100;
                                if (ratio >= m_nThresForSkippingAutofocus)
                                    ;
                                else
                                    m_image_operator.thread_auto_focus(new object());
                            }
                            else
                                m_image_operator.thread_auto_focus(new object());
                        }
                        else
                            m_image_operator.thread_auto_focus(new object());
                    }

                    if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != data.m_mes_type)
                    {
                    if (m_dbDelaySecondsBeforeMeasure > 0)
                            Thread.Sleep((int)(double)(m_dbDelaySecondsBeforeMeasure * 1000));
                    }

                    Point3d measure_crd = m_current_task_data[n].m_real_machine_crd;
                    m_motion.get_xyz_crds(ref measure_crd);
                    //Debugger.Log(0, null, string.Format("222222 diff = {0:0.000}, {1:0.000}, {2:0.000}", 
                    //    measure_crd.z - m_current_task_data[n].m_real_machine_crd.z, m_current_task_data[n].m_real_machine_crd.z, measure_crd.z));
                    dbHeightDiffSum += measure_crd.z - m_current_task_data[n].m_theory_machine_crd.z;
                    m_current_task_data[n].m_real_machine_crd.z = measure_crd.z;

                    // 测量
                    bool bIsGaugeOK = false;
                    switch (data.m_mes_type)
                    {
                        // 定位孔
                        case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                            if (true == data.m_bAutoFindMarkCircleInGuideCam)
                            {
                                // 先在导航图像中寻找定位孔，定位效果较粗糙
                                Point2d center = new Point2d(0, 0);
                                Point2d offset = new Point2d(0, 0);
                                if (true == m_image_operator.find_circle_in_cam(m_guide_camera, m_guide_cam_lock,
                                    m_guide_camera.m_pixels_per_um, data.m_metric_radius[0], ref center))
                                {
                                    offset.x = center.x - (double)(m_guide_camera.m_nCamWidth / 2);
                                    offset.y = center.y - (double)(m_guide_camera.m_nCamHeight / 2);

                                    offset.x = (offset.x / m_guide_camera.m_pixels_per_um) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                    offset.y = (offset.y / m_guide_camera.m_pixels_per_um) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;
                                    //Debugger.Log(0, null, string.Format("222222 n {0}: 定位孔 222 offset = [{1:0.000},{2:0.000}]", n, offset.x, offset.y));

                                    Point3d current_crd = new Point3d(0, 0, 0);
                                    m_motion.get_xyz_crds(ref current_crd);
                                    m_motion.linear_XYZ_wait_until_stop(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z, 50, 0.25, false);

                                    if (m_dbDelaySecondsBeforeMeasure > 0)
                                        Thread.Sleep((int)(double)(m_dbDelaySecondsBeforeMeasure * 1000));

                                    // 在导航图像上高亮显示通过粗定位找到的定位孔
                                    m_dbCoarseMarkRadius = data.m_metric_radius[0];
                                    m_bShowCoarseMark = true;

                                    // 然后在主图像中进一步寻找定位孔，以提高定位精度
                                    if (true)
                                    {
                                        bIsGaugeOK = send_message(string.Format("执行测量 第{0}个 定位孔", n + 1), true, m_current_task_data[n], n);
                                        if (true == bIsGaugeOK)
                                        {
                                            center = m_gauger.m_gauged_circle_center;
                                            offset.x = center.x - (double)(m_main_camera.m_nCamWidth / 2);
                                            offset.y = center.y - (double)(m_main_camera.m_nCamHeight / 2);

                                            offset.x = (offset.x / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                            offset.y = (offset.y / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                            current_crd = new Point3d(0, 0, 0);
                                            m_motion.get_xyz_crds(ref current_crd);
                                            current_crd.x += offset.x;
                                            current_crd.y += offset.y;

                                            data.m_real_machine_crd = current_crd;
                                        }
                                    }
                                }
                            }
                            break;

                        // BGA
                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                            bIsGaugeOK = send_message(string.Format("执行测量 第{0}个 位置", n - 3 + 1), true, m_current_task_data[n], n);
                            if (true == bIsGaugeOK)
                            {

                            }
                            break;

                        // 线宽
                        case MEASURE_TYPE.LINE_WIDTH_14:
                        case MEASURE_TYPE.LINE_WIDTH_23:
                        case MEASURE_TYPE.LINE_WIDTH_13:
                        case MEASURE_TYPE.LINE_WIDTH_1234:
                        case MEASURE_TYPE.LINE_SPACE:
                        case MEASURE_TYPE.ARC_LINE_SPACE:
                        case MEASURE_TYPE.LINE:
                        case MEASURE_TYPE.ARC_LINE_WIDTH:
                            bIsGaugeOK = send_message(string.Format("执行测量 第{0}个 位置", n - 3 + 1), true, m_current_task_data[n], n);
                            if (true == bIsGaugeOK)
                            {

                            }
                            break;

                        default:
                            break;
                    }

                    // 测量失败，需要用户进行手动测量
                    if (false == bIsGaugeOK)
                    {
                        switch (data.m_mes_type)
                        {
                            case MEASURE_TYPE.HAND_PICK_LINE:
                            case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                m_event_wait_for_manual_gauge.Reset();

                                send_message(string.Format("请手动操作"), false, m_current_task_data[n], n);
                                m_event_wait_for_manual_gauge.WaitOne();
                                break;

                            default:
                                m_event_wait_for_manual_gauge.Reset();

                                send_message(string.Format("第{0}个 位置 需要用户进行手动测量", n - 3 + 1), true, m_current_task_data[n], n);
                                m_event_wait_for_manual_gauge.WaitOne();
                                break;
                        }
                    }
                    else
                    {
                        // 判断是否需要确认定位结果
                        if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == data.m_mes_type)
                        {
                            if (true == m_bNeedConfirmFiducialMark)
                            {
                                m_event_wait_for_confirm_during_autorun.Reset();

                                send_message("等待用户确认定位孔", false, null, null);
                                m_event_wait_for_confirm_during_autorun.WaitOne();
                            }
                            else
                                Thread.Sleep(300);
                        }
                        else
                        {
                            if (true == m_bNeedConfirmMeasureResult)
                            {
                                m_event_wait_for_confirm_during_autorun.Reset();

                                send_message("等待用户确认测量结果", false, null, null);
                                m_event_wait_for_confirm_during_autorun.WaitOne();
                            }
                            else
                            {
                                m_bTriggerSaveGaugeImage = true;

                                Thread.Sleep(300);
                            }
                        }
                    }

                    // 三点仿射变换
                    if (2 == n)
                    {
                        int[] out_ints = new int[10];
                        double[] in_theory_crds = new double[10];
                        double[] in_real_crds = new double[10];

                        m_triangle_trans_matrix = new double[10];

                        for (int k = 0; k < 3; k++)
                        {
                            in_theory_crds[k * 2] = m_current_task_data[k].m_theory_machine_crd.x;
                            in_theory_crds[k * 2 + 1] = m_current_task_data[k].m_theory_machine_crd.y;
                            in_real_crds[k * 2] = m_current_task_data[k].m_real_machine_crd.x;
                            in_real_crds[k * 2 + 1] = m_current_task_data[k].m_real_machine_crd.y;
                        }

                        out_ints[0] = 0;
                        get_transform_matrix(out_ints, in_theory_crds, in_real_crds, m_triangle_trans_matrix);
                        if (1 == out_ints[0])
                        {
                            for (int k = 3; k < m_current_task_data.Count; k++)
                            {
                                MeasurePointData item = m_current_task_data[k];

                                Point3d item_crd = new Point3d(item.m_real_machine_crd.x, item.m_real_machine_crd.y, item.m_real_machine_crd.z);

                                item.m_real_machine_crd.x = m_triangle_trans_matrix[0] * item_crd.x + m_triangle_trans_matrix[1] * item_crd.y + m_triangle_trans_matrix[2];
                                item.m_real_machine_crd.y = m_triangle_trans_matrix[3] * item_crd.x + m_triangle_trans_matrix[4] * item_crd.y + m_triangle_trans_matrix[5];
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    //Thread.Sleep(600);
                }

                if (m_bStopTask || m_bEmergencyExit)
                    break;

                if (false == m_bRepeatRunningTask)
                    break;
                else
                {
                    m_nNumOfTaskRepetition--;

                    send_message("设置文本框内容", false, textBox_NumOfTaskRepetitions, string.Format("{0}", m_nNumOfTaskRepetition));

                    if (m_nNumOfTaskRepetition <= 0)
                        break;
                }
            }
            #endregion

            // 关闭吸附
            //if (false == m_bOfflineMode)
            //    send_message("关闭吸附", true, null, null);

            bool bOK = m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, 0, 20);
            Thread.Sleep(200);
            bOK = m_motion.linear_XY_no_wait(0, 0, 500, 1, false);

            // 保存报表
            if (true == m_bSaveGaugeResultExcelReport)
            {
                send_message("保存报表", true, null, null);
                thread_save_gauge_result_excel_report(new object());
                //Thread t = new Thread(thread_save_gauge_result_excel_report);
                //t.Start();
            }

            m_bShowSmallSelectionFrame = false;
            m_bShowFrameDuringTaskCreation = false;
            m_bShowCoarseMark = false;
            m_bShowAccurateMark = false;

            m_bIsRunningTask = false;

            m_gauger.clear_gauger_state();

            // 设置单位
            send_message("设置单位", false, m_nUnitTypeBeforeRunningTask, null);

            send_message("EnableButton(bool)", false, btn_RunTask, true);

            // 关绿灯，开黄灯
            m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_HIGH);
            m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_LOW);
        }
        
        // 线程：测量结束后，双击测量结果表格上某个测量项，相机自动移动到该测量点进行测量，相当于复现该点的测量，便于观察
        public void thread_measure_an_item_once_more(object obj)
        {
            dl_message_sender send_message = CBD_SendMessage;

            if (true == m_bThreadIsRunning_MeasureOnceMore)
                return;
            m_bThreadIsRunning_MeasureOnceMore = true;

            int idx = (int)obj;
            MeasurePointData data = m_current_task_data[idx];

            // 设置倍率
            send_message("设置倍率", false, data.m_len_ratio, null);

            // 设置单位
            send_message("设置单位", false, data.m_unit, null);

            // 设置光源和亮度
            send_message("设置光源和亮度", false, data, null);

            // 设置测量类型
            send_message("设置测量类型", false, data.m_mes_type, null);

            //m_motion.m_threaded_move_dest_X = data.m_real_machine_crd.x;
            //m_motion.m_threaded_move_dest_Y = data.m_real_machine_crd.y;
            //m_motion.m_threaded_move_dest_Z = data.m_real_machine_crd.z;
            //dl_message_sender messenger = CBD_SendMessage;
            //(new Thread(m_motion.threaded_linear_XYZ_wait_until_stop)).Start(messenger);

            Point3d pt = data.m_theory_machine_crd;
            if (idx >= 3)
                pt = data.m_real_machine_crd;
            if (data.m_len_ratio > 0)
            {
                pt.x += m_len_ratios_offsets[data.m_len_ratio].x;
                pt.y += m_len_ratios_offsets[data.m_len_ratio].y;
            }
            m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, pt.z, false);
            
            Thread.Sleep(150);

            // 确认已经完成变倍
            for (int k = 0; k < 500; k++)
            {
                if (false == m_len.m_bLenIsChangingRatio)
                    break;
                Thread.Sleep(10);
            }

            // 自动对焦
            bool bNeedFocus = true;
            if (true == m_bSelectivelySkipAutofocus)
            {
                double sharpness = 0;
                if (true == Gaugers.ImgOperators.get_image_sharpness(m_main_camera.m_pImageBuf,
                    m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                {
                    double ratio = (sharpness / data.m_sharpness_at_creation) * (double)100;
                    if (ratio >= m_nThresForSkippingAutofocus)
                        bNeedFocus = false;
                }
            }
            if (true == bNeedFocus)
            {
                switch (data.m_mes_type)
                {
                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.LINE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        m_image_operator.m_bFastFocus = true;
                        m_image_operator.m_mes_data = data;
                        m_image_operator.thread_auto_focus(new object());
                        m_image_operator.m_bFastFocus = false;
                        Thread.Sleep(200);
                        break;

                    default:
                        m_image_operator.thread_auto_focus(new object());
                        break;
                }
            }
            else
                Thread.Sleep(200);

            if (m_dbDelaySecondsBeforeMeasure > 0)
                Thread.Sleep((int)(m_dbDelaySecondsBeforeMeasure * 1000));

            m_nCurrentMeasureItemIdx = idx;
            switch (data.m_mes_type)
            {
                // BGA
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                    send_message(string.Format("执行测量 第{0}个 位置", idx - 3 + 1), false, m_current_task_data[idx], idx);
                    break;

                // 线宽
                case MEASURE_TYPE.LINE_WIDTH_14:
                case MEASURE_TYPE.LINE_WIDTH_23:
                case MEASURE_TYPE.LINE_WIDTH_13:
                case MEASURE_TYPE.LINE_WIDTH_1234:
                case MEASURE_TYPE.LINE_SPACE:
                case MEASURE_TYPE.ARC_LINE_SPACE:
                case MEASURE_TYPE.LINE:
                case MEASURE_TYPE.ARC_LINE_WIDTH:
                    send_message(string.Format("执行测量 第{0}个 位置", idx - 3 + 1), false, m_current_task_data[idx], idx);
                    break;
            }

            m_bThreadIsRunning_MeasureOnceMore = false;
        }

        // 线程：双击测量任务表格上某个测量项，相机自动移动到该测量点，便于观察
        public void thread_measure_an_item_once_more2(object obj)
        {
            dl_message_sender send_message = CBD_SendMessage;

            if (true == m_bThreadIsRunning_MeasureOnceMore)
                return;
            m_bThreadIsRunning_MeasureOnceMore = true;

            int idx = (int)obj;
            MeasurePointData data = m_current_task_data[idx];

            // 设置倍率
            send_message("设置倍率", false, data.m_len_ratio, null);

            // 设置单位
            send_message("设置单位", false, data.m_unit, null);

            // 设置光源和亮度
            send_message("设置光源和亮度", false, data, null);

            // 设置测量类型
            send_message("设置测量类型", false, data.m_mes_type, null);

            //m_motion.m_threaded_move_dest_X = data.m_real_machine_crd.x;
            //m_motion.m_threaded_move_dest_Y = data.m_real_machine_crd.y;
            //m_motion.m_threaded_move_dest_Z = data.m_real_machine_crd.z;
            //dl_message_sender messenger = CBD_SendMessage;
            //(new Thread(m_motion.threaded_linear_XYZ_wait_until_stop)).Start(messenger);

            Point3d pt = data.m_theory_machine_crd;
            if (idx >= 3)
                pt = data.m_real_machine_crd;
            if (data.m_len_ratio > 0)
            {
                pt.x += m_len_ratios_offsets[data.m_len_ratio].x;
                pt.y += m_len_ratios_offsets[data.m_len_ratio].y;
            }
            m_motion.linear_XYZ_wait_until_stop(pt.x, pt.y, pt.z, false);
            m_motion.linear_XYZ_wait_until_stop(data.m_real_machine_crd.x, data.m_real_machine_crd.y, data.m_real_machine_crd.z, false);

            Thread.Sleep(150);

            // 确认已经完成变倍
            for (int k = 0; k < 500; k++)
            {
                if (false == m_len.m_bLenIsChangingRatio)
                    break;
                Thread.Sleep(10);
            }

            // 自动对焦
            bool bNeedFocus = true;
            if (true == m_bSelectivelySkipAutofocus)
            {
                double sharpness = 0;
                if (true == Gaugers.ImgOperators.get_image_sharpness(m_main_camera.m_pImageBuf,
                    m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                {
                    double ratio = (sharpness / data.m_sharpness_at_creation) * (double)100;
                    if (ratio >= m_nThresForSkippingAutofocus)
                        bNeedFocus = false;
                }
            }
            if (true == bNeedFocus)
            {
                switch (data.m_mes_type)
                {
                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.LINE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        m_image_operator.m_bFastFocus = true;
                        m_image_operator.m_mes_data = data;
                        m_image_operator.thread_auto_focus(new object());
                        m_image_operator.m_bFastFocus = false;
                        Thread.Sleep(200);
                        break;

                    default:
                        m_image_operator.thread_auto_focus(new object());
                        break;
                }
            }
            else
                Thread.Sleep(200);

            //m_nCurrentMeasureItemIdx = idx;
            //switch (data.m_mes_type)
            //{
            //    // BGA
            //    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
            //        send_message(string.Format("执行测量 第{0}个 位置", idx - 3 + 1), false, m_current_task_data[idx], idx);
            //        break;

            //    // 线宽
            //    case MEASURE_TYPE.LINE_WIDTH_14:
            //    case MEASURE_TYPE.LINE_WIDTH_23:
            //    case MEASURE_TYPE.LINE_WIDTH_13:
            //    case MEASURE_TYPE.LINE_WIDTH_1234:
            //    case MEASURE_TYPE.LINE_SPACE:
            //        send_message(string.Format("执行测量 第{0}个 位置", idx - 3 + 1), false, m_current_task_data[idx], idx);
            //        break;
            //}

            m_bThreadIsRunning_MeasureOnceMore = false;
        }

        // 线程：监控获取图纸读取进度
        public static void thread_monitor_graph_reading_progress(object obj)
        {
            while (true)
            {
                MainUI.m_reset_event_for_updating_graphview_progress.WaitOne();

                if (MainUI.m_bExitProgram)
                    break;

                //string msg = string.Format("222222 渲染进度 {0}", Form_GraphOrientation.m_nThumbnailProgress);
                //Debugger.Log(0, null, msg);

                dl_message_sender send_message = obj as dl_message_sender;
                send_message("图纸读取进度", false, null, null);
            }
        }

        // 线程：读取ODB大图
        public void thread_get_ODB_big_image(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = true;

            string msg = string.Format("222222 开始读取ODB大图");
            Debugger.Log(0, null, msg);

            int   nOrientation = m_nGraphOrientation;

            char[] path = m_strGraphFilePath.ToCharArray();
            int[] pRetInts = new int[10];
            double[] pRetDoubles = new double[10];
            pRetInts[0] = 0;

            m_ODB_measure_items.Clear();

            dllapi_render_ODB_big_image(path, nOrientation, MainUI.m_nGraphZoomRatio, pRetInts, pRetDoubles);
            if (1 == pRetInts[0])
            {
                int nWidth = pRetInts[1];
                int nHeight = pRetInts[2];
                int nBytesPerLine = pRetInts[3];
                int nBytesPerLine2 = pRetInts[4];
                m_nGraphWidth = pRetInts[1];
                m_nGraphHeight = pRetInts[2];
                m_nGraphOffsetX = pRetInts[5];
                m_nGraphOffsetY = pRetInts[6];
                m_pixels_per_mm = pRetDoubles[0];
                
                Debugger.Log(0, null, string.Format("222222 图纸宽高 = [{0},{1}], 每行字节数 = {2}", nWidth, nHeight, nBytesPerLine));

                // 获取阵列信息
                rotated_array_rect.m_nSelectCount = 0;
                Form_GraphOrientation.get_ODB_array_info(GraphView.m_ODB_thumbnail_array_rects);

                // 获取ODB测量项信息
                if (true)
                {
                    int[] pRetInts2 = new int[300000];
                    double[] pRetDoubles2 = new double[500000];
                    pRetInts2[0] = 0;

                    dllapi_get_ODB_measure_items(pRetInts2, pRetDoubles2);
                    if (1 == pRetInts2[0])
                    {
                        int nItemsCount = pRetInts2[1];
                        
                        for (int n = 0; n < nItemsCount; n++)
                        {
                            ODBMeasureItem item = new ODBMeasureItem(0);

                            item.nMeasureType = (MEASURE_TYPE)(pRetInts2[n + 2]);
                            item.dbGraphCrdX = pRetDoubles2[n * 5];
                            item.dbGraphCrdY = pRetDoubles2[n * 5 + 1];
                            item.dbStandardValue = pRetDoubles2[n * 5 + 2];
                            item.dbUpper = pRetDoubles2[n * 5 + 3];
                            item.dbLower = pRetDoubles2[n * 5 + 4];

                            Debugger.Log(0, null, string.Format("222222 n {0}: item crd [{1:0.000},{2:0.000}], type = {3}, dbStandardValue = {4:0.000}", 
                                n, item.dbGraphCrdX, item.dbGraphCrdY, item.nMeasureType, item.dbStandardValue));

                            m_ODB_measure_items.Add(item);
                        }
                    }
                }

                // 拷贝图纸图像
                if (true)
                {
                    byte[] buf = new byte[nBytesPerLine * nHeight];

                    if (GraphView.m_ODB_thumbnail_array_rects.Count > 0)
                        m_graph_view.m_bDrawArrayRects = true;
                    else
                        m_graph_view.m_bDrawArrayRects = false;

                    dllapi_copy_ODB_big_image(buf, nBytesPerLine, nWidth, nHeight);

                    m_graph_view.m_bitmap_1bit = new Bitmap(nWidth, nHeight, PixelFormat.Format1bppIndexed);

                    MemoryStream stream = new MemoryStream(buf);
                    
                    Rectangle rect = new Rectangle(new Point(0, 0), m_graph_view.m_bitmap_1bit.Size);
                    BitmapData bmp_data = m_graph_view.m_bitmap_1bit.LockBits(rect, ImageLockMode.WriteOnly, m_graph_view.m_bitmap_1bit.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(buf, 0, bmp_data.Scan0, buf.Length);
                    m_graph_view.m_bitmap_1bit.UnlockBits(bmp_data);

                    ColorPalette palette = m_graph_view.m_bitmap_1bit.Palette;
                    Color[] colors = palette.Entries;
                    colors[0] = Color.FromArgb(255, 0, 0, 0);
                    colors[1] = Color.FromArgb(255, 0, 255, 0);

                    m_graph_view.m_bitmap_1bit.Palette = palette;

                    stream.Close();
                }
                
                Debugger.Log(0, null, string.Format("222222 m_graph_view 大图拷贝完成"));

                m_graph_view.m_bRequireRedraw32Bitmap = true;

                m_nGraphType = 2;
                
                send_message("从dll得到GraphView大图", false, null, null);
            }

            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = false;
            MainUI.m_reset_event_for_updating_graphview_progress.Set();
        }

        // 线程：读取gerber大图
        public static void thread_get_gerber_big_image(object obj)
        {
            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = true;
            
            Debugger.Log(0, null, string.Format("222222 开始读取图纸"));

            int nOrientation = m_nGraphOrientation;

            char[] path = m_strGraphFilePath.ToCharArray();
            int[] out_ints = new int[10];
            double[] out_doubles = new double[10];

            out_ints[0] = 0;
            dllapi_render_gerber_big_image(path, nOrientation, MainUI.m_nGraphZoomRatio, out_ints, out_doubles);
            if (1 == out_ints[0])
            {
                m_pixels_per_mm = out_doubles[0];
                m_nGraphWidth = out_ints[1];
                m_nGraphHeight = out_ints[2];
                m_nGraphOffsetX = out_ints[3];
                m_nGraphOffsetY = out_ints[4];
                m_nBytesPerLine = out_ints[5];
                
                Debugger.Log(0, null, string.Format("222222 图纸宽高 = [{0},{1}], 每行字节数 = {2}, m_pixels_per_mm = {3:0.000}", 
                    m_nGraphWidth, m_nGraphHeight, m_nBytesPerLine, m_pixels_per_mm));

                // 拷贝图纸图像
                if (true)
                {
                    byte[] buf = new byte[m_nBytesPerLine * m_nGraphHeight];
                    
                    copy_graph_view(buf, m_nBytesPerLine, m_nGraphWidth, m_nGraphHeight);
                    
                    m_graph_view.m_bitmap_1bit = new Bitmap(m_nGraphWidth, m_nGraphHeight, PixelFormat.Format1bppIndexed);

                    MemoryStream stream = new MemoryStream(buf);
                    
                    Rectangle rect = new Rectangle(new Point(0, 0), m_graph_view.m_bitmap_1bit.Size);
                    BitmapData bmp_data = m_graph_view.m_bitmap_1bit.LockBits(rect, ImageLockMode.WriteOnly, m_graph_view.m_bitmap_1bit.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(buf, 0, bmp_data.Scan0, buf.Length);
                    m_graph_view.m_bitmap_1bit.UnlockBits(bmp_data);
                    
                    //m_graph_view.m_bitmap_1bit.RotateFlip(RotateFlipType.Rotate180FlipX);            // 容易崩溃

                    ColorPalette palette = m_graph_view.m_bitmap_1bit.Palette;
                    Color[] colors = palette.Entries;
                    colors[0] = Color.FromArgb(255, 0, 0, 0);
                    colors[1] = Color.FromArgb(255, 0, 255, 0);

                    m_graph_view.m_bitmap_1bit.Palette = palette;

                    stream.Close();
                }
                
                Debugger.Log(0, null, string.Format("222222 m_graph_view 大图拷贝完成"));

                m_graph_view.m_bRequireRedraw32Bitmap = true;

                m_nGraphType = 1;

                dl_message_sender send_message = obj as dl_message_sender;
                send_message("从dll得到GraphView大图", false, null, null);
            }

            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = false;
            MainUI.m_reset_event_for_updating_graphview_progress.Set();
        }

        // 连接 sql 数据库
        void thread_connect_to_SQL(object obj)
        {
            //Debugger.Log(0, null, string.Format("222222 开始连接数据库"));

            dl_message_sender send_message = obj as dl_message_sender;

            try
            {
                string str = "Data Source=" + Form_Database.m_strDataSource + ";Initial Catalog=" + Form_Database.m_strDatabaseTask + ";UID="
                    + Form_Database.m_strSQLUser + ";PWD=" + Form_Database.m_strSQLPwd + ";Connect Timeout=3";
                m_SQL_conn_measure_task = new SqlConnection(str);
                m_SQL_conn_measure_task.Open();

                //Debugger.Log(0, null, string.Format("222222 数据库连接成功"));

                m_bIsSQLConnected = true;
                
                get_task_tables_from_database(m_vec_SQL_table_names);

                Thread.Sleep(2000);
                send_message("数据库连接成功", true, null, null);
            }
            catch (Exception e)
            {
                Debugger.Log(0, null, string.Format("222222 数据库连接异常: {0}", e.Message));

                send_message("数据库连接异常!", true, null, null);

                MessageBox.Show(this, e.Message, "数据库连接异常");

                return;
            }
        }

        // 线程：变倍镜头初始化
        void thread_init_len(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            if (false == m_bOfflineMode)
            {
                string name = string.Format("COM{0}", m_len.m_nComIndex + 1);

                Debugger.Log(0, null, string.Format("222222 初始化变倍镜头 name = {0}", name));
                Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 111"));
                bool bSuccess = m_len.init_port(name, 9600, 8, Parity.None, StopBits.One);
                if (false == bSuccess)
                {
                    Debugger.Log(0, null, string.Format("222222 变倍镜头初始化失败"));
                    send_message("变倍镜头初始化失败", false, null, null);
                    return;
                }
                Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 222"));
                // 镜头电机初始化
                bSuccess = m_len.init();
                Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 333"));
                if (true == bSuccess)
                    send_message("变倍镜头初始化成功", false, null, null);
                else
                    send_message("变倍镜头初始化失败", false, null, null);
            }
        }

        // 线程：硬件初始化
        void thread_init_hardwares(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            // 启动运动系统初始化线程
                (new Thread(thread_init_motion)).Start(obj);

            // 初始化光源串口
            if (true)
            {
                m_top_light.load_params(m_strTopLightFile);
                m_bottom_light.load_params(m_strBottomLightFile);
                
                string strPort = string.Format("COM{0}", m_top_light.m_nComIndex + 1);

                Debugger.Log(0, null, string.Format("222222 初始化光源串口 strPort = {0}", strPort));

                bool bSuccess = m_top_light.init_port(strPort, 19200, 8, Parity.None, StopBits.One);

                if (true == bSuccess)
                {
                    if (true == m_top_light.m_bOn)
                        m_top_light.open_light();
                    else
                        m_top_light.close_light();
                    if (true == m_bottom_light.m_bOn)
                        m_bottom_light.open_light();
                    else
                        m_bottom_light.close_light();

                    send_message("光源初始化成功", false, null, null);
                }
                else
                {
                    send_message("光源初始化失败", false, null, null);
                    m_event_suspend_hardware_init.WaitOne();
                }
                //Debugger.Log(0, null, string.Format("222222 初始化光源串口 222"));
            }

            // 初始化相机
            if (true)
            {
                if (MainUI.m_nCameraType == 0)//Pomeas
                {
                    m_main_camera.m_red = 2;
                    m_main_camera.m_blue = 1;
                    m_main_camera.m_green = 2;
                    m_main_camera.m_gain = 13;
                    m_guide_camera.m_red = 2;
                    m_guide_camera.m_blue = 1;
                    m_guide_camera.m_green = 2;
                    m_guide_camera.m_gain = 13;
                }
                else if (MainUI.m_nCameraType == 1)//MindVision
                {
                    m_main_camera.m_red = 300;
                    m_main_camera.m_blue = 300;
                    m_main_camera.m_green = 300;
                    m_main_camera.m_gain = 300;
                    m_guide_camera.m_red = 300;
                    m_guide_camera.m_blue = 300;
                    m_guide_camera.m_green = 300;
                    m_guide_camera.m_gain = 300;
                }
                //send_message("导航相机初始化成功", false, null, null);
                
                m_main_camera.load_params(m_strMainCameraFile);
                m_guide_camera.load_params(m_strGuideCameraFile);

                bool bSuccess = true;
                int nCamNum = 0;
                if (0 == m_nCameraType)
                {
                    PMSGigE.PMSInitLib();
                    PMSGigE.PMSUpdateCameraList();

                    // 寻找相机
                    PMSSTATUS result = PMSGigE.PMSGetNumOfCameras(out nCamNum);
                    if (nCamNum == 0)
                    {
                        MessageBox.Show(this, "没有找到相机，请确认相机连接和IP设置是否正确。", "提示");
                        bSuccess = false;
                    }
                }
                else if (1 == m_nCameraType)
                {

                }

                if (true == bSuccess)
                {
                    if (0 == m_nCameraType)
                        bSuccess = m_main_camera.init_PMS(CAMERA_TYPE.MAIN_CAMERA, 0);
                    else if (1 == m_nCameraType)
                        bSuccess = m_main_camera.init_MindVision(CAMERA_TYPE.MAIN_CAMERA, 0);
                }
                
                if (true == bSuccess)
                {
                    send_message("主相机初始化成功", false, null, null);
                    
                    //主相机增益与RGB先后设置
                    if (MainUI.m_nCameraType == 0)//Pomeas
                    {
                        m_main_camera.set_gain(m_main_camera.m_gain);

                    m_main_camera.set_red(m_main_camera.m_red);
                    m_main_camera.set_blue(m_main_camera.m_blue);
                    m_main_camera.set_green(m_main_camera.m_green);
                    }
                    else if (MainUI.m_nCameraType == 1)//MindVision
                    {
                        m_main_camera.MDVS_set_blue((int)m_main_camera.m_blue);
                        m_main_camera.MDVS_set_green((int)m_main_camera.m_green);
                        m_main_camera.MDVS_set_red((int)m_main_camera.m_red);
                        
                        Debugger.Log(0, null, string.Format("222222 RGB初始化相机 {0:0.000} {1:0.000} {2:0.000} {3:0.000}", m_main_camera.m_gain, m_main_camera.m_red, m_main_camera.m_blue, m_main_camera.m_green));
                    }

                    m_main_camera.set_exposure(m_main_camera.m_exposure);
                }
                else
                {
                    send_message("主相机初始化失败", false, null, null);
                    m_event_suspend_hardware_init.WaitOne();
                }
                
                if (true == bSuccess)
                {
                    if (0 == m_nCameraType)
                        bSuccess = m_guide_camera.init_PMS(CAMERA_TYPE.GUIDE_CAMERA, 1);
                    else if (1 == m_nCameraType)
                        bSuccess = m_guide_camera.init_MindVision(CAMERA_TYPE.GUIDE_CAMERA, 1);
                }
                
                if (true == bSuccess)
                {
                    send_message("导航相机初始化成功", false, null, null);

                    //导航增益与RGB先后设置
                    if (MainUI.m_nCameraType == 0)//Pomeas
                    {
                    m_guide_camera.set_red(m_guide_camera.m_red);
                    m_guide_camera.set_blue(m_guide_camera.m_blue);
                    m_guide_camera.set_green(m_guide_camera.m_green);

                    m_guide_camera.set_gain(m_guide_camera.m_gain);
                    }
                    else if (MainUI.m_nCameraType == 1)//MindVision
                    {
                        m_guide_camera.set_red(m_guide_camera.m_red);
                        m_guide_camera.set_blue(m_guide_camera.m_blue);
                        m_guide_camera.set_green(m_guide_camera.m_green);
                    }
                    
                    m_guide_camera.set_exposure(m_guide_camera.m_exposure);
                }
                else
                {
                    send_message("导航相机初始化失败", false, null, null);
                    m_event_suspend_hardware_init.WaitOne();
                }
            }
            
            m_event_wait_for_hardware_init.Set();
        }

        // 线程：初始化运动系统
        void thread_init_motion(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            m_motion.m_bHomed = false;
            m_motion.m_bInitialized = false;
            
            // 初始化运动控制
            m_motion.load_params(m_strMotionParamsPath);
            
            bool bSuccess = m_motion.init();
            if (bSuccess)
            {
                // 初始化红点指示器
                if (false == m_IO.set_IO_output(m_IO.m_output_red_dot, Hardwares.IO_STATE.IO_LOW))
                    MessageBox.Show(this, "红点指示器IO设置失败，请检查原因。", "提示");

                m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_LOW);
            }
            
            if (bSuccess && m_bNeedToInitMotionSystem)
                bSuccess = m_motion.do_home();
            
            if (bSuccess)
                send_message("运动系统初始化成功", false, null, null);
            else
            {
                send_message("运动系统初始化失败", false, null, null);
                m_event_suspend_hardware_init.WaitOne();
            }
        }

        // 线程：XYZ坐标信息实时更新
        public void thread_update_xyz_crd(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            send_message("更新XYZ坐标信息", false, null, null);

            while (true)
            {
                if (true == m_motion.get_xyz_crds(ref m_current_xyz))
                    send_message("更新XYZ坐标信息", false, null, null);

                //IO_STATE state = IO_STATE.NONE;
                //m_IO.get_IO_output_state(m_IO.m_vacuum_io_num, ref state);
                //Debugger.Log(0, null, string.Format("222222 state = {0}", state));
                
                for (int n = 0; n < 2; n++)
                {
                    Thread.Sleep(300);
                    if (m_bExitProgram)
                        return;
                }
            }
        }

        // 线程：IO状态监控
        public void thread_monitor_IO(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            IO_STATE prev_state_of_vacuum_io = IO_STATE.IO_HIGH;
            IO_STATE prev_state_of_height_sensor_io = IO_STATE.IO_HIGH;
            IO_STATE start_button_state = Hardwares.IO_STATE.NONE;
            while (true)
            {
                Thread.Sleep(30);
                if (m_bExitProgram)
                    return;

                if (false == m_motion.m_bInitialized)
                    continue;

                if (true)
                {
                    IO_STATE state = IO_STATE.NONE;
                    m_IO.get_IO_input(m_IO.m_input_vacuum, ref state);

                    if ((IO_STATE.IO_LOW == state) && (state != prev_state_of_vacuum_io))
                    {
                        IO_STATE vacuum_state = IO_STATE.NONE;
                        
                        m_IO.get_IO_output_state(m_IO.m_output_vacuum, ref vacuum_state);
                        if (IO_STATE.IO_HIGH == vacuum_state)
                        {

                            m_IO.set_IO_output(m_IO.m_output_vacuum_button, IO_STATE.IO_LOW);
                            send_message("开启吸附", true, null, null);
                        }
                        else
                        {
                            send_message("关闭吸附", true, null, null);
                            m_IO.set_IO_output(m_IO.m_output_vacuum_button, IO_STATE.IO_HIGH);
                        }
                            
                        //Debugger.Log(0, null, string.Format("222222 发生吸附按钮按下事件"));
                    }
                    prev_state_of_vacuum_io = state;
                }

                if (true)
                {
                    IO_STATE state = IO_STATE.NONE;
                    m_IO.get_IO_input(m_IO.m_input_emergency, ref state);
                    if (IO_STATE.IO_HIGH == state)
                    {
                        Debugger.Log(0, null, string.Format("222222 急停按钮被按下"));

                        m_bEmergencyExit = true;

                        m_motion.stop_all_axes();
                    }
                }

                if (true)
                {
                    m_IO.get_IO_input(m_IO.m_input_start_button, ref start_button_state);//平台运行按钮输入                   

                    //send_message(m_bPause.ToString(), true, null, null);

                    if (Hardwares.IO_STATE.IO_LOW == start_button_state && btn_RunTask.Enabled == true)//按下
                    {
                        //send_message(start_button_state.ToString()+" LOW", true, null, null);
     
                        if (true == m_bPause)
                        {
                            m_bPause = false;
                            btn_RunTask.Enabled = false;
                            btn_Pause.Enabled = true;
                            
                            continue;
                        }

                        if ((false == m_motion.m_bHomed) && (false == m_bOfflineMode))
                        {

                            continue;
                        }
                            
                        if (comboBox_Len.Items.Count <= 0)
                        {
                            MessageBox.Show(this, "变倍镜头初始化尚未完成，请稍等再试。", "提示");

                            continue;
                        }

                        m_gauger.clear_gauger_state();

                        // 检查任务最前面3个测量类型是不是都是定位孔，如果不是，则视为任务无效，不执行
                        if (m_current_task_data.Count < 3)
                        {
                            MessageBox.Show(this, "任务测量项目少于3个，任务无效。", "提示");

                            continue;
                        }
                        for (int n = 0; n < 3; n++)
                        {
                            if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != m_current_task_data[n].m_mes_type)
                            {
                                MessageBox.Show(this, "任务最前面3个测量类型不全是定位孔，任务无效。", "提示");

                                continue;
                            }
                        }

                        m_strTaskRunningStartingTime = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString()
                            + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();

                        m_strTaskRunningStartingDataTime = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString();

                        m_strTaskRunningStartingHMSTime = DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();


                        m_bStopTask = false;
                        m_bPause = false;
                        m_bIsRunningTask = true;

                        btn_RunTask.Enabled = false;

                        if (false == m_bIsAddingNewItemsToExistingTask)
                            this.tabControl_Task.SelectedIndex = 2;

                        // 打开吸附
                        if (false == m_bOfflineMode)
                            CBD_SendMessage("开启吸附", true, null, null);

                        dl_message_sender messenger = CBD_SendMessage;
                        if (0 == m_current_task_data[0].m_create_mode)
                            (new Thread(thread_run_handmade_task)).Start(messenger);
                        else
                            (new Thread(thread_run_CAD_task)).Start(messenger);

                        
                    }

                }

                if(btn_RunTask.Enabled == true)
                {
                    m_IO.set_IO_output(m_IO.m_output_start_button, IO_STATE.IO_HIGH);//平台按钮熄灯
                }
                else if(btn_RunTask.Enabled == false)
                {
                    m_IO.set_IO_output(m_IO.m_output_start_button, IO_STATE.IO_LOW);//平台按钮亮灯
                }
            }
        }

        // 线程：图像信息刷新线程
        public void thread_update_camera_image_info(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;
            
            byte[] pMainCamImg = null;
            byte[] pGuideCamImg = null;

            while (true)
            {
                if (false == m_bIsAppInited)
                {
                    Thread.Sleep(20);
                    continue;
                }
                
                if (null != m_main_camera.m_pImageBuf)
                {
                    if (null == pMainCamImg)
                        pMainCamImg = new byte[m_main_camera.m_pImageBuf.Length];
                    if (null == pGuideCamImg)
                        pGuideCamImg = new byte[m_guide_camera.m_pImageBuf.Length];
                    
                    lock (m_main_cam_lock)
                    {
                        Buffer.BlockCopy(m_main_camera.m_pImageBuf, 0, pMainCamImg, 0, pMainCamImg.Length);
                    }
                    lock (m_guide_cam_lock)
                    {
                        Buffer.BlockCopy(m_guide_camera.m_pImageBuf, 0, pGuideCamImg, 0, pGuideCamImg.Length);
                    }

                    double sharpness = 0;
                    double brightness = 0;
                    if (true == Gaugers.ImgOperators.get_image_sharpness(pMainCamImg, m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                    {
                            m_main_camera.m_current_sharpness = sharpness;
                            m_current_sharpness_of_main_cam = sharpness;
                    }
                    if (true == Gaugers.ImgOperators.get_image_brightness(pMainCamImg, m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref brightness))
                    {
                        m_main_camera.m_current_brightness = brightness;
                        m_current_brightness_of_main_cam = brightness;
                    }
                    
                    brightness = 0;
                    if (true == Gaugers.ImgOperators.get_image_brightness(pGuideCamImg, m_guide_camera.m_nCamWidth, m_guide_camera.m_nCamHeight, ref brightness))
                    {
                        m_guide_camera.m_current_brightness = brightness;
                        m_current_brightness_of_guide_cam = brightness;
                        //Debugger.Log(0, null, string.Format("222222 m_current_sharpness_of_main_cam = {0}", m_current_sharpness_of_main_cam));
                        //Debugger.Log(0, null, "222222 3333333333333333333333");
                    }
                }
                
                // 显示实时抓边效果
                if ((true == m_bLocateLinesInRealTime) && (null != ui_MainImage.Image))
                {
                    if (MEASURE_TYPE.LINE_WIDTH_14 == m_gauger.m_measure_type)
                    {
                        if ((true == m_gauger.m_bRotatedRectIsReady) && (m_gauger.m_down_pt.x > 0))
                        {
                            int nStride = 0;
                            int nWidth = 0;
                            int nHeight = 0;
                            byte[] pBuf = null;

                            this.Invoke(new Action(() =>
                            {
                                lock (m_main_cam_lock)
                                {
                                    pBuf = GeneralUtils.convert_image_to_bytes(ui_MainImage.Image, ui_MainImage.Image.RawFormat, ref nStride);
                                }
                                nWidth = ui_MainImage.Image.Width;
                                nHeight = ui_MainImage.Image.Height;
                            }));

                            //lock (m_gauger.m_real_time_lock)
                            {
                                if (true == m_gauger.gauge(ui_MainImage.Image, pBuf, nWidth, nHeight, nStride, m_nAlgorithm, true))
                                {
                                    m_gauger.m_bShowLineOnly = true;
                                }
                            }

                            //Debugger.Log(0, null, string.Format("222222  pBuf.Length = {0}", pBuf.Length));
                        }
                    }
                }

                for (int n = 0; n < 3; n++)
                {
                    Thread.Sleep(30);
                    if (m_bExitProgram)
                        return;
                }
            }
        }
        
        // 线程：执行JOG运动
        public void thread_jog_axis(object obj)
        {
            if (true == m_bIsJogThreadStarted)
                return;
            m_bIsJogThreadStarted = true;

            int counter = 0;
            double ratio = 1;
            while (true == m_bJogAxis)
            {
                if (counter < 2)
                {
                    if (MotionOps.AXIS_Z == m_nJogAxis)
                        ratio = 0.0035;
                    else
                        ratio = 0.01;
                }
                else
                {
                    if ((MotionOps.AXIS_Z == m_nJogAxis) && (counter < 5))
                        ratio = 0.015 * (double)(counter);
                    else
                        ratio = 0.036 * (double)(counter);
                    if (ratio > 1)
                        ratio = 1;
                }

                double rate = 1;
                if (0 == m_nJogSpeedRatio)
                    rate = 0.15;
                else if (1 == m_nJogSpeedRatio)
                    rate = 0.66;
                else if (3 == m_nJogSpeedRatio)
                    rate = 2;
                else if (4 == m_nJogSpeedRatio)
                    rate = 3.5;

                if (MotionOps.AXIS_Z == m_nJogAxis)
                {
                    double vel = (double)15 * ratio * rate;

                    //Debugger.Log(0, null, string.Format("222222 vel = {0}", vel));

                    m_motion.jog(m_nJogAxis, vel, 0.05, m_nJogDir);
                }
                else
                {
                    double vel = (double)100 * ratio * rate;
                    m_motion.jog(m_nJogAxis, vel, 0.05, m_nJogDir);
                }
                
                counter++;

                Thread.Sleep(100);
            }

            m_motion.stop_all_axes();

            m_bIsJogThreadStarted = false;
        }

        // 线程：执行图纸测量项批量测量
        public void execute_in_batch(object obj)
        {
            if (true == m_bIsExecutingInBatch)
                return;
            m_bIsExecutingInBatch = true;

            //m_event_wait_for_manual_gauge.Set();

            if (m_measure_items_on_graph.Count > 0)
            {
                for (int n = 0; n < m_measure_items_on_graph.Count; n++)
                {
                    MeasurePointData data = m_measure_items_on_graph[n];
                    
                    if (m_bStopExecution || m_bExitProgram)
                        break;

                    if (false == data.m_bHasBeenMeasured)
                    {
                        MeasurePointData new_data = data.cloneClass();
                        m_current_measure_graph_item = new_data;

                        if (true == data.m_bIsInvalidItem)
                            continue;

                        switch (data.m_mes_type)
                        {
                            case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                                data.m_len_ratio = 0;
                                thread_locate_and_measure_mark_pt(new_data);
                                break;
                            default:
                                thread_locate_and_measure_item(new_data);
                                break;
                        }

                        tabControl_Task.SelectedIndex = 1;
                    }
                }
            }
            else if (m_measure_items_from_txt.Count > 0)
            {
                for (int n = 0; n < m_measure_items_from_txt.Count; n++)
                {
                    if (m_bStopExecution || m_bExitProgram)
                        break;

                    MeasurePointData data = m_measure_items_from_txt[n];
                    Debugger.Log(0, null, string.Format("222222 n {0}: data.m_center_x_in_metric [{1:0.000},{2:0.000}]", n, data.m_center_x_in_metric, data.m_center_y_in_metric));
                    if (false == data.m_bHasBeenMeasured)
                    {
                        MeasurePointData new_data = data.cloneClass();
                        m_current_measure_graph_item = new_data;

                        switch (data.m_mes_type)
                        {
                            case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                                data.m_len_ratio = 0;
                                thread_locate_and_measure_mark_pt(new_data);
                                break;
                            default:
                                thread_locate_and_measure_item(new_data);
                                break;
                        }

                        tabControl_Task.SelectedIndex = 1;
                    }
                }
            }

            m_event_wait_for_finish_measurement.Set();
            Debugger.Log(0, null, string.Format("222222 execute_in_batch() thread exit"));
            m_bStopExecution = false;
            m_bIsMeasuringDuringCreation = false;
            m_bIsExecutingInBatch = false;
        }

        // 线程：保存测量结果图片
        void thread_save_gauge_result_image(object filePath)
        {
            object obj = new object();
            Monitor.Enter(obj);
            //lock (lockThis)
            {
                if (Directory.Exists(m_strImageSavingDir))
                {
                    // 创建料号目录
                    string dir = m_strImageSavingDir + "\\" + m_strCurrentTaskName;
                    if (false == Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    int idx = m_nCurrentMeasureItemIdx;
                    if ((idx < m_current_task_data.Count) && (m_current_task_data.Count > 0))
                    {
                        // 创建时间点目录
                        dir += "\\" + m_strTaskRunningStartingTime;
                        if (false == Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        //Debugger.Log(0, null, string.Format("222222 m_current_task_data[idx].m_ID: {0}", m_current_task_data[idx].m_ID));

                        string filepath = dir + "\\" + m_current_task_data[idx].m_ID + "_" + m_current_task_data[idx].m_name + ".jpg";
                        try
                        {
                            m_mv_bitmap.Save(filepath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        catch (Exception ex)
                        {
                            Debugger.Log(0, null, string.Format("222222 测量结果图片保存失败！异常信息：{0}", ex.Message));

                            dl_message_sender send_message = CBD_SendMessage;
                            send_message(string.Format("测量结果图片保存失败！异常信息：{0}", ex.Message), true, null, null);
                        }
                    }
                }
            }
            Monitor.Exit(obj);
        }

        // 线程：保存测量结果报表
        void thread_save_gauge_result_excel_report(object obj)
        {
            if (gridview_measure_results.Rows.Count <= 1)
                return;
            if (m_strCurrentTaskName.Length <= 0)
                return;

            string dir = m_strExcelSavingDir;
            if ("" == dir)
            {
                dir = "测量结果报表";
                if (false == Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            if (Directory.Exists(dir))
            {
                // 创建料号目录
                //dir = dir + "\\" + m_strCurrentTaskName;
                Debugger.Log(0, null, string.Format("222222  测量结果111  m_strTaskRunningStartingDataTime = {0}", m_strTaskRunningStartingDataTime));
                dir = dir + "\\" + m_strTaskRunningStartingDataTime + "\\" + m_strUseringName;
                if (false == Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                Debugger.Log(0, null, string.Format("222222  测量结果222  dir = {0}", dir));
                // 生成基于时间点的文件名
                //string filepath = dir + "\\" + m_strCurrentTaskName + " " + m_strTaskRunningStartingTime + ".xlsx";
                string filepath = dir + "\\" + m_strCurrentTaskName + ".xlsx";
                Debugger.Log(0, null, string.Format("222222  222  filepath = {0}", filepath));

                //判断改报表是否存在
                if (File.Exists(filepath))
                {

                    //存在
                    Debugger.Log(0, null, string.Format("222222 测量结果  存在报表"));

                    try
                    {

                        string[] fields = new string[] { "序号", "名称", "标准值", "上限", "下限", "单位", "测量值", "结果", "倍率", "类型", "层别", "批次", "操作人", "料号" };

                        int nRowsCount = gridview_measure_results.Rows.Count - 1;

                        //索引出错
                        m_excel_ops.creat_old_excel_file(filepath, fields, nRowsCount + 2, m_strTaskRunningStartingHMSTime);

                        m_excel_ops.open_old_file(filepath);

                        for (int n = 0; n < nRowsCount; n++)
                        {
                            ArrayList data = new ArrayList();

                            for (int k = 0; k < fields.Length - 4; k++)
                            {
                                if (0 == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k, n].Value.ToString());
                                else if ((fields.Length - 1 - 4) == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + 1, n].Value.ToString());
                                else
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k + 1, n].Value.ToString());
                            }

                            data.Add(m_strCurrentProductLayer);
                            data.Add(m_strBatchNum);
                            data.Add(m_strUseringName);
                            data.Add(m_strCurrentTaskName);

                            m_excel_ops.update_row(n + 1, data);

                        }

                        m_excel_ops.save_current_file();
                        m_excel_ops.close_current_file();

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message("报表保存成功", true, null, null);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(0, null, string.Format("222222 测量结果报表保存失败！异常信息：{0}", ex.Message));

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message(string.Format("测量结果报表保存失败！异常信息：{0}", ex.Message), true, null, null);
                    }

                }
                else
                {
                    //不存在
                    Debugger.Log(0, null, string.Format("222222 测量结果  不存在报表"));
                    try
                    {
                        string[] fields = new string[] { "序号", "名称", "标准值", "上限", "下限", "单位", "测量值", "结果", "倍率", "类型", "层别", "批次", "操作人", "料号" };

                        int nRowsCount = gridview_measure_results.Rows.Count - 1;

                        m_excel_ops.create_excel_file(filepath, fields, nRowsCount + 2, m_strTaskRunningStartingHMSTime);

                        m_excel_ops.open(filepath);

                        for (int n = 0; n < nRowsCount; n++)
                        {
                            ArrayList data = new ArrayList();

                            for (int k = 0; k < fields.Length - 4; k++)
                            {
                                if (0 == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k, n].Value.ToString());
                                else if ((fields.Length - 1 - 4) == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + 1, n].Value.ToString());
                                else
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k + 1, n].Value.ToString());
                            }

                            data.Add(m_strCurrentProductLayer);
                            data.Add(m_strBatchNum);
                            data.Add(m_strUseringName);
                            data.Add(m_strCurrentTaskName);
                            m_excel_ops.update_row(n + 1, data);
                        }

                        m_excel_ops.save_current_file();
                        m_excel_ops.close_current_file();

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message("报表保存成功", true, null, null);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(0, null, string.Format("222222 测量结果报表保存失败！异常信息：{0}", ex.Message));

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message(string.Format("测量结果报表保存失败！异常信息：{0}", ex.Message), true, null, null);
                    }
                }
            }
            else
            {
                dl_message_sender send_message = CBD_SendMessage;
                string info = string.Format("测量结果报表保存失败！异常信息：保存目录 {0} 不存在！", dir);

                send_message(info, true, null, null);
                MessageBox.Show(this, info, "提示");
            }
        }

        // 线程：触发加载第三方库
        public void thread_trigger_init_third_party_libraries(object obj)
        {
            Thread.Sleep(15000);
            HObject hObj;
            HOperatorSet.GenEmptyObj(out hObj);
        }

        // 线程：加载磁盘
        public void thread_create_and_format_disk(object obj)
        {
            if (false == Directory.Exists("Z:\\"))
            {
                Thread.Sleep(2000);

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                // 向cmd窗口发送输入信息
                p.StandardInput.WriteLine("imdisk -a -s 3900m -m Z: -p  \" /FS:NTFS /C /Y /Q\"" + "&exit");

                p.StandardInput.AutoFlush = true;
                Thread.Sleep(2000);
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
            }
        }

        // 线程：加载磁盘
        public void thread_zip_test(object obj)
        {
            if (true == Directory.Exists("Z:\\"))
            {
                Thread.Sleep(2000);

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = false;//不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine("D:\\" + "&exit");

                p.StandardInput.AutoFlush = true;
                Thread.Sleep(2000);
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();

                Debugger.Log(0, null, string.Format("222222 thread_zip_test = {0}", System.Environment.CurrentDirectory));
            }
        }

        // 线程：形状模板匹配测试
        public void thread_find_shape_model_test(object obj)
        {
            find_shape_model_test();
        }

        // 线程：光栅监控报警
        public void thread_monitor_grating(object obj)
        {
            dl_message_sender send_message = obj as dl_message_sender;

            IO_STATE state = Hardwares.IO_STATE.NONE;
            
            Debugger.Log(0, null, string.Format("222222 开启光栅监控"));

            while (true)
            {
                for (int n = 0; n < 2; n++)
                {
                    Thread.Sleep(30);
                    if (m_bExitProgram)
                        return;
                }
                
                // 光栅输入
                m_IO.get_IO_input(m_IO.m_input_grating, ref state);

                if ((Hardwares.IO_STATE.IO_HIGH == state) && (true == m_bUseHeightSensor))
                    m_bPlacedNewBoard = true;

                // 判断是否正在执行任务
                if (true == m_bIsRunningTask)
                {
                    if (state == Hardwares.IO_STATE.IO_HIGH)
                    {
                        // 暂停
                        m_bPause = true;
                        btn_RunTask.Enabled = true;
                        btn_Pause.Enabled = false;
                        
                        send_message("光栅报警", true, null, null);

                        // 开红灯，关绿灯
                        m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_HIGH);
                        m_IO.set_IO_output(m_IO.m_output_red_light, Hardwares.IO_STATE.IO_LOW);

                        //蜂鸣器发声
                        m_IO.set_IO_output(m_IO.m_output_beeper, Hardwares.IO_STATE.IO_LOW);

                        //鼓风机关闭
                        m_IO.set_IO_output(m_IO.m_output_vacuum, Hardwares.IO_STATE.IO_HIGH);

                        if (DialogResult.No == MessageBox.Show("触碰到光栅，暂停运行，是否继续执行任务？", "警告", MessageBoxButtons.YesNo))
                        {
                            // 终止任务
                            m_bStopExecution = true;
                            m_bStopTask = true;
                            m_bPause = false;
                            m_bIsRunningTask = false;
                            m_bShowAccurateMark = false;
                            m_bShowCoarseMark = false;
                            
                            m_bIsWaitingForConfirm = false;
                            m_bIsWaitingForUserManualGauge = false;

                            m_event_wait_for_confirm_during_autorun.Set();
                            m_event_wait_for_confirm_during_creation.Set();
                            m_event_wait_for_manual_gauge.Set();

                            m_gauger.clear_gauger_state();

                            btn_RunTask.Enabled = true;
                            btn_Pause.Enabled = true;
                            
                            // 开黄灯，关红灯
                            m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_LOW);
                            m_IO.set_IO_output(m_IO.m_output_red_light, Hardwares.IO_STATE.IO_HIGH);

                            // 蜂鸣器停止发声
                            m_IO.set_IO_output(m_IO.m_output_beeper, Hardwares.IO_STATE.IO_HIGH);

                            btn_StopTask_Click(null, null);
                        }
                        else
                        {
                            // 继续任务
                            if (true == m_bPause)
                            {
                                m_bPause = false;
                                btn_RunTask.Enabled = false;
                                btn_Pause.Enabled = true;
                                
                                // 开绿灯，关红灯
                                m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_LOW);
                                m_IO.set_IO_output(m_IO.m_output_red_light, Hardwares.IO_STATE.IO_HIGH);

                                // 蜂鸣器停止发声
                                m_IO.set_IO_output(m_IO.m_output_beeper, Hardwares.IO_STATE.IO_HIGH);
                                // 鼓风机开启
                                hardware_ops_enable_vacuum(true);
                            }
                        }

                    }
                }
            }
        }
    }
}
