using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIPS_lab1_
{
    public class pan : Control
    {
        private List<TwoPoints> points = new List<TwoPoints>();
        public IInterpolation interpol = (IInterpolation)new CubicInterpolation();
        private TwoPoints dragingPoint;
        private int origX;
        private int origY;
        private Bitmap b;
        private Graphics g;
        private pan.State state;
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int xPos, int yPos);

        public void switchToLinear()
        {
            this.interpol = (IInterpolation)new linearInterpolation();
            this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
            this.emit();
        }

        public void switchToCubic()
        {
            this.interpol = (IInterpolation)new CubicInterpolation();
            this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
            this.emit();
        }

        public event pan.changed_delegate changed_event;

        public pan()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Paint += new PaintEventHandler(this.pan_event);
            this.MouseClick += new MouseEventHandler(this.Pan_MouseClick);
            this.MouseDown += new MouseEventHandler(this.Pan_MouseDown);
            this.MouseUp += new MouseEventHandler(this.Pan_MouseUp);
            this.MouseMove += new MouseEventHandler(this.Pan_MouseMove);
            Timer y = new Timer();
            y.Interval = 30;
            y.Tick += (EventHandler)((s, a) => this.Refresh());
            this.VisibleChanged += (EventHandler)((s, a) => y.Start());
            this.SizeChanged += (EventHandler)((s, a) =>
            {
                Size size = this.Size;
                int width = size.Width;
                size = this.Size;
                int height = size.Height;
                this.b = new Bitmap(width, height);
                this.g = Graphics.FromImage((Image)this.b);
                this.points.Add(new TwoPoints()
                {
                    X = 0.0,
                    Y = (double)this.Size.Height - 1.0
                });
                this.points.Add(new TwoPoints()
                {
                    X = (double)this.Size.Width - 1.0,
                    Y = 0.0
                });
                this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
            });
        }

        public void emit()
        {
            pan.changed_delegate changedEvent = this.changed_event;
            if (changedEvent == null)
                return;
            changedEvent(this.interpol.copy());
        }

        public void remove()
        {
            this.points.RemoveRange(1, this.points.Count - 2);
            this.points[0].X = 0.0;
            this.points[0].Y = (double)this.Size.Height - 1.0;
            this.points[1].X = (double)this.Size.Width - 1.0;
            this.points[1].Y = 0.0;
            this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
            pan.changed_delegate changedEvent = this.changed_event;
            if (changedEvent == null)
                return;
            changedEvent(this.interpol.copy());
        }

        private void Pan_MouseMove(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < this.points.Count; ++i)
            {
                if (i != 0 && i != this.points.Count - 1)
                {
                    if (this.points[i].X == this.points[i - 1].X || this.points[i].X == this.points[i + 1].X)
                    {
                        this.points.Remove(this.dragingPoint);
                        this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
                        pan.changed_delegate changedEvent = this.changed_event;
                        if (changedEvent != null)
                            changedEvent(this.interpol.copy());
                        SetCursorPos(origX, origY);
                        this.state = State.FREE;
                    }
                }
                //else
                //{
                //    if (i == 0 && this.points[i].X == this.points[i + 1].X || i == this.points.Count - 1 && this.points[i].X == this.points[i - 1].X)
                //    {
                //        this.points.Remove(this.dragingPoint);
                //        this.interpol.calc(this.points.Select<TwoPoints, TwoPoints>((Func<TwoPoints, TwoPoints>)(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1)))).ToArray<TwoPoints>());
                //        pan.changed_delegate changedEvent = this.changed_event;
                //        if (changedEvent != null)
                //            changedEvent(this.interpol.copy());
                //    }
                //}
            }
            if (this.state != State.POINT_DRAG)
                return;
            TwoPoints dragingPoint1 = this.dragingPoint;
            int x = e.X;
            Size size = this.Size;
            int num1 = size.Width - 2;
            int num2;
            if (x < num1)
            {
                num2 = e.X <= 1 ? 1 : e.X;
            }
            else
            {
                size = this.Size;
                num2 = size.Width - 2;
            }
            dragingPoint1.X = num2;
            int y = e.Y;
            size = this.Size;
            int num3 = size.Height - 2;
            int num4;
            if (y < num3)
            {
                num4 = e.Y <= 1 ? 1 : e.Y;
            }
            else
            {
                size = this.Size;
                num4 = size.Height - 1;
            }
            dragingPoint1.Y = (double)num4;            
        }

        private void Pan_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            this.state = pan.State.FREE;
            pan.changed_delegate changedEvent = this.changed_event;
            if (changedEvent == null)
                return;
            changedEvent(this.interpol.copy());
        }

        private void Pan_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                origX = e.X;
                origY = e.Y;
                for (int index = 0; index < this.points.Count; ++index)
                {

                    TwoPoints point = this.points[index];
                    if (Math.Pow(point.X - (double)e.Location.X, 2.0) + Math.Pow(point.Y - (double)e.Location.Y, 2.0) <= 225.0)
                    {
                        this.state = pan.State.POINT_DRAG;
                        this.dragingPoint = point;
                        return;
                    }
                }

                Point location = e.Location;
                double y = (double)location.Y;
                IInterpolation interpol = this.interpol;
                location = e.Location;
                double _x = 1.0 * (double)location.X / (double)(this.Size.Width - 1);
                double num = interpol.f(_x) * (double)this.Size.Height;
                if (Math.Abs(y - num) >= 30.0)
                    return;
                TwoPoints twoPoint1 = new TwoPoints();
                location = e.Location;
                twoPoint1.X = (double)location.X;
                location = e.Location;
                twoPoint1.Y = (double)location.Y;
                TwoPoints twoPoint2 = twoPoint1;
                this.points.Add(twoPoint2);
                this.points.Sort((p1, p2) => p1.X.CompareTo(p2.X));
                this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
                this.state = pan.State.POINT_DRAG;
                this.dragingPoint = twoPoint2;
            }
            else
            {
                if (e.Button != MouseButtons.Right)
                    return;
                for (int index = 1; index < this.points.Count - 1; ++index)
                {
                    TwoPoints point = this.points[index];
                    if (Math.Pow(point.X - (double)e.Location.X, 2.0) + Math.Pow(point.Y - (double)e.Location.Y, 2.0) <= 225.0)
                    {
                        this.points.Remove(point);
                        this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
                    }
                    pan.changed_delegate changedEvent = this.changed_event;
                    if (changedEvent != null)
                        changedEvent(this.interpol.copy());
                }
            }
        }

        private void Pan_MouseClick(object sender, MouseEventArgs e)
        {
        }

        public void pan_event(object sender, PaintEventArgs e)
        {
            pan pan = sender as pan;
            Graphics graphics1 = e.Graphics;
            graphics1.InterpolationMode = InterpolationMode.NearestNeighbor;
            Graphics graphics2 = graphics1;            
            Size size1 = pan.Size;
            int width1 = size1.Width;
            size1 = pan.Size;
            int height1 = size1.Height;
            graphics2.FillRectangle(Brushes.White, 0, 0, width1, height1);
            if (this.state == pan.State.POINT_DRAG)
            {
                this.points.Sort((p1, p2) => p1.X.CompareTo(p2.X));
                this.interpol.calc(this.points.Select(p => new TwoPoints(p.X / (double)(this.Size.Width - 1), p.Y / (double)(this.Size.Height - 1))).ToArray());
            }
            foreach (TwoPoints point in this.points)
            {
                int width2 = 15;
                int height2 = 15;
                graphics1.FillRectangle(Brushes.DarkRed, (int)(point.X - 0.5 * (double)width2), (int)(point.Y - 0.5 * (double)height2), width2, height2);
            }
            if (this.points.Count > 2)
            {
                for (int index = (int)this.points[0].X + 1; (double)index < this.points[this.points.Count - 1].X; ++index)
                {
                    Graphics graphics3 = graphics1;
                    int x1 = index;
                    IInterpolation interpol1 = this.interpol;
                    double num1 = 1.0 * (double)index;
                    Size size2 = this.Size;
                    double width3 = (double)size2.Width;
                    double _x1 = num1 / width3;
                    double num2 = interpol1.f(_x1);
                    double height3 = (double)size2.Height;
                    int y1 = (int)(num2 * height3);
                    int x2 = index - 1;
                    IInterpolation interpol2 = this.interpol;
                    double num3 = 1.0 * (double)(index - 1);
                    double width4 = (double)size2.Width;
                    double _x2 = num3 / width4;
                    double num4 = interpol2.f(_x2);
                    if (num4 == 0)
                        continue;
                    double height4 = (double)size2.Height;
                    int y2 = (int)(num4 * height4);
                    if (y2 <= 1)
                        y2 = 1;
                    if (y1 <= 1)
                        y1 = 1;
                    graphics3.DrawLine(Pens.BlueViolet, x1, y1, x2, y2);
                }
            }
            else
            {
                Graphics graphics3 = graphics1;
                graphics3.DrawLine(Pens.BlueViolet, (int)this.points[0].X, (int)this.points[0].Y, (int)this.points[this.points.Count - 1].X, (int)this.points[this.points.Count - 1].Y);
            }
            if (this.points[0].X != 0)
            {
                Graphics graphics3 = graphics1;
                graphics3.DrawLine(Pens.BlueViolet, 0, (int)this.points[0].Y, (int)this.points[0].X, (int)this.points[0].Y);
            }
            if (this.points[this.points.Count - 1].X != 511)
            {
                Graphics graphics3 = graphics1;
                graphics3.DrawLine(Pens.BlueViolet, 511, (int)this.points[this.points.Count - 1].Y, (int)this.points[this.points.Count - 1].X, (int)this.points[this.points.Count - 1].Y);
            }
            if (this.state != pan.State.POINT_DRAG)
                return;
            int width5 = 15;
            int height5 = 15;
            graphics1.FillRectangle(Brushes.YellowGreen, (int)(this.dragingPoint.X - 0.5 * (double)width5), (int)(this.dragingPoint.Y - 0.5 * (double)height5), width5, height5);
            pan.changed_delegate changedEvent = this.changed_event;
            if (changedEvent == null)
                return;
            changedEvent(this.interpol.copy());
        }

        public delegate void changed_delegate(IInterpolation interpol);

        private enum State
        {
            FREE,
            POINT_DRAG,
        }
    }
}
