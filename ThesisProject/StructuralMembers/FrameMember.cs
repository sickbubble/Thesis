using System;
using System.Collections.Generic;
using ThesisProject.Sections;
using System.Linq;

namespace ThesisProject.Structural_Members
{

    public enum eFrameMemberType
    {
        Rectangle = 0,
        DiagonalFirst = 1,
        DiagonalSecond = 2
    }

    public class FrameMember : IStructuralMember
    {

        #region Ctor
        public FrameMember()
        {
            GetDefaultEndConditions();
        }
        #endregion

        #region Private Fields

        private Node _IEndNode;
        private Node _JEndNode;
        private int _ID;
        private EndCondition _IEndCondition;
        private EndCondition _JEndCondition;


        private eMemberType _MemberType;
        private FrameSection _Section;

        #endregion

        #region Thesis Specific Fields

        private eFrameMemberType _FrameType;

        #endregion

        #region Public Properties
        public Node IEndNode { get => _IEndNode; set => _IEndNode = value; }
        public Node JEndNode { get => _JEndNode; set => _JEndNode = value; }
        public int ID { get => _ID; set => _ID = value; }


        #endregion

        #region Interface Implementations
        public eMemberType MemberType { get => _MemberType; set => _MemberType = value; }
        public FrameSection Section { get => _Section; set => _Section = value; }
        public EndCondition IEndCondition { get => _IEndCondition; set => _IEndCondition = value; }
        public EndCondition JEndCondition { get => _JEndCondition; set => _JEndCondition = value; }
        public eFrameMemberType FrameType { get => _FrameType; set => _FrameType = value; }

        public MatrixCS GetGlobalStiffnessMatrix()
        {
            var rotationMatrix = this.GetRotationMatrix();
            var localStiffnessMatrix = this.GetLocalStiffnessMatrix();
            var transposedRotMatrix = rotationMatrix.Transpose();

            var secondPartOfMultiplication = localStiffnessMatrix.Multiply(rotationMatrix);
            var globalStiffnessMatrix = transposedRotMatrix.Multiply(secondPartOfMultiplication);

            return globalStiffnessMatrix;
        }

        public void SetAsTrussMember()
        {
            var trussReleases = new EndCondition();
            trussReleases.IsReleaseMx = true;
            trussReleases.IsReleaseMy = true;
            trussReleases.IsReleaseMz = true;



            this.IEndCondition = trussReleases;
            this.JEndCondition = trussReleases;
        }

        public void SetAllFixed()
        {
            this.IEndCondition.IsReleaseMx = false;
            this.IEndCondition.IsReleaseMy = false;
            this.IEndCondition.IsReleaseMz = false;

            this.JEndCondition.IsReleaseMx = false;
            this.JEndCondition.IsReleaseMy = false;
            this.JEndCondition.IsReleaseMz = false;

            ;
        }

        public double GetMass()
        {
            return this.Section.Material.Uw * this.Section.Area * this.GetLength();
        }

        public void SetUwByMass(double totalMass)
        {
            this.Section.Material.Uw = totalMass / (this.Section.Area * this.GetLength());
        }

        public double GetLength()
        {
            double length = 0;

            var iNode = this.IEndNode;
            var iX = iNode.Point.X;
            var iY = iNode.Point.Y;
            var iZ = iNode.Point.Z;


            var jNode = this.JEndNode;
            var jX = jNode.Point.X;
            var jY = jNode.Point.Y;
            var jZ = jNode.Point.Z;



            var sqrr = (iX - jX) * (iX - jX) + (iY - jY) * (iY - jY) + (iZ - jZ) * (iZ - jZ);
            length = Math.Sqrt(sqrr);


            return length;
        }

