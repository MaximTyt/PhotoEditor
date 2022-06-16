using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIPS_lab1_
{
    public partial class Form2 : Form
    {
        internal bool curveflag = false;        
        private Bitmap h = new Bitmap(256, 512);
        private Bitmap h_pb;
        private Bitmap img;
        private Graphics h_pb_g;
        private Bitmap b_out;
        private Bitmap b_in;
        private Bitmap h_pb_orig;
        private List<Task> tasks = new List<Task>();
        private Form2.update_delegate update_method;
        private byte[] b_in_bytes;
        private byte[] b_out_bytes;
        private pan canvas;
        private IContainer components;        
        public Form2(Bitmap image)
        {            
            InitializeComponent();
            this.update_method = new Form2.update_delegate(this.run);
            img = image;
            Bitmap bitmap = new Bitmap(1, 1);
            this.pictureBox2.Refresh();
            this.b_in = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            this.b_in.SetResolution(img.HorizontalResolution, img.VerticalResolution);
            this.b_out = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            this.b_out.SetResolution(img.HorizontalResolution, this.b_out.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage((Image)this.b_in))
            {
                graphics.DrawImageUnscaled((Image)img, 0, 0);                
                using (Graphics.FromImage((Image)this.b_out))
                {
                    graphics.DrawImageUnscaled((Image)img, 0, 0);
                    img.Dispose();
                    this.b_in_bytes = ImgFuncs.getImgBytes24(this.b_in);
                    this.b_out_bytes = new byte[this.b_in_bytes.Length];
                    int width1 = this.pictureBox2.Size.Width;
                    Size size = this.pictureBox2.Size;
                    int height1 = size.Height;
                    this.h_pb = new Bitmap(width1, height1);
                    size = this.pictureBox2.Size;
                    int width2 = size.Width;
                    size = this.pictureBox2.Size;
                    int height2 = size.Height;
                    this.h_pb_orig = new Bitmap(width2, height2);
                    this.h_pb_g = Graphics.FromImage((Image)this.h_pb);
                    this.h_pb_g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    this.pictureBox2.Image = (Image)this.h_pb;
                    this.canvas = new pan();
                    this.panel2.Controls.Add((Control)this.canvas);
                    this.canvas.Size = this.panel2.Size;
                    this.canvas.Location = new Point(0, 0);
                    this.comboBox1.SelectedIndex = 0;
                    this.pictureBox1.Image = (Image)this.b_out;
                    this.drawHistogram();                    
                    this.canvas.changed_event += (pan.changed_delegate)(interpol => this.tasks.Add(new Task(() => this.drawImageAndHihtogram(interpol))));
                    new Task((Action)(() =>
                    {
                        while (true)
                        {
                            if (this.tasks.Count >= 1)
                            {
                                this.tasks[this.tasks.Count - 1].RunSynchronously();
                                this.tasks.Clear();
                            }
                            Thread.Sleep(1);
                        }
                    })).Start();
                    this.Load += (EventHandler)((s, a) => this.canvas.emit());
                }
            }

        }

        public void run(object[] args)
        {
            if (!this.checkBox1.Checked)
                return;
            this.pictureBox1.Refresh();
            this.pictureBox2.Refresh();
        }

        public void drawImageAndHihtogram(IInterpolation interpol)
        {            
            if (checkBox2.Checked)
            {
                var height = pictureBox1.Image.Height;
                var width = pictureBox1.Image.Width;
                Parallel.For(0, height, (i) =>
                { 
                    var index = i * width;
                    Parallel.For(0, width / 2, (j) =>
                      {
                          var idj = index + j;
                          this.b_out_bytes[3 * idj + 0] = (byte)(255 * (1 - interpol.f(1.0 * (double)this.b_in_bytes[3 * idj + 0] / 255)));
                          this.b_out_bytes[3 * idj + 1] = (byte)(255 * (1 - interpol.f(1.0 * (double)this.b_in_bytes[3 * idj + 1] / 255)));
                          this.b_out_bytes[3 * idj + 2] = (byte)(255 * (1 - interpol.f(1.0 * (double)this.b_in_bytes[3 * idj + 2] / 255)));
                      });
                    Parallel.For(width / 2, width, (j) =>
                    {
                        var idj = index + j;
                        this.b_out_bytes[3 * idj + 0] = this.b_in_bytes[3 * idj + 0];
                        this.b_out_bytes[3 * idj + 1] = this.b_in_bytes[3 * idj + 1];
                        this.b_out_bytes[3 * idj + 2] = this.b_in_bytes[3 * idj + 2];
                    });
                });
            }
            else
            {
                Parallel.For(0, this.b_in_bytes.Length, i => this.b_out_bytes[i] = (byte)(255 * (1 - interpol.f(1.0 * (double)this.b_in_bytes[i] / 255))));                
            }
            Form2.writeImageBytes(this.b_out, this.b_out_bytes);
            int[] n = new int[256];
            Parallel.For(0, this.b_in_bytes.Length / 3, i => Interlocked.Increment(ref n[(int)((double)((int)this.b_out_bytes[i * 3] + (int)this.b_out_bytes[i * 3 + 1] + (int)this.b_out_bytes[i * 3 + 2]) / 3.0)]));
            int num1 = ((IEnumerable<int>)n).Max();
            using (Graphics graphics = Graphics.FromImage((Image)this.h))
            {
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.FillRectangle(Brushes.White, 0, 0, this.h.Width, this.h.Height);
                double num2 = 1.0 * (double)this.h.Height / (double)num1;
                for (int index = 0; index < n.Length; ++index)
                    graphics.DrawLine(Pens.Black, index, this.h.Height - 1, index, (int)((double)(this.h.Height - 1) - (double)n[index] * num2));
                this.h_pb_g.DrawImage((Image)this.h, 0, 0, this.h_pb.Width - 1, this.h_pb.Height - 1);
                this.Invoke((Delegate)this.update_method, (object)new object[0]);
            }
        }

        public void drawHistogram()
        {
            int[] n = new int[256];
            using (Bitmap bitmap = new Bitmap(256, 256))
            {
                Parallel.For(0, this.b_in_bytes.Length / 3, i => Interlocked.Increment(ref n[(int)((double)((int)this.b_in_bytes[i * 3] + (int)this.b_in_bytes[i * 3 + 1] + (int)this.b_in_bytes[i * 3 + 2]) / 3.0)]));
                int num1 = ((IEnumerable<int>)n).Max();
                using (Graphics graphics1 = Graphics.FromImage((Image)bitmap))
                {
                    graphics1.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics1.FillRectangle(Brushes.White, 0, 0, this.h.Width, this.h.Height);
                    double num2 = 1.0 * (double)bitmap.Height / (double)num1;
                    for (int index = 0; index < n.Length; ++index)
                        graphics1.DrawLine(Pens.Black, index, bitmap.Height - 1, index, (int)((double)(bitmap.Height - 1) - (double)n[index] * num2));
                    using (Graphics graphics2 = Graphics.FromImage((Image)this.h_pb_orig))
                    {
                        graphics2.InterpolationMode = InterpolationMode.NearestNeighbor;
                        graphics2.DrawImage((Image)bitmap, 0, 0, this.h_pb_orig.Width - 1, this.h_pb_orig.Height - 1);
                    }
                }
            }
        }

        static void writeImageBytes(Bitmap img, byte[] bytes)
        {
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),  //блокируем участок памати, занимаемый изображением
                ImageLockMode.WriteOnly,
                img.PixelFormat);
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length); //копируем байты массива в изображение

            img.UnlockBits(data);  //разблокируем изображение
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        public delegate void update_delegate(object[] arr);

        private void button1_Click(object sender, EventArgs e) => this.canvas.remove();

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox1.SelectedIndex == 0)
                this.canvas.switchToCubic();
            if (this.comboBox1.SelectedIndex != 1)
                return;
            this.canvas.switchToLinear();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                this.pictureBox1.Image = (Image)this.b_out;
                this.pictureBox2.Image = (Image)this.h_pb;
            }
            else
            {
                this.pictureBox2.Image = (Image)this.h_pb_orig;
                this.pictureBox1.Image = (Image)this.b_in;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox2.Checked)
            {
                Parallel.For(0, this.b_in_bytes.Length, i => this.b_out_bytes[i] = (byte)(255 * (1 - this.canvas.interpol.f(1.0 * (double)this.b_in_bytes[i] / 255))));
                Form2.writeImageBytes(this.b_out, this.b_out_bytes);
                this.pictureBox1.Refresh();
            }
            if (checkBox2.Checked)
            {
                var height = pictureBox1.Image.Height;
                var width = pictureBox1.Image.Width;
                Parallel.For(0, height, (i) =>
                {
                    var index = i * width;
                    Parallel.For(0, width / 2, (j) =>
                    {
                        var idj = index + j;
                        this.b_out_bytes[3 * idj + 0] = (byte)(255 * (1 - this.canvas.interpol.f(1.0 * (double)this.b_in_bytes[3 * idj + 0] / 255)));
                        this.b_out_bytes[3 * idj + 1] = (byte)(255 * (1 - this.canvas.interpol.f(1.0 * (double)this.b_in_bytes[3 * idj + 1] / 255)));
                        this.b_out_bytes[3 * idj + 2] = (byte)(255 * (1 - this.canvas.interpol.f(1.0 * (double)this.b_in_bytes[3 * idj + 2] / 255)));
                    });
                    Parallel.For(width / 2, width, (j) =>
                    {
                        var idj = index + j;
                        this.b_out_bytes[3 * idj + 0] = this.b_in_bytes[3 * idj + 0];
                        this.b_out_bytes[3 * idj + 1] = this.b_in_bytes[3 * idj + 1];
                        this.b_out_bytes[3 * idj + 2] = this.b_in_bytes[3 * idj + 2];
                    });
                });
                Form2.writeImageBytes(this.b_out, this.b_out_bytes);
                this.pictureBox1.Refresh();
            }
        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            curveflag = true;
            this.Close();
        }
    }
}
