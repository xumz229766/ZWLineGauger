using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZWLineGauger.Gaugers;

namespace ZWLineGauger
{
    public partial class MainUI : Form
    {
        public enum enum_customer
        {
            none,
            customer_fangzheng_F3                              // 珠海方正 F3
        }

        // 线程：复用奇偶层定位孔记录，适用于珠海方正F3
        public void thread_use_history_three_marks_record(object obj)
        {
            dl_message_sender send_message = CBD_SendMessage;

            int nRecordIndex = (int)obj;

            Debugger.Log(0, null, string.Format("222222 复用奇偶层定位孔记录，记录索引号 = {0}", nRecordIndex));
            
            ThreeMarksRecord record = m_list_three_marks_records[nRecordIndex];
            for (int n = 0; n < 3; n++)
            {
                // 设置倍率
                send_message("设置倍率", false, 0, null);

                // 设置测量类型
                send_message("设置测量类型", false, MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE, null);

                m_bShowSmallSelectionFrame = false;
                m_bShowFrameDuringTaskCreation = false;
                m_bShowCoarseMark = false;
                m_bShowAccurateMark = false;
                
                MeasurePointData data = new MeasurePointData();

                // 设置光源和亮度
                if (false == m_bOfflineMode)
                {
                    //record.m_bIsTopLightOn[n] = (0 == m_nLightTypeForGuideCamForMarkPt) ? true : false;
                    //record.m_bIsBottomLightOn[n] = (1 == m_nLightTypeForGuideCamForMarkPt) ? true : false;
                    //record.m_nTopBrightness[n] = m_top_light.m_nBrightness;
                    //record.m_nBottomBrightness[n] = m_bottom_light.m_nBrightness;

                    data.m_ID = n + 1;
                    data.m_create_mode = 2;
                    data.m_mes_type = MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE;
                    
                    data.m_metric_radius[0] = record.m_dbDiameterInMM[n];

                    data.m_center_x_in_metric = record.m_marks_pt_on_graph[n].x;
                    data.m_center_y_in_metric = record.m_marks_pt_on_graph[n].y;

                    data.m_bIsTopLightOn = record.m_bIsTopLightOn[n];
                    data.m_bIsBottomLightOn = record.m_bIsBottomLightOn[n];
                    data.m_nTopBrightness = record.m_nTopBrightness[n];
                    data.m_nBottomBrightness = record.m_nBottomBrightness[n];
                    
                    send_message("设置光源和亮度", false, data, null);
                }
                
                if (0 == n)
                {
                    Point3d current_crd = new Point3d(0, 0, 0);
                    m_motion.get_xyz_crds(ref current_crd);

                    m_motion.linear_XYZ_wait_until_stop(record.m_marks_pt_on_stage[n].x, record.m_marks_pt_on_stage[n].y, current_crd.z, false);
                }
                else
                    m_motion.linear_XYZ_wait_until_stop(record.m_marks_pt_on_stage[n].x, record.m_marks_pt_on_stage[n].y, record.m_marks_pt_on_stage[n].z, false);

                // 通过高度传感器到达清晰平面
                #region
                if (0 == n)
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
                if (false == m_bOfflineMode)
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
                        Debugger.Log(0, null, string.Format("222222 在导航图像中寻找定位孔失败"));

                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show(this, "在导航图像中寻找定位孔失败。", "提示");
                        }));

                        break;
                    }
                }
                #endregion

                // 获取三角变换矩阵
                if (get_fiducial_mark_count(m_current_task_data) >= 3)
                {
                    m_triangle_trans_matrix = new double[10];

                    generate_transform_matrix_by_three_pts(m_current_task_data, ref m_triangle_trans_matrix);

                    Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 获取三角变换矩阵"));
                }

                Thread.Sleep(2000);
            }
        }

        // 线程：ODB自动模式创建首件
        public void thread_create_task_by_ODB_data(object obj)
        {
            for (int n = 0; n < m_measure_items_on_graph.Count; n++)
            {
                if ((n < 3) && (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != m_measure_items_on_graph[n].m_mes_type))
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show(this, "前三个测量项需为定位孔，请检查！", "提示");
                    }));

                    return;
                }
            }

            for (int n = 0; n < m_measure_items_on_graph.Count; n++)
            {
                if (n < 3)
                {
                    m_measure_items_on_graph[n].m_len_ratio = 0;

                    m_nCreateTaskMode = 3;

                    thread_locate_and_measure_mark_pt_in_ODB_auto_mode(m_measure_items_on_graph[n]);
                }
                else
                {

                }
            }
        }

        // 线程：ODB自动模式创建首件时，寻找和测量定位孔
        public void thread_locate_and_measure_mark_pt_in_ODB_auto_mode(object obj)
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

                generate_transform_matrix_by_three_pts(m_current_task_data, ref m_triangle_trans_matrix);

                Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 获取三角变换矩阵"));
            }

            m_bIsMeasuringDuringCreation = false;
        }
    }
}
