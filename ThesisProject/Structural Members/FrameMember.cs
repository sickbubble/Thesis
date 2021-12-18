using System;
using System.Collections.Generic;
using ThesisProject.Sections;
using System.Linq;

namespace ThesisProject.Structural_Members
{
   

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
        private ISection _Section;


        #endregion

        #region Public Properties
        public Node IEndNode { get => _IEndNode; set => _IEndNode = value; }
        public Node JEndNode { get => _JEndNode; set => _JEndNode = value; }
        public int ID { get => _ID; set => _ID = value; }


        #endregion

        #region Interface Implementations
        public eMemberType MemberType { get => _MemberType; set => _MemberType = value; }
        public ISection Section { get => _Section; set => _Section = value; }
        internal EndCondition IEndCondition { get => _IEndCondition; set => _IEndCondition = value; }
        internal EndCondition JEndCondition { get => _JEndCondition; set => _JEndCondition = value; }



        public MatrixCS GetGlobalStiffnessMatrix()
        {
            var globalStiffnessMatrix = new MatrixCS(12, 12);
            var rotationMatrix = new MatrixCS(12, 12);
            var transposedRotMatrix = new MatrixCS(12, 12);
            var localStiffnessMatrix = new MatrixCS(12, 12);


            rotationMatrix = this.GetRotationMatrix();
            localStiffnessMatrix = this.GetLocalStiffnessMatrix();
            transposedRotMatrix = rotationMatrix.Transpose();

            var secondPartOfMultiplication = localStiffnessMatrix.Multiply(rotationMatrix);
            globalStiffnessMatrix = transposedRotMatrix.Multiply(secondPartOfMultiplication);

            return globalStiffnessMatrix;
        }

        public MatrixCS GetRotationMatrix()
        {
            var L = Math.Sqrt(Math.Pow(this.JEndNode.Point.X - this.IEndNode.Point.X, 2) + Math.Pow(this.JEndNode.Point.Y - this.IEndNode.Point.Y, 2) + Math.Pow(this.JEndNode.Point.Z - this.IEndNode.Point.Z, 2));

            var Cx = (this.JEndNode.Point.X - this.IEndNode.Point.X) / L;
            var Cy = (this.JEndNode.Point.Y - this.IEndNode.Point.Y) / L;
            var Cz = (this.JEndNode.Point.Z - this.IEndNode.Point.Z) / L;
            var Cxz = Math.Sqrt(Cx * Cx + Cy * Cy);
            var alpha = 0;

            var Ri = new MatrixCS(3, 3);

            Ri.Matrix[0, 0] = Cx;
            Ri.Matrix[0, 1] = Cz;
            Ri.Matrix[0, 2] = Cy;
            Ri.Matrix[1, 0] = (-Cz * Cx * Math.Cos(alpha)) - Cy * Math.Sin(alpha) / Cxz;
            Ri.Matrix[1, 1] = Cxz * Math.Cos(alpha);
            Ri.Matrix[1, 2] = (-Cz * Cy * Math.Cos(alpha)) + Cx * Math.Sin(alpha) / Cxz;
            Ri.Matrix[2, 0] = ((Cz * Cx * Math.Sin(alpha)) - Cy * Math.Cos(alpha)) / Cxz;
            Ri.Matrix[2, 1] = -Cxz * Math.Sin(alpha);
            Ri.Matrix[2, 2] = ((Cz * Cy * Math.Sin(alpha)) + Cx * Math.Cos(alpha)) / Cxz;


            var rotElm = new MatrixCS(12, 12); // 3 columns will be added by using InsertRange
            rotElm.InsertMatrix(Ri, 0, 0);
            rotElm.InsertMatrix(Ri, 3, 3);
            rotElm.InsertMatrix(Ri, 6, 6);
            rotElm.InsertMatrix(Ri, 9, 9);


            return rotElm;
        }

