﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessing
{
    public partial class MainFrame : Form
    {
        Bitmap image;

        Stopwatch timer = new Stopwatch();

        public MainFrame()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.png;*.jpg;*.bmp|All files(*.*)|*.*";
            if(openFileDialog.ShowDialog() == DialogResult.OK) {
                image = new Bitmap(openFileDialog.FileName);
                pictureBoxImage.Image = image;
                pictureBoxImage.Refresh();
                
            }
        }

        private void toolStripMenuItemInversion_Click(object sender, EventArgs e)
        {
            InversionFilter filter = new InversionFilter();
            backgroundWorkerProgress.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            timer.Restart();
            var rect = new Rectangle(0, 0, image.Width, image.Height);
            var data = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);
            var depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel
            var buffer = new byte[data.Width * data.Height * depth];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            BitmapData newImageData = ((Filter)e.Argument).ProcessImage(data, depth, buffer, backgroundWorkerProgress);
            if (backgroundWorkerProgress.CancellationPending != true)
                image.UnlockBits(newImageData);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBoxImage.Image = image;
                pictureBoxImage.Refresh();

            }
            progressBar1.Value = 0;
            timer.Stop();
            labelTime.Text = timer.Elapsed.ToString();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            backgroundWorkerProgress.CancelAsync();
        }

        private void toolStripMenuItemGrayscale_Click(object sender, EventArgs e)
        {
            GrayscaleConversionFilter filter = new GrayscaleConversionFilter();
            backgroundWorkerProgress.RunWorkerAsync(filter);
        }

        private void toolStripMenuItemBlur_Click(object sender, EventArgs e)
        {
            BlurFilter filter = new BlurFilter();
            backgroundWorkerProgress.RunWorkerAsync(filter);
        }
    }
}