        public MatrixCS GetRotationMatrix()
        {

            var L = GetLength();
            var cX = (this.JEndNode.Point.X - this.IEndNode.Point.X) / L;
            var cY = (this.JEndNode.Point.Y - this.IEndNode.Point.Y) / L;
            var cZ = (this.JEndNode.Point.Z - this.IEndNode.Point.Z) / L;
            var cXZ = Math.Sqrt((cX * cX) + (cZ * cZ));

            double compareVal = 0.0;
            double alpha = 0;
            double tol = 1e-8;

            MatrixCS retVal = new MatrixCS(3, 3);

            var areEqual = Math.Abs(Math.Abs(cXZ) - Math.Abs(compareVal)) <= tol;

            if (areEqual) // Vertical members
            {
                retVal.Matrix[0, 0] = 0.0;
                retVal.Matrix[0, 1] = cY;
                retVal.Matrix[0, 2] = 0.0;

                retVal.Matrix[1, 0] = -1 * cY * Math.Cos(alpha);
                retVal.Matrix[1, 1] = 0;
                retVal.Matrix[1, 2] = Math.Sin(alpha);

                retVal.Matrix[2, 0] = cY * Math.Sin(alpha);
                retVal.Matrix[2, 1] = 0.0;
                retVal.Matrix[2, 2] = Math.Cos(alpha);
            }
            else // General case
            {
                retVal.Matrix[0, 0] = cX;
                retVal.Matrix[0, 1] = cY;
                retVal.Matrix[0, 2] = cZ;

                retVal.Matrix[1, 0] = -1 * ((cX * cY * Math.Cos(alpha)) + (cZ * Math.Sin(alpha))) / cXZ;
                retVal.Matrix[1, 1] = cXZ * Math.Cos(alpha);
                retVal.Matrix[1, 2] = ((-cY * cZ * Math.Cos(alpha)) + (cX * Math.Sin(alpha))) / cXZ;

                retVal.Matrix[2, 0] = ((cX * cY * Math.Sin(alpha)) - (cZ * Math.Cos(alpha))) / cXZ;
                retVal.Matrix[2, 1] = -1 * cXZ * Math.Sin(alpha);
                retVal.Matrix[2, 2] = ((cY * cZ * Math.Sin(alpha)) + (cX * Math.Cos(alpha))) / cXZ;
            }


            var rotElm = new MatrixCS(12, 12);

            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 12; j++)
                    rotElm.Matrix[i, j] = 0;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    rotElm.Matrix[i, j] = retVal.Matrix[i, j];

            for (int i = 3; i < 6; i++)
                for (int j = 3; j < 6; j++)
                    rotElm.Matrix[i, j] = retVal.Matrix[i - 3, j - 3];

            for (int i = 6; i < 9; i++)
                for (int j = 6; j < 9; j++)
                    rotElm.Matrix[i, j] = retVal.Matrix[i - 6, j - 6];

            for (int i = 9; i < 12; i++)
                for (int j = 9; j < 12; j++)
                    rotElm.Matrix[i, j] = retVal.Matrix[i - 9, j - 9];



            return rotElm;

            //return rotElm;
        }

