using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;
using ThesisProject.Structural_Members;
using MathNet.Numerics.LinearAlgebra;
using ThesisProject.Loads;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Accord.Math.Decompositions;

namespace Solver
{
    public class AssemblyDataContainer
    {

        public AssemblyDataContainer(int numberOfUnknows, Dictionary<int, List<int>> nodeEquationData)
        {
            _NumberOfUnknowns = numberOfUnknows;
            _NodeEquationData = nodeEquationData;
        }

        private int _NumberOfUnknowns;
        private Dictionary<int, List<int>> _NodeEquationData;

        public int NumberOfUnknowns { get => _NumberOfUnknowns; set => _NumberOfUnknowns = value; }
        public Dictionary<int, List<int>> NodeEquationData { get => _NodeEquationData; set => _NodeEquationData = value; }
    }
    public class LinearSolver
    {


        #region Ctor

        public LinearSolver()
        {
            

        }

       
        #endregion

        #region Private Fields

        private LatticeModelData _LatticeModelData;
        private AssemblyDataContainer _AssemblyData;
        private ShellModelData _ShellModelData;


        #endregion

        #region Public Methods

        public List<double> GetPeriodsOfTheSystem(MatrixCS kG, MatrixCS mass)
        {
            var periods = new List<double>();

            var externalKG = Matrix<double>.Build.DenseOfArray(kG.Matrix);
            var externalMass = Matrix<double>.Build.DenseOfArray(mass.Matrix);

            var res = externalMass.Solve(externalKG);

            var solver = new GeneralizedEigenvalueDecomposition(kG.Matrix,mass.Matrix,true);
            var w2 = solver.RealEigenvalues;


            var w = new List<double>();

            for (int i = 0; i < w2.Length; i++)
            {
                w.Add(Math.Sqrt(w2[i]));
            }

            var T = new List<double>();

            for (int i = 0; i < w.Count; i++)
            {
                T.Add(2 * Math.PI / w[i]);
            }



            //var eigen1 = Matrix<double>.Build.Diagonal(kG.NRows, kG.NRows);

            //    eigen
            //Evd<double> eigen = eigen1.Evd();

            //generalizedeigensolver

            //eigen.Solve(externalKG, externalMass);

            ////Eigen::GeneralizedEigenSolver<Eigen::MatrixXd> ges;
            ////ges.compute(K, M);
            ////auto eigs = ges.eigenvalues().real();

            ////for (int i = eigs.size() - 1; -1 < i; i--)
            ////    t.push_back(2 * pi / sqrt(eigs(i)));



            //var w2 = new List<double>();
            
            //for (int i = 0; i < res.RowCount -1 ; i++)
            //{
            //     w2.Add( Math.Sqrt(Convert.ToDouble(res[i, 0])));
            //}
            //w2.Sort();

            //for (int i = 0; i < w.Count; i++)
            //{
            //    periods.Add(2 * Math.PI / w2[i]);
            //}



            return T;

        }

