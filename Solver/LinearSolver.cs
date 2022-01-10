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

        public LinearSolver(LatticeModelData latticemodeldata)
        {
            _LatticeModelData = latticemodeldata;
            
             _AssemblyData = GetNodeAssemblyData();

        }
        #endregion

        #region Private Fields

        private LatticeModelData _LatticeModelData;
        private LatticeModelResultData _LatticeModelResultData ;
        private AssemblyDataContainer _AssemblyData;

        public LatticeModelResultData LatticeModelResultData { get => _LatticeModelResultData; set => _LatticeModelResultData = value; }


        #endregion

        #region Public Methods

        public void RunAnalysis()
        {
            var KG = GetGlobalStiffness();
            var RHS = GetRightHandSide();


            var size = KG.NRows;


            var externalKG = Matrix<double>.Build.SparseOfArray(KG.Matrix);
            var externalRHS = Matrix<double>.Build.SparseOfArray(RHS.Matrix);


            //for (int i = 0; i < KG.NRows; i++)
            //{
            //    for (int j = 0; j < KG.NColumns; j++)
            //    {
            //        externalKG[i, j] = KG.Matrix[i, j];
            //    }

            //}

            //var externalRHS = Matrix<double>.Build.Random(RHS.NRows, RHS.NColumns);

            //for (int i = 0; i < RHS.NRows; i++)
            //{
            //    for (int j = 0; j < RHS.NColumns; j++)
            //    {
            //        externalRHS[i, j] = RHS.Matrix[i, j];
            //    }

            //}

            //double GetK(int i, int j) => KG.Matrix[i, j];

            //var externalKG = Matrix<double>.Build.Sparse(size, size, GetK);
            //double GetRHS(int i, int j) => RHS.Matrix[i, j];
            var dispRes = externalKG.Solve(externalRHS);

            var dispResAsArray = dispRes.ToArray();
            _LatticeModelResultData = new LatticeModelResultData();
            _LatticeModelResultData.NodeResults = new Dictionary<int, List<double>>();
            _LatticeModelResultData.FrameResults = new Dictionary<int, FrameMemberResults>();

            FillNodeResults(dispResAsArray);
            FillFrameMemberResults();



            //// Get node results
            //var listOfNodes = _LatticeModelData.ListOfNodes;
            //int counter = 0;

            //for (int i = 0; i < listOfNodes.Count; i++)
            //{
            //    var node = listOfNodes[i];
            //    var nodeEqnData = _AssemblyData.NodeEquationData[node.ID];
            //    var nodeResults = new List<double>();

            //    for (int j = 0; j < nodeEqnData.Count; j++)
            //    {
            //        double dofResult = 0;

            //        if (nodeEqnData[j] != -1) // unknown
            //        {

            //            dofResult = dispResAsArray[counter,0];
            //            counter ++;

            //        }

            //        nodeResults.Add(dofResult);
            //    }
            //    _LatticeModelResultData.NodeResults.Add(node.ID, nodeResults);

            //}

            ////Get frame results.
            //var listOFFrames = _LatticeModelData.ListOfMembers;
            //counter = 0;
            //for (int i = 0; i < listOFFrames.Count; i++)
            //{
            //    var frame = listOFFrames[i];
            //    var frameRes = new FrameMemberResults();
            //    frameRes.INodeDisplacements_Global = new List<double>();
            //    var InodeEqnData = _AssemblyData.NodeEquationData[frame.IEndNode.ID];
            //    for (int j = 0; j < InodeEqnData.Count; j++)
            //    {
            //        double dofResult = 0;

            //        if (InodeEqnData[j] != -1) // unknown
            //        {
            //            dofResult = dispRes[counter, 0];
            //            counter++;
            //        }

            //        frameRes.INodeDisplacements_Global.Add(dofResult);

            //    }

            //    var JnodeEqnData = _AssemblyData.NodeEquationData[frame.IEndNode.ID];
            //    frameRes.JNodeDisplacements_Global = new List<double>();

            //    for (int k   = 0; k < JnodeEqnData.Count; k++)
            //    {
            //        double dofResult = 0;

            //        if (JnodeEqnData[k] != -1) // unknown
            //        {
            //            dofResult = dispRes[counter, 0];
            //            counter++;
            //        }

            //        frameRes.JNodeDisplacements_Global.Add(dofResult);
            //    }

            //    //TODO add local results also. 
            //    _LatticeModelResultData.FrameResults.Add(frame.ID, frameRes);
            //}

        }

        #endregion


        #region Private Methods

        private MatrixCS GetRightHandSide()
        {
            var numOfUnknowns = _AssemblyData.NumberOfUnknowns;
            MatrixCS rightHandSide = new MatrixCS(numOfUnknowns, 1);
            var listOfLoads = _LatticeModelData.ListOfLoads;

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
                            rightHandSide.Matrix[eqnNumber -1 ,0] = pLoad.Magnitude;
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

        private MatrixCS GetGlobalStiffness()
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

        private AssemblyDataContainer GetNodeAssemblyData()
        {

            int numberOfUnknowns = 0;
            var listOfNodes = _LatticeModelData.ListOfNodes;

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

        private void FillNodeResults(double [,] nodeDisps)
        {
            // Get node results
            var listOfNodes = _LatticeModelData.ListOfNodes;
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
                _LatticeModelResultData.NodeResults.Add(node.ID, nodeResults);

            }


        }

        private void FillFrameMemberResults()
        {
            var nodeResults = _LatticeModelResultData.NodeResults;

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


                _LatticeModelResultData.FrameResults.Add(frm.ID, frameResults);
            }
        }
        #endregion
    }
}
