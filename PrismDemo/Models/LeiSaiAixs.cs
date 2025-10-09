using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismDemo.Models
{
    public class LeiSaiAxis : IAxis
    {
        public int CardID { get; set; }
        public int AxisID { get; set; }
        public double Equivalent { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int HomeMode { get; set; }
        public double HomeLowSpeed { get; set; }
        public double HomeHighSpeed { get; set; }
        public double AccTime { get; set; }
        public double DecTime { get; set; }
        public double STime { get; set; }
        public double MoveTimeOut { get; set; }
        public double HomeOffset { get; set; }
        public bool IsHomed { get; set; }
        public double CurrentPosition { get; set; }
        public double TargetPosition { get; set; }
        public double StartSpeed { get; set; }
        public double StopSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public int Direction { get; set; }
        public string GroupId { get; set; }



    }
}
