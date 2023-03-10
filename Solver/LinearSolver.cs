﻿using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using ThesisProject;
using ThesisProject.Structural_Members;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Accord.Math.Decompositions;
using MathNet.Numerics.LinearAlgebra.Double;
using Accord.Math;
using ThesisProject.AssemblyInfo;

namespace Solver
{

    public class LinearSolver
    {


        #region Ctor

        public LinearSolver()
        {
            

        }

       
        #endregion

        #region Private Fields

        #endregion

        #region Public Methods


        public List<double> GetPeriodsOfTheSystem(MatrixCS kG, MatrixCS mass, ref double lastEigen)
        {

            var solver = new GeneralizedEigenvalueDecomposition(kG.Matrix, mass.Matrix,true);



            var w2 = solver.RealEigenvalues;

            lastEigen = w2.FirstOrDefault();

            var w = new List<double>();

            for (int i = 0; i < w2.Length; i++)
            {
                w.Add(Math.Sqrt(w2[i]));
            }

            var T = new List<double>();


            for (int i = 0; i < w.Count; i++)
            {
                T.Add((2 * Math.PI )/ w[i]);
            }



            return T;

        }
       

        public double[][] GetModeShapesOfSystem(MatrixCS kG, MatrixCS mass, int numberOfModes)
        {
            
            var evd = new GeneralizedEigenvalueDecomposition(kG.Matrix,mass.Matrix,true);

            // Extract the eigenvalues and eigenvectors
            double[] eigenvalues = evd.RealEigenvalues;
            double[,] eigenvectors = evd.Eigenvectors;


            // Get the index of the first n eigenvalues of interest
            double[] index = new double[2];
            for (int i = 0; i < numberOfModes; i++)
            {
                index[i] = eigenvalues[i];
            }




            var modeShapes = new double[numberOfModes][];

            for (int i = 0; i < numberOfModes; i++)
            {
                var modeShape = new double[eigenvectors.GetLength(0)];
                for (int j = 0; j < modeShape.Length; j++)
                {
                    modeShape[j] = eigenvectors[j, i] / eigenvectors.GetColumn(i).Euclidean();
                }
                modeShapes[i] = modeShape;
            }

            return modeShapes;


        }

        public void EigenvalueAnalysis(double[,] _matrix)
        {
            var matrix = Matrix<double>.Build.DenseOfArray(_matrix);
            var evd = matrix.Evd();
            var eigenvalues = evd.EigenValues;
            var eigenvectors = evd.EigenVectors;
        }

        public double[][] GetModeShapes(Evd<double> evd, double[,] _matrix)
        {
            var eigenvectors = evd.EigenVectors;
            var numModes = eigenvectors.RowCount;
            var modeShapes = new double[numModes][];

            for (int i = 0; i < numModes; i++)
            {
                var modeShape = new double[_matrix.GetLength(0)];
                for (int j = 0; j < modeShape.Length; j++)
                {
                    modeShape[j] = eigenvectors[j, i] / eigenvectors.Column(i).Norm(2);
                }
                modeShapes[i] = modeShape;
            }

            return modeShapes;
        }


        public void RunEigenValueAnalysis(FiniteElementModel modelData) 
        {

            modelData.SetNodeAssemblyData();
            
            var KG = modelData.GetGlobalStiffness();
            var mass = modelData.GetMassMatrix();


            var externalKG = DenseMatrix.OfArray(KG.Matrix);
            var externalMass = DenseMatrix.OfArray(mass.Matrix);


            
            //var evd = externalKG.Evd(Symmetricity.Symmetric,externalMass);


        }

        public LatticeModelResultData RunAnalysis_Lattice(LatticeModelData latticemodeldata)
        {
            var latticeModelResultData = new LatticeModelResultData();
            latticemodeldata.SetNodeAssemblyData();


            var KG = latticemodeldata. GetGlobalStiffness();
            var RHS = latticemodeldata.GetRightHandSide();
            var mass = latticemodeldata.GetMassMatrix();
            
            var modeShapesLattice = GetModeShapesOfSystem(KG, mass, 2);


            //var res = GetPeriodsOfTheSystem(KG, mass);

            //for (int i = 0; i < 6; i++)
            //{
            //    latticeModelResultData.ListOfPeriods.Add(res[i]);
            //}
            //Console.WriteLine("Lattice Model Periods");

            //Console.WriteLine("--------------------------");
            //for (int i = 0; i < res.Count; i++)
            //{
            //    Console.WriteLine("Mode No: " + (i + 1).ToString() + " Period:" + res[i].ToString());
            //}
            //Console.WriteLine("--------------------------");



            var externalKG = Matrix<double>.Build.SparseOfArray(KG.Matrix);
            var externalRHS = Matrix<double>.Build.SparseOfArray(RHS.Matrix);
            

            var dispRes = externalKG.Solve(externalRHS);




            var dispResAsArray = dispRes.ToArray();
            var dispResMatrix = new MatrixCS(dispResAsArray.Length, 1);

            dispResMatrix.Matrix = dispResAsArray;

            latticeModelResultData.DispRes = dispResMatrix;
            latticeModelResultData.NodeResults = new Dictionary<ModelInfo.Point, List<double>>();
            latticeModelResultData.FrameResults = new Dictionary<int, FrameMemberResults>();

            GetNodeResults(dispResAsArray,latticeModelResultData.NodeResults, latticemodeldata);
            FillFrameMemberResults(latticeModelResultData,latticemodeldata);

           

            return latticeModelResultData;
        }


    

