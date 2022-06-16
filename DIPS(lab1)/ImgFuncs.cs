using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DIPS_lab1_
{
    internal class ImgFuncs
    {
        internal static byte[] getImgBytes32(Bitmap img)
        {
            byte[] bytes = new byte[img.Width * img.Height * 4];  //выделяем память под массив байтов
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),  //блокируем участок памати, занимаемый изображением
                ImageLockMode.ReadOnly,
                img.PixelFormat);
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);  //копируем байты изображения в массив
            img.UnlockBits(data);   //разблокируем изображение
            return bytes; //возвращаем байты
        }
        internal static byte[] getImgBytes24(Bitmap img)
        {
            byte[] bytes = new byte[img.Width * img.Height * 3];  //выделяем память под массив байтов
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),  //блокируем участок памати, занимаемый изображением
                ImageLockMode.ReadOnly,
                img.PixelFormat);
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);  //копируем байты изображения в массив
            img.UnlockBits(data);   //разблокируем изображение
            return bytes; //возвращаем байты
        }
        internal static void writeImageBytes(Bitmap img, byte[] bytes)
        {
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),  //блокируем участок памати, занимаемый изображением
                ImageLockMode.WriteOnly,
                img.PixelFormat);
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length); //копируем байты массива в изображение

            img.UnlockBits(data);  //разблокируем изображение
        }
        public static Bitmap Method_Gavrilova(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;

            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }
            byte[] b_in_bytes = getImgBytes24(input);
            byte[] b_out_bytes = new byte[b_in_bytes.Length];
            byte[] I = new byte[b_in_bytes.Length];
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    I[3 * idj] = (byte)Functions.Clamp((0.2125 * b_in_bytes[3 * idj + 2] + 0.7154 * b_in_bytes[3 * idj + 1] + 0.0721 * b_in_bytes[3 * idj + 0]), 0, 255);
                    I[3 * idj + 2] = I[3 * idj + 1] = I[3 * idj];
                });
            });
            byte[] out_bytes = new byte[width * height * 3];
            var t = I.Average(x => x);
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    out_bytes[3 * idj + 0] = out_bytes[3 * idj + 1] = out_bytes[3 * idj + 2] = (I[3 * idj] <= t) ? (byte)0 : (byte)255;
                });
            });
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }

        public static Bitmap Method_Otsy(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;

            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }
            byte[] b_in_bytes = getImgBytes24(input);
            byte[] b_out_bytes = new byte[b_in_bytes.Length];
            byte[] I = new byte[b_in_bytes.Length];
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    I[3 * idj] = (byte)Functions.Clamp((0.2125 * b_in_bytes[3 * idj + 2] + 0.7154 * b_in_bytes[3 * idj + 1] + 0.0721 * b_in_bytes[3 * idj + 0]), 0, 255);
                    I[3 * idj + 2] = I[3 * idj + 1] = I[3 * idj];
                });
            });
            byte[] out_bytes = new byte[width * height * 3];
            double[] N = new double[256];
            double[] sum_N = new double[256];
            double[] sum_iN = new double[256];
            double sum = 0.0, isum = 0.0, max_t = 0.0, max_sigma = 0.0, w1 = 0.0, w2 = 0.0, u1 = 0.0, u2 = 0.0;
            var max_I = I.Max(x => x);
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    N[I[3 * idj]] += 1.0 / (width * height);
                });
            });

            for (int i = 0; i <= max_I; ++i)
            {
                sum += N[i];
                isum += i * N[i];
                sum_N[i] = sum;
                sum_iN[i] = isum;
            }

            for (int t = 1; t <= max_I; ++t)
            {
                w1 = sum_N[t - 1];
                w2 = 1.0 - w1;
                u1 = sum_iN[t - 1] / w1;
                u2 = (sum_iN[max_I] - u1 * w1) / w2;
                var sigma = w1 * w2 * Math.Pow(u1 - u2, 2);
                if (sigma > max_sigma)
                {
                    max_sigma = sigma;
                    max_t = t;
                }
            }

            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    out_bytes[3 * idj + 0] = out_bytes[3 * idj + 1] = out_bytes[3 * idj + 2] = (I[3 * idj] <= max_t) ? (byte)0 : (byte)255;
                });
            });
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }

        public static Bitmap Method_Nibleka(Bitmap input, int a = 15, double k = -0.2)
        {
            int width = input.Width;
            int height = input.Height;

            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }
            byte[] b_in_bytes = getImgBytes24(input);
            byte[] b_out_bytes = new byte[b_in_bytes.Length];
            byte[] I = new byte[b_in_bytes.Length];
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    I[3 * idj] = (byte)Functions.Clamp((0.2125 * b_in_bytes[3 * idj + 2] + 0.7154 * b_in_bytes[3 * idj + 1] + 0.0721 * b_in_bytes[3 * idj + 0]), 0, 255);
                    I[3 * idj + 2] = I[3 * idj + 1] = I[3 * idj];
                });
            });
            byte[] out_bytes = new byte[width * height * 3];
            var integral_mat = new long[height, width];
            var integral_sqr_mat = new long[height, width];
            var a_2 = (int)Math.Ceiling(1.0 * a / 2);
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    integral_mat[i, j] = I[i * width * 3 + j * 3] +
                                    (j >= 1 ? integral_mat[i, j - 1] : 0) +
                                    (i >= 1 ? integral_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? integral_mat[i - 1, j - 1] : 0);

                    integral_sqr_mat[i, j] = I[i * width * 3 + j * 3] * I[i * width * 3 + j * 3] +
                                    (j >= 1 ? integral_sqr_mat[i, j - 1] : 0) +
                                    (i >= 1 ? integral_sqr_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? integral_sqr_mat[i - 1, j - 1] : 0);
                }
            }
            for (int i = 0; i < height; ++i)
            {
                var y1 = i - a_2;
                y1 = (y1 < 0) ? 0 : y1;
                var y2 = i + a_2;
                y2 = (y2 >= height) ? height - 1 : y2;
                for (int j = 0; j < width; ++j)
                {
                    int index = 3 * (i * width + j);
                    long sum = 0;
                    long sqr_sum = 0;
                    var x1 = j - a_2;
                    x1 = (x1 < 0) ? 0 : x1;
                    var x2 = j + a_2;
                    x2 = (x2 >= width) ? width - 1 : x2;
                    sum = ((x1 >= 1 && y1 >= 1) ? integral_mat[y1 - 1, x1 - 1] : 0) +
                        integral_mat[y2, x2] -
                        ((y1 >= 1) ? integral_mat[y1 - 1, x2] : 0) -
                        ((x1 >= 1) ? integral_mat[y2, x1 - 1] : 0);

                    sqr_sum = ((x1 >= 1 && y1 >= 1) ? integral_sqr_mat[y1 - 1, x1 - 1] : 0) +
                              integral_sqr_mat[y2, x2] -
                          ((y1 >= 1) ? integral_sqr_mat[y1 - 1, x2] : 0) -
                          ((x1 >= 1) ? integral_sqr_mat[y2, x1 - 1] : 0);

                    sqr_sum /= (x2 - x1 + 1) * (y2 - y1 + 1);
                    sum /= (x2 - x1 + 1) * (y2 - y1 + 1);

                    double D = Math.Sqrt(sqr_sum - sum * sum);
                    double t = sum + k * D;

                    out_bytes[index + 0] = out_bytes[index + 1] = out_bytes[index + 2] = (I[index] <= t) ? (byte)0 : (byte)255;
                }
            }
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }

        public static Bitmap Method_Sayvoly(Bitmap input, int a = 15, double k = 0.25, int R = 128)
        {
            int width = input.Width;
            int height = input.Height;

            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }
            byte[] b_in_bytes = getImgBytes24(input);
            byte[] b_out_bytes = new byte[b_in_bytes.Length];
            byte[] I = new byte[b_in_bytes.Length];
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    I[3 * idj] = (byte)Functions.Clamp((0.2125 * b_in_bytes[3 * idj + 2] + 0.7154 * b_in_bytes[3 * idj + 1] + 0.0721 * b_in_bytes[3 * idj + 0]), 0, 255);
                    I[3 * idj + 2] = I[3 * idj + 1] = I[3 * idj];
                });
            });
            byte[] out_bytes = new byte[width * height * 3];
            var integral_mat = new long[height, width];
            var integral_sqr_mat = new long[height, width];
            var a_2 = (int)Math.Ceiling(1.0 * a / 2);
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    integral_mat[i, j] = I[i * width * 3 + j * 3] +
                                    (j >= 1 ? integral_mat[i, j - 1] : 0) +
                                    (i >= 1 ? integral_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? integral_mat[i - 1, j - 1] : 0);

                    integral_sqr_mat[i, j] = I[i * width * 3 + j * 3] * I[i * width * 3 + j * 3] +
                                    (j >= 1 ? integral_sqr_mat[i, j - 1] : 0) +
                                    (i >= 1 ? integral_sqr_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? integral_sqr_mat[i - 1, j - 1] : 0);
                }
            }
            for (int i = 0; i < height; ++i)
            {
                var y1 = i - a_2;
                y1 = (y1 < 0) ? 0 : y1;
                var y2 = i + a_2;
                y2 = (y2 >= height) ? height - 1 : y2;
                for (int j = 0; j < width; ++j)
                {
                    int index = 3 * (i * width + j);
                    long sum = 0;
                    long sqr_sum = 0;
                    var x1 = j - a_2;
                    x1 = (x1 < 0) ? 0 : x1;
                    var x2 = j + a_2;
                    x2 = (x2 >= width) ? width - 1 : x2;
                    sum = ((x1 >= 1 && y1 >= 1) ? integral_mat[y1 - 1, x1 - 1] : 0) +
                        integral_mat[y2, x2] -
                        ((y1 >= 1) ? integral_mat[y1 - 1, x2] : 0) -
                        ((x1 >= 1) ? integral_mat[y2, x1 - 1] : 0);

                    sqr_sum = ((x1 >= 1 && y1 >= 1) ? integral_sqr_mat[y1 - 1, x1 - 1] : 0) +
                              integral_sqr_mat[y2, x2] -
                          ((y1 >= 1) ? integral_sqr_mat[y1 - 1, x2] : 0) -
                          ((x1 >= 1) ? integral_sqr_mat[y2, x1 - 1] : 0);

                    sqr_sum /= (x2 - x1 + 1) * (y2 - y1 + 1);
                    sum /= (x2 - x1 + 1) * (y2 - y1 + 1);

                    double D = Math.Sqrt(sqr_sum - sum * sum);
                    double t = sum * (1 + k * (D / R - 1));

                    out_bytes[index + 0] = out_bytes[index + 1] = out_bytes[index + 2] = (I[index] <= t) ? (byte)0 : (byte)255;
                }
            }
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }

        public static Bitmap Method_KristianaWolf(Bitmap input, int a = 15, double _a = 0.5)
        {
            int width = input.Width;
            int height = input.Height;

            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }
            byte[] b_in_bytes = getImgBytes24(input);
            byte[] b_out_bytes = new byte[b_in_bytes.Length];
            byte[] I = new byte[b_in_bytes.Length];
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    I[3 * idj] = (byte)Functions.Clamp((0.2125 * b_in_bytes[3 * idj + 2] + 0.7154 * b_in_bytes[3 * idj + 1] + 0.0721 * b_in_bytes[3 * idj + 0]), 0, 255);
                    I[3 * idj + 2] = I[3 * idj + 1] = I[3 * idj];
                });
            });
            byte[] out_bytes = new byte[width * height * 3];
            var integral_mat = new long[height, width];
            var integral_sqr_mat = new long[height, width];
            var a_2 = (int)Math.Ceiling(1.0 * a / 2);
            var minI = I.Min(x => x);
            double R = 0;
            double[] sigma = new double[width * height];
            double[] mat = new double[width * height];
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    integral_mat[i, j] = I[i * width * 3 + j * 3] +
                                    (j >= 1 ? integral_mat[i, j - 1] : 0) +
                                    (i >= 1 ? integral_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? integral_mat[i - 1, j - 1] : 0);

                    integral_sqr_mat[i, j] = I[i * width * 3 + j * 3] * I[i * width * 3 + j * 3] +
                                    (j >= 1 ? integral_sqr_mat[i, j - 1] : 0) +
                                    (i >= 1 ? integral_sqr_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? integral_sqr_mat[i - 1, j - 1] : 0);
                }
            }
            for (int i = 0; i < height; ++i)
            {
                var y1 = i - a_2;
                y1 = (y1 < 0) ? 0 : y1;
                var y2 = i + a_2;
                y2 = (y2 >= height) ? height - 1 : y2;
                for (int j = 0; j < width; ++j)
                {
                    int index = 3 * (i * width + j);
                    long sum = 0;
                    long sqr_sum = 0;
                    var x1 = j - a_2;
                    x1 = (x1 < 0) ? 0 : x1;
                    var x2 = j + a_2;
                    x2 = (x2 >= width) ? width - 1 : x2;
                    sum = ((x1 >= 1 && y1 >= 1) ? integral_mat[y1 - 1, x1 - 1] : 0) +
                        integral_mat[y2, x2] -
                        ((y1 >= 1) ? integral_mat[y1 - 1, x2] : 0) -
                        ((x1 >= 1) ? integral_mat[y2, x1 - 1] : 0);

                    sqr_sum = ((x1 >= 1 && y1 >= 1) ? integral_sqr_mat[y1 - 1, x1 - 1] : 0) +
                              integral_sqr_mat[y2, x2] -
                          ((y1 >= 1) ? integral_sqr_mat[y1 - 1, x2] : 0) -
                          ((x1 >= 1) ? integral_sqr_mat[y2, x1 - 1] : 0);

                    sqr_sum /= (x2 - x1 + 1) * (y2 - y1 + 1);
                    sum /= (x2 - x1 + 1) * (y2 - y1 + 1);

                    double D = Math.Sqrt(sqr_sum - sum * sum);

                    mat[i * width + j] = sum;
                    sigma[i * width + j] = D;

                    if (D > R) R = D;
                }
            }

            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    double t = (1 - _a) * mat[idj] + _a * minI + _a * sigma[idj] / R * (mat[idj] - minI);
                    out_bytes[3 * idj + 0] = out_bytes[3 * idj + 1] = out_bytes[3 * idj + 2] = (I[3 * idj] <= t) ? (byte)0 : (byte)255;
                });
            });
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }

        public static Bitmap Method_BradleyRota(Bitmap input, int a = 15, double k = 0.15)
        {
            int width = input.Width;
            int height = input.Height;

            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }
            byte[] b_in_bytes = getImgBytes24(input);
            byte[] b_out_bytes = new byte[b_in_bytes.Length];
            byte[] I = new byte[b_in_bytes.Length];
            Parallel.For(0, height, (i) =>
            {
                var index = i * width;
                Parallel.For(0, width, (j) =>
                {
                    var idj = index + j;
                    I[3 * idj] = (byte)Functions.Clamp((0.2125 * b_in_bytes[3 * idj + 2] + 0.7154 * b_in_bytes[3 * idj + 1] + 0.0721 * b_in_bytes[3 * idj + 0]), 0, 255);
                    I[3 * idj + 2] = I[3 * idj + 1] = I[3 * idj];
                });
            });
            byte[] out_bytes = new byte[width * height * 3];
            var integral_mat = new long[height, width];
            var a_2 = (int)Math.Ceiling(1.0 * a / 2);
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    integral_mat[i, j] = I[i * width * 3 + j * 3] +
                                    (j >= 1 ? integral_mat[i, j - 1] : 0) +
                                    (i >= 1 ? integral_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? integral_mat[i - 1, j - 1] : 0);
                }
            }
            for (int i = 0; i < height; ++i)
            {
                var y1 = i - a_2;
                y1 = (y1 < 0) ? 0 : y1;
                var y2 = i + a_2;
                y2 = (y2 >= height) ? height - 1 : y2;
                for (int j = 0; j < width; ++j)
                {
                    int index = 3 * (i * width + j);
                    long sum = 0;
                    var x1 = j - a_2;
                    x1 = (x1 < 0) ? 0 : x1;
                    var x2 = j + a_2;
                    x2 = (x2 >= width) ? width - 1 : x2;
                    sum = ((x1 >= 1 && y1 >= 1) ? integral_mat[y1 - 1, x1 - 1] : 0) +
                        integral_mat[y2, x2] -
                        ((y1 >= 1) ? integral_mat[y1 - 1, x2] : 0) -
                        ((x1 >= 1) ? integral_mat[y2, x1 - 1] : 0);
                    var count = (x2 - x1 + 1) * (y2 - y1 + 2);

                    out_bytes[index + 0] = out_bytes[index + 1] = out_bytes[index + 2] = (I[index] * count < sum * (1 - k)) ? (byte)0 : (byte)255;
                }
            }
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }
        public static Bitmap MatrixFilter(Bitmap input, string matrix)
        {
            int width = input.Width;
            int height = input.Height;


            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }


            byte[] in_bytes = getImgBytes24(_tmp);
            byte[] out_bytes = new byte[width * height * 3];


            var core = Functions.GetMatrixFromStr(matrix);
            int M = core.GetLength(0);
            int N = core.GetLength(1);

            Parallel.For(0, width * height, arr_i =>
            {
                int _i = arr_i / width;
                int _j = arr_i - _i * width;

                double sum1 = 0;
                double sum2 = 0;
                double sum3 = 0;

                for (int ii = 0; ii < M; ++ii)
                {
                    int i = _i + ii - M / 2;

                    i = i < 0 ? Math.Abs(i) : i;
                    i = i >= height ? 2 * height - i - 1 : i;

                    var index = width * i;
                    for (int jj = 0; jj < N; ++jj)
                    {
                        int j = _j + jj - N / 2;


                        j = j < 0 ? Math.Abs(j) : j;
                        j = j >= width ? 2 * width - j - 1 : j;

                        var idj = index + j;

                        sum1 += in_bytes[3 * idj + 0] * core[ii, jj];
                        sum2 += in_bytes[3 * idj + 1] * core[ii, jj];
                        sum3 += in_bytes[3 * idj + 2] * core[ii, jj];
                    }
                }
                out_bytes[arr_i * 3 + 0] = (byte)Functions.Clamp(sum1, 0, 255);
                out_bytes[arr_i * 3 + 1] = (byte)Functions.Clamp(sum2, 0, 255);
                out_bytes[arr_i * 3 + 2] = (byte)Functions.Clamp(sum3, 0, 255);
            });

            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }
        public static Bitmap Median(Bitmap input, int a)
        {
            int width = input.Width;
            int height = input.Height;

            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
            }

            byte[] in_bytes = getImgBytes24(_tmp);
            byte[] out_bytes = new byte[width * height * 3];



            Parallel.For(0, height, _i =>
            {
                int[] aR = new int[a * a];
                int[] aG = new int[a * a];
                int[] aB = new int[a * a];
                var idi = width * _i;
                for (int _j = 0; _j < width; ++_j)
                {
                    var idj = idi + _j;
                    for (int ii = 0; ii < a; ++ii)
                    {
                        int i = _i + ii - a / 2;

                        i = i < 0 ? Math.Abs(i) : i;
                        i = i >= height ? 2 * height - i - 1 : i;

                        var index = width * i;
                        for (int jj = 0; jj < a; ++jj)
                        {
                            int j = _j + jj - a / 2;

                            j = j < 0 ? Math.Abs(j) : j;
                            j = j >= width ? 2 * width - j - 1 : j;

                            var idjj = index + j;

                            aR[ii * a + jj] = in_bytes[3 * idjj + 0];
                            aG[ii * a + jj] = in_bytes[3 * idjj + 1];
                            aB[ii * a + jj] = in_bytes[3 * idjj + 2];
                        }
                    }
                    out_bytes[3 * idj + 0] = (byte)Functions.QuickSelect(aR, 0, a * a, a * a / 2);
                    out_bytes[3 * idj + 1] = (byte)Functions.QuickSelect(aG, 0, a * a, a * a / 2);
                    out_bytes[3 * idj + 2] = (byte)Functions.QuickSelect(aB, 0, a * a, a * a / 2);
                }

            });
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, out_bytes);

            return new_bitmap;
        }
        public static (Bitmap, Bitmap, Bitmap) Frequency_filtering(
            Bitmap input,
            int filter_type,
            double[][] filter_params_double,
            double furier_multiplyer = 1.0,
            double in_filter_zone = 1.0,
            double out_filter_zone = 0.0)
        {
            int width = input.Width;
            int height = input.Height;

            int new_width = width;
            int new_height = height;

            var p = Math.Log(width, 2);
            if (p != Math.Floor(p))
                new_width = (int)Math.Pow(2, Math.Ceiling(p));
            p = Math.Log(height, 2);
            if (p != Math.Floor(p))
                new_height = (int)Math.Pow(2, Math.Ceiling(p));

            Bitmap _tmp = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            byte[] new_bytes = new byte[new_width * new_height * 3];
            byte[] furier_bytes = new byte[new_width * new_height * 3];
            byte[] filter_bytes = new byte[new_width * new_height * 3];

            Graphics g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);

            byte[] old_bytes = getImgBytes24(_tmp);

            
            Complex[] complex_bytes = new Complex[new_width * new_height];

            for (int rgb = 0; rgb <= 2; rgb++)
            {

                for (int i = 0; i < new_width * new_height; ++i)
                {
                    int y = i / new_width;
                    int x = i - y * new_width;
                    complex_bytes[i] = Math.Pow(-1, x + y) * old_bytes[i * 3 + rgb];
                }

                complex_bytes = Functions.FFT1(complex_bytes, new_width, new_height);

                var max_bright = complex_bytes.Max(x => Functions.Bright(x.Imaginary));

                Complex[] complex_bytes_filtered = null;


                if (filter_type == 0) //Полосовой    
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        int y = i / new_width;
                        int x = i - y * new_width - new_width / 2;
                        y -= new_height / 2;
                        foreach (var v in filter_params_double)
                        {
                            if (Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) >= Math.Pow(v[2], 2) &&
                                Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) <= Math.Pow(v[3], 2))
                            {
                                filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * in_filter_zone, 0, 255);
                                return a * in_filter_zone;
                            }

                        }
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * out_filter_zone, 0, 255);
                        return a * out_filter_zone;

                    }).ToArray();
                }
                else
                if (filter_type == 1) //Режекторный
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        int y = i / new_width;
                        int x = i - y * new_width - new_width / 2;
                        y -= new_height / 2;

                        foreach (var v in filter_params_double)
                        {
                            //if (Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) <= Math.Pow(v[2], 2) ||
                            //    Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) >= Math.Pow(v[3], 2))
                            //{
                            //    filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * in_filter_zone, 0, 255);
                            //    return a * in_filter_zone;
                            //}
                            if (Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) >= Math.Pow(v[2], 2) &&
                                Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) <= Math.Pow(v[3], 2))
                            {
                                filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * out_filter_zone, 0, 255);
                                return a * out_filter_zone;
                            }
                        }
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * in_filter_zone, 0, 255);
                        return a * in_filter_zone;
                        

                    }).ToArray();
                }
                else
                if (filter_type == 2) //Баттерворта ФНЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width - new_width / 2;
                            y -= new_height / 2;

                            double D0 = v[2];
                            double h = v[2] - D0;
                            double b = Functions.Butter(x, y, D0, (int)out_filter_zone, v[0], v[1], in_filter_zone, h);
                            return b;
                        }).Max();
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * val, 0, 255);
                        return a * val;
                    }).ToArray();
                }
                else if (filter_type == 3) //Баттерворта ФВЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width - new_width / 2;
                            y -= new_height / 2;

                            double D0 = v[2];
                            double h = v[2] - D0;
                            double b = in_filter_zone - Functions.Butter(x, y, D0, (int)out_filter_zone, v[0], v[1], in_filter_zone, h);
                            return b;
                        }).Min();
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * val, 0, 255);
                        return a * val;
                    }).ToArray();
                }
                else if (filter_type == 4)  //Гаусса ФНЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width - new_width / 2;
                            y -= new_height / 2;                            
                            double D0 = v[2];
                            double h = v[2] - D0;
                            double b = Functions.Gauss(x, y, D0, v[0], v[1], in_filter_zone, h);
                            return b;
                        }).Max();
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * val, 0, 255);
                        return a * val;
                    }).ToArray();
                }
                else if (filter_type == 5) //Гаусса ФВЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width - new_width / 2;
                            y -= new_height / 2;
                            double D0 = v[2];
                            double h = v[2] - D0;
                            double b = in_filter_zone - Functions.Gauss(x, y, D0, v[0], v[1], in_filter_zone, h);
                            return b;
                        }).Min();
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * val, 0, 255);
                        return a * val;
                    }).ToArray();
                }
                else
                if (filter_type == 6) //Узкополосный режекторный
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        int y = i / new_width;
                        int x = i - y * new_width - new_width / 2;
                        y -= new_height / 2;

                        foreach (var v in filter_params_double)
                        {                            
                            if (Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) <= Math.Pow(v[2], 2))
                            {
                                filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * out_filter_zone, 0, 255);
                                return a * out_filter_zone;
                            }
                        }
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * in_filter_zone, 0, 255);
                        return a * in_filter_zone;

                    }).ToArray();
                }
                else
                if (filter_type == 7) //Узкополосный полосовой
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        int y = i / new_width;
                        int x = i - y * new_width - new_width / 2;
                        y -= new_height / 2;

                        foreach (var v in filter_params_double)
                        {
                            if (Math.Pow(x - v[0], 2) + Math.Pow(y - v[1], 2) <= Math.Pow(v[2], 2))
                            {
                                filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * in_filter_zone, 0, 255);
                                return a * in_filter_zone;
                            }

                        }
                        filter_bytes[i * 3 + rgb] = (byte)Functions.Clamp(255 * out_filter_zone, 0, 255);
                        return a * out_filter_zone;

                    }).ToArray();
                }

                //Обратное FFT
                var complex_bytes_result = Functions.FFT2(complex_bytes_filtered, new_width, new_height);

                for (int i = 0; i < new_width * new_height; ++i)
                {
                    int y = i / new_width;
                    int x = i - y * new_width - new_width / 2;
                    y -= new_height / 2;
                    new_bytes[i * 3 + rgb] = (byte)Functions.Clamp(Math.Round((Math.Pow(-1, x + y) * complex_bytes_result[i]).Real), 0, 255);
                    furier_bytes[i * 3 + rgb] = (byte)Functions.Clamp(furier_multiplyer * Functions.Bright(complex_bytes[i].Magnitude) * 255 / max_bright, 0, 255);
                }
            }

            //Bitmap для восстановленного изображения
            Bitmap new_bitmap = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, new_bytes);

            //Рисуем восстановленное изображение
            Bitmap new_bitmap_img = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap_img.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g1 = Graphics.FromImage(new_bitmap_img))
            {
                g1.DrawImageUnscaled(new_bitmap, 0, 0);
            }

            //Фурье-образ и кружки
            Bitmap new_bitmap_fur = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            new_bitmap_fur.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap_fur, furier_bytes);
            var g_fur = Graphics.FromImage(new_bitmap_fur);
            if (filter_type < 2)
                foreach (var v in filter_params_double)
                {
                    g_fur.DrawEllipse(Pens.GreenYellow, (int)v[0] - (int)v[2] + new_width / 2, (int)v[1] - (int)v[2] + new_height / 2, (int)v[2] * 2, (int)v[2] * 2);
                    g_fur.DrawEllipse(Pens.GreenYellow, (int)v[0] - (int)v[3] + new_width / 2, (int)v[1] - (int)v[3] + new_height / 2, (int)v[3] * 2, (int)v[3] * 2);
                }
            else
                foreach (var v in filter_params_double)
                {
                    g_fur.DrawEllipse(Pens.GreenYellow, (int)v[0] - (int)v[2] + new_width / 2, (int)v[1] - (int)v[2] + new_height / 2, (int)v[2] * 2, (int)v[2] * 2);                    
                }

            //Маска фильтра
            Bitmap new_bitmap_mask = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            new_bitmap_mask.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap_mask, filter_bytes);

            return (new_bitmap_img, new_bitmap_fur, new_bitmap_mask);
        }
    }
}
