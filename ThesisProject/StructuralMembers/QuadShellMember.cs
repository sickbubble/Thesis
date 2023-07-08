using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;
using ThesisProject.LocalDataHolders;
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
        private ShellSection _Section;
        private int _ID;

        private eMembraneType _MembraneType;
        private ePlateType _PlateType;
        private bool _IsOnlyPlate;

        private double _MemberMass;

        #endregion

        #region Public Properties
        public Node IEndNode { get => _IEndNode; set => _IEndNode = value; }
        public Node JEndNode { get => _JEndNode; set => _JEndNode = value; }
        public Node KEndNode { get => _KEndNode; set => _KEndNode = value; }
        public Node LEndNode { get => _LEndNode; set => _LEndNode = value; }
        public ShellSection Section { get => _Section; set => _Section = value; }

        #endregion

        #region Interface Implementations
        public eMemberType MemberType { get => _MemberType; set => _MemberType = value; }
        public eMembraneType MembraneType { get => _MembraneType; set => _MembraneType = value; }
        public eMembraneType MembraneType1 { get => _MembraneType; set => _MembraneType = value; }
        public ePlateType PlateType { get => _PlateType; set => _PlateType = value; }
        public int ID { get => _ID; set => _ID = value; }
        public bool IsOnlyPlate { get => _IsOnlyPlate; set => _IsOnlyPlate = value; }
        public double MemberMass { get => _MemberMass; private set => _MemberMass = value; }

        public MatrixCS GetLocalStiffnessMatrix(bool useEI = false)
        {
            // Stiffness matrix is calculated at four Gauss points using Gauss Quadrature
            // It is assumed that nodes are oriented in counter-clock wise direction

            // Material parameters
            var e = this.Section.Material.E;
            var v = this.Section.Material.Poissons;

            var eMult = e / (1 - (v * v));
            var eMat = new MatrixCS(3, 3);



            eMat.Matrix[0, 0] = eMult * 1; eMat.Matrix[0, 1] = eMult * v;
            eMat.Matrix[1, 0] = eMult * v; eMat.Matrix[1, 1] = eMult * 1;
            eMat.Matrix[2, 2] = eMult * (1 - v) / 2;

            // Thickness
            var thickness = this.Section.Thickness;

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
            var alpha0 = (Math.PI) / 2;
            var alpha1 = (Math.PI) / 2;
            var alpha2 = (Math.PI) / 2;

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

            //mappedCoords.Print();

            // Membrane action resists in-plane translational degrees of freedom and the plate 
            // action resists bending effects. 

            MatrixCS elmK = new MatrixCS(24, 24);

            MatrixCS kMembrane = new MatrixCS(8, 8);
            MembraneType = eMembraneType.Drilling;
            PlateType = ePlateType.MindlinFourNode;

            if (!this.IsOnlyPlate)
            {
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
                else if (MembraneType == eMembraneType.Incompatible)
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
                else if (MembraneType == eMembraneType.Drilling)
                {
                    var kDrilling = new MatrixCS(12, 12);

                    double V = 0.0;
                    var gpCoeff = 1 / Math.Sqrt(3);


                    double[,] gauss2x2PointsWeights = new double[4, 3] { { -gpCoeff, -gpCoeff, 1 }, { gpCoeff, -gpCoeff, 1 }, { gpCoeff, gpCoeff, 1 }, { -gpCoeff, gpCoeff, 1 } };
                    int[,] jk = new int[4, 2] { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 } };
                    int[,] ms = new int[4, 2] { { 7, 4 }, { 4, 5 }, { 5, 6 }, { 6, 7 } };
                    int[] ml = new int[4] { 3, 0, 1, 2 };
                    double[,] nL = new double[4, 2];

                    for (int i = 0; i < 4; i++)
                    {
                        var edgeStartNodeCoordX = mappedCoords.Matrix[jk[i, 0], 0];
                        var edgeStartNodeCoordY = mappedCoords.Matrix[jk[i, 0], 1];
                        var edgeEndNodeCoordX = mappedCoords.Matrix[jk[i, 1], 0];
                        var edgeEndNodeCoordY = mappedCoords.Matrix[jk[i, 1], 1];

                        nL[i, 0] = edgeEndNodeCoordY - edgeStartNodeCoordY;
                        nL[i, 1] = edgeStartNodeCoordX - edgeEndNodeCoordX;
                    }


                    for (int gaussCounter = 0; gaussCounter < 4; gaussCounter++)
                    {
                        var ksi = gauss2x2PointsWeights[gaussCounter, 0];
                        var eta = gauss2x2PointsWeights[gaussCounter, 1];
                        var weight = gauss2x2PointsWeights[gaussCounter, 2];

                        // Bilinear shape functions
                        double[] N4 = new double[] {
                        0.25 * (1 - ksi) * (1 - eta),
                        0.25 * (1 + ksi) * (1 - eta),
                        0.25 * (1 + ksi) * (1 + eta),
                        0.25 * (1 - ksi) * (1 + eta)
                    };

                        // Derivative of bilinear shape functions with respect to ksi
                        double[] dN4Ksi = new double[4];
                        dN4Ksi[0] = -0.25 * (1 - eta);
                        dN4Ksi[1] = 0.25 * (1 - eta);
                        dN4Ksi[2] = 0.25 * (1 + eta);
                        dN4Ksi[3] = -0.25 * (1 + eta);

                        // Derivative of bilinear shape functions with respect to eta
                        double[] dN4Eta = new double[4];
                        dN4Eta[0] = -0.25 * (1 - ksi);
                        dN4Eta[1] = -0.25 * (1 + ksi);
                        dN4Eta[2] = 0.25 * (1 + ksi);
                        dN4Eta[3] = 0.25 * (1 - ksi);

                        // Calculate jacobi
                        MatrixCS j1 = new MatrixCS(2, 4);
                        j1.Matrix[0, 0] = dN4Ksi[0]; j1.Matrix[0, 1] = dN4Ksi[1]; j1.Matrix[0, 2] = dN4Ksi[2]; j1.Matrix[0, 3] = dN4Ksi[3];
                        j1.Matrix[1, 0] = dN4Eta[0]; j1.Matrix[1, 1] = dN4Eta[1]; j1.Matrix[1, 2] = dN4Eta[2]; j1.Matrix[1, 3] = dN4Eta[3];
                        var jacobi = j1.Multiply(mappedCoords);

                        var inverseJacobi = new MatrixCS(2, 2);

                        var detJacobi = (jacobi.Matrix[0, 0] * jacobi.Matrix[1, 1]) - (jacobi.Matrix[0, 1] * jacobi.Matrix[1, 0]);

                        inverseJacobi.Matrix[0, 0] = jacobi.Matrix[1, 1] / detJacobi; inverseJacobi.Matrix[0, 1] = -1 * jacobi.Matrix[0, 1] / detJacobi;
                        inverseJacobi.Matrix[1, 0] = -1 * jacobi.Matrix[1, 0] / detJacobi; inverseJacobi.Matrix[1, 1] = jacobi.Matrix[0, 0] / detJacobi;

                        double[] dN4X = new double[4];

                        dN4X[0] = (inverseJacobi.Matrix[0, 0] * dN4Ksi[0]) + (inverseJacobi.Matrix[0, 1] * dN4Eta[0]);
                        dN4X[1] = (inverseJacobi.Matrix[0, 0] * dN4Ksi[1]) + (inverseJacobi.Matrix[0, 1] * dN4Eta[1]);
                        dN4X[2] = (inverseJacobi.Matrix[0, 0] * dN4Ksi[2]) + (inverseJacobi.Matrix[0, 1] * dN4Eta[2]);
                        dN4X[3] = (inverseJacobi.Matrix[0, 0] * dN4Ksi[3]) + (inverseJacobi.Matrix[0, 1] * dN4Eta[3]);


                        // Derivative of bilinear shape functions with respect to y
                        double[] dN4Y = new double[4];

                        dN4Y[0] = (inverseJacobi.Matrix[1, 0] * dN4Ksi[0]) + (inverseJacobi.Matrix[1, 1] * dN4Eta[0]);
                        dN4Y[1] = (inverseJacobi.Matrix[1, 0] * dN4Ksi[1]) + (inverseJacobi.Matrix[1, 1] * dN4Eta[1]);
                        dN4Y[2] = (inverseJacobi.Matrix[1, 0] * dN4Ksi[2]) + (inverseJacobi.Matrix[1, 1] * dN4Eta[2]);
                        dN4Y[3] = (inverseJacobi.Matrix[1, 0] * dN4Ksi[3]) + (inverseJacobi.Matrix[1, 1] * dN4Eta[3]);

                        // Quadratic shape functions
                        double[] N8 = new double[8] {
                -0.25 * (-1 + ksi) * (-1 + eta) * (ksi + eta + 1),
                0.25 * (1 + ksi) * (-1 + eta) * (-ksi + eta + 1),
                0.25 * (1 + ksi) * (1 + eta) * (ksi + eta - 1),
                -0.25 * (-1 + ksi) * (1 + eta) * (-ksi + eta - 1),
                0.50 * (-1 + (ksi * ksi)) * (-1 + eta),
                -0.50 * (1 + ksi) * (-1 + (eta * eta)),
                -0.50 * (-1 + (ksi * ksi)) * (1 + eta),
                0.50 * (-1 + ksi) * (-1 + (eta * eta))
            };

                        // Derivatives of quadratic shape functions with respect to natural coordinates
                        double[] dN8Ksi = new double[8] {
                0.25 * ((2 * ksi) - (2 * eta * ksi) - (eta * eta) + (eta)),
                0.25 * ((2 * ksi) - (2 * eta * ksi) + (eta * eta) - (eta)),
                0.25 * ((2 * ksi) + (2 * eta * ksi) + (eta * eta) + (eta)),
                0.25 * ((2 * ksi) + (2 * eta * ksi) - (eta * eta) - (eta)),
                0.25 * (4 * (ksi) * (-1 + eta)),
                0.25 * (2 - (2 * eta * eta)),
                0.25 * (-4 * (ksi) * (1 + eta)),
                0.25 * (-2 + (2 * eta * eta))
            };

                        double[] dN8Eta = new double[8] {
                0.25 * ((2 * eta) - (ksi * ksi) - (2 * eta * ksi) + (ksi)),
                0.25 * ((2 * eta) - (ksi * ksi) + (2 * eta * ksi) - (ksi)),
                0.25 * ((2 * eta) + (ksi * ksi) + (2 * eta * ksi) + (ksi)),
                0.25 * ((2 * eta) + (ksi * ksi) - (2 * eta * ksi) - (ksi)),
                0.25 * (-2 + (2 * ksi * ksi)),
                0.25 * (-4 * (1 + ksi) * (eta)),
                0.25 * (2 - (2 * ksi * ksi)),
                0.25 * (4 * (-1 + ksi) * eta)
            };

                        // Derivatives of quadratic shape functions with respect to cartesian coordinates
                        double[] dN8X = new double[8] {
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[0]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[0])),
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[1]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[1])),
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[2]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[2])),
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[3]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[3])),
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[4]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[4])),
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[5]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[5])),
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[6]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[6])),
                ((inverseJacobi.Matrix[0, 0] * dN8Ksi[7]) + (inverseJacobi.Matrix[0, 1] * dN8Eta[7]))
            };

                        double[] dN8Y = new double[8] {
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[0]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[0])),
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[1]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[1])),
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[2]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[2])),
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[3]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[3])),
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[4]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[4])),
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[5]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[5])),
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[6]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[6])),
                ((inverseJacobi.Matrix[1, 0] * dN8Ksi[7]) + (inverseJacobi.Matrix[1, 1] * dN8Eta[7]))
            };

                        // Create strain-displacement relationship
                        var B = new MatrixCS(3, 12);

                        for (int i = 0; i < 4; i++)
                        {
                            int j = (i * 3);
                            int k = j + 1;
                            int m = j + 2;

                            B.Matrix[0, j] = dN4X[i];
                            B.Matrix[1, k] = dN4Y[i];
                            B.Matrix[2, j] = dN4Y[i];
                            B.Matrix[2, k] = dN4X[i];

                            B.Matrix[0, m] = ((dN8X[ms[i, 0]] * nL[ml[i], 0]) - (dN8X[ms[i, 1]] * nL[i, 0])) / 8.0;
                            B.Matrix[1, m] = ((dN8Y[ms[i, 0]] * nL[ml[i], 1]) - (dN8Y[ms[i, 1]] * nL[i, 1])) / 8.0;
                            B.Matrix[2, m] = ((dN8Y[ms[i, 0]] * nL[ml[i], 0]) - (dN8Y[ms[i, 1]] * nL[i, 0])) / 8.0;
                            B.Matrix[2, m] += ((dN8X[ms[i, 0]] * nL[ml[i], 1]) - (dN8X[ms[i, 1]] * nL[i, 1])) / 8.0;
                        }

                        var vi = thickness * detJacobi * weight;
                        var littleK = ((B.Transpose().Multiply(eMat)).Multiply(B)).Multiply(vi);
                        kDrilling = kDrilling.Sum(littleK);
                        V += vi;

                    }


                    // Create stabilizator for zero-energy modes
                    var kD = new MatrixCS(12, 12);
                    var em = this.Section.Material.E;
                    var alpha = (1.0 / 150000.0);
                    var kDia = alpha * em * V;

                    for (int i = 1; i < 5; i++)
                        kD.Matrix[(3 * i) - 1, (3 * i) - 1] = 1.75 * kDia;

                    kD.Matrix[2, 5] = -0.75 * kDia;
                    kD.Matrix[2, 8] = -0.25 * kDia;
                    kD.Matrix[2, 11] = -0.75 * kDia;
                    kD.Matrix[5, 8] = -0.75 * kDia;
                    kD.Matrix[5, 11] = -0.25 * kDia;
                    kD.Matrix[8, 11] = -0.75 * kDia;
                    kD.Matrix[5, 2] = -0.75 * kDia;
                    kD.Matrix[8, 2] = -0.25 * kDia;
                    kD.Matrix[11, 2] = -0.75 * kDia;
                    kD.Matrix[8, 5] = -0.75 * kDia;
                    kD.Matrix[11, 5] = -0.25 * kDia;
                    kD.Matrix[11, 8] = -0.75 * kDia;
                    kDrilling = kDrilling.Sum(kD);
                    //kDrilling.Print();

                    // Map membrane stifness to element stiffness(For a plate at XY - plane, it resists translation - X, translation - Y and rotation - Z)
                    int mapper1 = 0; // Row updader
                    for (int i = 0; i < 12; i++)
                    {
                        int mapper2 = 0; // Column updater
                        for (int j = 0; j < 12; j++)
                        {
                            elmK.Matrix[i + mapper1, j + mapper2] = kDrilling.Matrix[i, j];
                            if (((j + 2) % 3) == 0)
                                mapper2 += 3;
                        }
                        if (((i + 2) % 3) == 0)
                            mapper1 += 3;
                    }

                    if ((this.MembraneType == eMembraneType.Bilinear) || (this.MembraneType == eMembraneType.Incompatible))
                    {
                        elmK.Matrix[0, 0] = kMembrane.Matrix[0, 0]; elmK.Matrix[0, 1] = kMembrane.Matrix[0, 1]; elmK.Matrix[0, 6] = kMembrane.Matrix[0, 2]; elmK.Matrix[0, 7] = kMembrane.Matrix[0, 3]; elmK.Matrix[0, 12] = kMembrane.Matrix[0, 4]; elmK.Matrix[0, 13] = kMembrane.Matrix[0, 5]; elmK.Matrix[0, 18] = kMembrane.Matrix[0, 6]; elmK.Matrix[0, 19] = kMembrane.Matrix[0, 7];
                        elmK.Matrix[1, 0] = kMembrane.Matrix[1, 0]; elmK.Matrix[1, 1] = kMembrane.Matrix[1, 1]; elmK.Matrix[1, 6] = kMembrane.Matrix[1, 2]; elmK.Matrix[1, 7] = kMembrane.Matrix[1, 3]; elmK.Matrix[1, 12] = kMembrane.Matrix[1, 4]; elmK.Matrix[1, 13] = kMembrane.Matrix[1, 5]; elmK.Matrix[1, 18] = kMembrane.Matrix[1, 6]; elmK.Matrix[1, 19] = kMembrane.Matrix[1, 7];
                        elmK.Matrix[6, 0] = kMembrane.Matrix[2, 0]; elmK.Matrix[6, 1] = kMembrane.Matrix[2, 1]; elmK.Matrix[6, 6] = kMembrane.Matrix[2, 2]; elmK.Matrix[6, 7] = kMembrane.Matrix[2, 3]; elmK.Matrix[6, 12] = kMembrane.Matrix[2, 4]; elmK.Matrix[6, 13] = kMembrane.Matrix[2, 5]; elmK.Matrix[6, 18] = kMembrane.Matrix[2, 6]; elmK.Matrix[6, 19] = kMembrane.Matrix[2, 7];
                        elmK.Matrix[7, 0] = kMembrane.Matrix[3, 0]; elmK.Matrix[7, 1] = kMembrane.Matrix[3, 1]; elmK.Matrix[7, 6] = kMembrane.Matrix[3, 2]; elmK.Matrix[7, 7] = kMembrane.Matrix[3, 3]; elmK.Matrix[7, 12] = kMembrane.Matrix[3, 4]; elmK.Matrix[7, 13] = kMembrane.Matrix[3, 5]; elmK.Matrix[7, 18] = kMembrane.Matrix[3, 6]; elmK.Matrix[7, 19] = kMembrane.Matrix[3, 7];

                        elmK.Matrix[12, 0] = kMembrane.Matrix[4, 0]; elmK.Matrix[12, 1] = kMembrane.Matrix[4, 1]; elmK.Matrix[12, 6] = kMembrane.Matrix[4, 2]; elmK.Matrix[12, 7] = kMembrane.Matrix[4, 3]; elmK.Matrix[12, 12] = kMembrane.Matrix[4, 4]; elmK.Matrix[12, 13] = kMembrane.Matrix[4, 5]; elmK.Matrix[12, 18] = kMembrane.Matrix[4, 6]; elmK.Matrix[12, 19] = kMembrane.Matrix[4, 7];
                        elmK.Matrix[13, 0] = kMembrane.Matrix[5, 0]; elmK.Matrix[13, 1] = kMembrane.Matrix[5, 1]; elmK.Matrix[13, 6] = kMembrane.Matrix[5, 2]; elmK.Matrix[13, 7] = kMembrane.Matrix[5, 3]; elmK.Matrix[13, 12] = kMembrane.Matrix[5, 4]; elmK.Matrix[13, 13] = kMembrane.Matrix[5, 5]; elmK.Matrix[13, 18] = kMembrane.Matrix[5, 6]; elmK.Matrix[13, 19] = kMembrane.Matrix[5, 7];
                        elmK.Matrix[18, 0] = kMembrane.Matrix[6, 0]; elmK.Matrix[18, 1] = kMembrane.Matrix[6, 1]; elmK.Matrix[18, 6] = kMembrane.Matrix[6, 2]; elmK.Matrix[18, 7] = kMembrane.Matrix[6, 3]; elmK.Matrix[18, 12] = kMembrane.Matrix[6, 4]; elmK.Matrix[18, 13] = kMembrane.Matrix[6, 5]; elmK.Matrix[18, 18] = kMembrane.Matrix[6, 6]; elmK.Matrix[18, 19] = kMembrane.Matrix[6, 7];
                        elmK.Matrix[19, 0] = kMembrane.Matrix[7, 0]; elmK.Matrix[19, 1] = kMembrane.Matrix[7, 1]; elmK.Matrix[19, 6] = kMembrane.Matrix[7, 2]; elmK.Matrix[19, 7] = kMembrane.Matrix[7, 3]; elmK.Matrix[19, 12] = kMembrane.Matrix[7, 4]; elmK.Matrix[19, 13] = kMembrane.Matrix[7, 5]; elmK.Matrix[19, 18] = kMembrane.Matrix[7, 6]; elmK.Matrix[19, 19] = kMembrane.Matrix[7, 7];
                    }

                }
            }



            // For an element on XY-Plane, plate action resists translation-Z, rotation-X and rotation-Y. For bending stiffness, use 2x2 gauss integration (weight is 1 for each point) 
            // and for shear part, use 1x1 gauss integration (weight is 2 for midpoint integration).
            if (this.PlateType == ePlateType.MindlinFourNode)
            {
                // Use bilinear shape functions for bending part
                var kBending = new MatrixCS(12, 12);
                var gpCoeff2 = 1 / Math.Sqrt(3);
                double[,] gaussPoints2 = new double[4, 2]
          { { -gpCoeff2, -gpCoeff2}, { gpCoeff2, -gpCoeff2}, { gpCoeff2, gpCoeff2}, { -gpCoeff2, gpCoeff2} };
                var rowCounter = 0;

                var flexuralRigidity = new MatrixCS(3, 3);
                var elas = this.Section.Material.E;
                var pois = this.Section.Material.Poissons;
                var thick = this.Section.Thickness;
                var fRMult = elas * (thick * thick * thick) / (12 * (1 - (pois * pois)));
                flexuralRigidity.Matrix[0, 0] = fRMult * 1; flexuralRigidity.Matrix[0, 1] = fRMult * pois;
                flexuralRigidity.Matrix[1, 0] = fRMult * pois; flexuralRigidity.Matrix[1, 1] = fRMult * 1;
                flexuralRigidity.Matrix[2, 2] = fRMult * (1 - pois) / 2;

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var ksi = gaussPoints2[rowCounter, 0]; var eta = gaussPoints2[rowCounter, 1];

                        // Calculate jacobi
                        var j1 = new MatrixCS(2, 4);
                        j1.Matrix[0, 0] = eta - 1; j1.Matrix[0, 1] = 1 - eta; j1.Matrix[0, 2] = eta + 1; j1.Matrix[0, 3] = -eta - 1;
                        j1.Matrix[1, 0] = ksi - 1; j1.Matrix[1, 1] = -ksi - 1; j1.Matrix[1, 2] = ksi + 1; j1.Matrix[1, 3] = 1 - ksi;
                        var j2 = j1.Multiply(mappedCoords);
                        var jacobi = j2.Multiply(0.25);

                        var inversejacobi = new MatrixCS(2, 2);
                        var detjacobi = jacobi.Matrix[0, 0] * jacobi.Matrix[1, 1] - jacobi.Matrix[0, 1] * jacobi.Matrix[1, 0];
                        inversejacobi.Matrix[0, 0] = jacobi.Matrix[1, 1] / detjacobi; inversejacobi.Matrix[0, 1] = -1 * jacobi.Matrix[0, 1] / detjacobi;
                        inversejacobi.Matrix[1, 0] = -1 * jacobi.Matrix[1, 0] / detjacobi; inversejacobi.Matrix[1, 1] = jacobi.Matrix[0, 0] / detjacobi;

                        // Bilinear shape functions
                        var n1 = 0.25 * (1 - ksi) * (1 - eta);
                        var n2 = 0.25 * (1 + ksi) * (1 - eta);
                        var n3 = 0.25 * (1 + ksi) * (1 + eta);
                        var n4 = 0.25 * (1 - ksi) * (1 + eta);

                        // Derivative of shape functions with respect to ksi
                        var dN1Ksi = -0.25 * (1 - eta);
                        var dN2Ksi = 0.25 * (1 - eta);
                        var dN3Ksi = 0.25 * (1 + eta);
                        var dN4Ksi = -0.25 * (1 + eta);

                        // Derivative of shape functions with respect to eta
                        var dN1Eta = -0.25 * (1 - ksi);
                        var dN2Eta = -0.25 * (1 + ksi);
                        var dN3Eta = 0.25 * (1 + ksi);
                        var dN4Eta = 0.25 * (1 - ksi);

                        // Derivative of shape functions with respect to x                                     
                        var dN1X = (inversejacobi.Matrix[0, 0] * dN1Ksi) + (inversejacobi.Matrix[0, 1] * dN1Eta);
                        var dN2X = (inversejacobi.Matrix[0, 0] * dN2Ksi) + (inversejacobi.Matrix[0, 1] * dN2Eta);
                        var dN3X = (inversejacobi.Matrix[0, 0] * dN3Ksi) + (inversejacobi.Matrix[0, 1] * dN3Eta);
                        var dN4X = (inversejacobi.Matrix[0, 0] * dN4Ksi) + (inversejacobi.Matrix[0, 1] * dN4Eta);

                        // Derivative of shape functions with respect to y
                        var dN1Y = (inversejacobi.Matrix[1, 0] * dN1Ksi) + (inversejacobi.Matrix[1, 1] * dN1Eta);
                        var dN2Y = (inversejacobi.Matrix[1, 0] * dN2Ksi) + (inversejacobi.Matrix[1, 1] * dN2Eta);
                        var dN3Y = (inversejacobi.Matrix[1, 0] * dN3Ksi) + (inversejacobi.Matrix[1, 1] * dN3Eta);
                        var dN4Y = (inversejacobi.Matrix[1, 0] * dN4Ksi) + (inversejacobi.Matrix[1, 1] * dN4Eta);




                        var bB = new MatrixCS(3, 12);
                        bB.Matrix[0, 2] = dN1X; bB.Matrix[0, 5] = dN2X; bB.Matrix[0, 8] = dN3X; bB.Matrix[0, 11] = dN4X;
                        bB.Matrix[1, 1] = -dN1Y; bB.Matrix[1, 4] = -dN2Y; bB.Matrix[1, 7] = -dN3Y; bB.Matrix[1, 10] = -dN4Y;
                        bB.Matrix[2, 1] = -dN1X; bB.Matrix[2, 4] = -dN2X; bB.Matrix[2, 7] = -dN3X; bB.Matrix[2, 10] = -dN4X;
                        bB.Matrix[2, 2] = dN1Y; bB.Matrix[2, 5] = dN2Y; bB.Matrix[2, 8] = dN3Y; bB.Matrix[2, 11] = dN4Y;
                        bB = bB.Multiply(-1);

                        var pointBendingStiffness = ((bB.Transpose().Multiply(flexuralRigidity)).Multiply(bB)).Multiply(detjacobi);
                        kBending = kBending.Sum(pointBendingStiffness);

                        rowCounter++;
                    }
                }

                // Again, use bilinear shape functions for shear stiffness part but use midpoint integration
                // (Multiply stiffness value calculated for ksi = 0 and eta = 0 by 2)
                // Use bilinear shape functions for bending part
                var kShear = new MatrixCS(12, 12);
                var shearRigidity = new MatrixCS(2, 2);
                double sR = (5.0 / 6.0) * this.Section.Material.G * this.Section.Thickness;
                // Shear rigidity is multiplied by two since shear stifness is calculated at only midpoint
                // and weight of midpoint is 2 for gauss-quadrature
                shearRigidity.Matrix[0, 0] = 4.0 * sR; shearRigidity.Matrix[1, 1] = 4.0 * sR;

                for (int j = 0; j < 1; j++)
                {
                    var ksi = 0.0; var eta = 0.0;

                    // Calculate jacobi
                    var j1 = new MatrixCS(2, 4);
                    j1.Matrix[0, 0] = eta - 1; j1.Matrix[0, 1] = 1 - eta; j1.Matrix[0, 2] = eta + 1; j1.Matrix[0, 3] = -eta - 1;
                    j1.Matrix[1, 0] = ksi - 1; j1.Matrix[1, 1] = -ksi - 1; j1.Matrix[1, 2] = ksi + 1; j1.Matrix[1, 3] = 1 - ksi;
                    var j2 = j1.Multiply(mappedCoords);
                    var jacobi = j2.Multiply(0.25);

                    var inversejacobi = new MatrixCS(2, 2);
                    var detjacobi = jacobi.Matrix[0, 0] * jacobi.Matrix[1, 1] - jacobi.Matrix[0, 1] * jacobi.Matrix[1, 0];
                    inversejacobi.Matrix[0, 0] = jacobi.Matrix[1, 1] / detjacobi; inversejacobi.Matrix[0, 1] = -1 * jacobi.Matrix[0, 1] / detjacobi;
                    inversejacobi.Matrix[1, 0] = -1 * jacobi.Matrix[1, 0] / detjacobi; inversejacobi.Matrix[1, 1] = jacobi.Matrix[0, 0] / detjacobi;


                    // Bilinear shape functions
                    var n1 = 0.25 * (1 - ksi) * (1 - eta);
                    var n2 = 0.25 * (1 + ksi) * (1 - eta);
                    var n3 = 0.25 * (1 + ksi) * (1 + eta);
                    var n4 = 0.25 * (1 - ksi) * (1 + eta);

                    // Derivative of shape functions with respect to ksi
                    var dN1Ksi = -0.25 * (1 - eta);
                    var dN2Ksi = 0.25 * (1 - eta);
                    var dN3Ksi = 0.25 * (1 + eta);
                    var dN4Ksi = -0.25 * (1 + eta);

                    // Derivative of shape functions with respect to eta
                    var dN1Eta = -0.25 * (1 - ksi);
                    var dN2Eta = -0.25 * (1 + ksi);
                    var dN3Eta = 0.25 * (1 + ksi);
                    var dN4Eta = 0.25 * (1 - ksi);

                    // Derivative of shape functions with respect to x
                    var dN1X = (inversejacobi.Matrix[0, 0] * dN1Ksi) + (inversejacobi.Matrix[0, 1] * dN1Eta);
                    var dN2X = (inversejacobi.Matrix[0, 0] * dN2Ksi) + (inversejacobi.Matrix[0, 1] * dN2Eta);
                    var dN3X = (inversejacobi.Matrix[0, 0] * dN3Ksi) + (inversejacobi.Matrix[0, 1] * dN3Eta);
                    var dN4X = (inversejacobi.Matrix[0, 0] * dN4Ksi) + (inversejacobi.Matrix[0, 1] * dN4Eta);

                    // Derivative of shape functions with respect to) y                                      
                    var dN1Y = (inversejacobi.Matrix[1, 0] * dN1Ksi) + (inversejacobi.Matrix[1, 1] * dN1Eta);
                    var dN2Y = (inversejacobi.Matrix[1, 0] * dN2Ksi) + (inversejacobi.Matrix[1, 1] * dN2Eta);
                    var dN3Y = (inversejacobi.Matrix[1, 0] * dN3Ksi) + (inversejacobi.Matrix[1, 1] * dN3Eta);
                    var dN4Y = (inversejacobi.Matrix[1, 0] * dN4Ksi) + (inversejacobi.Matrix[1, 1] * dN4Eta);

                    var bS = new MatrixCS(2, 12);
                    bS.Matrix[0, 0] = dN1X; bS.Matrix[0, 2] = n1;
                    bS.Matrix[0, 3] = dN2X; bS.Matrix[0, 5] = n2;
                    bS.Matrix[0, 6] = dN3X; bS.Matrix[0, 8] = n3;
                    bS.Matrix[0, 9] = dN4X; bS.Matrix[0, 11] = n4;

                    bS.Matrix[1, 0] = -dN1Y; bS.Matrix[1, 1] = n1;
                    bS.Matrix[1, 3] = -dN2Y; bS.Matrix[1, 4] = n2;
                    bS.Matrix[1, 6] = -dN3Y; bS.Matrix[1, 7] = n3;
                    bS.Matrix[1, 9] = -dN4Y; bS.Matrix[1, 10] = n4;

                    var pointShearStiffness = ((bS.Transpose().Multiply(shearRigidity)).Multiply(bS)).Multiply(detjacobi);
                    kShear = kShear.Sum(pointShearStiffness);
                }

                var kPlate = kBending.Sum(kShear);

                if (this.IsOnlyPlate) return kPlate;
                
                // Map plate stiffness to element stiffness
                elmK.Matrix[2, 2] = kPlate.Matrix[0, 0]; elmK.Matrix[2, 3] = kPlate.Matrix[0, 1]; elmK.Matrix[2, 4] = kPlate.Matrix[0, 2]; elmK.Matrix[2, 8] = kPlate.Matrix[0, 3]; elmK.Matrix[2, 9] = kPlate.Matrix[0, 4]; elmK.Matrix[2, 10] = kPlate.Matrix[0, 5]; elmK.Matrix[2, 14] = kPlate.Matrix[0, 6]; elmK.Matrix[2, 15] = kPlate.Matrix[0, 7]; elmK.Matrix[2, 16] = kPlate.Matrix[0, 8]; elmK.Matrix[2, 20] = kPlate.Matrix[0, 9]; elmK.Matrix[2, 21] = kPlate.Matrix[0, 10]; elmK.Matrix[2, 22] = kPlate.Matrix[0, 11];
                elmK.Matrix[3, 2] = kPlate.Matrix[1, 0]; elmK.Matrix[3, 3] = kPlate.Matrix[1, 1]; elmK.Matrix[3, 4] = kPlate.Matrix[1, 2]; elmK.Matrix[3, 8] = kPlate.Matrix[1, 3]; elmK.Matrix[3, 9] = kPlate.Matrix[1, 4]; elmK.Matrix[3, 10] = kPlate.Matrix[1, 5]; elmK.Matrix[3, 14] = kPlate.Matrix[1, 6]; elmK.Matrix[3, 15] = kPlate.Matrix[1, 7]; elmK.Matrix[3, 16] = kPlate.Matrix[1, 8]; elmK.Matrix[3, 20] = kPlate.Matrix[1, 9]; elmK.Matrix[3, 21] = kPlate.Matrix[1, 10]; elmK.Matrix[3, 22] = kPlate.Matrix[1, 11];
                elmK.Matrix[4, 2] = kPlate.Matrix[2, 0]; elmK.Matrix[4, 3] = kPlate.Matrix[2, 1]; elmK.Matrix[4, 4] = kPlate.Matrix[2, 2]; elmK.Matrix[4, 8] = kPlate.Matrix[2, 3]; elmK.Matrix[4, 9] = kPlate.Matrix[2, 4]; elmK.Matrix[4, 10] = kPlate.Matrix[2, 5]; elmK.Matrix[4, 14] = kPlate.Matrix[2, 6]; elmK.Matrix[4, 15] = kPlate.Matrix[2, 7]; elmK.Matrix[4, 16] = kPlate.Matrix[2, 8]; elmK.Matrix[4, 20] = kPlate.Matrix[2, 9]; elmK.Matrix[4, 21] = kPlate.Matrix[2, 10]; elmK.Matrix[4, 22] = kPlate.Matrix[2, 11];
                elmK.Matrix[8, 2] = kPlate.Matrix[3, 0]; elmK.Matrix[8, 3] = kPlate.Matrix[3, 1]; elmK.Matrix[8, 4] = kPlate.Matrix[3, 2]; elmK.Matrix[8, 8] = kPlate.Matrix[3, 3]; elmK.Matrix[8, 9] = kPlate.Matrix[3, 4]; elmK.Matrix[8, 10] = kPlate.Matrix[3, 5]; elmK.Matrix[8, 14] = kPlate.Matrix[3, 6]; elmK.Matrix[8, 15] = kPlate.Matrix[3, 7]; elmK.Matrix[8, 16] = kPlate.Matrix[3, 8]; elmK.Matrix[8, 20] = kPlate.Matrix[3, 9]; elmK.Matrix[8, 21] = kPlate.Matrix[3, 10]; elmK.Matrix[8, 22] = kPlate.Matrix[3, 11];
                elmK.Matrix[9, 2] = kPlate.Matrix[4, 0]; elmK.Matrix[9, 3] = kPlate.Matrix[4, 1]; elmK.Matrix[9, 4] = kPlate.Matrix[4, 2]; elmK.Matrix[9, 8] = kPlate.Matrix[4, 3]; elmK.Matrix[9, 9] = kPlate.Matrix[4, 4]; elmK.Matrix[9, 10] = kPlate.Matrix[4, 5]; elmK.Matrix[9, 14] = kPlate.Matrix[4, 6]; elmK.Matrix[9, 15] = kPlate.Matrix[4, 7]; elmK.Matrix[9, 16] = kPlate.Matrix[4, 8]; elmK.Matrix[9, 20] = kPlate.Matrix[4, 9]; elmK.Matrix[9, 21] = kPlate.Matrix[4, 10]; elmK.Matrix[9, 22] = kPlate.Matrix[4, 11];
                elmK.Matrix[10, 2] = kPlate.Matrix[5, 0]; elmK.Matrix[10, 3] = kPlate.Matrix[5, 1]; elmK.Matrix[10, 4] = kPlate.Matrix[5, 2]; elmK.Matrix[10, 8] = kPlate.Matrix[5, 3]; elmK.Matrix[10, 9] = kPlate.Matrix[5, 4]; elmK.Matrix[10, 10] = kPlate.Matrix[5, 5]; elmK.Matrix[10, 14] = kPlate.Matrix[5, 6]; elmK.Matrix[10, 15] = kPlate.Matrix[5, 7]; elmK.Matrix[10, 16] = kPlate.Matrix[5, 8]; elmK.Matrix[10, 20] = kPlate.Matrix[5, 9]; elmK.Matrix[10, 21] = kPlate.Matrix[5, 10]; elmK.Matrix[10, 22] = kPlate.Matrix[5, 11];
                elmK.Matrix[14, 2] = kPlate.Matrix[6, 0]; elmK.Matrix[14, 3] = kPlate.Matrix[6, 1]; elmK.Matrix[14, 4] = kPlate.Matrix[6, 2]; elmK.Matrix[14, 8] = kPlate.Matrix[6, 3]; elmK.Matrix[14, 9] = kPlate.Matrix[6, 4]; elmK.Matrix[14, 10] = kPlate.Matrix[6, 5]; elmK.Matrix[14, 14] = kPlate.Matrix[6, 6]; elmK.Matrix[14, 15] = kPlate.Matrix[6, 7]; elmK.Matrix[14, 16] = kPlate.Matrix[6, 8]; elmK.Matrix[14, 20] = kPlate.Matrix[6, 9]; elmK.Matrix[14, 21] = kPlate.Matrix[6, 10]; elmK.Matrix[14, 22] = kPlate.Matrix[6, 11];
                elmK.Matrix[15, 2] = kPlate.Matrix[7, 0]; elmK.Matrix[15, 3] = kPlate.Matrix[7, 1]; elmK.Matrix[15, 4] = kPlate.Matrix[7, 2]; elmK.Matrix[15, 8] = kPlate.Matrix[7, 3]; elmK.Matrix[15, 9] = kPlate.Matrix[7, 4]; elmK.Matrix[15, 10] = kPlate.Matrix[7, 5]; elmK.Matrix[15, 14] = kPlate.Matrix[7, 6]; elmK.Matrix[15, 15] = kPlate.Matrix[7, 7]; elmK.Matrix[15, 16] = kPlate.Matrix[7, 8]; elmK.Matrix[15, 20] = kPlate.Matrix[7, 9]; elmK.Matrix[15, 21] = kPlate.Matrix[7, 10]; elmK.Matrix[15, 22] = kPlate.Matrix[7, 11];
                elmK.Matrix[16, 2] = kPlate.Matrix[8, 0]; elmK.Matrix[16, 3] = kPlate.Matrix[8, 1]; elmK.Matrix[16, 4] = kPlate.Matrix[8, 2]; elmK.Matrix[16, 8] = kPlate.Matrix[8, 3]; elmK.Matrix[16, 9] = kPlate.Matrix[8, 4]; elmK.Matrix[16, 10] = kPlate.Matrix[8, 5]; elmK.Matrix[16, 14] = kPlate.Matrix[8, 6]; elmK.Matrix[16, 15] = kPlate.Matrix[8, 7]; elmK.Matrix[16, 16] = kPlate.Matrix[8, 8]; elmK.Matrix[16, 20] = kPlate.Matrix[8, 9]; elmK.Matrix[16, 21] = kPlate.Matrix[8, 10]; elmK.Matrix[16, 22] = kPlate.Matrix[8, 11];
                elmK.Matrix[20, 2] = kPlate.Matrix[9, 0]; elmK.Matrix[20, 3] = kPlate.Matrix[9, 1]; elmK.Matrix[20, 4] = kPlate.Matrix[9, 2]; elmK.Matrix[20, 8] = kPlate.Matrix[9, 3]; elmK.Matrix[20, 9] = kPlate.Matrix[9, 4]; elmK.Matrix[20, 10] = kPlate.Matrix[9, 5]; elmK.Matrix[20, 14] = kPlate.Matrix[9, 6]; elmK.Matrix[20, 15] = kPlate.Matrix[9, 7]; elmK.Matrix[20, 16] = kPlate.Matrix[9, 8]; elmK.Matrix[20, 20] = kPlate.Matrix[9, 9]; elmK.Matrix[20, 21] = kPlate.Matrix[9, 10]; elmK.Matrix[20, 22] = kPlate.Matrix[9, 11];
                elmK.Matrix[21, 2] = kPlate.Matrix[10, 0]; elmK.Matrix[21, 3] = kPlate.Matrix[10, 1]; elmK.Matrix[21, 4] = kPlate.Matrix[10, 2]; elmK.Matrix[21, 8] = kPlate.Matrix[10, 3]; elmK.Matrix[21, 9] = kPlate.Matrix[10, 4]; elmK.Matrix[21, 10] = kPlate.Matrix[10, 5]; elmK.Matrix[21, 14] = kPlate.Matrix[10, 6]; elmK.Matrix[21, 15] = kPlate.Matrix[10, 7]; elmK.Matrix[21, 16] = kPlate.Matrix[10, 8]; elmK.Matrix[21, 20] = kPlate.Matrix[10, 9]; elmK.Matrix[21, 21] = kPlate.Matrix[10, 10]; elmK.Matrix[21, 22] = kPlate.Matrix[10, 11];
                elmK.Matrix[22, 2] = kPlate.Matrix[11, 0]; elmK.Matrix[22, 3] = kPlate.Matrix[11, 1]; elmK.Matrix[22, 4] = kPlate.Matrix[11, 2]; elmK.Matrix[22, 8] = kPlate.Matrix[11, 3]; elmK.Matrix[22, 9] = kPlate.Matrix[11, 4]; elmK.Matrix[22, 10] = kPlate.Matrix[11, 5]; elmK.Matrix[22, 14] = kPlate.Matrix[11, 6]; elmK.Matrix[22, 15] = kPlate.Matrix[11, 7]; elmK.Matrix[22, 16] = kPlate.Matrix[11, 8]; elmK.Matrix[22, 20] = kPlate.Matrix[11, 9]; elmK.Matrix[22, 21] = kPlate.Matrix[11, 10]; elmK.Matrix[22, 22] = kPlate.Matrix[11, 11];
            }

            return elmK;
        }

        public MatrixCS GetGlobalStiffnessMatrix()
        {
            //var globalStiffnesMatrix = new MatrixCS()

            var rotMatrix = this.GetRotationMatrix();
            //rotMatrix.Print();
            var localStiffness = this.GetLocalStiffnessMatrix();
            //localStiffness.Print();
            var rotTrans = rotMatrix.Transpose();

            var globalStiffnesMatrix = (rotTrans.Multiply(localStiffness)).Multiply(rotMatrix);

            return globalStiffnesMatrix;
        }

        public void SetMass() 
        {
            // Map coordinates of flat plane to 2-D surface
            var d1 = this.IEndNode.Point.DistTo(this.JEndNode.Point);
            var d2 = this.JEndNode.Point.DistTo(this.KEndNode.Point);
            var d3 = this.KEndNode.Point.DistTo(this.LEndNode.Point);
            var d4 = this.LEndNode.Point.DistTo(this.IEndNode.Point);

            Vector p1V = new Vector(this.IEndNode.Point);
            Vector p2V = new Vector(this.JEndNode.Point);
            Vector p3V = new Vector(this.KEndNode.Point);
            Vector p4V = new Vector(this.LEndNode.Point);

            // Angle between first line and fourth line
            var firstVector0 = p2V.Extract(p1V);
            var secondVector0 = p4V.Extract(p1V);
            var alpha0 = firstVector0.AngleTo(secondVector0);

            // Angle between first line and second line
            var firstVector1 = p1V.Extract(p2V);
            var secondVector1 = p3V.Extract(p2V);
            var alpha1 = firstVector1.AngleTo(secondVector1);

            // Angle between second line and third line
            var firstVector2 = p2V.Extract(p3V);
            var secondVector2 = p4V.Extract(p3V);
            var alpha2 = firstVector2.AngleTo(secondVector2);

            // Map 3D coordinates to 2D plane using angles and length found above to be able to
            // use natural coordinates
            var x1 = 0.0; var y1 = 0.0;
            var x2 = d1; var y2 = 0.0;
            var x3 = x2 - (d2 * Math.Cos(alpha1)); var y3 = d2 * Math.Sin(alpha1);
            var x4 = d4 * Math.Cos(alpha0); var y4 = d4 * Math.Sin(alpha0);

            // Calculate area of the polygon
            var area = Math.Abs(((x1 * y2) - (x2 * y1)) + ((x2 * y3) - (x3 * y2)) + ((x3 * y4) - (x4 * y3)) + ((x4 * y1) - (x1 * y4))) * 0.5;

            // Get total mass
            var totalMass = area * this.Section.Thickness * this.Section.Material.Uw;
            _MemberMass = totalMass;
        }

        public MatrixCS GetLocalMassMatrix(bool useEI = false)
        {
            {
                // Get nodal mass contribution
                // TODO : Assumption is shell is rectangular. For non-rectangular shells (i.e. edges are not perpendicular), implement 
                // mass matrix with shape functions and Gauss integration.
                var lM = this.MemberMass / 4.0;

                // Fill mass matrix
                MatrixCS m = new MatrixCS(24, 24);
                m.Matrix[0, 0] = lM; m.Matrix[1, 1] = lM;
                m.Matrix[6, 6] = lM; m.Matrix[7, 7] = lM;
                m.Matrix[12, 12] = lM; m.Matrix[13, 13] = lM;
                m.Matrix[18, 18] = lM; m.Matrix[19, 19] = lM;

                return m;
            }
        }
        public MatrixCS GetGlobalMassMatrix()
        {
            var rot = this.GetRotationMatrix();
            var rotTrans = rot.Transpose();
            var localMassMatrix = this.GetLocalMassMatrix();

            return (rotTrans.Multiply(localMassMatrix)).Multiply(rot);


        }

        public MatrixCS GetRotationMatrix()
        {
            if (this.IsOnlyPlate)
            {

            }
            var rotationMatrix = this.IsOnlyPlate ? new MatrixCS(12, 12) :  new MatrixCS(24, 24);

            Vector xVector = new Vector(this.IEndNode.Point, this.JEndNode.Point);

            Vector secondPlaneVector = new Vector(this.JEndNode.Point, this.KEndNode.Point);

            var normalVector = xVector.CrossProduct(secondPlaneVector);

            Vector globalZ = new Vector(0.0, 0.0, 1.0);
            double skewAngle = globalZ.AngleTo(normalVector);

            var minorRot = GetTranslationalRotationMatrix(xVector, skewAngle);


            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i, j];

            for (int i = 3; i < 6; i++)
                for (int j = 3; j < 6; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i - 3, j - 3];

            for (int i = 6; i < 9; i++)
                for (int j = 6; j < 9; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i - 6, j - 6];

            for (int i = 9; i < 12; i++)
                for (int j = 9; j < 12; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i - 9, j - 9];

            if (!this.IsOnlyPlate)
            {

            for (int i = 12; i < 15; i++)
                for (int j = 12; j < 15; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i - 12, j - 12];

            for (int i = 15; i < 18; i++)
                for (int j = 15; j < 18; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i - 15, j - 15];

            for (int i = 18; i < 21; i++)
                for (int j = 18; j < 21; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i - 18, j - 18];

            for (int i = 21; i < 24; i++)
                for (int j = 21; j < 24; j++)
                    rotationMatrix.Matrix[i, j] = minorRot.Matrix[i - 21, j - 21];
            }




            return rotationMatrix;
        }

        private MatrixCS GetTranslationalRotationMatrix(Vector elmVector, double rotationAngle)
        {
            // For vertical members (i.e., both cX and cY are zero, different approach should be followed)
            var cX = elmVector.X / elmVector.Length;
            var cY = elmVector.Y / elmVector.Length;
            var cZ = elmVector.Z / elmVector.Length;
            var cXZ = Math.Sqrt((cX * cX) + (cZ * cZ));

            double compareVal = 0.0;
            double alpha = rotationAngle;
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

            return retVal;
        }

        public MatrixCS InvertMatrix(MatrixCS matrix)
        {

            double A2323 = matrix.Matrix[2, 2] * matrix.Matrix[3, 3] - matrix.Matrix[2, 3] * matrix.Matrix[3, 2];
            double A1323 = matrix.Matrix[2, 1] * matrix.Matrix[3, 3] - matrix.Matrix[2, 3] * matrix.Matrix[3, 1];
            double A1223 = matrix.Matrix[2, 1] * matrix.Matrix[3, 2] - matrix.Matrix[2, 2] * matrix.Matrix[3, 1];
            double A0323 = matrix.Matrix[2, 0] * matrix.Matrix[3, 3] - matrix.Matrix[2, 3] * matrix.Matrix[3, 0];
            double A0223 = matrix.Matrix[2, 0] * matrix.Matrix[3, 2] - matrix.Matrix[2, 2] * matrix.Matrix[3, 0];
            double A0123 = matrix.Matrix[2, 0] * matrix.Matrix[3, 1] - matrix.Matrix[2, 1] * matrix.Matrix[3, 0];
            double A2313 = matrix.Matrix[1, 2] * matrix.Matrix[3, 3] - matrix.Matrix[1, 3] * matrix.Matrix[3, 2];
            double A1313 = matrix.Matrix[1, 1] * matrix.Matrix[3, 3] - matrix.Matrix[1, 3] * matrix.Matrix[3, 1];
            double A1213 = matrix.Matrix[1, 1] * matrix.Matrix[3, 2] - matrix.Matrix[1, 2] * matrix.Matrix[3, 1];
            double A2312 = matrix.Matrix[1, 2] * matrix.Matrix[2, 3] - matrix.Matrix[1, 3] * matrix.Matrix[2, 2];
            double A1312 = matrix.Matrix[1, 1] * matrix.Matrix[2, 3] - matrix.Matrix[1, 3] * matrix.Matrix[2, 1];
            double A1212 = matrix.Matrix[1, 1] * matrix.Matrix[2, 2] - matrix.Matrix[1, 2] * matrix.Matrix[2, 1];
            double A0313 = matrix.Matrix[1, 0] * matrix.Matrix[3, 3] - matrix.Matrix[1, 3] * matrix.Matrix[3, 0];
            double A0213 = matrix.Matrix[1, 0] * matrix.Matrix[3, 2] - matrix.Matrix[1, 2] * matrix.Matrix[3, 0];
            double A0312 = matrix.Matrix[1, 0] * matrix.Matrix[2, 3] - matrix.Matrix[1, 3] * matrix.Matrix[2, 0];
            double A0212 = matrix.Matrix[1, 0] * matrix.Matrix[2, 2] - matrix.Matrix[1, 2] * matrix.Matrix[2, 0];
            double A0113 = matrix.Matrix[1, 0] * matrix.Matrix[3, 1] - matrix.Matrix[1, 1] * matrix.Matrix[3, 0];
            double A0112 = matrix.Matrix[1, 0] * matrix.Matrix[2, 1] - matrix.Matrix[1, 1] * matrix.Matrix[2, 0];

            var det = matrix.Matrix[0, 0] * (matrix.Matrix[1, 1] * A2323 - matrix.Matrix[1, 2] * A1323 + matrix.Matrix[1, 3] * A1223)
                - matrix.Matrix[0, 1] * (matrix.Matrix[1, 0] * A2323 - matrix.Matrix[1, 2] * A0323 + matrix.Matrix[1, 3] * A0223)
                + matrix.Matrix[0, 2] * (matrix.Matrix[1, 0] * A1323 - matrix.Matrix[1, 1] * A0323 + matrix.Matrix[1, 3] * A0123)
                - matrix.Matrix[0, 3] * (matrix.Matrix[1, 0] * A1223 - matrix.Matrix[1, 1] * A0223 + matrix.Matrix[1, 2] * A0123);
            det = 1 / det;

            MatrixCS im = new MatrixCS(4, 4);

            im.Matrix[0, 0] = det * (matrix.Matrix[1, 1] * A2323 - matrix.Matrix[1, 2] * A1323 + matrix.Matrix[1, 3] * A1223);
            im.Matrix[0, 1] = det * -(matrix.Matrix[0, 1] * A2323 - matrix.Matrix[0, 2] * A1323 + matrix.Matrix[0, 3] * A1223);
            im.Matrix[0, 2] = det * (matrix.Matrix[0, 1] * A2313 - matrix.Matrix[0, 2] * A1313 + matrix.Matrix[0, 3] * A1213);
            im.Matrix[0, 3] = det * -(matrix.Matrix[0, 1] * A2312 - matrix.Matrix[0, 2] * A1312 + matrix.Matrix[0, 3] * A1212);
            im.Matrix[1, 0] = det * -(matrix.Matrix[1, 0] * A2323 - matrix.Matrix[1, 2] * A0323 + matrix.Matrix[1, 3] * A0223);
            im.Matrix[1, 1] = det * (matrix.Matrix[0, 0] * A2323 - matrix.Matrix[0, 2] * A0323 + matrix.Matrix[0, 3] * A0223);
            im.Matrix[1, 2] = det * -(matrix.Matrix[0, 0] * A2313 - matrix.Matrix[0, 2] * A0313 + matrix.Matrix[0, 3] * A0213);
            im.Matrix[1, 3] = det * (matrix.Matrix[0, 0] * A2312 - matrix.Matrix[0, 2] * A0312 + matrix.Matrix[0, 3] * A0212);
            im.Matrix[2, 0] = det * (matrix.Matrix[1, 0] * A1323 - matrix.Matrix[1, 1] * A0323 + matrix.Matrix[1, 3] * A0123);
            im.Matrix[2, 1] = det * -(matrix.Matrix[0, 0] * A1323 - matrix.Matrix[0, 1] * A0323 + matrix.Matrix[0, 3] * A0123);
            im.Matrix[2, 2] = det * (matrix.Matrix[0, 0] * A1313 - matrix.Matrix[0, 1] * A0313 + matrix.Matrix[0, 3] * A0113);
            im.Matrix[2, 3] = det * -(matrix.Matrix[0, 0] * A1312 - matrix.Matrix[0, 1] * A0312 + matrix.Matrix[0, 3] * A0112);
            im.Matrix[3, 0] = det * -(matrix.Matrix[1, 0] * A1223 - matrix.Matrix[1, 1] * A0223 + matrix.Matrix[1, 2] * A0123);
            im.Matrix[3, 1] = det * (matrix.Matrix[0, 0] * A1223 - matrix.Matrix[0, 1] * A0223 + matrix.Matrix[0, 2] * A0123);
            im.Matrix[3, 2] = det * -(matrix.Matrix[0, 0] * A1213 - matrix.Matrix[0, 1] * A0213 + matrix.Matrix[0, 2] * A0113);
            im.Matrix[3, 3] = det * (matrix.Matrix[0, 0] * A1212 - matrix.Matrix[0, 1] * A0212 + matrix.Matrix[0, 2] * A0112);

            return im;

        }

       
        #endregion
    }
}
