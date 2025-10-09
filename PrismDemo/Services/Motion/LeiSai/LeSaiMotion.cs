using PrismDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrismDemo.Services.Motion.LeiSai
{
    public class LeiSaiMotion : IMotion
    {
        // 存储控制卡是否初始化成功（卡级状态）
        public bool IsEnables { get; private set; } = false;

        private readonly object _lock = new object();
        private short nRet = -1;


        // 初始化控制卡
        public bool InitCard()
        {
            lock (_lock)
            {

                nRet = LTDMC.dmc_board_init();
                if (nRet <= 0 || nRet >= 8)
                    return false;
                else
                    return true;
            }
        }

        #region 多卡支持
        //// 多卡场景下的状态存储（键：cardId，值：是否初始化成功）
        //private Dictionary<ushort, bool> _cardInitializedStatus = new Dictionary<ushort, bool>();

        //// 初始化指定卡（修改方法参数以支持多卡）
        //public bool InitCard(ushort cardId)
        //{
        //    lock (_lock)
        //    {
        //        nRet = LTDMC.dmc_board_init(cardId); // 假设底层API支持指定cardId
        //        bool success = (nRet > 0 && nRet < 8);
        //        _cardInitializedStatus[cardId] = success;
        //        return success;
        //    }
        //}
        #endregion

        // 热复位
        public bool HotReset(ushort cardNo)
        {
            lock (_lock)
            {
                LTDMC.dmc_soft_reset(cardNo);
                LTDMC.dmc_board_close();
                Thread.Sleep(15000);
                nRet = LTDMC.dmc_board_init();

                if (nRet != 0)
                    return false;
                else
                    return true;
            }
        }

        // 冷复位
        public bool CoolReset()
        {
            lock (_lock)
            {
                LTDMC.dmc_board_reset();
                LTDMC.dmc_board_close();
                Thread.Sleep(15000);
                nRet = LTDMC.dmc_board_init();
                if (nRet != 0)
                    return false;
                else
                    return true;
            }
        }

        // 加载INI文件
        public bool LoadINI(ushort cardNo, string INIPath)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(INIPath))
                {
                    //MessageBox.Show("文件路径无效！");
                    return false;
                }

                if (!File.Exists(INIPath))
                {
                    return false;
                }


                nRet = LTDMC.nmc_set_cycletime(cardNo, 2, 1000);            //设置总线周期
                nRet = LTDMC.dmc_download_configfile(cardNo, INIPath);      //下载INI文件

                if (nRet == 0) return true;
                else return false;


            }
        }

        // 加载ENI文件
        public bool LoadENI(ushort cardNo, string ENIPath)
        {
            if (string.IsNullOrEmpty(ENIPath))
            {
                //MessageBox.Show("文件路径无效！");
                return false;
            }

            FileStream fs = File.Open(ENIPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            string str = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            byte[] fileincontrol = Encoding.UTF8.GetBytes("");
            ushort filetype = 200;
            nRet = LTDMC.dmc_download_memfile(cardNo, buffer, (uint)buffer.Length, fileincontrol, filetype);     //下载ENI文件

            if (nRet == 0) return true;
            else return false;
        }

        // 使能所有轴
        public bool EnableAllAxes(ushort cardNo)
        {
            nRet = LTDMC.nmc_set_axis_enable(cardNo, 255);
            if (nRet == 0) return IsEnables = true;
            else return IsEnables = false;
        }

        // 使能单轴
        public bool SingleAxisEnable(IAxis axisModel)
        {
            lock (_lock)
            {
                ushort cardNo = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;

                nRet = LTDMC.nmc_set_axis_enable(cardNo, axisID); //使能对应轴
                if (nRet == 0) return true;
                else return false;
            }
        }

        // 失能单轴
        public bool SingleAxisDisable(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;
                

                ushort cardNo = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;

                nRet = LTDMC.nmc_set_axis_disable(cardNo, axisID); //失能对应轴
                if (nRet == 0) return true;
                else return false;
            }
        }

        // 单轴回零
        public bool SingleAxisGoHome(IAxis axisModel)
        {
            if (IsEnables) return false;

            ushort cardID = (ushort)axisModel.CardID;
            ushort axisID = (ushort)axisModel.AxisID;
            ushort homeMode = (ushort)axisModel.HomeMode;
            double homeLowSpeed = axisModel.HomeLowSpeed;
            double homeHighSpeed = axisModel.HomeHighSpeed;
            double accTime = axisModel.AccTime;
            double decTime = axisModel.DecTime;
            double homeOffset = axisModel.HomeOffset;

            nRet = LTDMC.nmc_set_home_profile(cardID, axisID, homeMode, homeLowSpeed, homeHighSpeed, accTime, decTime, homeOffset);
            nRet = LTDMC.nmc_home_move(cardID, axisID);//启动回零

            if (nRet == 0) return true;
            else return false;
        }

        // 获取回零结果
        public bool GetAxisGoHomeReslut(IAxis axisModel)
        {
            if (IsEnables) return false;

            ushort cardID = (ushort)axisModel.CardID;
            ushort axisID = (ushort)axisModel.AxisID;

            ushort result = 0;
            LTDMC.dmc_get_home_result(cardID, axisID, ref result);

            if (result == 1) return true; //回零完成
            else return false;
        }

        // 获取轴状态
        public string GetAixsState(IAxis axisModel)
        {
            //if (IsEnables) return false;
            //lock (_lock)
            //{

            ushort cardID = (ushort)axisModel.CardID;
            ushort axisID = (ushort)axisModel.AxisID;

            ushort AxisState = 0;
            LTDMC.nmc_get_axis_state_machine(cardID, axisID, ref AxisState);

            string strError = "";
            switch (AxisState)// 读取指定轴状态机
            {
                case 0:
                    strError = " 轴处于未启动状态";
                    break;
                case 1:
                    strError = " 轴处于启动禁止状态";
                    break;
                case 2:
                    strError = " 轴处于准备启动状态";
                    break;
                case 3:
                    strError = " 轴处于启动状态";
                    break;
                case 4:
                    strError = " 轴处于操作使能状态"; //Normal
                    break;
                case 5:
                    strError = " 轴处于停止状态";
                    break;
                case 6:
                    strError = " 轴处于错误触发状态";
                    break;
                case 7:
                    strError = " 轴处于错误状态";
                    break;
            }
                ;
            return strError;
            //}
        }

        // 单轴减速停止
        public bool SingleAxisDecStop(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                ushort accTime = (ushort)axisModel.AccTime;//减速时间


                LTDMC.dmc_set_dec_stop_time(cardID, axisID, accTime);
                nRet = LTDMC.dmc_stop(cardID, axisID, 0);

                if (nRet == 0) return true;
                else return false;
            }
        }

        // 卡急停
        public bool CardEmgStop(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;

                ushort cardID = (ushort)axisModel.CardID;
                short SRtn = LTDMC.dmc_emg_stop(cardID);

                if (SRtn == 0) return true;
                else return false;
            }
        }

        // 单轴位置清零
        public bool SetSingleAxisPositionZero(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;

                nRet = LTDMC.dmc_set_position_unit(cardID, axisID, 0);//位置清零

                if (nRet == 0) return true;
                else return false;
            }
        }
        public double GetSingleAxisPosition(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return 0;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                double dPos = 0;
                LTDMC.dmc_get_position_unit(cardID, axisID, ref dPos);

                return dPos;

            }
        }
        public double GetSingleAxisEncoder(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return 0;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                double dPos = 0;
                LTDMC.dmc_get_encoder_unit(cardID, axisID, ref dPos);

                return dPos;
            }
        }

        public bool SetSingleAxisPosition(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;

                nRet = LTDMC.dmc_set_position_unit(cardID, axisID, 0);

                if (nRet == 0) return true;
                else return false;
            }
        }

        public bool SetSingleAxisEncoder(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                nRet = LTDMC.dmc_set_encoder_unit(cardID, axisID, 0);
                if (nRet == 0) return true;
                else return false;
            }
        }

        public bool GetSingleAxisPositiveLimit(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                uint limitNum = LTDMC.dmc_axis_io_status(cardID, axisID);
                if ((limitNum & 2) == 2)//检测正限位信号
                    return true;
                else
                    return false;
            }
        }
        public bool GetSingleAxisNegativeLimit(IAxis axisModel)
        {
            lock (_lock)
            {
                if (IsEnables) return false;

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                uint limitNum = LTDMC.dmc_axis_io_status(cardID, axisID);
                if ((limitNum & 4) == 4)
                    return true;
                else
                    return false;
            }
        }
        public bool GetSingleHomeLimit(IAxis axisModel)
        {
            lock (_lock)
            {

                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                uint limitNum = LTDMC.dmc_axis_io_status(cardID, axisID);
                if ((limitNum & 16) == 16)
                    return true;
                else
                    return false;
            }
        }
        public bool GetSingleAxisMoveDone(IAxis axisModel)
        {
            lock (_lock)
            {
                ushort cardID = (ushort)axisModel.CardID;
                ushort axisID = (ushort)axisModel.AxisID;
                nRet = LTDMC.dmc_check_done(cardID, axisID);

                if (nRet == 0) return true;
                else return false;
            }
        }
        public bool SingleAxisRelativeMove(IAxis axisModel)
        {

            ushort cardID = (ushort)axisModel.CardID;
            ushort axisID = (ushort)axisModel.AxisID;
            double startSpeed = axisModel.StartSpeed;
            double maxSpeed = axisModel.MaxSpeed;
            double accTime = axisModel.AccTime;
            double decTime = axisModel.DecTime;
            double stopSpeed = axisModel.StopSpeed;
            double sTime = axisModel.STime;
            double targetPosition = axisModel.TargetPosition; //目标位置
            ushort usMode = 0; //0:相对 1:绝对
            int timeoutMs = (int)axisModel.MoveTimeOut; // 运动超时，单位毫秒

            LTDMC.dmc_set_profile_unit(cardID, axisID, startSpeed, maxSpeed, accTime, decTime, stopSpeed);//设置速度参数
            LTDMC.dmc_set_s_profile(cardID, axisID, 0, sTime);                                            //设置S段速度参数
            nRet = LTDMC.dmc_pmove_unit(cardID, axisID, targetPosition, usMode);

            if (nRet != 0)
                return false; // 启动运动失败

            // 超时控制
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                Thread.Sleep(10);

                if (GetSingleAxisMoveDone(axisModel))
                    return true;
                if (sw.ElapsedMilliseconds > timeoutMs)
                {
                    SingleAxisDecStop(axisModel);
                    return false;
                }
            }
        }

        public bool SingleAxisAbsoluteMove(IAxis axisModel)
        {
            ushort cardID = (ushort)axisModel.CardID;
            ushort axisID = (ushort)axisModel.AxisID;
            double startSpeed = axisModel.StartSpeed;
            double maxSpeed = axisModel.MaxSpeed;
            double accTime = axisModel.AccTime;
            double decTime = axisModel.DecTime;
            double stopSpeed = axisModel.StopSpeed;
            double sTime = axisModel.STime;
            double targetPosition = axisModel.TargetPosition; //目标位置
            ushort usMode = 1; //0:相对 1:绝对
            int timeoutMs = (int)axisModel.MoveTimeOut; // 运动超时，单位毫秒

            LTDMC.dmc_set_profile_unit(cardID, axisID, startSpeed, maxSpeed, accTime, decTime, stopSpeed);//设置速度参数
            LTDMC.dmc_set_s_profile(cardID, axisID, 0, sTime);                                            //设置S段速度参数
            nRet = LTDMC.dmc_pmove_unit(cardID, axisID, targetPosition, usMode);

            if (nRet != 0)
                return false;

            // 超时控制
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                Thread.Sleep(10);

                if (GetSingleAxisMoveDone(axisModel))
                    return true;
                if (sw.ElapsedMilliseconds > timeoutMs)
                {
                    SingleAxisDecStop(axisModel);
                    return false;
                }
            }

        }


        public bool SingleAxisJog(IAxis axisModel)
        {

            ushort cardID = (ushort)axisModel.CardID;
            ushort axisID = (ushort)axisModel.AxisID;
            double startSpeed = axisModel.StartSpeed;
            double maxSpeed = axisModel.MaxSpeed;
            double accTime = axisModel.AccTime;
            double decTime = axisModel.DecTime;
            double stopSpeed = axisModel.StopSpeed;
            double sTime = axisModel.STime;
            ushort direction = (ushort)axisModel.Direction; //轴的方向，正向或负向


            LTDMC.dmc_set_profile_unit(cardID, axisID, startSpeed, maxSpeed, accTime, decTime, stopSpeed);//设置速度参数
            LTDMC.dmc_set_s_profile(cardID, axisID, 0, sTime);                                            //设置S段速度参数
            nRet = LTDMC.dmc_vmove(cardID, axisID, direction);

            if (nRet == 0) return true;
            else return false;


        }

        /// <summary>
        /// 2轴相对直线插补运动
        /// </summary>
        /// <param name="xAxis">X轴参数</param>
        /// <param name="yAxis">Y轴参数</param>
        /// <param name="xTarget">X轴相对目标位置</param>
        /// <param name="yTarget">Y轴相对目标位置</param>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <returns>是否成功完成</returns>
        public bool RelativeLine2D(IAxis xAxis, IAxis yAxis,
                                  double xTarget, double yTarget, int timeoutMs)
        {
            // 验证控制卡一致性
            if (xAxis.CardID != yAxis.CardID)
                return false;

            ushort cardID = (ushort)xAxis.CardID;
            ushort coord = 0; // 坐标系编号，根据实际配置调整
            ushort axisNum = 2;
            ushort[] axisList = { (ushort)xAxis.AxisID, (ushort)yAxis.AxisID };
            ushort posMode = 0; // 0:相对模式

            // 设置脉冲当量
            LTDMC.dmc_set_equiv(cardID, axisList[0], xAxis.Equivalent);
            LTDMC.dmc_set_equiv(cardID, axisList[1], yAxis.Equivalent);

            // 设置插补速度参数（使用X轴参数，可根据需要调整为取两者最小值）
            LTDMC.dmc_set_vector_profile_unit(
                cardID, coord,
                xAxis.StartSpeed,  // 起始速度
                xAxis.MaxSpeed,    // 最大速度
                xAxis.AccTime,     // 加速时间
                xAxis.DecTime,     // 减速时间
                xAxis.StopSpeed    // 停止速度
            );

            // 设置S曲线参数（如果需要）
            LTDMC.dmc_set_vector_s_profile(cardID, coord, 0, xAxis.STime);

            // 执行直线插补
            short nRet = LTDMC.dmc_line_unit(
                cardID, coord, axisNum,
                axisList,
                new double[] { xTarget, yTarget },
                posMode
            );

            if (nRet != 0)
                return false;

            // 等待运动完成或超时
            return WaitForInterpolationDone(cardID, coord, timeoutMs, axisList);
        }

        /// <summary>
        /// 2轴绝对直线插补运动
        /// </summary>
        /// <param name="xAxis">X轴参数</param>
        /// <param name="yAxis">Y轴参数</param>
        /// <param name="xTarget">X轴绝对目标位置</param>
        /// <param name="yTarget">Y轴绝对目标位置</param>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <returns>是否成功完成</returns>
        public bool AbsoluteLine2D(LeiSaiAxis xAxis, LeiSaiAxis yAxis,
                                  double xTarget, double yTarget, int timeoutMs)
        {
            // 防呆判断卡ID一致性
            if (xAxis.CardID != yAxis.CardID)
                return false;

            ushort cardID = (ushort)xAxis.CardID;
            ushort coord = 0;
            ushort axisNum = 2;
            ushort[] axisList = { (ushort)xAxis.AxisID, (ushort)yAxis.AxisID };
            ushort posMode = 1; // 1:绝对模式

            // 设置脉冲当量和速度参数（同相对运动）
            LTDMC.dmc_set_equiv(cardID, axisList[0], xAxis.Equivalent);
            LTDMC.dmc_set_equiv(cardID, axisList[1], yAxis.Equivalent);

            // 设置插补参数
            LTDMC.dmc_set_vector_profile_unit(
                cardID, coord,
                xAxis.StartSpeed,
                xAxis.MaxSpeed,
                xAxis.AccTime,
                xAxis.DecTime,
                xAxis.StopSpeed
            );

            LTDMC.dmc_set_vector_s_profile(cardID, coord, 0, xAxis.STime);

            // 执行绝对直线插补
            short nRet = LTDMC.dmc_line_unit(
                cardID, coord, axisNum,
                axisList,
                new double[] { xTarget, yTarget },
                posMode
            );

            if (nRet != 0)
                return false;

            // 等待运动完成
            return WaitForInterpolationDone(cardID, coord, timeoutMs, axisList);
        }

        /// <summary>
        /// 3轴相对直线插补运动
        /// </summary>
        /// <param name="xAxis">X轴参数</param>
        /// <param name="yAxis">Y轴参数</param>
        /// <param name="zAxis">Z轴参数</param>
        /// <param name="xTarget">X轴相对目标位置</param>
        /// <param name="yTarget">Y轴相对目标位置</param>
        /// <param name="zTarget">Z轴相对目标位置</param>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <returns>是否成功完成</returns>
        public bool RelativeLine3D(IAxis xAxis, IAxis yAxis, IAxis zAxis,
                                  double xTarget, double yTarget, double zTarget, int timeoutMs)
        {
            // 验证控制卡一致性
            if (xAxis.CardID != yAxis.CardID || xAxis.CardID != zAxis.CardID)
                return false;

            ushort cardID = (ushort)xAxis.CardID;
            ushort coord = 0;
            ushort axisNum = 3;
            ushort[] axisList = { (ushort)xAxis.AxisID, (ushort)yAxis.AxisID, (ushort)zAxis.AxisID };
            ushort posMode = 0; // 相对模式

            // 设置脉冲当量
            foreach (var axis in axisList)
            {
                var axisModel = new[] { xAxis, yAxis, zAxis }.First(m => m.AxisID == axis);
                LTDMC.dmc_set_equiv(cardID, axis, axisModel.Equivalent);
            }

            // 设置插补速度参数
            LTDMC.dmc_set_vector_profile_unit(
                cardID, coord,
                xAxis.StartSpeed,
                xAxis.MaxSpeed,
                xAxis.AccTime,
                xAxis.DecTime,
                xAxis.StopSpeed
            );

            // 设置S曲线
            LTDMC.dmc_set_vector_s_profile(cardID, coord, 0, xAxis.STime);

            // 执行3轴直线插补
            short nRet = LTDMC.dmc_line_unit(
                cardID, coord, axisNum,
                axisList,
                new double[] { xTarget, yTarget, zTarget },
                posMode
            );

            if (nRet != 0)
                return false;

            // 等待运动完成
            return WaitForInterpolationDone(cardID, coord, timeoutMs, axisList);
        }

        /// <summary>
        /// 3轴绝对直线插补运动
        /// </summary>
        /// <param name="xAxis">X轴参数</param>
        /// <param name="yAxis">Y轴参数</param>
        /// <param name="zAxis">Z轴参数</param>
        /// <param name="xTarget">X轴绝对目标位置</param>
        /// <param name="yTarget">Y轴绝对目标位置</param>
        /// <param name="zTarget">Z轴绝对目标位置</param>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <returns>是否成功完成</returns>
        public bool AbsoluteLine3D(IAxis xAxis, IAxis yAxis, IAxis zAxis,
                                  double xTarget, double yTarget, double zTarget, int timeoutMs)
        {
            // 与相对运动逻辑一致，仅位置模式不同
            if (xAxis.CardID != yAxis.CardID || xAxis.CardID != zAxis.CardID)
                return false;

            ushort cardID = (ushort)xAxis.CardID;
            ushort coord = 0;
            ushort axisNum = 3;
            ushort[] axisList = { (ushort)xAxis.AxisID, (ushort)yAxis.AxisID, (ushort)zAxis.AxisID };
            ushort posMode = 1; // 绝对模式

            // 设置脉冲当量和速度参数（同相对运动）
            foreach (var axis in axisList)
            {
                var axisModel = new[] { xAxis, yAxis, zAxis }.First(m => m.AxisID == axis);
                LTDMC.dmc_set_equiv(cardID, axis, axisModel.Equivalent);
            }

            LTDMC.dmc_set_vector_profile_unit(
                cardID, coord,
                xAxis.StartSpeed,
                xAxis.MaxSpeed,
                xAxis.AccTime,
                xAxis.DecTime,
                xAxis.StopSpeed
            );

            LTDMC.dmc_set_vector_s_profile(cardID, coord, 0, xAxis.STime);

            // 执行绝对插补
            short nRet = LTDMC.dmc_line_unit(
                cardID, coord, axisNum,
                axisList,
                new double[] { xTarget, yTarget, zTarget },
                posMode
            );

            if (nRet != 0)
                return false;

            // 等待运动完成
            return WaitForInterpolationDone(cardID, coord, timeoutMs, axisList);
        }



        /// <summary>
        /// 等待插补运动完成或超时
        /// </summary>
        private bool WaitForInterpolationDone(ushort cardID, ushort coord, int timeoutMs, ushort[] axisList)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                Thread.Sleep(10);

                // 检查坐标系是否运动完成
                if (LTDMC.dmc_check_done_multicoor(cardID, coord) == 1)
                    return true;

                // 超时处理
                if (sw.ElapsedMilliseconds > timeoutMs)
                {
                    // 紧急停止所有轴
                    foreach (var axis in axisList)
                    {
                        LTDMC.dmc_emg_stop(cardID);
                    }
                    return false;
                }
            }
        }


    }
}