        public MatrixCS GetLocalStiffnessMatrix(bool useEI = false)
        {
            var section = (FrameSection)_Section;
            var kElm = new MatrixCS(12, 12);
            var L = GetLength();
            var L2 = L * L;
            var L3 = Math.Pow(L, 3);
            var E = section.Material.E;
            var G = section.Material.G;
            var poisson = section.Material.Poissons;
            var A = section.Area;
            var I11 = section.I11;
            var I22 = section.I22;
            var J = section.J;



            var EI = section.EI;

            kElm.Matrix[0, 0] = E * A / L;
            kElm.Matrix[0, 6] = -1 * E * A / L;

            if (useEI)
            {
                kElm.Matrix[1, 1] = 12 * EI / L3;
                kElm.Matrix[1, 5] = 6 * EI / L2;
                kElm.Matrix[1, 7] = -12 * EI / L3;
                kElm.Matrix[1, 11] = 6 * EI / L2;
            }
            else
            {
                kElm.Matrix[1, 1] = 12 * E * I22 / L3;
                kElm.Matrix[1, 5] = 6 * E * I22 / L2;
                kElm.Matrix[1, 7] = -12 * E * I22 / L3;
                kElm.Matrix[1, 11] = 6 * E * I22 / L2;
            }

            if (useEI)
            {

                kElm.Matrix[2, 2] = 12 * EI / L3;
                kElm.Matrix[2, 4] = -6 * EI / L2;
                kElm.Matrix[2, 8] = -12 * EI / L3;
                kElm.Matrix[2, 10] = -6 * EI / L2;

            }
            else
            {

                kElm.Matrix[2, 2] = 12 * E * I11 / L3;
                kElm.Matrix[2, 4] = -6 * E * I11 / L2;
                kElm.Matrix[2, 8] = -12 * E * I11 / L3;
                kElm.Matrix[2, 10] = -6 * E * I11 / L2;
            }

            kElm.Matrix[3, 3] = G * J / L;
            kElm.Matrix[3, 9] = -1 * G * J / L;

            kElm.Matrix[4, 2] = kElm.Matrix[2, 4];

            if (useEI)
            {
                kElm.Matrix[4, 4] = 4 * EI / L;
                kElm.Matrix[4, 8] = 6 * EI / L2;
                kElm.Matrix[4, 10] = 2 * EI / L;

            }
            else
            {

                kElm.Matrix[4, 4] = 4 * E * I22 / L;
                kElm.Matrix[4, 8] = 6 * E * I22 / L2;
                kElm.Matrix[4, 10] = 2 * E * I22 / L;
            }


            kElm.Matrix[5, 1] = kElm.Matrix[1, 5];
            if (useEI)
            {
                kElm.Matrix[5, 5] = 4 * EI / L;
                kElm.Matrix[5, 7] = -6 * EI / L2;
                kElm.Matrix[5, 11] = 2 * EI / L;

            }
            else
            {
                kElm.Matrix[5, 5] = 4 * E * I11 / L;
                kElm.Matrix[5, 7] = -6 * E * I11 / L2;
                kElm.Matrix[5, 11] = 2 * E * I11 / L;
            }


            kElm.Matrix[6, 0] = kElm.Matrix[0, 6];
            kElm.Matrix[6, 6] = E * A / L;

            kElm.Matrix[7, 1] = kElm.Matrix[1, 7];
            kElm.Matrix[7, 5] = kElm.Matrix[5, 7];

            if (useEI)
            {
                kElm.Matrix[7, 7] = 12 * EI / L3;
                kElm.Matrix[7, 11] = -6 * EI / L2;
            }
            else
            {
                kElm.Matrix[7, 7] = 12 * E * I22 / L3;
                kElm.Matrix[7, 11] = -6 * E * I22 / L2;
            }

            kElm.Matrix[8, 2] = kElm.Matrix[2, 8];
            kElm.Matrix[8, 4] = kElm.Matrix[4, 8];

            if (useEI)
            {
                kElm.Matrix[8, 8] = 12 * EI / L3;
                kElm.Matrix[8, 10] = 6 * EI / L2;
            }
            else
            {
                kElm.Matrix[8, 8] = 12 * E * I11 / L3;
                kElm.Matrix[8, 10] = 6 * E * I11 / L2;

            }

            kElm.Matrix[9, 3] = kElm.Matrix[3, 9];
            kElm.Matrix[9, 9] = G * J / L;

            kElm.Matrix[10, 2] = kElm.Matrix[2, 10];
            kElm.Matrix[10, 4] = kElm.Matrix[4, 10];
            kElm.Matrix[10, 8] = kElm.Matrix[8, 10];

            kElm.Matrix[10, 10] = useEI ? 4 * EI / L : 4 * E * I22 / L;

            kElm.Matrix[11, 1] = kElm.Matrix[1, 11];
            kElm.Matrix[11, 5] = kElm.Matrix[5, 11];
            kElm.Matrix[11, 7] = kElm.Matrix[7, 11];
            kElm.Matrix[11, 11] = useEI ? 4 * EI / L : 4 * E * I11 / L;


            // Check I-End Releases

            if (this._IEndCondition.IsReleaseFx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[0, i] = 0.0;
                    kElm.Matrix[i, 0] = 0.0;
                }
            }

