using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DIPS_lab1_
{
    internal static class Functions
    {
        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                return min;
            return val.CompareTo(max) > 0 ? max : val;
        }
        public static void Swap<T>(ref T val1, ref T val2) 
        {
            T temp = val1;
            val1 = val2;
            val2 = temp;
        }
        public static double[,] GetMatrixFromStr(string matrix)
        {
            char[] sep = { '\n' };
            matrix = matrix.Replace('\r', ' ');
            var str_list = matrix.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            double[,] mat = new double[0, 0];
            char[] sep_space = { ' ' };

            for (int i = 0; i < str_list.Count(); ++i)
            {
                //str_list[i] = str_list[i].Replace('\r', ' ');
                var chars = str_list[i].Split(sep_space, StringSplitOptions.RemoveEmptyEntries);

                if (i == 0)
                {
                    mat = new double[str_list.Length, chars.Length];
                }

                for (int j = 0; j < chars.Length; ++j)
                {                    
                    mat[i, j] = Convert.ToDouble(Convert.ToString(chars[j]).Replace('.', ','));
                }
            }
            return mat;
        }
        
        public static int QuickSelect(int[] arr, int left, int right, int k)
        {
            if (right - left == 1)
                return arr[left];

            int left_count = 0;
            int eqv_count = 0;            

            for (int i = left; i < right - 1; ++i)
            {
                if (arr[i] < arr[right - 1])
                {
                    Swap(ref arr[i], ref arr[left + left_count]);                    
                    left_count++;
                }
            }
            for (int i = left + left_count; i < right - 1; ++i)
            {
                if (arr[i] == arr[right - 1])
                {
                    Swap(ref arr[i], ref arr[left + left_count + eqv_count]);                    
                    eqv_count++;
                }
            }
            Swap(ref arr[right - 1], ref arr[left + left_count + eqv_count]);

            if (k < left_count)
                return QuickSelect(arr, left, left + left_count, k);
            else if (k < left_count + eqv_count)
                return arr[left + left_count];
            else
                return QuickSelect(arr, left + left_count + eqv_count, right, k - left_count - eqv_count);

        }

        public static string GetGaussMat(int r, double sig)
        {
            double s = 0;
            double g;
            string ss="";                        

            double sig_sqr = 2.0 * sig * sig;
            double pi_siq_sqr = sig_sqr * Math.PI;

            for (int i = -r; i <= r; ++i)
            {
                for (int j = -r; j <= r; ++j)
                {
                    g = 1.0 / pi_siq_sqr * Math.Pow(Math.E,(-1.0 * (i * i + j * j) / (sig_sqr)));
                    s += g;
                    
                    if (j == r)
                        ss += Math.Round(g, 5).ToString() + "\r\n";
                    else
                        ss += Math.Round(g, 5).ToString() + " ";
                }
                
            }
            return ss;
        }
        //Быстрое преобразование Фурье (FFT).
        public static Complex[] FFT(Complex[] arr, int x0, int N, int s)
        {
            Complex[] X = new Complex[N];
            if (N == 1)
            {
                X[0] = arr[x0];
            }
            else
            {
                FFT(arr, x0, N / 2, 2 * s).CopyTo(X, 0);
                FFT(arr, x0 + s, N / 2, 2 * s).CopyTo(X, N / 2);

                for (int k = 0; k < N / 2; k++)
                {
                    var t = X[k];
                    double u = -2.0 * Math.PI * k / N;
                    X[k] = t + new Complex(Math.Cos(u), Math.Sin(u)) * X[k + N / 2];
                    X[k + N / 2] = t - new Complex(Math.Cos(u), Math.Sin(u)) * X[k + N / 2];
                }
            }
            return X;
        }

        public static Complex[] FFT1(Complex[] arr, int width, int height)
        {
            Complex[] X = new Complex[arr.Length];

            
            Parallel.For(0, height, i =>
            {
                Complex[] tmp = new Complex[width];
                Array.Copy(arr, i * width, tmp, 0, width);

                tmp = FFT(tmp, 0, width, 1);

                for (int k = 0; k < width; ++k)
                    X[i * width + k] = tmp[k] / width;
            }
            );
            
            Parallel.For(0, width, j =>
            {
                Complex[] tmp = new Complex[height];
                for (int k = 0; k < height; ++k)
                    tmp[k] = X[j + k * width];

                tmp = FFT(tmp, 0, tmp.Length, 1);

                for (int k = 0; k < height; ++k)
                    X[j + k * width] = tmp[k] / height;
            }
            );
            return X;
        }

        public static Complex[] FFT2(Complex[] arr, int width, int height)
        {
            Complex[] X = new Complex[arr.Length];
            
            Parallel.For(0, height, i =>
            {
                Complex[] tmp = new Complex[width];
                Array.Copy(arr, i * width, tmp, 0, width);
                for (int k = 0; k < width; ++k)
                    tmp[k] = new Complex(arr[i * width + k].Real, -arr[i * width + k].Imaginary);

                tmp = FFT(tmp, 0, width, 1);

                for (int k = 0; k < width; ++k)
                    X[i * width + k] = (new Complex(tmp[k].Real, -tmp[k].Imaginary));
            }
            );
            
            Parallel.For(0, width, j =>
            {
                Complex[] tmp = new Complex[height];
                for (int k = 0; k < height; ++k)
                    tmp[k] = new Complex(X[j + k * width].Real, -X[j + k * width].Imaginary);

                tmp = FFT(tmp, 0, tmp.Length, 1);

                for (int k = 0; k < height; ++k)
                    X[j + k * width] = (new Complex(tmp[k].Real, -tmp[k].Imaginary));
            }
            );
            return X;
        }
        public static double Butter(double x, double y, double D0, double n, double dx = 0, double dy = 0, double G = 1.0, double h = 0)
        {
            double D = Math.Sqrt((x - dx) * (x - dx) + (y - dy) * (y - dy)) - h;
            return G / (1 + Math.Pow(D / D0, 2 * n));
        }

        public static double Gauss(double x, double y, double D0, double dx = 0, double dy = 0, double G = 1.0, double h = 0)
        {
            double D = Math.Sqrt((x - dx) * (x - dx) + (y - dy) * (y - dy)) - h;
            return G * Math.Exp(-(D * D / (2.0 * D0 * D0)));
        }
        public static double Bright(double x)
        {
            return Math.Log(x + 1);
        }
    }
}
