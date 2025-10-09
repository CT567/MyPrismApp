using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismDemo.Models
{
    public interface IAxis
    {
        int CardID { get; }         // 卡号
        int AxisID { get; }        // 轴号
        double Equivalent { get; }    // 轴当量
        string Name { get; }          // 轴名:XYZU
        bool IsEnabled { get; }       // 当前是否使能
        int HomeMode { get; }         // 回零模式
        double HomeLowSpeed { get; }     // 回零低速
        double HomeHighSpeed { get; }    // 回零高速
        double AccTime { get; }         // 加速时间
        double DecTime { get; }         // 减速时间
        double STime { get; }           // S段时间
        double MoveTimeOut { get; }     // 运动超时
        double HomeOffset { get; }      // 回零偏移量
        bool IsHomed { get; }         // 是否回零完成
        double CurrentPosition { get; } // 当前坐标
        double TargetPosition { get; }  // 目标坐标
        double StartSpeed { get; }      // 起始速度
        double StopSpeed { get; }       // 停止速度
        double MaxSpeed { get; }         // 最大速度
        int Direction { get; }         // 轴的方向，正向或负向
        string GroupId { get; }       // 所属轴组-支持插补
    }


}
