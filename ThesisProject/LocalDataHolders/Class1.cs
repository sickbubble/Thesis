using ModelInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject.LocalDataHolders
{
    class Vector
    {
        public Vector(Point p)
        {
            _X = p.X;
            _Y = p.Y;
            _Z = p.Z;
            _Length = Math.Sqrt(_X * _X + _Y * _Y + _Z * _Z);

        }
        public Vector(double x, double y, double z)
        {
            _X = x;
            _Y = y;
            _Z = z;
            _Length = Math.Sqrt(_X * _X + _Y * _Y + _Z * _Z);

        }
        public Vector(Point start, Point end)
        {
            _X = (end.X - start.X);
            _Y = (end.Y - start.Y);
            _Z = (end.Z - start.Z);
            _Length = end.DistTo(start);

        }

        public Vector()
        {


        }
        #region Private Fields 

        private double _X;
        private double _Y;
        private double _Z;
        private double _Length;




        #endregion

        #region Public Properties

        public double X { get => _X; set => _X = value; }
        public double Y { get => _Y; set => _Y = value; }
        public double Z { get => _Z; set => _Z = value; }
        public double Length { get => _Length; set => _Length = value; }

        #endregion


        #region Public Methods

        public Vector GetUnitVector()
        {
            Vector v = new Vector();
            v.X = this.X / this.Length;
            v.Y = this.Y / this.Length;
            v.Z = this.Z / this.Length;
            v.Length = 1;
            return v;
        }

        public double AngleTo(Vector v)
        {
            double pi = 3.141592653589793;
            var x = this.X;
            var y = this.Y;
            var z = this.Z;

            return Math.Acos(((this.X * v.X) + (this.Y * v.Y) + (this.Z * v.Z)) / (this.Length * v.Length));
        }

        public Vector Sum(Vector v)
        {
            Vector vec = new Vector();
            Point point = new Point(this.X + v.X, this.Y + v.Y, this.Z + v.Z);
            Point origin = new Point(0.0, 0.0, 0.0);

            vec.X = point.X;
            vec.Y = point.Y;
            vec.Z = point.Z;
            vec.Length = point.DistTo(origin);

            return vec;
        }


        public Vector Extract(Vector v)
        {
            Vector vec = new Vector();
            Point point = new Point(this.X - v.X, this.Y - v.Y, this.Z - v.Z);
            Point origin = new Point(0.0, 0.0, 0.0);

            vec.X = point.X;
            vec.Y = point.Y;
            vec.Z = point.Z;
            vec.Length = point.DistTo(origin);

            return vec;
        }

        public Vector CrossProduct(Vector v)
        {
            Vector vec = new Vector();


            Point origin = new Point(0.0, 0.0, 0.0);

            vec.X = (this.Y * v.Z) - (this.Z * v.Y);
            vec.Y = (this.Z * v.X) - (this.X * v.Z);
            vec.Z = (this.X * v.Y) - (this.Y * v.X);
            Point point = new Point(vec.X, vec.Y, vec.Z);
            var length = point.DistTo(origin);

            //Normalize Vector
            vec.X /= length;
            vec.Y /= length;
            vec.Z /= length;

            Point p2 = new Point(vec.X, vec.Y, vec.Z);
            vec.Length = p2.DistTo(origin);

            return vec;
        }

        public Vector CrossProduct(double multiplier)
        {
            // Multiply
            Vector vec = new Vector();
            vec.X = multiplier * this.X;

            vec.Y = multiplier * this.Y;

            vec.Z = multiplier * this.Z;

            Point p = new Point(vec.X, vec.Y, vec.Z);

            Point origin = new Point(0.0, 0.0, 0.0);
            vec.Length = p.DistTo(origin);
            return vec;
        }

        #endregion
    }
}
