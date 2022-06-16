using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DIPS_lab1_
{
    public partial class Form1 : Form
    {
        //private Bitmap image = null;
        private List<Bitmap> image = new List<Bitmap>();
        private Bitmap newnewimage = null;
        private Bitmap newimage = null;
        private Bitmap clonemask = null;
        private List<Bitmap> cloneimage = new List<Bitmap>();
        private List<Bitmap> clonenewimage = new List<Bitmap>();
        private bool maskoff = false;
        private int n = 0;
        private int wg = 0;
        private int w;
        private int h;
        private List<GroupBox> gpmas = new List<GroupBox>();
        private List<PictureBox> picmas = new List<PictureBox>();
        private List<ComboBox> combomas = new List<ComboBox>();
        private List<NumericUpDown> nummas = new List<NumericUpDown>();
        private List<Button> buttmas = new List<Button>();
        private List<CheckBox> checkmas = new List<CheckBox>();
        public Form1()
        {
            InitializeComponent();
            
            newnewimage = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = newnewimage;
            using (Graphics g = Graphics.FromImage(newnewimage))
            {
                g.Clear(Color.White);
            }
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            maskoff = true;
            toolStripTextBox2.Text = null;
            toolStripTextBox4.Text = null;
            toolStripTextBox6.Text = null;
            toolStripTextBox8.Text = null;
            maskoff = false;
        }
        
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        public static Bitmap PixelsOperation(Bitmap input, Bitmap input1, int oper)
        {
            int width = input.Width;
            int height = input.Height;
            //создаем временное изображние с нужным нам форматом хранения.
            //так как обработка побайтовая, там важно расположение байтов в картинке.
            //а оно опеределено форматом хранения

            byte[] input_bytes = new byte[0]; //пустой массивчик байт
            byte[] input_bytes1 = new byte[0]; //пустой массивчик байт


            //по этому создадим новый битмап с нужным нам 3х байтовым форматом.
            using (Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                //устанавливаем DPI такой же как у исходного
                _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

                //рисуем исходное изображение на временном, "типо-копируем"
                using (var g = Graphics.FromImage(_tmp))
                {
                    g.DrawImageUnscaled(input, 0, 0);
                }
                input_bytes = ImgFuncs.getImgBytes24(_tmp); //получаем байты изображения, см. описание ф-ции ниже
            }

            using (Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                //устанавливаем DPI такой же как у исходного
                _tmp.SetResolution(input1.HorizontalResolution, input1.VerticalResolution);

                //рисуем исходное изображение на временном, "типо-копируем"
                using (var g = Graphics.FromImage(_tmp))
                {
                    g.DrawImageUnscaled(input1, 0, 0);
                }
                input_bytes1 = ImgFuncs.getImgBytes24(_tmp); //получаем байты изображения, см. описание ф-ции ниже
            }
            //Допустим, мы обработаки картинку и сложили результат сюда:
            byte[] bytes = new byte[width * height * 3];

            //for (int i = 0; i < height; i++)
            //{
            //    var index = i * width;
            //    for (int j = 0; j < width; j++)
            //    {
            //        var idj = index + j;
            //        bytes[3 * idj + 2] = input_bytes[3 * idj + 2];
            //        bytes[3 * idj + 1] = input_bytes[3 * idj + 1];
            //        bytes[3 * idj + 0] = input_bytes[3 * idj + 0];
            //    }
            //}
            switch (oper)
            {
                case 1: //сумма
                    {
                        Parallel.For(0, height, (i) =>
                        {
                            var index = i * width;
                            Parallel.For(0, width, (j) =>
                            {
                                var idj = index + j;
                                bytes[3 * idj + 2] = (byte)Clamp(input_bytes[3 * idj + 2] + input_bytes1[3 * idj + 2], 0, 255);
                                bytes[3 * idj + 1] = (byte)Clamp(input_bytes[3 * idj + 1] + input_bytes1[3 * idj + 1], 0, 255);
                                bytes[3 * idj + 0] = (byte)Clamp(input_bytes[3 * idj + 0] + input_bytes1[3 * idj + 0], 0, 255);
                            });
                        });
                        break;
                    };
                case 2: //разность
                    {
                        Parallel.For(0, height, (i) =>
                        {
                            var index = i * width;
                            Parallel.For(0, width, (j) =>
                            {
                                var idj = index + j;
                                bytes[3 * idj + 2] = (byte)Clamp(input_bytes[3 * idj + 2] > input_bytes1[3 * idj + 2] ? input_bytes[3 * idj + 2] - input_bytes1[3 * idj + 2] : 0, 0, 255);
                                bytes[3 * idj + 1] = (byte)Clamp(input_bytes[3 * idj + 1] > input_bytes1[3 * idj + 1] ? input_bytes[3 * idj + 1] - input_bytes1[3 * idj + 1] : 0, 0, 255);
                                bytes[3 * idj + 0] = (byte)Clamp(input_bytes[3 * idj + 0] > input_bytes1[3 * idj + 0] ? input_bytes[3 * idj + 0] - input_bytes1[3 * idj + 0] : 0, 0, 255);
                            });
                        });
                        break;
                    };

                case 3: // умножение
                    {
                        Parallel.For(0, height, (i) =>
                        {
                            var index = i * width;
                            Parallel.For(0, width, (j) =>
                            {
                                var idj = index + j;
                                bytes[3 * idj + 2] = (byte)Clamp(input_bytes[3 * idj + 2] * input_bytes1[3 * idj + 2] / 255, 0, 255);
                                bytes[3 * idj + 1] = (byte)Clamp(input_bytes[3 * idj + 1] * input_bytes1[3 * idj + 1] / 255, 0, 255);
                                bytes[3 * idj + 0] = (byte)Clamp(input_bytes[3 * idj + 0] * input_bytes1[3 * idj + 0] / 255, 0, 255);
                            });
                        });
                        break;
                    };
                case 4: // среднее арифметическое
                    {
                        Parallel.For(0, height, (i) =>
                        {
                            var index = i * width;
                            Parallel.For(0, width, (j) =>
                            {
                                var idj = index + j;
                                var r = input_bytes[3 * idj + 2];
                                var g = input_bytes[3 * idj + 1];
                                var b = input_bytes[3 * idj + 0];
                                bytes[3 * idj + 2] = (byte)Clamp((input_bytes[3 * idj + 2] + input_bytes1[3 * idj + 2]) / 2, 0, 255);
                                bytes[3 * idj + 1] = (byte)Clamp((input_bytes[3 * idj + 1] + input_bytes1[3 * idj + 1]) / 2, 0, 255);
                                bytes[3 * idj + 0] = (byte)Clamp((input_bytes[3 * idj + 0] + input_bytes1[3 * idj + 0]) / 2, 0, 255);
                            });
                        });
                        break;
                    };
                case 5: // минимум
                    {
                        Parallel.For(0, height, (i) =>
                        {
                            var index = i * width;
                            Parallel.For(0, width, (j) =>
                            {
                                var idj = index + j;
                                var r1 = input_bytes[3 * idj + 2];
                                var g1 = input_bytes[3 * idj + 1];
                                var b1 = input_bytes[3 * idj + 0];
                                var r2 = input_bytes1[3 * idj + 2];
                                var g2 = input_bytes1[3 * idj + 1];
                                var b2 = input_bytes1[3 * idj + 0];
                                if (r1 > r2)
                                    bytes[3 * idj + 2] = r2;
                                else
                                    bytes[3 * idj + 2] = r1;
                                if (g1 > g2)
                                    bytes[3 * idj + 1] = g2;
                                else
                                    bytes[3 * idj + 1] = g1;
                                if (b1 > b2)
                                    bytes[3 * idj + 0] = b2;
                                else
                                    bytes[3 * idj + 0] = b1;
                            });
                        });
                        break;
                    };
                case 6: // максимум
                    {
                        Parallel.For(0, height, (i) =>
                        {
                            var index = i * width;
                            Parallel.For(0, width, (j) =>
                            {
                                var idj = index + j;
                                var r1 = input_bytes[3 * idj + 2];
                                var g1 = input_bytes[3 * idj + 1];
                                var b1 = input_bytes[3 * idj + 0];
                                var r2 = input_bytes1[3 * idj + 2];
                                var g2 = input_bytes1[3 * idj + 1];
                                var b2 = input_bytes1[3 * idj + 0];
                                if (r1 > r2)
                                    bytes[3 * idj + 2] = r1;
                                else
                                    bytes[3 * idj + 2] = r2;
                                if (g1 > g2)
                                    bytes[3 * idj + 1] = g1;
                                else
                                    bytes[3 * idj + 1] = g2;
                                if (b1 > b2)
                                    bytes[3 * idj + 0] = b1;
                                else
                                    bytes[3 * idj + 0] = b2;
                            });
                        });
                        break;
                    };
            }

                    //Теперь надо сложить новые байты в битмап, 
                    //создаем выходное изображние (отбратите внимание, без using!!, иначе будет нечего возвращать)
                    Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            ImgFuncs.writeImageBytes(img_ret, bytes);

            return img_ret;
        }

        public static Bitmap ChangeColor(Bitmap input, bool R, bool G, bool B)
        {
            int width = input.Width;
            int height = input.Height;
            //создаем временное изображние с нужным нам форматом хранения.
            //так как обработка побайтовая, там важно расположение байтов в картинке.
            //а оно опеределено форматом хранения

            byte[] input_bytes = new byte[0]; //пустой массивчик байт

            //по этому создадим новый битмап с нужным нам 3х байтовым форматом.
            using (Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                //устанавливаем DPI такой же как у исходного
                _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

                //рисуем исходное изображение на временном, "типо-копируем"
                using (var g = Graphics.FromImage(_tmp))
                {
                    g.DrawImageUnscaled(input, 0, 0);
                }
                input_bytes = ImgFuncs.getImgBytes32(_tmp); //получаем байты изображения, см. описание ф-ции ниже

            }

            //Допустим, мы обработаки картинку и сложили результат сюда:
            byte[] bytes = new byte[width * height * 4];            

            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    bytes[4 * idj + 3] = input_bytes[4 * idj + 3];
                    if (!R)
                        bytes[4 * idj + 2] = 0;
                    else
                        bytes[4 * idj + 2] = input_bytes[4 * idj + 2];
                    if (!G)
                        bytes[4 * idj + 1] = 0;
                    else
                        bytes[4 * idj + 1] = input_bytes[4 * idj + 1];
                    if (!B)
                        bytes[4 * idj + 0] = 0;
                    else
                        bytes[4 * idj + 0] = input_bytes[4 * idj + 0];
                });
            });
            //Теперь надо сложить новые байты в битмап, 
            //создаем выходное изображние (отбратите внимание, без using!!, иначе будет нечего возвращать)
            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            ImgFuncs.writeImageBytes(img_ret, bytes);

            return img_ret;
        }

        public static Bitmap Mask(Bitmap input, int side, int oper)
        {
            int width = input.Width;
            int height = input.Height;
            int centerW = input.Width / 2;
            int centerH = input.Height / 2;
            //создаем временное изображние с нужным нам форматом хранения.
            //так как обработка побайтовая, там важно расположение байтов в картинке.
            //а оно опеределено форматом хранения

            byte[] input_bytes = new byte[0]; //пустой массивчик байт

            //по этому создадим новый битмап с нужным нам 3х байтовым форматом.
            using (Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                //устанавливаем DPI такой же как у исходного
                _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

                //рисуем исходное изображение на временном, "типо-копируем"
                using (var g = Graphics.FromImage(_tmp))
                {
                    g.DrawImageUnscaled(input, 0, 0);
                }
                input_bytes = ImgFuncs.getImgBytes32(_tmp); //получаем байты изображения, см. описание ф-ции ниже

            }

            //Допустим, мы обработаки картинку и сложили результат сюда:
            byte[] bytes = new byte[width * height * 4];

            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    if (oper == 0)
                    {
                        if (Math.Pow((i - centerH), 2) + Math.Pow((j - centerW), 2) > Math.Pow(side, 2))
                        { }
                        else
                        {
                            bytes[4 * idj + 3] = input_bytes[4 * idj + 3];
                            bytes[4 * idj + 2] = input_bytes[4 * idj + 2];
                            bytes[4 * idj + 1] = input_bytes[4 * idj + 1];
                            bytes[4 * idj + 0] = input_bytes[4 * idj + 0];
                        }
                    }
                    else
                    {
                        if (j < centerW - side / 2 || j > centerW + side / 2 || i < centerH - side / 2 || i > centerH + side / 2)
                        { }
                        else
                        {
                            bytes[4 * idj + 3] = input_bytes[4 * idj + 3];
                            bytes[4 * idj + 2] = input_bytes[4 * idj + 2];
                            bytes[4 * idj + 1] = input_bytes[4 * idj + 1];
                            bytes[4 * idj + 0] = input_bytes[4 * idj + 0];
                        }
                    }
                });
            });
            //Теперь надо сложить новые байты в битмап, 
            //создаем выходное изображние (отбратите внимание, без using!!, иначе будет нечего возвращать)
            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            ImgFuncs.writeImageBytes(img_ret, bytes);

            return img_ret;
        }

        public static Bitmap ChangeColor(Bitmap input, bool R, bool G, bool B, int clarity)
        {
            int width = input.Width;
            int height = input.Height;
            //создаем временное изображние с нужным нам форматом хранения.
            //так как обработка побайтовая, там важно расположение байтов в картинке.
            //а оно опеределено форматом хранения

            byte[] input_bytes = new byte[0]; //пустой массивчик байт

            //по этому создадим новый битмап с нужным нам 3х байтовым форматом.
            using (Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                //устанавливаем DPI такой же как у исходного
                _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

                //рисуем исходное изображение на временном, "типо-копируем"
                using (var g = Graphics.FromImage(_tmp))
                {
                    g.DrawImageUnscaled(input, 0, 0);
                }
                input_bytes = ImgFuncs.getImgBytes32(_tmp); //получаем байты изображения, см. описание ф-ции ниже

            }

            //Допустим, мы обработаки картинку и сложили результат сюда:
            byte[] bytes = new byte[width * height * 4];

            

            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    bytes[4 * idj + 3] = (byte)(clarity * 255 / 100);
                    if (!R)
                        bytes[4 * idj + 2] = 0;
                    else
                        bytes[4 * idj + 2] = input_bytes[4 * idj + 2];
                    if (!G)
                        bytes[4 * idj + 1] = 0;
                    else
                        bytes[4 * idj + 1] = input_bytes[4 * idj + 1];
                    if (!B)
                        bytes[4 * idj + 0] = 0;
                    else
                        bytes[4 * idj + 0] = input_bytes[4 * idj + 0];
                });
            });
            //Теперь надо сложить новые байты в битмап, 
            //создаем выходное изображние (отбратите внимание, без using!!, иначе будет нечего возвращать)
            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            ImgFuncs.writeImageBytes(img_ret, bytes);

            return img_ret;
        }

        
        private void ChangeImage()
        {
            var flag = 0;

            for (int ii = n - 1; ii >= 0; ii--)
            {
                if (combomas[ii].SelectedIndex == 0)
                {
                    flag++;
                    if (flag == n)
                    {
                        using (Graphics g = Graphics.FromImage(newnewimage))
                        {
                            g.Clear(Color.White);
                        }
                        pictureBox2.Image = newnewimage;
                    }
                }
                else
                {
                    var copyflag = flag;                    
                    flag = 0;
                    newimage = (Bitmap)image[ii].Clone();
                    if (ii == n-1 - copyflag)
                    {
                        newnewimage = (Bitmap)newimage.Clone();
                        pictureBox2.Image = newimage;
                        //pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                        if (toolStripTextBox2.Text != "" || toolStripTextBox4.Text != "" || (toolStripTextBox6.Text != "" && toolStripTextBox8.Text != ""))
                            Mask();
                        Mask();
                        continue;
                    }
                    if (ii == n - 1 - copyflag - 1 && clonemask!=null)
                        newnewimage = (Bitmap)clonemask.Clone();
                    else
                        newnewimage = (Bitmap)pictureBox2.Image.Clone();
                    h = newnewimage.Height;
                    w = newnewimage.Width;
                    if (w * h != newimage.Width * newimage.Height)
                    {
                        if (w * h > newimage.Width * newimage.Height)
                        {
                            newimage = ResizeImage(newimage, w, h);
                        }
                        else if (w * h < newimage.Width * newimage.Height)
                        {
                            newnewimage = ResizeImage(newnewimage, newimage.Width, newimage.Height);
                            w = newimage.Width;
                            h = newimage.Height;
                        }
                    }
                    var img_out = PixelsOperation(newnewimage, newimage, combomas[ii].SelectedIndex);

                    newnewimage = (Bitmap)img_out.Clone();
                    pictureBox2.Image = newnewimage;
                    //pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;                    
                    if (toolStripTextBox2.Text!="" || toolStripTextBox4.Text != "" || (toolStripTextBox6.Text != "" && toolStripTextBox8.Text != ""))
                        Mask();
                }
            }
        }   
        private void combo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ChangeImage();
        }
        private void bDelete_Click(object sender, EventArgs e)
        {
            int loc = 0;
            Button temp = sender as Button;
            var name = temp.Name;
            int index = 0;
            for (int ii = 0; ii <=n-1 ; ii++)
            {
                if (buttmas[ii].Name == name)
                {
                    index = ii;
                    //this.Controls.Remove(gpmas[ii] as Control);
                    gpmas[ii].Dispose();
                    gpmas.RemoveAt(ii);
                    image.RemoveAt(ii);
                    clonenewimage.RemoveAt(ii);
                    picmas.RemoveAt(ii);
                    combomas.RemoveAt(ii);
                    nummas.RemoveAt(ii);
                    buttmas.RemoveAt(ii);
                    checkmas.RemoveAt(ii * 3);
                    checkmas.RemoveAt(ii * 3);
                    checkmas.RemoveAt(ii * 3);
                    n--;
                    //if(n==0)
                    //{
                    //    newnewimage = new Bitmap(pictureBox2.Width, pictureBox2.Height);
                    //    pictureBox2.Image = newnewimage;
                    //    using (Graphics g = Graphics.FromImage(newnewimage))
                    //    {
                    //        g.Clear(Color.White);
                    //    }
                    //}
                    break;
                }
            }
            if (index!=0)
                loc = gpmas[index-1].Location.Y;
            for (int ii = index; ii < n; ii++)
            {
                gpmas[ii].Location = new Point(10, ii == 0 ? loc : loc + 200);
                gpmas[ii].Name = "grpBox" + (ii + 1).ToString();
                gpmas[ii].Text = "Изображение №" + (ii + 1).ToString();
                loc = gpmas[ii].Location.Y;
            }
            pictureBox2.Image = newnewimage;
            using (Graphics g = Graphics.FromImage(newnewimage))
            {
                g.Clear(Color.White);
            }
            ChangeImage();
        }


        private void ChangeColorChanel(bool R, bool G, bool B, int ii)
        {
            //int r, g, b;
            if (newimage != null)
            {
                if (clonenewimage[ii] != null)
                {
                    image[ii] = ChangeColor(clonenewimage[ii], true, true, true);
                    clonenewimage[ii] = null;
                }
                clonenewimage[ii] = (Bitmap)image[ii].Clone();
                image[ii]= ChangeColor(clonenewimage[ii],R,G,B, (int)nummas[ii].Value);                
                
                //Mask();
            }
            //pictureBox2.Image = newnewimage;
        }

        private void cChange_Color(object sender, EventArgs e)
        {
            bool r, g, b;
            for (int ii = 0; ii <= 3 * n - 1; ii += 3)
            {
                r = false;
                g = false;
                b = false;
                if (checkmas[ii].Checked == true)
                    r = true;
                if (checkmas[ii + 1].Checked == true)
                    g = true;
                if (checkmas[ii + 2].Checked == true)
                    b = true;                
                ChangeColorChanel(r, g, b, ii / 3);
            }
            ChangeImage();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.Filter = "Картинки (png, jpg, bmp, gif) |*.png;*.jpg;*.bmp;*.gif|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    var i = 0;
                    if (n != 0)
                        i = 3*n;
                    var flag = 0;
                    try
                    {
                        //if (image != null)
                        //{
                        //    //pictureBox.Image = null;
                        //    image.Dispose();
                        //}
                        image.Add(new Bitmap(file));
                        //image[n] = new Bitmap(image[n].Width, image[n].Height, PixelFormat.Format32bppRgb);
                        clonenewimage.Add(image[n]);
                        clonenewimage[n] = null;
                        cloneimage.Add(image[n]);
                        cloneimage[n] = null;
                        //pictureBox.Image = image;/*Image.FromFile(openFileDialog.FileName);*/
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось открыть картинку", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //image.RemoveAt(n);
                        flag = 1;
                    }
                    if (flag == 0)
                    {
                        var groupBox = new GroupBox
                        {
                            Name = "grpBox" + (n + 1).ToString(),
                            Size = new Size(275, 196),
                            Location = new Point(5, n == 0 ? wg : wg +200),
                            Text = "Изображение №" + (n + 1).ToString()
                        };
                        var pictureBox = new PictureBox
                        {
                            Name = "picBox" + (n + 1).ToString(),
                            Location = new Point(70, 26),
                            Size = new Size(180, 100),
                            BorderStyle = BorderStyle.FixedSingle,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            //Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                            Image = (Image)image[n].Clone()
                        };
                        var comboBox = new ComboBox
                        {
                            Name = "combBox" + (n + 1).ToString(),
                            Location = new Point(85, 137),
                            Size = new Size(149, 24),
                            Items = { "нет", "сумма", "разность", "умножение","среднее арифм.", "минимум", "максимум" },
                            SelectedItem = "нет",
                            //Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                        };
                        var label0 = new Label
                        {
                            Name = "labell" + (n + 1).ToString(),
                            Location = new Point(38, 164),
                            Size = new Size(90, 18),
                            Font = new Font("Times New Roman", 9),
                            Text = "Прозрачность:",
                        };
                        var numUpDown = new NumericUpDown
                        {
                            Name = "numUpDown" + (n + 1).ToString(),
                            Location = new Point(130, 164),
                            Size = new Size(45, 22),
                            Value = 100
                        };
                        var label = new Label
                        {
                            Name = "labelll" + (n + 1).ToString(),
                            Location = new Point(177, 166),
                            Size = new Size(18, 18),
                            Font = new Font("Times New Roman", 9),
                            Text = "%"
                        };
                        var button = new Button
                        {
                            Name = "buton" + (n + 1).ToString(),
                            Location = new Point(250, 16),
                            Size = new Size(20, 20),                            
                            Text = "X",
                        };
                        var checkbox = new CheckBox
                        {
                            Name = "checkbox" + (n + 1).ToString(),
                            Location = new Point(10, 20),
                            Size = new Size(28, 30),
                            Text = "R",
                            Checked=true
                        };
                        var checkbox1 = new CheckBox
                        {
                            Name = "checkbox1" + (n + 1).ToString(),
                            Location = new Point(10, 42),
                            Size = new Size(28, 30),
                            Text = "G",
                            Checked=true
                        };
                        var checkbox2 = new CheckBox
                        {
                            Name = "checkbox2" + (n + 1).ToString(),
                            Location = new Point(10, 64),
                            Size = new Size(28, 30),
                            Text = "B",
                            Checked=true
                        };
                        clonenewimage[n] = null;
                        gpmas.Add(groupBox);
                        picmas.Add(pictureBox);
                        combomas.Add(comboBox);
                        nummas.Add(numUpDown);
                        buttmas.Add(button);
                        checkmas.Add(checkbox);
                        checkmas.Add(checkbox1);
                        checkmas.Add(checkbox2);
                        groupBox.Controls.Add(pictureBox);
                        groupBox.Controls.Add(comboBox);
                        groupBox.Controls.Add(label0);
                        groupBox.Controls.Add(numUpDown);
                        groupBox.Controls.Add(label);
                        groupBox.Controls.Add(button);
                        groupBox.Controls.Add(checkbox);
                        groupBox.Controls.Add(checkbox1);
                        groupBox.Controls.Add(checkbox2);
                        wg = groupBox.Location.Y;
                        panel1.Controls.Add(groupBox);
                        //pictureBox2.Image = (Image)image[n].Clone();
                        //this.Controls.OfType<ComboBox>().AsParallel().ForAll(item => { item.SelectionChangeCommitted += new EventHandler(combo_SelectionChangeCommitted); });
                        combomas[n].SelectionChangeCommitted += new EventHandler(combo_SelectionChangeCommitted);
                        buttmas[n].Click += new EventHandler(bDelete_Click);
                        nummas[n].ValueChanged += new EventHandler(cChange_Сlarity);
                        for(int ii=i;ii<i+3;ii++)
                            checkmas[ii].CheckedChanged += new EventHandler(cChange_Color);
                        n++;
                    }
                }
            }
        }
        public void Clarity (int clarity, int ii)
        {
            if (newimage != null)
            {
                if (cloneimage[ii] != null)
                {
                    image[ii] = ChangeColor(image[ii], true, true, true);
                    cloneimage[ii] = null;
                }
                cloneimage[ii] = (Bitmap)image[ii].Clone();
                image[ii] = ChangeColor(cloneimage[ii], true, true, true, clarity);                
            }
        }

        private void cChange_Сlarity(object sender, EventArgs e)
        {            
            NumericUpDown temp = sender as NumericUpDown;
            var name = temp.Name;            
            for (int ii = 0; ii <= n - 1; ii++)
            {
                if (nummas[ii].Name == name)
                {
                    Clarity((int)temp.Value, ii);
                    break;
                }
            }            
            ChangeImage();
            //Mask();
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileFialog = new SaveFileDialog();
            saveFileFialog.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileFialog.Filter = "Картинки (png, jpg, bmp, gif) |*.png;*.jpg;*.bmp;*.gif|All files (*.*)|*.*";
            saveFileFialog.RestoreDirectory = true;

            if (saveFileFialog.ShowDialog() == DialogResult.OK)
            {
                newnewimage = (Bitmap)pictureBox2.Image.Clone();
                if (newnewimage != null)
                {
                    newnewimage.Save(saveFileFialog.FileName);
                }
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);  
            foreach (string file in files)
            {
                var i = 0;
                if (n != 0)
                    i = 3 * n;
                var flag = 0;
                try
                {
                    //if (image != null)
                    //{
                    //    //pictureBox.Image = null;
                    //    image.Dispose();
                    //}
                    image.Add(new Bitmap(Image.FromFile(file)));
                    //image[n] = new Bitmap(image[n].Width, image[n].Height, PixelFormat.Format32bppRgb);
                    clonenewimage.Add(image[n]);
                    clonenewimage[n] = null;
                    cloneimage.Add(image[n]);
                    cloneimage[n] = null;
                    //pictureBox.Image = image;/*Image.FromFile(openFileDialog.FileName);*/
                }
                catch
                {
                    MessageBox.Show("Не удалось открыть картинку", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //image.RemoveAt(n);
                    flag = 1;
                }
                if (flag == 0)
                {
                    var groupBox = new GroupBox
                    {
                        Name = "grpBox" + (n + 1).ToString(),
                        Size = new Size(275, 196),
                        Location = new Point(5, n == 0 ? wg : wg + 200),
                        Text = "Изображение №" + (n + 1).ToString()
                    };
                    var pictureBox = new PictureBox
                    {
                        Name = "picBox" + (n + 1).ToString(),
                        Location = new Point(70, 26),
                        Size = new Size(180, 100),
                        BorderStyle = BorderStyle.FixedSingle,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        //Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                        Image = (Image)image[n].Clone()
                    };
                    var comboBox = new ComboBox
                    {
                        Name = "combBox" + (n + 1).ToString(),
                        Location = new Point(85, 137),
                        Size = new Size(149, 24),
                        Items = { "нет", "сумма", "разность", "умножение", "среднее арифм.", "минимум", "максимум" },
                        SelectedItem = "нет",
                        //Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                    };
                    var label0 = new Label
                    {
                        Name = "labell" + (n + 1).ToString(),
                        Location = new Point(38, 164),
                        Size = new Size(90, 18),
                        Font = new Font("Times New Roman", 9),
                        Text = "Прозрачность:",
                    };
                    var numUpDown = new NumericUpDown
                    {
                        Name = "numUpDown" + (n + 1).ToString(),
                        Location = new Point(130, 164),
                        Size = new Size(45, 22),
                        Value = 100
                    };
                    var label = new Label
                    {
                        Name = "labelll" + (n + 1).ToString(),
                        Location = new Point(177, 166),
                        Size = new Size(18, 18),
                        Font = new Font("Times New Roman", 9),
                        Text = "%"
                    };
                    var button = new Button
                    {
                        Name = "buton" + (n + 1).ToString(),
                        Location = new Point(250, 16),
                        Size = new Size(20, 20),
                        Text = "X",
                    };
                    var checkbox = new CheckBox
                    {
                        Name = "checkbox" + (n + 1).ToString(),
                        Location = new Point(10, 20),
                        Size = new Size(28, 30),
                        Text = "R",
                        Checked = true
                    };
                    var checkbox1 = new CheckBox
                    {
                        Name = "checkbox1" + (n + 1).ToString(),
                        Location = new Point(10, 42),
                        Size = new Size(28, 30),
                        Text = "G",
                        Checked = true
                    };
                    var checkbox2 = new CheckBox
                    {
                        Name = "checkbox2" + (n + 1).ToString(),
                        Location = new Point(10, 64),
                        Size = new Size(28, 30),
                        Text = "B",
                        Checked = true
                    };
                    clonenewimage[n] = null;
                    gpmas.Add(groupBox);
                    picmas.Add(pictureBox);
                    combomas.Add(comboBox);
                    nummas.Add(numUpDown);
                    buttmas.Add(button);
                    checkmas.Add(checkbox);
                    checkmas.Add(checkbox1);
                    checkmas.Add(checkbox2);
                    groupBox.Controls.Add(pictureBox);
                    groupBox.Controls.Add(comboBox);
                    groupBox.Controls.Add(label0);
                    groupBox.Controls.Add(numUpDown);
                    groupBox.Controls.Add(label);
                    groupBox.Controls.Add(button);
                    groupBox.Controls.Add(checkbox);
                    groupBox.Controls.Add(checkbox1);
                    groupBox.Controls.Add(checkbox2);
                    wg = groupBox.Location.Y;
                    panel1.Controls.Add(groupBox);                    
                    combomas[n].SelectionChangeCommitted += new EventHandler(combo_SelectionChangeCommitted);
                    buttmas[n].Click += new EventHandler(bDelete_Click);
                    nummas[n].ValueChanged += new EventHandler(cChange_Сlarity);
                    for (int ii = i; ii < i + 3; ii++)
                        checkmas[ii].CheckedChanged += new EventHandler(cChange_Color);
                    n++;
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Mask()
        {
            var side = 0;
            var width = 0;
            var height = 0;
            var radius = 0;
            var param = -1;
            if (toolStripTextBox2.Text != "")
                param = 0;
            else if (toolStripTextBox4.Text != "")
                param = 1;
            else if (toolStripTextBox6.Text != "" && toolStripTextBox8.Text != "")
                param = 2;
            var flag = true;
            var maskimage = (Bitmap)newnewimage.Clone();
            if (maskimage != null)
            {
                switch (param)
                {
                    case 0:
                        {
                            try
                            {
                                radius = Convert.ToInt32(toolStripTextBox2.ToString());
                            }
                            catch
                            {
                                if (radius == 0)
                                    MessageBox.Show("Введите целое число > 0!", "Ошибка ввода радиуса!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                pictureBox2.Image = clonemask;
                                maskoff = true;
                                toolStripTextBox2.Text = null;
                                maskoff = false;
                                flag = false;
                            }
                            if (flag)
                            {
                                clonemask = (Bitmap)maskimage.Clone();
                                maskimage = Mask(clonemask, radius, 0);
                            }
                            break;
                        };
                    case 1:
                        {
                            try
                            {
                                side = Convert.ToInt32(toolStripTextBox4.ToString());
                            }
                            catch
                            {
                                if (side == 0)
                                    MessageBox.Show("Введите целое число!", "Ошибка ввода стороны квадрата!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                pictureBox2.Image = clonemask;
                                maskoff = true;
                                toolStripTextBox4.Text = null;
                                maskoff = false;
                                flag = false;
                            }
                            if (flag)
                            {
                                clonemask = (Bitmap)maskimage.Clone();
                                maskimage = Mask(clonemask, side, 1);
                            }
                            break;
                        };
                    case 2:
                        {
                            try
                            {                                
                                height = Convert.ToInt32(toolStripTextBox6.ToString());
                                width = Convert.ToInt32(toolStripTextBox8.ToString());
                            }
                            catch
                            {
                                if (height == 0)
                                {
                                    MessageBox.Show("Введите целое число > 0!", "Ошибка ввода длины!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    maskoff = true;
                                    toolStripTextBox6.Text = null;
                                    maskoff = false;
                                }
                                if (width == 0)
                                {
                                    MessageBox.Show("Введите целое число > 0!", "Ошибка ввода ширины!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    maskoff = true;
                                    toolStripTextBox8.Text = null;
                                    maskoff = false;
                                }
                                pictureBox2.Image = clonemask;
                                
                                flag = false;
                            }
                            if (flag)
                            {
                                clonemask = (Bitmap)maskimage.Clone();
                                maskimage = MaskRect(clonemask, height, width);
                            }
                            break;
                        };
                }
                if(param!=-1)
                    pictureBox2.Image = maskimage;
            }
        }

        private Bitmap MaskRect(Bitmap input, int h, int w)
        {
            int width = input.Width;
            int height = input.Height;
            int centerW = input.Width / 2;
            int centerH = input.Height / 2;
            //создаем временное изображние с нужным нам форматом хранения.
            //так как обработка побайтовая, там важно расположение байтов в картинке.
            //а оно опеределено форматом хранения

            byte[] input_bytes = new byte[0]; //пустой массивчик байт

            //по этому создадим новый битмап с нужным нам 3х байтовым форматом.
            using (Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                //устанавливаем DPI такой же как у исходного
                _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

                //рисуем исходное изображение на временном, "типо-копируем"
                using (var g = Graphics.FromImage(_tmp))
                {
                    g.DrawImageUnscaled(input, 0, 0);
                }
                input_bytes = ImgFuncs.getImgBytes32(_tmp); //получаем байты изображения, см. описание ф-ции ниже
            }
            //Допустим, мы обработаки картинку и сложили результат сюда:
            byte[] bytes = new byte[width * height * 4];

            int index = 0, idj = 0;

            for (int i = 0; i < height; i++)
            {
                index = i * width;
                for (int j = 0; j < width; j++)
                {
                    idj = index + j;
                    if (j < centerW - w / 2 || j > centerW + w / 2 || i < centerH - h / 2 || i > centerH + h / 2)
                    { }
                    else
                    {
                        bytes[4 * idj + 3] = input_bytes[4 * idj + 3];
                        bytes[4 * idj + 2] = input_bytes[4 * idj + 2];
                        bytes[4 * idj + 1] = input_bytes[4 * idj + 1];
                        bytes[4 * idj + 0] = input_bytes[4 * idj + 0];
                    }
                }
            }
            //Теперь надо сложить новые байты в битмап, 
            //создаем выходное изображние (отбратите внимание, без using!!, иначе будет нечего возвращать)
            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            ImgFuncs.writeImageBytes(img_ret, bytes);

            return img_ret;
        }

        private void MaskOff()
        {           
            pictureBox2.Image = clonemask;
            maskoff = true;
            toolStripTextBox2.Text = null;
            toolStripTextBox4.Text = null;
            toolStripTextBox6.Text = null;
            toolStripTextBox8.Text = null;
            maskoff = false;
        }
        private void toolStripTextBox4_TextChanged(object sender, EventArgs e)
        {
            if(!maskoff)
            {
                maskoff = true;
                toolStripTextBox2.Text = null;
                toolStripTextBox6.Text = null;
                toolStripTextBox8.Text = null;
                maskoff = false;
                Mask();
            }
        }

        private void отменаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MaskOff();
        }

        private void toolStripTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (!maskoff)
            {
                maskoff = true;
                toolStripTextBox4.Text = null;
                toolStripTextBox6.Text = null;
                toolStripTextBox8.Text = null;
                maskoff = false;
                Mask();
            }
        }

        private void toolStripTextBox6_TextChanged(object sender, EventArgs e)
        {
            if (!maskoff)
            {
                maskoff = true;
                toolStripTextBox2.Text = null;
                toolStripTextBox4.Text = null;                
                maskoff = false;
                if (toolStripTextBox8.Text != "") 
                    Mask();
            }
        }

        private void toolStripTextBox8_TextChanged(object sender, EventArgs e)
        {
            if (!maskoff)
            {
                maskoff = true;
                toolStripTextBox2.Text = null;
                toolStripTextBox4.Text = null;
                maskoff = false;
                if (toolStripTextBox6.Text != "")
                    Mask();
            }
        }

        private void градПреобразованияToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            Form2 f2 = new Form2((Bitmap)pictureBox2.Image.Clone());            
            f2.ShowDialog();
            if (f2.curveflag)
                pictureBox2.Image = (Image)f2.pictureBox1.Image.Clone();
            
        }

        private void бинаризацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3((Bitmap)pictureBox2.Image.Clone());
            f3.ShowDialog();
        }

        private void линейнаяФильтрацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 f4 = new Form4((Bitmap)pictureBox2.Image.Clone());
            f4.ShowDialog();
        }

        private void медианнаяФильтрацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form5 f5 = new Form5((Bitmap)pictureBox2.Image.Clone());
            f5.ShowDialog();
        }

        private void частотнаяФильтрацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form6 f6 = new Form6((Bitmap)pictureBox2.Image.Clone());
            f6.ShowDialog();
        }
    }
}
