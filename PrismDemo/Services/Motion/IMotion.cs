using PrismDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismDemo.Services.Motion
{
    public interface IMotion
    {
        bool InitCard();
        bool HotReset(ushort cardID);
        bool CoolReset();
        bool LoadINI(ushort cardID, string INIPath);
        bool LoadENI(ushort cardNo, string ENIPath);
        bool EnableAllAxes(ushort cardNo);
        bool SingleAxisEnable(IAxis axis);
        bool SingleAxisDisable(IAxis axis);
        bool SingleAxisGoHome(IAxis axis);
        bool GetAxisGoHomeReslut(IAxis axis);
        string GetAixsState(IAxis axis);
        bool SingleAxisDecStop(IAxis axis);
        bool CardEmgStop(IAxis axisModel);
        bool SetSingleAxisPositionZero(IAxis axis);
        double GetSingleAxisPosition(IAxis axis);
        double GetSingleAxisEncoder(IAxis axis);
        bool GetSingleAxisPositiveLimit(IAxis axis);
        bool GetSingleAxisNegativeLimit(IAxis axis);
        bool GetSingleHomeLimit(IAxis axis);
        bool GetSingleAxisMoveDone(IAxis axis);
        //bool SingleAxisRelativeMove(IAxis axis, double targetStep, double speed);
        //bool SingleAxisAbsoluteMove(IAxis axis, double targetPos, double speed);
        //bool SingleAxisJog(IAxis axis, double speed, bool isPositive);
        //bool RelativeLine2D(IAxis axis,IAxis axis1, double targetStep, double speed, double acceleration, double deceleration);
        //bool AbsoluteLine2D(IAxis axis, IAxis axis1, double targetStep, double speed, double acceleration, double deceleration);


    }
}
