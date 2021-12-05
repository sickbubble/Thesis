﻿using System;
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
            else if (MembraneType == eMembraneType.Drilling)
            {
                var kDrilling = new MatrixCS(12, 12);

                double V = 0.0;
                var gpCoeff = 1 / Math.Sqrt(3);


                double[,] gauss2x2PointsWeights = new double[4, 3] { { -gpCoeff, -gpCoeff, 1 }, { gpCoeff, -gpCoeff, 1 }, { gpCoeff, gpCoeff, 1 }, { -gpCoeff, gpCoeff, 1 } };
                int[,] jk=new int [4,2]  { { 0, 1} , { 1, 2} , { 2, 3} , { 3, 0} };
                int[,] ms=new int [4,2]  { { 7, 4} , { 4, 5} , { 5, 6} , { 6, 7} };
                int[] ml=new int [4] { 3, 0, 1, 2 };
                double[,] nL = new double[4,2];

                for (int i = 0; i < 4; i++)
                {
                    var edgeStartNodeCoordX = mappedCoords.Matrix[jk[i,0], 0];
                    var edgeStartNodeCoordY = mappedCoords.Matrix[jk[i,0], 1];
                    var edgeEndNodeCoordX = mappedCoords.Matrix[jk[i,1], 0];
                    var edgeEndNodeCoordY = mappedCoords.Matrix[jk[i,1], 1];

                    nL[i,0] = edgeEndNodeCoordY - edgeStartNodeCoordY;
                    nL[i,1] = edgeStartNodeCoordX - edgeEndNodeCoordX;
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
                    MatrixCS j1 =  new MatrixCS(2, 4);
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
                    double[] N8 =   new double [8] {
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

                    B.Matrix[0, m] = ((dN8X[ms[i,0]] * nL[ml[i],0]) - (dN8X[ms[i,1]] * nL[i,0])) / 8.0;
                    B.Matrix[1, m] = ((dN8Y[ms[i,0]] * nL[ml[i],1]) - (dN8Y[ms[i,1]] * nL[i,1])) / 8.0;
                    B.Matrix[2, m] = ((dN8Y[ms[i,0]] * nL[ml[i],0]) - (dN8Y[ms[i,1]] * nL[i,0])) / 8.0;
                    B.Matrix[2, m] += ((dN8X[ms[i,0]] * nL[ml[i],1]) - (dN8X[ms[i,1]] * nL[i,1])) / 8.0;
                }

                var vi = thickness * detJacobi * weight;
                var littleK = B.Transpose().Multiply(eMat).Multiply(B).Multiply(vi);
                kDrilling = littleK.Sum(kDrilling);
                V += vi;

            }


                // Create stabilizator for zero-energy modes
                var  kD =  new MatrixCS(12, 12);
                var em = this.Material.E;
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