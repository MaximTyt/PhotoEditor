using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIPS_lab1_
{
    public partial class Form3 : Form
    {
        private Bitmap img;        
        private byte[] I;        
        private Bitmap b_in;
        public Form3(Bitmap image)
        {
            InitializeComponent();
            img = image;            
            pictureBox1.Image = img;
            pictureBox2.Image = img;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex!=-1)
            {
                button1.Visible = true;
            }
            switch(comboBox1.SelectedIndex)
            {
                case 2:
                    {
                        groupBox1.Visible = true;
                        groupBox2.Visible = false;
                        groupBox3.Visible = false;
                        groupBox4.Visible = false;
                        break;
                    }
                case 3:
                    {
                        groupBox2.Visible = true;
                        groupBox1.Visible = false;                        
                        groupBox3.Visible = false;
                        groupBox4.Visible = false;
                        break;
                    }
                case 4:
                    {
                        groupBox3.Visible = true;
                        groupBox1.Visible = false;
                        groupBox2.Visible = false;                        
                        groupBox4.Visible = false;
                        break;
                    }
                case 5:
                    {
                        groupBox4.Visible = true;
                        groupBox1.Visible = false;
                        groupBox2.Visible = false;
                        groupBox3.Visible = false;                        
                        break;
                    }
                default:
                    {
                        groupBox1.Visible = false;
                        groupBox2.Visible = false;
                        groupBox3.Visible = false;
                        groupBox4.Visible = false;
                        break;
                    }
            }
        }

        
        //public static byte[] GetI(Bitmap img)
        //{
        //    byte[] b_in_bytes = Form1.getImgBytes24(img);
        //    byte[] b_out_bytes = new byte[b_in_bytes.Length];
        //    byte[] I = new byte[b_in_bytes.Length];
        //    Parallel.For(0, img.Height, (i) =>
        //    {
        //        var index = i * img.Width;
        //        Parallel.For(0, img.Width, (j) =>
        //        {
        //            var idj = index + j;
        //            I[3 * idj] = (byte)Functions.Clamp((0.2125 * b_in_bytes[3 * idj + 2] + 0.7154 * b_in_bytes[3 * idj + 1] + 0.0721 * b_in_bytes[3 * idj + 0]), 0, 255);
        //            I[3 * idj + 2] = I[3 * idj + 1] = I[3 * idj];
        //        });
        //    });
        //    return I;
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    {
                        pictureBox1.Image = ImgFuncs.Method_Gavrilova((Bitmap)pictureBox2.Image);                        
                        break;
                    }
                case 1:
                    {
                        pictureBox1.Image = ImgFuncs.Method_Otsy((Bitmap)pictureBox2.Image);                        
                        break;
                    }
                case 2:
                    {
                        try
                        {
                            double k = Convert.ToDouble(textBox1.Text.Replace(".",","));
                            int a = Convert.ToInt32(numericUpDown1.Value);
                            pictureBox1.Image = ImgFuncs.Method_Nibleka((Bitmap)pictureBox2.Image, a, k);
                        }
                        catch
                        {
                            MessageBox.Show("Неправильнный ввод данных!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }

                        break;
                    }
                case 3:
                    {
                        try
                        {
                            double k = Convert.ToDouble(textBox3.Text.Replace(".", ","));
                            int a = Convert.ToInt32(numericUpDown3.Value);
                            pictureBox1.Image = ImgFuncs.Method_Sayvoly((Bitmap)pictureBox2.Image, a, k);
                        }
                        catch
                        {
                            MessageBox.Show("Неправильнный ввод данных!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                        break;
                    }
                case 4:
                    {
                        try
                        {
                            double _a = Convert.ToDouble(textBox2.Text.Replace(".", ","));
                            int a = Convert.ToInt32(numericUpDown2.Value);
                            pictureBox1.Image = ImgFuncs.Method_KristianaWolf((Bitmap)pictureBox2.Image, a, _a);                            
                        }
                        catch
                        {
                            MessageBox.Show("Неправильнный ввод данных!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                        break;
                    }
                case 5:
                    {
                        try
                        {
                            double k = Convert.ToDouble(textBox4.Text.Replace(".", ","));
                            int a = Convert.ToInt32(numericUpDown4.Value);
                            pictureBox1.Image = ImgFuncs.Method_BradleyRota((Bitmap)pictureBox2.Image, a, k);                            
                        }
                        catch
                        {
                            MessageBox.Show("Неправильнный ввод данных!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                        break;
                    }
            }
            
        }
    }
}
