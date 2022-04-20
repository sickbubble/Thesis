using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class Point
    {
        public Point()
        {

        }

        public Point(double x, double y, double z)
        {
            _X = x;
            _Y = y;
            _Z = z;
        }


        private double _X;
        private double _Y;
        private double _Z;

        public double X { get => _X; set => _X = value; }
        public double Y { get => _Y; set => _Y = value; }
        public double Z { get => _Z; set => _Z = value; }
    
    public double DistTo(Point pointTo)
        {
            var xDiffSqr = Math.Pow(_X - pointTo.X,2);
            var yDiffSqr = Math.Pow(_Y - pointTo.Y,2);
            var zDiffSqr = Math.Pow(_Z - pointTo.Z,2);


            return Math.Sqrt(xDiffSqr+yDiffSqr+ zDiffSqr);

        }
    }
}
