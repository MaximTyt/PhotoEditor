using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIPS_lab1_
{
    public partial class Form6 : Form
    {
        private Bitmap img;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        public Form6(Bitmap image)
        {
            InitializeComponent();
            img = image;
            pictureBox1.Image = img;
            pictureBox2.Image = img;
            pictureBox3 = new PictureBox
            {
                Name = "pictureBox3",
                Location = new Point(20, 480),
                Size = pictureBox1.Size,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,                
            };
            pictureBox3.MouseMove += new MouseEventHandler(pictureBox3_MouseMove);
            pictureBox4 = new PictureBox
            {
                Name = "pictureBox4",
                Location = new Point(20, 970),
                Size = pictureBox1.Size,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            panel1.Controls.Add(pictureBox3);
            panel1.Controls.Add(pictureBox4);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var filter = textBox1.Text;
            double[][] filter_params=null;
            double[][] simmert = null;
            double p1 = 0;
            double p2 = 0;
            double p3 = 0;
            bool error_flag = true;
            try
            {
                //Преобразуем строки в массив
                char[] sep1 = { '\n' };
                var filter_params_strings = filter.Split(sep1, StringSplitOptions.RemoveEmptyEntries);
                filter_params_strings = (from s in filter_params_strings where (s.Trim() != string.Empty) select s).ToArray();
                var cult = new CultureInfo("en-US");
                char[] sep2 = { ';' };
                filter_params = filter_params_strings.Select(a => a.Split(sep2, StringSplitOptions.RemoveEmptyEntries)
                    .Select(b => Convert.ToDouble(b.Trim(), cult)).ToArray()).ToArray();
                if (comboBox1.SelectedIndex == 6 || comboBox1.SelectedIndex == 7)
                {
                    int j = 0;
                    for (var i = 0; i < filter_params.Length; ++i)
                    {                        
                        if (filter_params[i][0] == 0 && filter_params[i][1] == 0)
                        {
                            j++;
                        }
                        else if (filter_params[i][0] != 0 && filter_params[i][1] != 0)
                        {
                            j += 4;
                        }
                        else
                        {
                            j += 2;   
                        }
                    }
                    simmert = new double[j][];
                    j = 0;
                    for (var i = 0; i < filter_params.Length; ++i)
                    {
                        if (filter_params[i][0] == 0 && filter_params[i][1] == 0)
                        {
                            simmert[j++] = new double[3] { filter_params[i][0], filter_params[i][1], filter_params[i][2] };
                        }
                        else if (filter_params[i][0] != 0 && filter_params[i][1] != 0)
                        {
                            simmert[j++] = new double[3] { filter_params[i][0], filter_params[i][1], filter_params[i][2] };
                            simmert[j++] = new double[3] { filter_params[i][0] * (-1), filter_params[i][1] * (-1), filter_params[i][2] };
                            simmert[j++] = new double[3] { filter_params[i][0] * (-1), filter_params[i][1], filter_params[i][2] };
                            simmert[j++] = new double[3] { filter_params[i][0], filter_params[i][1] * (-1), filter_params[i][2] };
                        }
                        else
                        {
                            simmert[j++] = new double[3] { filter_params[i][0], filter_params[i][1], filter_params[i][2] };
                            simmert[j++] = new double[3] { filter_params[i][0] * (-1), filter_params[i][1] * (-1), filter_params[i][2] };
                        }
                    }                    
                    filter_params = new double[simmert.Length][];
                    simmert.CopyTo(filter_params,0);                    
                }
            }
            catch
            {
                error_flag = false;
                MessageBox.Show("Неправильный ввод области фильтра!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            if (error_flag)
            {
                try
                {
                    p1 = Convert.ToDouble(textBox2.Text);
                }
                catch
                {
                    error_flag = false;
                    MessageBox.Show("Неправильный ввод 1-ого множителя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            if (error_flag)
            {
                try
                {
                    p2 = Convert.ToDouble(textBox3.Text);
                }
                catch
                {
                    error_flag = false;
                    MessageBox.Show("Неправильный ввод 2-ого множителя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            if (error_flag)
            {
                try
                {
                    p3 = Convert.ToDouble(textBox4.Text);
                }
                catch
                {
                    error_flag = false;
                    MessageBox.Show("Неправильный ввод 3-его множителя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            if (error_flag)
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        {

                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2, p3);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                    case 1:
                        {
                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2, p3);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                    case 2:
                        {
                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2, p3);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                    case 3:
                        {
                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2, p3);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                    case 4:
                        {
                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                    case 5:
                        {
                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                    case 6:
                        {
                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2, p3);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                    case 7:
                        {
                            try
                            {
                                (pictureBox1.Image, pictureBox3.Image, pictureBox4.Image) = ImgFuncs.Frequency_filtering(img, comboBox1.SelectedIndex, filter_params, p1, p2, p3);
                            }
                            catch
                            {
                                MessageBox.Show("Неправильнный ввод области фильтрации!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            break;
                        }
                }
                pictureBox1.Refresh();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                groupBox1.Visible = true;
                groupBox2.Visible = true;
                button1.Visible = true;
                textBox1.Text = "0; 0; 0; 10";
                textBox2.Text = "1";
                textBox3.Text = "1";
                textBox4.Text = "0";
                pictureBox5.Visible = true;
            }
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Условие фильтра(В)";
                        label3.Text = "Условие фильтра(ВНЕ)";
                        textBox4.Visible = true;
                        pictureBox5.Image = new Bitmap("image/Полосовой.jpg");
                        break;
                    }
                case 1:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Условие фильтра(В)";
                        label3.Text = "Условие фильтра(ВНЕ)";
                        textBox4.Visible = true;
                        pictureBox5.Image = new Bitmap("image/Режекторный.jpg");
                        break;
                    }
                case 2:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Коэф. усиления G";
                        label3.Text = "Порядок фильтра n";
                        textBox1.Text = "0; 0; 50";
                        textBox4.Text = "2";
                        textBox4.Visible = true;
                        pictureBox5.Image = new Bitmap("image/Баттерфорт ФНЧ.jpg");
                        break;
                    }
                case 3:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Коэф. усиления G";
                        label3.Text = "Порядок фильтра n";
                        textBox1.Text = "0; 0; 50";
                        textBox4.Text = "2";
                        textBox4.Visible = true;
                        pictureBox5.Image = new Bitmap("image/Баттерфорт ФВЧ.jpg");
                        break;
                    }
                case 4:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Коэф. усиления";
                        label3.Text = "";
                        textBox1.Text = "0; 0; 50";
                        textBox4.Visible = false;
                        pictureBox5.Image = new Bitmap("image/Гаусс ФНЧ.jpg");
                        break;
                    }
                case 5:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Коэф. усиления";
                        label3.Text = "";
                        textBox1.Text = "0; 0; 50";
                        textBox4.Visible = false;
                        pictureBox5.Image = new Bitmap("image/Гаусс ФВЧ.jpg");
                        break;
                    }
                case 6:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Условие фильтра(В)";
                        label3.Text = "Условие фильтра(ВНЕ)";
                        textBox1.Text = "0; 0; 10";
                        textBox4.Visible = true;
                        pictureBox5.Image = new Bitmap("image/Узкополосный режекторный.jpg");
                        break;
                    }
                case 7:
                    {
                        label1.Text = "Множитель Фурье-образа";
                        label2.Text = "Условие фильтра(В)";
                        label3.Text = "Условие фильтра(ВНЕ)";
                        textBox1.Text = "0; 0; 10";
                        textBox4.Visible = true;
                        pictureBox5.Image = new Bitmap("image/Узкополосный полосовой.jpg");
                        break;
                    }
            }
        }

        private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        {
            //var x = e.X - (pictureBox3.Width  - pictureBox3.Image.Width / 2);
            //var y = e.Y - (pictureBox3.Height - pictureBox3.Image.Height / 2);
            //label4.Text = "Коорд. курсора (x,y) = " + x + " " + y;
        }
    }
}