            if (this._IEndCondition.IsReleaseFy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[1, i] = 0.0;
                    kElm.Matrix[i, 1] = 0.0;
                }
            }
            if (this._IEndCondition.IsReleaseFz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[2, i] = 0.0;
                    kElm.Matrix[i, 2] = 0.0;
                }
            }

            if (this._IEndCondition.IsReleaseMx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[3, i] = 0.0;
                    kElm.Matrix[i, 3] = 0.0;
                }
            }
            if (this._IEndCondition.IsReleaseMy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[4, i] = 0.0;
                    kElm.Matrix[i, 4] = 0.0;
                }
            }

            if (this._IEndCondition.IsReleaseMz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[5, i] = 0.0;
                    kElm.Matrix[i, 5] = 0.0;
                }
            }

            // Check J-End Releases

            if (this._JEndCondition.IsReleaseFx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[6, i] = 0.0;
                    kElm.Matrix[i, 6] = 0.0;
                }
            }

            if (this._JEndCondition.IsReleaseFy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[7, i] = 0.0;
                    kElm.Matrix[i, 7] = 0.0;
                }
            }
            if (this._JEndCondition.IsReleaseFz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[8, i] = 0.0;
                    kElm.Matrix[i, 8] = 0.0;
                }
            }

            if (this._JEndCondition.IsReleaseMx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[9, i] = 0.0;
                    kElm.Matrix[i, 9] = 0.0;
                }
            }
            if (this._JEndCondition.IsReleaseMy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[10, i] = 0.0;
                    kElm.Matrix[i, 10] = 0.0;
                }
            }

            if (this._JEndCondition.IsReleaseMz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[11, i] = 0.0;
                    kElm.Matrix[i, 11] = 0.0;
                }
            }

            return kElm;
        }

        public MatrixCS GetLocalMassMatrix()
        {
            var massMatrix = new MatrixCS(12, 12);

            var A = this.Section.Area;
            var rho = this.Section.Material.Uw * A;
            double L = this.GetLength();


            var m = 0.5 * rho * L;
            massMatrix.Matrix[0, 0] = m;
            massMatrix.Matrix[1, 1] = m;
            massMatrix.Matrix[2, 2] = m;
            massMatrix.Matrix[4, 4] = 1e-6 * m * L * L;
            massMatrix.Matrix[5, 5] = 1e-6 * m * L * L;
            massMatrix.Matrix[6, 6] = m;
            massMatrix.Matrix[7, 7] = m;
            massMatrix.Matrix[8, 8] = m;
            massMatrix.Matrix[10, 10] = 1e-6 * m * L * L;
            massMatrix.Matrix[11, 11] = 1e-6 * m * L * L;


            return massMatrix;
        }

        public MatrixCS GetGlobalMassMatrix()
        {
            var rot = this.GetRotationMatrix();
            var rotTrans = rot.Transpose();
            var localMassMatrix = this.GetLocalMassMatrix();

            return (rotTrans.Multiply(localMassMatrix)).Multiply(rot);


        }

        #endregion


        #region Private Methods

        private void GetDefaultEndConditions()
        {
            this.IEndCondition = new EndCondition();
            this.JEndCondition = new EndCondition();
            //this.IEndCondition
        }




        #endregion

    }

    public class EndCondition
    {
        public EndCondition()
        {
            SetDefaultEndCond();
        }
        private bool _IsReleaseFx;
        private bool _IsReleaseFy;
        private bool _IsReleaseFz;
        private bool _IsReleaseMx;
        private bool _IsReleaseMy;
        private bool _IsReleaseMz;

        public bool IsReleaseFx { get => _IsReleaseFx; set => _IsReleaseFx = value; }
        public bool IsReleaseFy { get => _IsReleaseFy; set => _IsReleaseFy = value; }
        public bool IsReleaseFz { get => _IsReleaseFz; set => _IsReleaseFz = value; }
        public bool IsReleaseMx { get => _IsReleaseMx; set => _IsReleaseMx = value; }
        public bool IsReleaseMy { get => _IsReleaseMy; set => _IsReleaseMy = value; }
        public bool IsReleaseMz { get => _IsReleaseMz; set => _IsReleaseMz = value; }

        /// <summary>
        /// All not released for deafault
        /// </summary>
        private void SetDefaultEndCond()
        {
            _IsReleaseFx = false;
            _IsReleaseFy = false;
            _IsReleaseFz = false;
            _IsReleaseMx = false;
            _IsReleaseMy = false;
            _IsReleaseMz = false;
        }
    }
}