        public LatticeModelResultData RunAnalysis_Lattice(LatticeModelData latticemodeldata)
        {
            _LatticeModelData = latticemodeldata;
            _AssemblyData = GetNodeAssemblyData(latticemodeldata.ListOfNodes);

            var KG = GetGlobalStiffness_Latttice();
            var RHS = GetRightHandSide();


            var mass = GetMassMatrix_Latttice();
            var res = GetPeriodsOfTheSystem(KG, mass);




            var externalKG = Matrix<double>.Build.SparseOfArray(KG.Matrix);
            var externalRHS = Matrix<double>.Build.SparseOfArray(RHS.Matrix);

            var dispRes = externalKG.Solve(externalRHS);


            var dispResAsArray = dispRes.ToArray();
            var dispResMatrix = new MatrixCS(dispResAsArray.Length, 1);

            dispResMatrix.Matrix = dispResAsArray;

            var latticeModelResultData = new LatticeModelResultData();
            latticeModelResultData.DispRes = dispResMatrix;
            latticeModelResultData.NodeResults = new Dictionary<int, List<double>>();
            latticeModelResultData.FrameResults = new Dictionary<int, FrameMemberResults>();

            GetNodeResults(dispResAsArray,latticeModelResultData.NodeResults);
            FillFrameMemberResults(latticeModelResultData);

            return latticeModelResultData;
        }
        public LatticeModelData EqualizeSystems(ShellModelResultData shellModelRes,LatticeModelResultData latticeModelRes,LatticeModelData latticeModel)
        {
            var shellInternalEnergy = GetShellModelInternalEnergy(shellModelRes.DispRes);
            var latticeInternalEnergy = GetLatticeModelInternalEnergy(latticeModelRes.DispRes, latticeModel);

            var oldArea = latticeModel.ListOfMembers.FirstOrDefault().Section.Area;

            var newArea = oldArea * latticeInternalEnergy / shellInternalEnergy;


            for (int i = 0; i < latticeModel.ListOfMembers.Count; i++)
            {
                latticeModel.ListOfMembers[i].Section.Area = newArea;
            }


            var newLatticeRes = RunAnalysis_Lattice(latticeModel);



            var latticeInternalEnergyNew = GetLatticeModelInternalEnergy(newLatticeRes.DispRes, latticeModel);


            return latticeModel;
        }

        public ShellModelResultData RunAnalysis_Shell(ShellModelData shellModelData)
        {
            _ShellModelData = shellModelData;
            _AssemblyData = GetNodeAssemblyData(shellModelData.ListOfNodes);
            
            var KG = GetGlobalStiffness_Shell();
            var RHS = GetRightHandSide();

            var externalKG = Matrix<double>.Build.SparseOfArray(KG.Matrix);
            var externalRHS = Matrix<double>.Build.SparseOfArray(RHS.Matrix);

            var dispRes = externalKG.Solve(externalRHS);

            var dispResAsArray = dispRes.ToArray();
            var dispResMatrix = new MatrixCS(dispResAsArray.Length, 1);

            dispResMatrix.Matrix = dispResAsArray;


            var internalEnergy = GetShellModelInternalEnergy(dispResMatrix);


            var shellModelResultData = new ShellModelResultData();
            shellModelResultData.DispRes = dispResMatrix;
            shellModelResultData.NodeResults = new Dictionary<int, List<double>>();

            GetNodeResults(dispResAsArray,shellModelResultData.NodeResults);

            return shellModelResultData;
        }

        public double GetShellModelInternalEnergy(MatrixCS dispMatrix)
        {
            double ret;
            var KG = GetGlobalStiffness_Shell();

            var firstPart = dispMatrix.Transpose().Multiply(KG);
            ret = firstPart.Multiply(dispMatrix).Matrix[0,0];


            return ret;
        }

        public double GetLatticeModelInternalEnergy(MatrixCS dispMatrix,LatticeModelData latticeModelData)
        {
            double ret;
            _LatticeModelData = latticeModelData;
            _AssemblyData = GetNodeAssemblyData(latticeModelData.ListOfNodes);
            var KG = GetGlobalStiffness_Latttice();

            var firstPart = dispMatrix.Transpose().Multiply(KG);
            ret = firstPart.Multiply(dispMatrix).Matrix[0, 0];


            return ret;
        }



        #endregion


        #region Private Methods

        private MatrixCS GetRightHandSide()
        {
            var numOfUnknowns = _AssemblyData.NumberOfUnknowns;
            MatrixCS rightHandSide = new MatrixCS(numOfUnknowns, 1);

            var listOfLoads  = _LatticeModelData!= null ? _LatticeModelData.ListOfLoads : _ShellModelData.ListOfLoads;


            for (int i = 0; i < listOfLoads.Count; i++)
            {
                var load = listOfLoads[i];

                switch (load.LoadType)
                {
                    case ThesisProject.Loads.eLoadType.Point:
                        var pLoad = (PointLoad)load;
                        var eqnNumber = _AssemblyData.NodeEquationData[pLoad.Node.ID][pLoad.DofID];
                        if (eqnNumber != -1 )
                        {
                            // eqnNumber is one based. 
                            rightHandSide.Matrix[eqnNumber  ,0] = pLoad.Magnitude;
                        } 

                        break;
                    case ThesisProject.Loads.eLoadType.Distributed:
                        //load = (PointLoad)load;
                        break;
                    case ThesisProject.Loads.eLoadType.Area:
                        //load = (PointLoad)load;
                        break;
                    default:
                        break;
                }

            }



            return rightHandSide;
        }

