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
        }
        private double[,] _Matrix;

        public double[,] Matrix { get => _Matrix; set => _Matrix = value; }



        #region Public Methods

        public void InsertMatrix(double[,] matrix, int startingRow, int startingColumn)
        {
            var lastColumn = startingColumn + matrix.Length;
            var lastRow = startingRow+ matrix.Length;

            int rowCounter = 0;
            int columnCounter = 0;

            for (int i = startingRow; i < lastRow; i++) 
            { 
                for (int j = startingColumn; j < lastColumn; j++)
                {
                    this.Matrix[i, j] = matrix[rowCounter, columnCounter];
                        columnCounter++;
                }

            columnCounter = 0;
            
            rowCounter++;
            }


        }

        public double[,] Multiply(double[,] matrix2)
        {

            double[,] matrix1 = this.Matrix;

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
            double[,] product = new double[matrix1Rows, matrix2Cols];

            // looping through matrix 1 rows  
            for (int matrix1_row = 0; matrix1_row < matrix1Rows; matrix1_row++)
            {
                // for each matrix 1 row, loop through matrix 2 columns  
                for (int matrix2_col = 0; matrix2_col < matrix2Cols; matrix2_col++)
                {
                    // loop through matrix 1 columns to calculate the dot product  
                    for (int matrix1_col = 0; matrix1_col < matrix1Cols; matrix1_col++)
                    {
                        product[matrix1_row, matrix2_col] +=
                          matrix1[matrix1_row, matrix1_col] *
                          matrix2[matrix1_col, matrix2_col];
                    }
                }
            }

            return product;
        }

        public double [,] Transpose()
        {
            var m =  this.Matrix.GetLength(0);
            var n = this.Matrix.GetLength(0);

            var arr1 = this.Matrix;

            var transpose = new double[n, m];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    transpose[j, i] = arr1[i, j];
                }
            }

            return transpose;

        }

        #endregion

    }
}
