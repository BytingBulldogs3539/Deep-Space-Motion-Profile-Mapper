using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelocityMap
{
    public static class FloatExtension
    {
        public static void NoiseReduction(this float[] src, int severity = 1)
        {
            float[] ret = new float[src.Length];

            for (int i = 1; i < src.Length; i++)
            {
                //---------------------------------------------------------------avg
                var start = (i - severity > 0 ? i - severity : 0);
                var end = (i + severity < src.Length ? i + severity : src.Length);

                float sum = 0;

                for (int j = start; j < end; j++)
                {
                    sum += src[j];
                }

                var avg = sum / (end - start);
                //---------------------------------------------------------------
                ret[i] = avg;

            }
            for(int i = 1; i < src.Length; i++)
                src[i] = ret[i];
        }
    }
    public static class ListExtention
    {
        public static void NoiseReduction(this List<float> src, int severity = 1)
        {
            float[] ret = new float[src.Count];

            for (int i = 1; i < src.Count; i++)
            {
                //---------------------------------------------------------------avg
                var start = (i - severity > 0 ? i - severity : 0);
                var end = (i + severity < src.Count ? i + severity : src.Count);

                float sum = 0;

                for (int j = start; j < end; j++)
                {
                    sum += src[j];
                }

                var avg = sum / (end - start);
                //---------------------------------------------------------------
                ret[i] = avg;

            }
            for (int i = 1; i < src.Count; i++)
                src[i] = ret[i];
        }
    }
}