        private MatrixCS GetGlobalStiffness_Latttice()
         {

            var numOfUnknowns = _AssemblyData.NumberOfUnknowns;
            var nodeEquationData = _AssemblyData.NodeEquationData;

            MatrixCS KG = new MatrixCS(numOfUnknowns, numOfUnknowns);

            var listOfFrameMembers = _LatticeModelData.ListOfMembers;

            for (int i = 0; i < listOfFrameMembers.Count; i++)
            {
                var frmMember = listOfFrameMembers[i];
                var K = frmMember.GetGlobalStiffnessMatrix();
                var iNode = frmMember.IEndNode;
                var jNode = frmMember.JEndNode;

                var g = new List<int>();
                g.AddRange(nodeEquationData[iNode.ID]);
                g.AddRange(nodeEquationData[jNode.ID]);

                var matrixLength = K.NRows;//should be 12 for frame

                for (int k = 0; k < matrixLength; k++)
                {
                    for (int l = 0; l < matrixLength; l++)
                    {
                        var gk = g[k];
                        var gl = g[l];
                        if (g[k] != -1 && g[l] != -1) // -1 means free a.k.a unknown
                        {
                            KG.Matrix[g[k], g[l]] = KG.Matrix[g[k], g[l]] + K.Matrix[k, l];
                        }
                    }
                }
            }

            return KG;

        }

        public MatrixCS GetMassMatrix_Latttice()
        {

            var numOfUnknowns = _AssemblyData.NumberOfUnknowns;
            var nodeEquationData = _AssemblyData.NodeEquationData;

            MatrixCS globalMass = new MatrixCS(numOfUnknowns, numOfUnknowns);

            var listOfFrameMembers = _LatticeModelData.ListOfMembers;

            for (int i = 0; i < listOfFrameMembers.Count; i++)
            {
                var frmMember = listOfFrameMembers[i];
                var m = frmMember.GetGlobalMassMatrix();
                var iNode = frmMember.IEndNode;
                var jNode = frmMember.JEndNode;

                var g = new List<int>();
                g.AddRange(nodeEquationData[iNode.ID]);
                g.AddRange(nodeEquationData[jNode.ID]);

                var matrixLength = m.NRows;//should be 12 for frame

                for (int k = 0; k < matrixLength; k++)
                {
                    for (int l = 0; l < matrixLength; l++)
                    {
                        var gk = g[k];
                        var gl = g[l];
                        if (g[k] != -1 && g[l] != -1) // -1 means free a.k.a unknown
                        {
                            globalMass.Matrix[g[k], g[l]] = globalMass.Matrix[g[k], g[l]] + m.Matrix[k, l];
                        }
                    }
                }
            }

            return globalMass;
        }

        public MatrixCS GetMassMatrix_Shell()
        {

            var numOfUnknowns = _AssemblyData.NumberOfUnknowns;
            var nodeEquationData = _AssemblyData.NodeEquationData;

            MatrixCS massMatrix = new MatrixCS(numOfUnknowns, numOfUnknowns);

            var listOfShellMembers = _ShellModelData.ListOfMembers;

            for (int i = 0; i < listOfShellMembers.Count; i++)
            {
                var shellMember = listOfShellMembers[i];
                var m = shellMember.GetGlobalMassMatrix();
                var iNode = shellMember.IEndNode;
                var jNode = shellMember.JEndNode;
                var kNode = shellMember.KEndNode;
                var lNode = shellMember.LEndNode;

                var g = new List<int>();

                g.AddRange(nodeEquationData[iNode.ID]);
                g.AddRange(nodeEquationData[jNode.ID]);
                g.AddRange(nodeEquationData[kNode.ID]);
                g.AddRange(nodeEquationData[lNode.ID]);

                var matrixLength = m.NRows;//should be 12 for frame

                for (int k = 0; k < matrixLength; k++)
                {
                    for (int l = 0; l < matrixLength; l++)
                    {
                        if (g[k] != -1 && g[l] != -1) // -1 means free a.k.a unknown
                        {
                            massMatrix.Matrix[g[k], g[l]] = massMatrix.Matrix[g[k], g[l]] + m.Matrix[k, l];
                        }
                    }
                }
            }

            return massMatrix;
        }


