using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject
{
    public class MatrixCS
    {
        public MatrixCS(int numOfRows, int numOfCols)
        {
            _Matrix = new double[numOfRows, numOfCols];
            _nRows = numOfRows;
            _nColumns = numOfCols;
        }
        private double[,] _Matrix;
        private int _nRows;
        private int _nColumns;


        public double[,] Matrix { get => _Matrix; set => _Matrix = value; }
        public int NRows { get => _nRows; set => _nRows = value; }
        public int NColumns { get => _nColumns; set => _nColumns = value; }






        #region Public Methods

        public void Print()
        {
            //Console.WriteLine("This Matrix");
            //for (int i = 0; i < this.NRows; i++)
            //{
            //    for (int j = 0; j < this.NColumns; j++)
            //    {
            //        Console.Write(this.Matrix[i, j].ToString() + " ");
            //    }
            //    Console.WriteLine();
            //}
        }

        public void InsertMatrix(MatrixCS matrix, int startingRow, int startingColumn)
        {
            var lastColumn = startingColumn + matrix.NColumns;
            var lastRow = startingRow+ matrix.NRows;

            int rowCounter = 0;
            int columnCounter = 0;

            for (int i = startingRow; i < lastRow; i++) 
            { 
                for (int j = startingColumn; j < lastColumn; j++)
                {
                    this.Matrix[i, j] = matrix.Matrix[rowCounter, columnCounter];
                        columnCounter++;
                }

            columnCounter = 0;
            
            rowCounter++;
            }


        }
        public MatrixCS Multiply(double multiplyWith)
        {
            MatrixCS product = new MatrixCS(this.NRows, this.NColumns);
            for (int i = 0; i < _nRows; i++)
            {
                for (int j = 0; j < _nColumns; j++)
                {
                    product.Matrix[i, j] = this.Matrix[i, j] * multiplyWith;

                }

            }

            return product;
        }

        public MatrixCS Multiply(MatrixCS multiplyWith)
        {
            MatrixCS product = new MatrixCS(this.NRows,multiplyWith.NColumns);
            double[,] matrix1 = this.Matrix;

            double[,] matrix2 = multiplyWith.Matrix;


            // cahing matrix lengths for better performance  
            var matrix1Rows = matrix1.GetLength(0);
            var matrix1Cols = matrix1.GetLength(1);
            var matrix2Rows = matrix2.GetLength(0);
            var matrix2Cols = matrix2.GetLength(1);

            // checking if product is defined  
            if (matrix1Cols != matrix2Rows)
                throw new InvalidOperationException
                  ("Product is undefined. n columns of first matrix must equal to n rows of second matrix");

            // creating the final product matrix  

            // looping through matrix 1 rows  
            for (int matrix1_row = 0; matrix1_row < matrix1Rows; matrix1_row++)
            {
                // for each matrix 1 row, loop through matrix 2 columns  
                for (int matrix2_col = 0; matrix2_col < matrix2Cols; matrix2_col++)
                {
                    // loop through matrix 1 columns to calculate the dot product  
                    for (int matrix1_col = 0; matrix1_col < matrix1Cols; matrix1_col++)
                    {
                        product.Matrix[matrix1_row, matrix2_col] +=
                          matrix1[matrix1_row, matrix1_col] *
                          matrix2[matrix1_col, matrix2_col];
                    }
                }
            }

            return product;
        }

        public MatrixCS Sum(MatrixCS sumWith)
        {
            MatrixCS product = new MatrixCS(this.NRows, this.NColumns);

            for (int i = 0; i < sumWith.NRows; i++)
            {
                for (int j = 0; j < sumWith.NColumns; j++)
                {
                    product.Matrix[i, j] += this.Matrix[i, j] + sumWith.Matrix[i, j];

                }

            }

            return product;
        }

        public MatrixCS Transpose()
        {
            var m =  this.NRows;
            var n = this.NColumns;
            var product = new MatrixCS(n, m);

            var arr1 = this.Matrix;

            var transpose = new double[n, m];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    transpose[j, i] = arr1[i, j];
                }
            }
            product.Matrix = transpose;
            return product ;

        }

        #endregion

    }
}
