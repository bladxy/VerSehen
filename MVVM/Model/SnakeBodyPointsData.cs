using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerSehen.MVVM.Model
{
    public class SnakeBodyPointsData
    {
        [LoadColumn(0)]
        public string Image;

        [LoadColumn(1)]
        public string Label;
    }
}
