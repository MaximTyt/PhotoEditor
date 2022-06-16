using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPS_lab1_
{
    internal class CubicInterpolation : IInterpolation
    {
        public CubicSpline cs = new CubicSpline();

        public double f(double _x) => Functions.Clamp<double>(this.cs.Interpolate(_x), 0.0, 1.0);

        public void calc(TwoPoints[] points) => this.cs.BuildSpline(((IEnumerable<TwoPoints>)points).Select(x => x.X).ToArray<double>(), ((IEnumerable<TwoPoints>)points).Select(x => x.Y).ToArray<double>(), points.Length);

        public IInterpolation copy() => (IInterpolation)this;
    }
    internal class CubicSpline
    {
        private SplineTuple[] splines;

        public void BuildSpline(double[] x, double[] y, int n)
        {
            this.splines = new SplineTuple[n];
            for (int index = 0; index < n; ++index)
            {
                this.splines[index].x = x[index];
                this.splines[index].a = y[index];
            }
            this.splines[0].c = this.splines[n - 1].c = 0.0;
            double[] numArray1 = new double[n - 1];
            double[] numArray2 = new double[n - 1];
            numArray1[0] = numArray2[0] = 0.0;
            for (int index = 1; index < n - 1; ++index)
            {
                double num1 = x[index] - x[index - 1];
                double num2 = x[index + 1] - x[index];
                double num3 = num1;
                double num4 = 2.0 * (num1 + num2);
                double num5 = num2;
                double num6 = 6.0 * ((y[index + 1] - y[index]) / num2 - (y[index] - y[index - 1]) / num1);
                double num7 = num3 * numArray1[index - 1] + num4;
                numArray1[index] = -num5 / num7;
                numArray2[index] = (num6 - num3 * numArray2[index - 1]) / num7;
            }
            for (int index = n - 2; index > 0; --index)
                this.splines[index].c = numArray1[index] * this.splines[index + 1].c + numArray2[index];
            for (int index = n - 1; index > 0; --index)
            {
                double num = x[index] - x[index - 1];
                this.splines[index].d = (this.splines[index].c - this.splines[index - 1].c) / num;
                this.splines[index].b = num * (2.0 * this.splines[index].c + this.splines[index - 1].c) / 6.0 + (y[index] - y[index - 1]) / num;
            }
        }

        public double Interpolate(double x)
        {
            if (this.splines == null)
                return double.NaN;
            int length = this.splines.Length;
            CubicSpline.SplineTuple spline;
            if (x <= this.splines[0].x)
                spline = this.splines[0];
            else if (x >= this.splines[this.splines.Length - 1].x)
            {
                spline = this.splines[this.splines.Length - 1];
            }
            else
            {
                int num = 0;
                int index1 = this.splines.Length - 1;
                while (num + 1 < index1)
                {
                    int index2 = num + (index1 - num) / 2;
                    if (x <= this.splines[index2].x)
                        index1 = index2;
                    else
                        num = index2;
                }
                index1 = index1 > this.splines.Length - 1 ? this.splines.Length - 1 : index1;
                spline = this.splines[index1];
            }
            double num1 = x - spline.x;
            return spline.a + (spline.b + (spline.c / 2.0 + spline.d * num1 / 6.0) * num1) * num1;
        }

        private struct SplineTuple
        {
            public double a;
            public double b;
            public double c;
            public double d;
            public double x;
        }
    }
    public interface IInterpolation
    {
        double f(double _x);

        void calc(TwoPoints[] points);

        IInterpolation copy();
    }

    public class linearInterpolation : IInterpolation
    {
        private double[] x = new double[0];
        private double[] y = new double[0];

        public void calc(TwoPoints[] points)
        {
            this.x = ((IEnumerable<TwoPoints>)points).Select(_p => _p.X).ToArray<double>();
            this.y = ((IEnumerable<TwoPoints>)points).Select(_p => _p.Y).ToArray<double>();
        }

        public IInterpolation copy() => (IInterpolation)new linearInterpolation()
        {
            x = ((IEnumerable<double>)this.x).Select<double, double>(_x => _x).ToArray<double>(),
            y = ((IEnumerable<double>)this.y).Select<double, double>(_x => _x).ToArray<double>()
        };

        public double f(double _x)
        {
            int length = this.x.Length;
            if (length < 2)
                return 0.0;
            for (int index = 0; index < length - 1; ++index)
            {
                if (_x >= this.x[index] && _x < this.x[index + 1])
                {
                    double num1 = this.x[index + 1] - this.x[index];
                    double num2 = _x - this.x[index];
                    double num3 = this.y[index + 1] - this.y[index];
                    //return this.y[index] * (1.0 - num2 / num1) + this.y[index + 1] * num2 / num1;
                    return this.y[index] + (1.0 * num2 / num1 * num3);
                }
            }
            return 0.0;
        }
    }
}
