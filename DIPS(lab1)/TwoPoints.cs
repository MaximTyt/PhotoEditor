using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPS_lab1_
{
    public class TwoPoints
    {
        public double X { set; get; }
        public double Y { set; get; }
        public TwoPoints(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public TwoPoints()
        { }
    }
}