        public RunData EqualizeSystems(ShellModelResultData shellModelRes,LatticeModelResultData latticeModelRes,LatticeModelData latticeModel,ShellModelData shellModalData,double alphaRatio)
        {
            //Calculate internal energies to have the ratio to equalize systems
            var shellInternalEnergy = GetShellModelInternalEnergy(shellModelRes.DispRes,shellModalData);
            var latticeInternalEnergy = GetLatticeModelInternalEnergy(latticeModelRes.DispRes, latticeModel);
            
            
            //Assign ratio to equalize internal eneries
            latticeModel.AssignRatio(shellInternalEnergy / latticeInternalEnergy, alphaRatio);


   
            //shellModelRes.DispRes.Print();
            //newLatticeRes.DispRes.Print();


            
            
            //Initialize result data
            var resultData = new RunData();
            //TODO: Fill eigen value part of result data
            
            // Get node compare data
            resultData.NodeCompareData = GetNodeCompareDataList( shellModelRes, RunAnalysis_Lattice(latticeModel), shellModalData);

            return resultData;
        }


        private List<NodeCompareData> GetNodeCompareDataList(ShellModelResultData shellModelRes, LatticeModelResultData newLatticeRes,ShellModelData shellModelData) 
        {
            Console.WriteLine("Lattice/Shell Node Vertical Deflections");
            var nodeCompareList = new List<NodeCompareData>();

            for (int i = 0; i < shellModelRes.NodeResults.Count; i++)
            {
                var nodeCompareData = new NodeCompareData();
                var nodePt = shellModelRes.NodeResults.Keys.ElementAt(i);
                var nodeResShell = shellModelRes.NodeResults[nodePt];

                var key = newLatticeRes.NodeResults.Keys.FirstOrDefault(pt => pt.X == nodePt.X &&
                                                        pt.Y == nodePt.Y);
                var nodeResLattice = newLatticeRes.NodeResults[key];
                var node = shellModelData.ListOfNodes.FirstOrDefault(x => x.Point == nodePt);
                var nodeID = node.ID;
                nodeCompareData.NodeID = node.ID;
                var nodePoint = nodePt;
                var verticalDef = nodeResLattice[2];
                var verticalDefShell = nodeResShell[2];

                var percentDiff = (verticalDef - verticalDefShell) / verticalDefShell * 100;

                if (double.IsNaN(percentDiff))
                {
                    percentDiff = 0;
                }

                nodeCompareData.PercentDiff = percentDiff;
                nodeCompareData.LatticeVerticalDisp = verticalDef;
                nodeCompareData.ShellVerticalDisp = verticalDefShell;
                nodeCompareData.Point = nodePoint;

                Console.WriteLine(nodeID.ToString() + "; " + nodePoint.X.ToString() + ";" + nodePoint.Y.ToString() + "  ;  " + verticalDef.ToString() + " ; " + verticalDefShell.ToString() + " ; " + percentDiff.ToString());

                nodeCompareList.Add(nodeCompareData);
            }

            return nodeCompareList;
        }

        public ShellModelResultData RunAnalysis_Shell(ShellModelData shellModelData)
        {
            var shellModelResultData = new ShellModelResultData();
            

            shellModelData.SetNodeAssemblyData();

            var KG = shellModelData.GetGlobalStiffness();
            var RHS = shellModelData.GetRightHandSide();

            var externalKG = Matrix<double>.Build.SparseOfArray(KG.Matrix);
            var externalRHS = Matrix<double>.Build.SparseOfArray(RHS.Matrix);

            var dispRes = externalKG.Solve(externalRHS);

            var dispResAsArray = dispRes.ToArray();
            var dispResMatrix = new MatrixCS(dispResAsArray.Length, 1);

            dispResMatrix.Matrix = dispResAsArray;
            var mass = shellModelData.GetMassMatrix();
            //var res = GetPeriodsOfTheSystem(KG, mass);


            //for (int i = 0; i < 6; i++)
            //{
            //    shellModelResultData.ListOfPeriods.Add(res[i]);
            //}

            //Console.WriteLine("Shell Model Periods");

            //Console.WriteLine("--------------------------");
            //for (int i = 0; i < res.Count; i++)
            //{
            //    Console.WriteLine("Mode No: " + (i + 1).ToString() + " Period:" + res[i].ToString());
            //}
            //Console.WriteLine("--------------------------");

            var internalEnergy = GetShellModelInternalEnergy(dispResMatrix,shellModelData);


            shellModelResultData.DispRes = dispResMatrix;
            shellModelResultData.NodeResults = new Dictionary<ModelInfo.Point, List<double>>();

            GetNodeResults(dispResAsArray,shellModelResultData.NodeResults, shellModelData);



            return shellModelResultData;
        }