        public MatrixCS GetLocalStiffnessMatrix()
        {
            var section = (FrameSection)_Section;
            var kElm = new MatrixCS(12, 12);
            var L = Math.Sqrt(Math.Pow(this.JEndNode.Point.X - this.IEndNode.Point.X, 2) + Math.Pow(this.JEndNode.Point.Y - this.IEndNode.Point.Y, 2)+ Math.Pow(this.JEndNode.Point.Z - this.IEndNode.Point.Z, 2));
            var L2 = L * L;
            var L3 = Math.Pow(L, 3);
            var E = section.Material.E;
            var G = section.Material.G;
            var poisson = section.Material.Poissons;
            var A = section.Area;
            var I11 = section.I11;
            var I22 = section.I22;
            var J = section.J;

            kElm.Matrix[0,0] = E * A / L;
            kElm.Matrix[0,6] = -1 * E * A / L;

            kElm.Matrix[1,1] = 12 * E * I11 / L3;
            kElm.Matrix[1,5] = 6 * E * I11 / L2;
            kElm.Matrix[1,7] = -12 * E * I11 / L3;
            kElm.Matrix[1,11] = 6 * E * I11 / L2;

            kElm.Matrix[2,2] = 12 * E * I22 / L3;
            kElm.Matrix[2,4] = -6 * E * I22 / L2;
            kElm.Matrix[2,8] = -12 * E * I22 / L3;
            kElm.Matrix[2,10] = -6 * E * I22 / L2;

            kElm.Matrix[3,3] = G * J / L;
            kElm.Matrix[3,9] = -1 * G * J / L;

            kElm.Matrix[4,2] = kElm.Matrix[2,4];
            kElm.Matrix[4,4] = 4 * E * I11 / L;
            kElm.Matrix[4,8] = 6 * E * I11 / L2;
            kElm.Matrix[4,10] = 2 * E * I11 / L;

            kElm.Matrix[5,1] = kElm.Matrix[1,5];
            kElm.Matrix[5,5] = 4 * E * I22 / L;
            kElm.Matrix[5,7] = -6 * E * I22 / L2;
            kElm.Matrix[5,11] = 2 * E * I22 / L;

            kElm.Matrix[6,0] = kElm.Matrix[0,6];
            kElm.Matrix[6,6] = E * A / L;

            kElm.Matrix[7,1] = kElm.Matrix[1,7];
            kElm.Matrix[7,5] = kElm.Matrix[5,7];
            kElm.Matrix[7,7] = 12 * E * I11 / L3;
            kElm.Matrix[7,11] = -6 * E * I11 / L2;

            kElm.Matrix[8,2] = kElm.Matrix[2,8];
            kElm.Matrix[8,4] = kElm.Matrix[4,8];
            kElm.Matrix[8,8] = 12 * E * I22 / L3;
            kElm.Matrix[8,10] = 6 * E * I22 / L2;

            kElm.Matrix[9,3] = kElm.Matrix[3,9];
            kElm.Matrix[9,9] = G * J / L;

            kElm.Matrix[10,2] = kElm.Matrix[2,10];
            kElm.Matrix[10,4] = kElm.Matrix[4,10];
            kElm.Matrix[10,8] = kElm.Matrix[8,10];
            kElm.Matrix[10,10] = 4 * E * I11 / L;

            kElm.Matrix[11,1] = kElm.Matrix[1,11];
            kElm.Matrix[11,5] = kElm.Matrix[5,11];
            kElm.Matrix[11,7] = kElm.Matrix[7,11];
            kElm.Matrix[11,11] = 4 * E * I22 / L;


            // Check I-End Releases

            if (this._IEndCondition.IsReleaseFx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[0,i] = 0.0;
                    kElm.Matrix[i,0] = 0.0;
                }
            }

            if (this._IEndCondition.IsReleaseFy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[1,i] = 0.0;
                    kElm.Matrix[i,1] = 0.0;
                }
            }
            if (this._IEndCondition.IsReleaseFz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[2,i] = 0.0;
                    kElm.Matrix[i,2] = 0.0;
                }
            }

            if (this._IEndCondition.IsReleaseMx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[3,i] = 0.0;
                    kElm.Matrix[i,3] = 0.0;
                }
            }
            if (this._IEndCondition.IsReleaseMy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[4,i] = 0.0;
                    kElm.Matrix[i,4] = 0.0;
                }
            }

            if (this._IEndCondition.IsReleaseMz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[5,i] = 0.0;
                    kElm.Matrix[i,5] = 0.0;
                }
            }

            // Check J-End Releases

            if (this._JEndCondition.IsReleaseFx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[6,i] = 0.0;
                    kElm.Matrix[i,6] = 0.0;
                }
            }

            if (this._JEndCondition.IsReleaseFy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[7,i] = 0.0;
                    kElm.Matrix[i,7] = 0.0;
                }
            }
            if (this._JEndCondition.IsReleaseFz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[8,i] = 0.0;
                    kElm.Matrix[i,8] = 0.0;
                }
            }

            if (this._JEndCondition.IsReleaseMx)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[9,i] = 0.0;
                    kElm.Matrix[i,9] = 0.0;
                }
            }
            if (this._JEndCondition.IsReleaseMy)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[10,i] = 0.0;
                    kElm.Matrix[i,10] = 0.0;
                }
            }

            if (this._JEndCondition.IsReleaseMz)
            {
                for (int i = 0; i < 12; i++)
                {
                    kElm.Matrix[11,i] = 0.0;
                    kElm.Matrix[i,11] = 0.0;
                }
            }

            return kElm;
        }


        #endregion


        #region Private Methods

        private void GetDefaultEndConditions()
        {
            this.IEndCondition= new EndCondition();
            this.JEndCondition = new EndCondition();
            //this.IEndCondition
        }


        #endregion 

    }

    class EndCondition
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
            _IsReleaseFx=false;
            _IsReleaseFy=false;
            _IsReleaseFz=false;
            _IsReleaseMx=false;
            _IsReleaseMy=false;
            _IsReleaseMz = false;
        }
    }
}
