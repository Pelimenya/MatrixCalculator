using System;
using System.Text;
using MatrixCalculator.Exceptions;

namespace MatrixCalculator.Models
{
    public sealed class Matrix
    {
        private readonly double[,] _data;
        private const double EPS = 1e-12;

        public int Rows { get; }
        public int Columns { get; }

        public Matrix(int rows, int columns)
        {
            if (rows <= 0 || columns <= 0)
                throw new ArgumentException("Количество строк и столбцов должно быть больше нуля.");

            Rows = rows;
            Columns = columns;
            _data = new double[rows, columns];
        }

        public Matrix(double[,] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Rows = data.GetLength(0);
            Columns = data.GetLength(1);
            _data = (double[,])data.Clone();
        }

        public static Matrix FromArray(double[][] rows)
        {
            if (rows == null || rows.Length == 0)
                throw new ArgumentException("Пустой массив строк матрицы.");

            int cols = rows[0].Length;
            var m = new Matrix(rows.Length, cols);

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].Length != cols)
                    throw new ArgumentException("Строки матрицы имеют разную длину.");

                for (int j = 0; j < cols; j++)
                    m[i, j] = rows[i][j];
            }

            return m;
        }

        public static Matrix Identity(int n)
        {
            var I = new Matrix(n, n);
            for (int i = 0; i < n; i++)
                I[i, i] = 1.0;

            return I;
        }

        public double this[int r, int c]
        {
            get => _data[r, c];
            set => _data[r, c] = value;
        }

        public Matrix Clone()
        {
            return new Matrix(_data);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(this[i, j].ToString("G6"));
                    if (j + 1 < Columns) sb.Append('\t');
                }

                if (i + 1 < Rows) sb.AppendLine();
            }

            return sb.ToString();
        }

        public Matrix Add(Matrix other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (Rows != other.Rows || Columns != other.Columns)
                throw new DimensionMismatchException("Сложение возможно только для матриц одинакового размера.");

            var res = new Matrix(Rows, Columns);

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    res[i, j] = this[i, j] + other[i, j];

            return res;
        }

        public Matrix Subtract(Matrix other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (Rows != other.Rows || Columns != other.Columns)
                throw new DimensionMismatchException("Вычитание возможно только для матриц одинакового размера.");

            var res = new Matrix(Rows, Columns);

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    res[i, j] = this[i, j] - other[i, j];

            return res;
        }

        public Matrix Multiply(double scalar)
        {
            var res = new Matrix(Rows, Columns);

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    res[i, j] = this[i, j] * scalar;

            return res;
        }

        public Matrix Multiply(Matrix other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (Columns != other.Rows)
                throw new DimensionMismatchException("Количество столбцов первой матрицы должно совпадать с количеством строк второй.");

            var res = new Matrix(Rows, other.Columns);

            for (int i = 0; i < Rows; i++)
                for (int k = 0; k < Columns; k++)
                {
                    var aik = this[i, k];
                    for (int j = 0; j < other.Columns; j++)
                        res[i, j] += aik * other[k, j];
                }

            return res;
        }

        public Matrix Transpose()
        {
            var res = new Matrix(Columns, Rows);

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    res[j, i] = this[i, j];

            return res;
        }

        public Matrix Power(int exponent)
        {
            if (Rows != Columns)
                throw new DimensionMismatchException("Возведение в степень определено только для квадратных матриц.");

            if (exponent == 0)
                return Identity(Rows);

            if (exponent < 0)
            {
                var inv = Inverse();
                return inv.Power(-exponent);
            }

            Matrix result = Identity(Rows);
            Matrix baseM = this.Clone();
            int e = exponent;

            while (e > 0)
            {
                if ((e & 1) == 1)
                    result = result.Multiply(baseM);

                baseM = baseM.Multiply(baseM);
                e >>= 1;
            }

            return result;
        }

        public double Determinant()
        {
            if (Rows != Columns)
                throw new DimensionMismatchException("Определитель вычисляется только для квадратных матриц.");

            var a = (double[,])_data.Clone();
            int n = Rows;
            int[] pivot = new int[n];

            for (int i = 0; i < n; i++)
                pivot[i] = i;

            double detSign = 1.0;

            for (int k = 0; k < n; k++)
            {
                int pivRow = k;
                double max = Math.Abs(a[k, k]);

                for (int i = k + 1; i < n; i++)
                {
                    double val = Math.Abs(a[i, k]);
                    if (val > max)
                    {
                        max = val;
                        pivRow = i;
                    }
                }

                if (max < EPS)
                    return 0.0;

                if (pivRow != k)
                {
                    for (int j = 0; j < n; j++)
                    {
                        double tmp = a[k, j];
                        a[k, j] = a[pivRow, j];
                        a[pivRow, j] = tmp;
                    }

                    int tmpi = pivot[k];
                    pivot[k] = pivot[pivRow];
                    pivot[pivRow] = tmpi;

                    detSign = -detSign;
                }

                for (int i = k + 1; i < n; i++)
                {
                    a[i, k] /= a[k, k];
                    double factor = a[i, k];

                    for (int j = k + 1; j < n; j++)
                        a[i, j] -= factor * a[k, j];
                }
            }

            double det = detSign;

            for (int i = 0; i < n; i++)
                det *= a[i, i];

            return det;
        }

        public Matrix Inverse()
        {
            if (Rows != Columns)
                throw new DimensionMismatchException("Обратная матрица определена только для квадратных матриц.");

            int n = Rows;
            var aug = new double[n, 2 * n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = this[i, j];

                aug[i, n + i] = 1.0;
            }

            for (int col = 0; col < n; col++)
            {
                int pivotRow = col;
                double max = Math.Abs(aug[col, col]);

                for (int r = col + 1; r < n; r++)
                {
                    double val = Math.Abs(aug[r, col]);
                    if (val > max)
                    {
                        max = val;
                        pivotRow = r;
                    }
                }

                if (max < EPS)
                    throw new SingularMatrixException("Матрица вырождена и не имеет обратной.");

                if (pivotRow != col)
                {
                    for (int j = 0; j < 2 * n; j++)
                    {
                        double tmp = aug[col, j];
                        aug[col, j] = aug[pivotRow, j];
                        aug[pivotRow, j] = tmp;
                    }
                }

                double pivotVal = aug[col, col];

                for (int j = 0; j < 2 * n; j++)
                    aug[col, j] /= pivotVal;

                for (int r = 0; r < n; r++)
                {
                    if (r == col) continue;

                    double factor = aug[r, col];

                    if (Math.Abs(factor) < EPS) continue;

                    for (int j = 0; j < 2 * n; j++)
                        aug[r, j] -= factor * aug[col, j];
                }
            }

            var inv = new Matrix(n, n);

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inv[i, j] = aug[i, n + j];

            return inv;
        }

        public int Rank()
        {
            var a = (double[,])_data.Clone();
            int m = Rows;
            int n = Columns;
            int rank = 0;
            int r = 0;

            for (int c = 0; c < n && r < m; c++)
            {
                int pivot = r;
                double max = Math.Abs(a[pivot, c]);

                for (int i = r + 1; i < m; i++)
                {
                    double val = Math.Abs(a[i, c]);
                    if (val > max)
                    {
                        max = val;
                        pivot = i;
                    }
                }

                if (max < EPS)
                    continue;

                if (pivot != r)
                {
                    for (int j = c; j < n; j++)
                    {
                        double tmp = a[r, j];
                        a[r, j] = a[pivot, j];
                        a[pivot, j] = tmp;
                    }
                }

                double diag = a[r, c];

                for (int j = c; j < n; j++)
                    a[r, j] /= diag;

                for (int i = r + 1; i < m; i++)
                {
                    double factor = a[i, c];

                    for (int j = c; j < n; j++)
                        a[i, j] -= factor * a[r, j];
                }

                r++;
                rank++;
            }

            return rank;
        }
    }
}
