using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;
using ThesisProject.Sections;

namespace ThesisProject.Structural_Members
{
    public enum eMembraneType
    {
        NA = 0,
        Bilinear = 1,
        Incompatible = 2,
        Drilling = 3
    }

    public enum ePlateType
    {
        NONE = 0,
        MindlinFourNode = 1
    };
    public class QuadShellMember : IStructuralMember
    {
        #region Ctor
        public QuadShellMember()
        {

        }
        #endregion

        #region Private Fields

        private Node _IEndNode;
        private Node _JEndNode;
        private Node _KEndNode;
        private Node _LEndNode;
        private eMemberType _MemberType;
        private ISection _Section;
        private Material _Material;

        private double _Thickness;

        private eMembraneType _MembraneType;
        private ePlateType _PlateType;

        #endregion

        #region Public Properties
        public Node IEndNode { get => _IEndNode; set => _IEndNode = value; }
        public Node JEndNode { get => _JEndNode; set => _JEndNode = value; }
        public Node KEndNode { get => _KEndNode; set => _KEndNode = value; }
        public Node LEndNode { get => _LEndNode; set => _LEndNode = value; }

        #endregion

        #region Interface Implementations
        public eMemberType MemberType { get => _MemberType; set => _MemberType = value; }
        public ISection Section { get => _Section; set => _Section = value; }
        public Material Material { get => _Material; set => _Material = value; }
        public double Thickness { get => _Thickness; set => _Thickness = value; }
        public eMembraneType MembraneType { get => _MembraneType; set => _MembraneType = value; }
        public eMembraneType MembraneType1 { get => _MembraneType; set => _MembraneType = value; }
        public ePlateType PlateType { get => _PlateType; set => _PlateType = value; }

        public void GetGlobalStiffnessMatrix()
        {
        }

