﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    internal class MedianFilter : MatrixFilter
    {
        public MedianFilter()
        {
            kernel = new float[3, 3];
        }

        public override Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker backgroundWorker1)
        {
            var rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
            var data = sourceImage.LockBits(rect, ImageLockMode.ReadWrite, sourceImage.PixelFormat);
            var depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel
            var buffer = new byte[data.Width * data.Height * depth];
            var resultBuffer = new byte[data.Width * data.Height * depth];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            Parallel.Invoke(
                () => {
                    for (int i = 0; i < data.Height; i++)
                    {
                        var offset = CalculateOffset(0, i, data.Width, depth);
                        var newColor = CalculateNewPixelColor(buffer, 0, i, data.Width, depth);
                        for (int k = 0; k < depth; k++)
                            resultBuffer[offset[k]] = newColor[k];
                        backgroundWorker1.ReportProgress((int)((float)Clamp(i * Environment.ProcessorCount, 0, data.Width) / data.Width * 100));
                    }
                },
                () =>
                {
                    Parallel.For(1, data.Width, i =>
                    {
                        if (backgroundWorker1.CancellationPending)
                            return;
                        for (int j = 0; j < data.Height; j++)
                        {
                            var offset = CalculateOffset(i, j, data.Width, depth);
                            var newColor = CalculateNewPixelColor(buffer, i, j, data.Width, depth);
                            for (int k = 0; k < depth; k++)
                                resultBuffer[offset[k]] = newColor[k];
                        }
                    });
                });
            Marshal.Copy(resultBuffer, 0, data.Scan0, buffer.Length);
            sourceImage.UnlockBits(data);
            return sourceImage;
        }

        protected override byte[] CalculateNewPixelColor(byte[] buffer, int x, int y, int width, int depth)
        {
            // создать контейнер с парами ключ(offset)-значение(greycolor), отсортировать по значению, выбрать значение из середины
            var dic = new Dictionary<int, byte>();
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, width - 1);
                    int idY = Clamp(y + l, 0, buffer.Length / depth / width - 1);
                    var offsetNeighbor = CalculateOffset(idX, idY, width, depth);
                    GrayscaleConversionFilter grayscaleConversionFilter = new GrayscaleConversionFilter();
                    dic[offsetNeighbor[0]] = grayscaleConversionFilter.GrayscalePixel(buffer, idX, idY, width, depth)[0];
                }
            var sortDic = dic.OrderByDescending(d => d.Value).ThenBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            if (sortDic.Count % 2 == 1)
            {
                var resOffset = sortDic.Keys.ToList()[sortDic.Count / 2];
                var resultColor = new byte[depth];
                for (int i = 0; i < depth; i++)
                    resultColor[i] = buffer[resOffset + i];
                return resultColor;
            }
            else
            {
                var resOffset = (sortDic.Keys.ToList()[sortDic.Count / 2 - 1]) / 2 + (sortDic.Keys.ToList()[sortDic.Count / 2]) / 2;
                var resultColor = new byte[depth];
                for (int i = 0; i < depth; i++)
                    resultColor[i] = buffer[resOffset + i];
                return resultColor;
            } 
        }
    }
}