        private MatrixCS GetGlobalStiffness_Shell()
        {

            var numOfUnknowns = _AssemblyData.NumberOfUnknowns;
            var nodeEquationData = _AssemblyData.NodeEquationData;

            MatrixCS KG = new MatrixCS(numOfUnknowns, numOfUnknowns);

            var listOfShellMembers = _ShellModelData.ListOfMembers;

            for (int i = 0; i < listOfShellMembers.Count; i++)
            {
                var shellMember = listOfShellMembers[i];
                var K = shellMember.GetGlobalStiffnessMatrix();
                var iNode = shellMember.IEndNode;
                var jNode = shellMember.JEndNode;
                var kNode = shellMember.KEndNode;
                var lNode = shellMember.LEndNode;

                var g = new List<int>();

                g.AddRange(nodeEquationData[iNode.ID]);
                g.AddRange(nodeEquationData[jNode.ID]);
                g.AddRange(nodeEquationData[kNode.ID]);
                g.AddRange(nodeEquationData[lNode.ID]);

                var matrixLength = K.NRows;//should be 12 for frame

                for (int k = 0; k < matrixLength; k++)
                {
                    for (int l = 0; l < matrixLength; l++)
                    {
                        if (g[k] != -1 && g[l] != -1) // -1 means free a.k.a unknown
                        {
                            KG.Matrix[g[k], g[l]] = KG.Matrix[g[k], g[l]] + K.Matrix[k, l];
                        }
                    }
                }
            }

            return KG;

        }

        private AssemblyDataContainer GetNodeAssemblyData(List<Node> listOfNodes )
        {

            int numberOfUnknowns = 0;

            var nodeEquationData = new Dictionary<int, List<int>>();


            for (int i = 0; i < listOfNodes.Count; i++)
            {
                var node = listOfNodes[i];
                var supportCondition = node.SupportCondition;
                var listOfEquationNumbers = new List<int>();
                for (int j = 0; j < supportCondition.Restraints.Count; j++)
                {
                    var restraint = supportCondition.Restraints.Values.ElementAt(j);
                    if (restraint == eRestrainedCondition.NotRestrained)
                    {
                        listOfEquationNumbers.Add(numberOfUnknowns);
                        numberOfUnknowns++;
                    }
                    else
                    {
                        listOfEquationNumbers.Add(-1);
                    }
                }

                nodeEquationData.Add(node.ID, listOfEquationNumbers);

            }

            var assemblyData = new AssemblyDataContainer(numberOfUnknowns, nodeEquationData);

            return assemblyData;
        }

        private void GetNodeResults(double [,] nodeDisps, Dictionary<int,List<double>> nodeResultsDict)
        {
            // Get node results
            var listOfNodes = _LatticeModelData != null ? _LatticeModelData.ListOfNodes : _ShellModelData.ListOfNodes;
            int counter = 0;

            for (int i = 0; i < listOfNodes.Count; i++)
            {
                var node = listOfNodes[i];
                var nodeEqnData = _AssemblyData.NodeEquationData[node.ID];
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
                nodeResultsDict.Add(node.ID, nodeResults);

            }


        }

        private void FillFrameMemberResults(LatticeModelResultData resultData)
        {
            var nodeResults = resultData.NodeResults;

            var listOfFrames = _LatticeModelData.ListOfMembers;

            for (int i = 0; i < listOfFrames.Count; i++)
            {
                var frm = listOfFrames[i];
                var frameResults = new FrameMemberResults();
                frameResults.INodeDisplacements_Global = nodeResults[frm.IEndNode.ID];
                frameResults.JNodeDisplacements_Global = nodeResults[frm.JEndNode.ID];




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