        public void GetLocalStiffnessMatrix()
        {
            // Stiffness matrix is calculated at four Gauss points using Gauss Quadrature
            // It is assumed that nodes are oriented in counter-clock wise direction

            // Material parameters
            var e = this.Material.E;
            var v = this.Material.Poissons;

            var eMult = e / (1 - (v * v));
            var eMat = new MatrixCS(3, 3);



            eMat.Matrix[0, 0] = eMult * 1; eMat.Matrix[0, 1] = eMult * v;
            eMat.Matrix[1, 0] = eMult * v; eMat.Matrix[1, 1] = eMult * 1;
            eMat.Matrix[2, 2] = eMult * (1 - v) / 2;

            // Thickness
            var thickness = _Thickness;

            // Map coordinates of flat plane to 2-D surface
            var d1 = this.IEndNode.Point.DistTo(JEndNode.Point);
            var d2 = this.JEndNode.Point.DistTo(KEndNode.Point);
            var d3 = this.KEndNode.Point.DistTo(LEndNode.Point);
            var d4 = this.LEndNode.Point.DistTo(IEndNode.Point);


            //Vector p1V(Nodes[0]->Coordinate);
            //Vector p2V(Nodes[1]->Coordinate);
            //Vector p3V(Nodes[2]->Coordinate);
            //Vector p4V(Nodes[3]->Coordinate);

            //// Angle between first line and fourth line
            //var firstVector0 = p2V - p1V;
            //var secondVector0 = p4V - p1V;
            var alpha0 = 90d;
            var alpha1 = 90d;
            var alpha2 = 90d;

            // Map 3D coordinates to 2D plane using angles and length found above to be able to
            // use natural coordinates
            var x1 = 0.0; var y1 = 0.0;
            var x2 = d1; var y2 = 0.0;
            var x3 = x2 - (d2 * Math.Cos(alpha1)); var y3 = d2 * Math.Sin(alpha1);
            var x4 = d4 * Math.Cos(alpha0); var y4 = d4 * Math.Sin(alpha0);


            MatrixCS mappedCoords = new MatrixCS(4, 2);

            mappedCoords.Matrix[0, 0] = x1; mappedCoords.Matrix[0, 1] = y1;
            mappedCoords.Matrix[1, 0] = x2; mappedCoords.Matrix[1, 1] = y2;
            mappedCoords.Matrix[2, 0] = x3; mappedCoords.Matrix[2, 1] = y3;
            mappedCoords.Matrix[3, 0] = x4; mappedCoords.Matrix[3, 1] = y4;



            // Membrane action resists in-plane translational degrees of freedom and the plate 
            // action resists bending effects. 
            MatrixCS elmK = new MatrixCS(24, 24);

            MatrixCS kMembrane = new MatrixCS(8, 8);

            if (MembraneType == eMembraneType.Bilinear)
            {
                var gp = 1 / Math.Sqrt(3);
                double[,] gaussPoints = { { -gp, -gp }, { gp, -gp }, { gp, gp }, { -gp, gp } };

                var rowCounter = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        // Get Gauss point
                        var ksi = gaussPoints[rowCounter, 0]; var eta = gaussPoints[rowCounter, 1];

                        // Calculate jacobi
                        MatrixCS j1 = new MatrixCS(2, 4);

                        j1.Matrix[0, 0] = eta - 1; j1.Matrix[0, 1] = 1 - eta; j1.Matrix[0, 2] = eta + 1; j1.Matrix[0, 3] = -eta - 1;
                        j1.Matrix[1, 0] = ksi - 1; j1.Matrix[1, 1] = -ksi - 1; j1.Matrix[1, 2] = ksi + 1; j1.Matrix[1, 3] = 1 - ksi;
                        var j2 = j1.Multiply(mappedCoords);

                        var jacobi = j2.Multiply(0.25);

                        MatrixCS inversejacobi = new MatrixCS(2, 2);


                        var detjacobi = (jacobi.Matrix[0, 0] * jacobi.Matrix[1, 1]) - (jacobi.Matrix[0, 1] * jacobi.Matrix[1, 0]);
                        inversejacobi.Matrix[0, 0] = jacobi.Matrix[1, 1] / detjacobi; inversejacobi.Matrix[0, 1] = -1 * jacobi.Matrix[0, 1] / detjacobi;
                        inversejacobi.Matrix[1, 0] = -1 * jacobi.Matrix[1, 0] / detjacobi; inversejacobi.Matrix[1, 1] = jacobi.Matrix[0, 0] / detjacobi;

                        // Calculate strain-displacement matrix (B]
                        MatrixCS mat1 = new MatrixCS(3, 4);
                        mat1.Matrix[0, 0] = 1; mat1.Matrix[1, 3] = 1; mat1.Matrix[2, 1] = 1; mat1.Matrix[2, 2] = 1;

                        MatrixCS mat2 = new MatrixCS(4, 4);
                        mat2.Matrix[0, 0] = inversejacobi.Matrix[0, 0]; mat2.Matrix[0, 1] = inversejacobi.Matrix[0, 1]; mat2.Matrix[1, 0] = inversejacobi.Matrix[1, 0]; mat2.Matrix[1, 1] = inversejacobi.Matrix[1, 1];
                        mat2.Matrix[2, 2] = inversejacobi.Matrix[0, 0]; mat2.Matrix[2, 3] = inversejacobi.Matrix[0, 1]; mat2.Matrix[3, 2] = inversejacobi.Matrix[1, 0]; mat2.Matrix[3, 3] = inversejacobi.Matrix[1, 1];

                        MatrixCS mat3 = new MatrixCS(4, 8);
                        mat3.Matrix[0, 0] = eta - 1; mat3.Matrix[0, 2] = 1 - eta; mat3.Matrix[0, 4] = eta + 1; mat3.Matrix[0, 6] = -eta - 1;
                        mat3.Matrix[1, 0] = ksi - 1; mat3.Matrix[1, 2] = -ksi - 1; mat3.Matrix[1, 4] = ksi + 1; mat3.Matrix[1, 6] = 1 - ksi;
                        mat3.Matrix[2, 1] = eta - 1; mat3.Matrix[2, 3] = 1 - eta; mat3.Matrix[2, 5] = eta + 1; mat3.Matrix[2, 7] = -eta - 1;
                        mat3.Matrix[3, 1] = ksi - 1; mat3.Matrix[3, 3] = -ksi - 1; mat3.Matrix[3, 5] = ksi + 1; mat3.Matrix[3, 7] = 1 - ksi;
                        mat3.Multiply(0.25);


                        var b = (mat1.Multiply(mat2)).Multiply(mat3);
                        var bT = b.Transpose();

                        // Stiffness calculated at given Gauss point
                        var firstPart = bT.Multiply(eMat);
                        var secondPart = b.Multiply(thickness * detjacobi);
                        var kPt = firstPart.Multiply(secondPart);

                        // Update stifness
                        for (int row = 0; row < 8; row++)
                            for (int col = 0; col < 8; col++)
                                kMembrane.Matrix[row, col] += kPt.Matrix[row, col];

                        rowCounter++;
                    }
                }
            }
            else if (MembraneType == eMembraneType.Incompatible )
            {
                var gp = 1 / Math.Sqrt(3);
                double[,] gaussPoints = { { -gp, -gp }, { gp, -gp }, { gp, gp }, { -gp, gp } };

                var rowCounter = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        // Get Gauss point
                        var ksi = gaussPoints[rowCounter, 0]; var eta = gaussPoints[rowCounter, 1];

                        // Calculate jacobi
                        MatrixCS j1 = new MatrixCS(2, 4);

                        j1.Matrix[0, 0] = eta - 1; j1.Matrix[0, 1] = 1 - eta; j1.Matrix[0, 2] = eta + 1; j1.Matrix[0, 3] = -eta - 1;
                        j1.Matrix[1, 0] = ksi - 1; j1.Matrix[1, 1] = -ksi - 1; j1.Matrix[1, 2] = ksi + 1; j1.Matrix[1, 3] = 1 - ksi;
                        var j2 = j1.Multiply(mappedCoords);

                        var jacobi = j2.Multiply(0.25);

                        MatrixCS inversejacobi = new MatrixCS(2, 2);


                        var detjacobi = (jacobi.Matrix[0, 0] * jacobi.Matrix[1, 1]) - (jacobi.Matrix[0, 1] * jacobi.Matrix[1, 0]);
                        inversejacobi.Matrix[0, 0] = jacobi.Matrix[1, 1] / detjacobi; inversejacobi.Matrix[0, 1] = -1 * jacobi.Matrix[0, 1] / detjacobi;
                        inversejacobi.Matrix[1, 0] = -1 * jacobi.Matrix[1, 0] / detjacobi; inversejacobi.Matrix[1, 1] = jacobi.Matrix[0, 0] / detjacobi;

                        // Calculate strain-displacement matrix (B]
                        MatrixCS mat1 = new MatrixCS(3, 4);
                        mat1.Matrix[0, 0] = 1; mat1.Matrix[1, 3] = 1; mat1.Matrix[2, 1] = 1; mat1.Matrix[2, 2] = 1;

                        MatrixCS mat2 = new MatrixCS(4, 4);
                        mat2.Matrix[0, 0] = inversejacobi.Matrix[0, 0]; mat2.Matrix[0, 1] = inversejacobi.Matrix[0, 1]; mat2.Matrix[1, 0] = inversejacobi.Matrix[1, 0]; mat2.Matrix[1, 1] = inversejacobi.Matrix[1, 1];
                        mat2.Matrix[2, 2] = inversejacobi.Matrix[0, 0]; mat2.Matrix[2, 3] = inversejacobi.Matrix[0, 1]; mat2.Matrix[3, 2] = inversejacobi.Matrix[1, 0]; mat2.Matrix[3, 3] = inversejacobi.Matrix[1, 1];

                        MatrixCS mat3 = new MatrixCS(4, 8);
                        mat3.Matrix[0, 0] = eta - 1; mat3.Matrix[0, 2] = 1 - eta; mat3.Matrix[0, 4] = eta + 1; mat3.Matrix[0, 6] = -eta - 1;
                        mat3.Matrix[1, 0] = ksi - 1; mat3.Matrix[1, 2] = -ksi - 1; mat3.Matrix[1, 4] = ksi + 1; mat3.Matrix[1, 6] = 1 - ksi;
                        mat3.Matrix[2, 1] = eta - 1; mat3.Matrix[2, 3] = 1 - eta; mat3.Matrix[2, 5] = eta + 1; mat3.Matrix[2, 7] = -eta - 1;
                        mat3.Matrix[3, 1] = ksi - 1; mat3.Matrix[3, 3] = -ksi - 1; mat3.Matrix[3, 5] = ksi + 1; mat3.Matrix[3, 7] = 1 - ksi;
                        mat3.Multiply(0.25);


                        var b = (mat1.Multiply(mat2)).Multiply(mat3);

                        b.Matrix[0, 8] = inversejacobi.Matrix[0, 0] * -2 * ksi;
                        b.Matrix[0, 9] = inversejacobi.Matrix[0, 1] * -2 * eta;
                        b.Matrix[1, 10] = inversejacobi.Matrix[1, 0] * -2 * ksi;
                        b.Matrix[1, 11] = inversejacobi.Matrix[1, 1] * -2 * eta;
                        b.Matrix[2, 8] = inversejacobi.Matrix[1, 0] * -2 * ksi;
                        b.Matrix[2, 9] = inversejacobi.Matrix[1, 1] * -2 * eta;
                        b.Matrix[2, 10] = inversejacobi.Matrix[0, 0] * -2 * ksi;
                        b.Matrix[2, 11] = inversejacobi.Matrix[0, 1] * -2 * eta;


                        var bT = b.Transpose();


                        // Stiffness calculated at given Gauss point
                        var firstPart = bT.Multiply(eMat);
                        var secondPart = b.Multiply(thickness * detjacobi);
                        var kPt = firstPart.Multiply(secondPart);

                        // Update stifness
                        for (int row = 0; row < 12; row++)
                            for (int col = 0; col < 12; col++)
                                kMembrane.Matrix[row, col] += kPt.Matrix[row, col];

                        rowCounter++;
                    }
                }

            }
        }

        MatrixCS IStructuralMember.GetLocalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }

        MatrixCS IStructuralMember.GetGlobalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }

        public MatrixCS GetRotationMatrix()
        {
            throw new NotImplementedException();
        }
    }
}

#endregion