        public double GetShellModelInternalEnergy(MatrixCS dispMatrix,ShellModelData shellModelData)
        {
            double ret;
            var KG = shellModelData.GetGlobalStiffness();

            var firstPart = (dispMatrix.Transpose()).Multiply(KG);
            ret = firstPart.Multiply(dispMatrix).Matrix[0,0];


            return ret;
        }

        public double GetLatticeModelInternalEnergy(MatrixCS dispMatrix,LatticeModelData latticeModelData,double coeff = 1)
        {
            double ret;
            
            latticeModelData.SetNodeAssemblyData();


            var KG = latticeModelData.GetGlobalStiffness();
            KG = KG.Multiply(coeff);
            var firstPart = dispMatrix.Transpose().Multiply(KG);
            ret = firstPart.Multiply(dispMatrix).Matrix[0, 0];


            return Math.Abs(ret);
        }



        #endregion


        #region Private Methods

        private void GetNodeResults(double [,] nodeDisps, Dictionary<ModelInfo.Point,List<double>> nodeResultsDict, IFiniteElementModel latticeModelData)
        {
            // Get node results
            int counter = 0;

            for (int i = 0; i < latticeModelData.ListOfNodes.Count; i++)
            {
                var node = latticeModelData.ListOfNodes[i];
                var nodeEqnData = latticeModelData.AssemblyData.NodeEquationData[node.ID];
                var nodeResults = new List<double>();

                for (int j = 0; j < nodeEqnData.Count; j++)
                {
                    double dofResult = 0;

                    if (nodeEqnData[j] != -1) // unknown
                    {

                        dofResult = nodeDisps[counter, 0];
                        counter++;

                    }

                    nodeResults.Add(dofResult);
                }
                nodeResultsDict.Add(node.Point, nodeResults);

            }


        }

        private void FillFrameMemberResults(LatticeModelResultData resultData,LatticeModelData latticeModelData)
        {
            var nodeResults = resultData.NodeResults;

            var listOfFrames = latticeModelData.ListOfMembers;

            for (int i = 0; i < listOfFrames.Count; i++)
            {
                var frm = listOfFrames[i] as FrameMember;
                var frameResults = new FrameMemberResults();
                frameResults.INodeDisplacements_Global = nodeResults[frm.IEndNode.Point];
                frameResults.JNodeDisplacements_Global = nodeResults[frm.JEndNode.Point];




                var globalResultsMatrix = new MatrixCS(12, 1);
                for (int k= 0; k < 6; k++)
                {
                    globalResultsMatrix.Matrix[k, 0] = frameResults.INodeDisplacements_Global[k];
                    globalResultsMatrix.Matrix[k+6, 0] = frameResults.JNodeDisplacements_Global[k];
                }

                var R = frm.GetRotationMatrix();
                var uLocal = R.Multiply(globalResultsMatrix);
                var kLocal = frm.GetLocalStiffnessMatrix();
                var fLocal = kLocal.Multiply(uLocal);
                var RPrime = R.Transpose();
                var fGlobal = RPrime.Multiply(fLocal);


                frameResults.INodeDisplacements_Local = new List<double>();
                frameResults.INodeForces_Global = new List<double>();
                frameResults.INodeForces_Local = new List<double>();
                frameResults.JNodeDisplacements_Local = new List<double>();
                frameResults.JNodeForces_Global = new List<double>();
                frameResults.JNodeForces_Local = new List<double>();


                for (int k = 0; k < 12; k++)
                {
                    if (k>=6)
                    {
                        frameResults.JNodeDisplacements_Local.Add(uLocal.Matrix[k, 0]);
                        frameResults.JNodeForces_Local.Add(fLocal.Matrix[k, 0]);

                        frameResults.JNodeForces_Global.Add(fGlobal.Matrix[k, 0]);
                    }
                    else
                    {
                        frameResults.INodeDisplacements_Local.Add(uLocal.Matrix[k, 0]);
                        frameResults.INodeForces_Local.Add(fLocal.Matrix[k, 0]);
                         
                        frameResults.INodeForces_Global.Add(fGlobal.Matrix[k, 0]);

                    }

                }
                resultData.FrameResults.Add(frm.ID, frameResults);
            }
        }
        #endregion
    }
}
