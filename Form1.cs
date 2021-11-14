using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.VisualBasic;
using ZedGraph;

namespace lab5
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> inputIm = null;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
            inputIm = new Image<Bgr, byte>(openFileDialog1.FileName);
            pictureBox1.Image = inputIm.Bitmap;
        }

        private void findContoursToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double f = double.Parse(Interaction.InputBox("Введите масштаб", "Поиск площади"));
            int l = int.Parse(Interaction.InputBox("Введите число бросаний", "Поиск площади"));
            Image<Gray, byte> img = inputIm.SmoothGaussian(7).Convert<Gray, byte>().ThresholdBinaryInv(new Gray(50), new Gray(255)); //Значения площади могут заметно отличаться в зависимости от аргумента SmoothGaussian()
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hieratchy = new Mat();
            Image<Gray, byte> blackBackground = new Image<Gray, byte>(inputIm.Width, inputIm.Height, new Gray(0));
            CvInvoke.FindContours(img, contours, hieratchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            int n = FindLake(blackBackground, contours, l);
            pictureBox2.Image = blackBackground.Bitmap;
            var s = GetS(blackBackground, l, f, contours, n);
            CvInvoke.DrawContours(blackBackground, contours, n , new MCvScalar(255, 0, 0));
            pictureBox2.Image = blackBackground.Bitmap;
            textBox1.Text = s.s.ToString() + "\t" + s.sigma.ToString();
        }

        private static int FindLake (Image<Gray, byte> blackBackground, VectorOfVectorOfPoint contours, int l)
        {
            Random rnd = new Random();
            var countOfPointsInContour = new double[contours.Size];
            for (int i = 0; i < l / 2; i++)
            {
                var point = new PointF(rnd.Next(blackBackground.Width), rnd.Next(blackBackground.Height));
                for (int j = 0; j < contours.Size; j++)
                {
                    var a = CvInvoke.PointPolygonTest(contours[j], new PointF(rnd.Next(blackBackground.Width), rnd.Next(blackBackground.Height)), false);
                    if (a >= 0)
                        countOfPointsInContour[j] += 1;
                }
            }
            var count = double.MinValue;
            int index = 0;
            for (int i = 0; i<countOfPointsInContour.Length; i++)
            {
                if (countOfPointsInContour[i] > count)
                {
                    count = countOfPointsInContour[i];
                    index = i;
                }
            }
            return index;
        }
        private static (double s, double sigma) GetS (Image<Gray, byte> blackBackground, double l, double f, VectorOfVectorOfPoint contours, int n)
        {
            Random rnd = new Random();
            int m = 10;
            var s = new double[m];
            for (int j = 0; j < m; j++)
            {
                int k = 0;
                for (int i = 0; i < l; i++)
                {
                    var a = CvInvoke.PointPolygonTest(contours[n], new PointF(rnd.Next(blackBackground.Width), rnd.Next(blackBackground.Height)), false);
                    if (a > 0)
                        k += 1;
                }
                s[j] = k / l * f;
            }
            double avarage = 0;
            double sqAvarage = 0;
            for (int i = 0; i < m; i++)
            {
                avarage += s[i] / m;
                sqAvarage += s[i] * s[i] / m;
            }
            double sigmaM = Math.Sqrt((sqAvarage - avarage * avarage));
            var sTurple = (Math.Round(avarage,3), sigmaM);
            return sTurple;
        }

        
    }
}
