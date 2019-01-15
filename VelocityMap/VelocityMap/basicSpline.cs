﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelocityMap
{
        /// <summary>
        /// Spline interpolation class.
        /// </summary>
        public class SplineInterpolator
        {
            private readonly double[] _keys;

            private readonly double[] _values;

            private readonly double[] _h;

            private readonly double[] _a;

            /// <summary>
            /// Class constructor.
            /// </summary>
            /// <param name="nodes">Collection of known points for further interpolation.
            /// Should contain at least two items.</param>
            public SplineInterpolator(IDictionary<double, double> nodes)
            {
                if (nodes == null)
                {
                    throw new ArgumentNullException("nodes");
                }

                var n = nodes.Count;

                if (n < 2)
                {
                    throw new ArgumentException("At least two point required for interpolation.");
                }

                _keys = nodes.Keys.ToArray();
                _values = nodes.Values.ToArray();
                _a = new double[n];
                _h = new double[n];

                for (int i = 1; i < n; i++)
                {
                    _h[i] = _keys[i] - _keys[i - 1];
                }

                if (n > 2)
                {
                    var sub = new double[n - 1];
                    var diag = new double[n - 1];
                    var sup = new double[n - 1];

                    for (int i = 1; i <= n - 2; i++)
                    {
                        diag[i] = (_h[i] + _h[i + 1]) / 3;
                        sup[i] = _h[i + 1] / 6;
                        sub[i] = _h[i] / 6;
                        _a[i] = (_values[i + 1] - _values[i]) / _h[i + 1] - (_values[i] - _values[i - 1]) / _h[i];
                    }

                    SolveTridiag(sub, diag, sup, ref _a, n - 2);
                }
            }

            /// <summary>
            /// Gets interpolated value for specified argument.
            /// </summary>
            /// <param name="key">Argument value for interpolation. Must be within 
            /// the interval bounded by lowest ang highest <see cref="_keys"/> values.</param>
            public double GetValue(double key)
            {
                int gap = 0;
                var previous = double.MinValue;

                // At the end of this iteration, "gap" will contain the index of the interval
                // between two known values, which contains the unknown z, and "previous" will
                // contain the biggest z value among the known samples, left of the unknown z
                for (int i = 0; i < _keys.Length; i++)
                {
                    if (_keys[i] < key && _keys[i] > previous)
                    {
                        previous = _keys[i];
                        gap = i + 1;
                    }
                }

                var x1 = key - previous;
                var x2 = _h[gap] - x1;

                return ((-_a[gap - 1] / 6 * (x2 + _h[gap]) * x1 + _values[gap - 1]) * x2 +
                    (-_a[gap] / 6 * (x1 + _h[gap]) * x2 + _values[gap]) * x1) / _h[gap];
            }


            /// <summary>
            /// Solve linear system with tridiagonal n*n matrix "a"
            /// using Gaussian elimination without pivoting.
            /// </summary>
            private static void SolveTridiag(double[] sub, double[] diag, double[] sup, ref double[] b, int n)
            {
                int i;

                for (i = 2; i <= n; i++)
                {
                    sub[i] = sub[i] / diag[i - 1];
                    diag[i] = diag[i] - sub[i] * sup[i - 1];
                    b[i] = b[i] - sub[i] * b[i - 1];
                }

                b[n] = b[n] / diag[n];

                for (i = n - 1; i >= 1; i--)
                {
                    b[i] = (b[i] - sup[i] * b[i + 1]) / diag[i];
                }
            }
        }
}
