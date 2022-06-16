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
    public partial class Form5 : Form
    {
        private Bitmap img; 
        private int height;
        private int width;        
        public Form5(Bitmap image)
        {
            InitializeComponent();
            img = image;            
            pictureBox1.Image = img;
            pictureBox2.Image = img;
        }

        private void button1_Click(object sender, EventArgs e)
        {            
            try
            {
                int size = Convert.ToInt32(textBox1.Text);
                if (size < 1)
                    throw new ArgumentNullException(null);
                pictureBox1.Image = ImgFuncs.Median((Bitmap)pictureBox2.Image, size);
            }
            catch
            {
                MessageBox.Show("Неправильнный ввод данных!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
    }
}
