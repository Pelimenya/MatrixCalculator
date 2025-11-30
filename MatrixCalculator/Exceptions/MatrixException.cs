using System;

namespace MatrixCalculator.Exceptions
{
    public class MatrixException : Exception
    {
        public MatrixException(string message) : base(message) { }
    }

    public class DimensionMismatchException : MatrixException
    {
        public DimensionMismatchException(string message) : base(message) { }
    }

    public class SingularMatrixException : MatrixException
    {
        public SingularMatrixException(string message) : base(message) { }
    }

    public class InvalidMatrixFormatException : MatrixException
    {
        public InvalidMatrixFormatException(string message) : base(message) { }
    }
}