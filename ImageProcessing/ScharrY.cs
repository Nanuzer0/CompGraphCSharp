﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    internal class ScharrY : MatrixFilter
    {
        public ScharrY() {
            kernel = new float[,] { { 3, 10, 3 }, { 0, 0, 0 }, { -3, -10, -3 } };
        }
    }
}
