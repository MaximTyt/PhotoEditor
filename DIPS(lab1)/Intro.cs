using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIPS_lab1_
{
    public partial class Intro : Form
    {
        public Intro()
        {
            InitializeComponent();
            List<Bitmap> list = new List<Bitmap>();
            for (var i = 0; i < 5; i++)
                list.Add(new Bitmap($"image/{i+1}.gif"));
            pictureBox1.Image= list[new Random().Next(list.Count)];
            this.TransparencyKey = System.Drawing.Color.AliceBlue;
            this.BackColor = System.Drawing.Color.AliceBlue;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Opacity += 0.01d;
            if (Opacity == 1)
                this.Close();
        }
    }
}
