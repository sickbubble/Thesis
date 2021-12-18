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


            double GetK(int i, int j) => KG.Matrix[i, j];
            var externalKG = Matrix<double>.Build.Sparse(size, size, GetK);
            double GetRHS(int i, int j) => RHS.Matrix[i, j];
            var externalRHS = Matrix<double>.Build.Sparse(size, 1, GetRHS);
            var dispRes = externalKG.Solve(externalRHS);
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
                            rightHandSide.Matrix[eqnNumber,0] = pLoad.Magnitude;
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

        private void FillNodeResults(Matrix<double> nodeDisps)
        {
            _LatticeModelResultData = new LatticeModelResultData();
            var nodeEquationData = _AssemblyData.NodeEquationData;

            var listOfNodes = _LatticeModelData.ListOfNodes;
            for (int i = 0; i < listOfNodes.Count; i++)
            {
                var node = listOfNodes[i];
                var nodeData = nodeEquationData[node.ID];

                var nodeResults = new List<double>();

                for (int j   = 0; j < nodeData.Count; j++)
                {
                    if (nodeData[j] == -1)
                    {
                        nodeResults.Add(0);
                    }
                    else
                    {
                        nodeResults.Add(nodeDisps[nodeData[j], 0]);
                        //TODO: Check with debug.  matrix index i ile equation number arasında fark olabilir. 
                    }

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
                var frameGlobalResults = new FrameMemberResults();
                frameGlobalResults.INodeDisplacements = nodeResults[frm.IEndNode.ID];
                frameGlobalResults.JNodeDisplacements = nodeResults[frm.JEndNode.ID];




                var globalResultsMatrix = new MatrixCS(12, 1);
                for (int k= 0; k < 6; k++)
                {
                    globalResultsMatrix.Matrix[k, 1] = frameGlobalResults.INodeDisplacements[k];
                    globalResultsMatrix.Matrix[k+6, 1] = frameGlobalResults.JNodeDisplacements[k];
                }

                var R = frm.GetRotationMatrix();
                var uLocal = R.Multiply(globalResultsMatrix);
                var kLocal = frm.GetLocalStiffnessMatrix();
                var fLocal = kLocal.Multiply(uLocal);
                var RPrime = R.Transpose();
                var fGlobal = RPrime.Multiply(fLocal);


                var frameLocalResults = new FrameMemberResults();
                frameLocalResults.INodeDisplacements = new List<double>();
                frameLocalResults.JNodeDisplacements = new List<double>();


                for (int k = 0; k < 12; k++)
                {
                    if (k>=6)
                    {
                        frameLocalResults.JNodeDisplacements.Add(uLocal.Matrix[k, 1]);
                        frameLocalResults.JNodeForces.Add(fLocal.Matrix[k, 1]);

                        frameGlobalResults.JNodeDisplacements.Add(fGlobal.Matrix[k, 1]);
                    }
                    else
                    {
                        frameLocalResults.INodeDisplacements.Add(uLocal.Matrix[k, 1]);
                        frameLocalResults.INodeForces.Add(fLocal.Matrix[k, 1]);

                        frameGlobalResults.INodeDisplacements.Add(fGlobal.Matrix[k, 1]);

                    }

                }

                _LatticeModelResultData.GlobalFrameResults.Add(frm.ID, frameGlobalResults);
                _LatticeModelResultData.LocalFrameResults.Add(frm.ID, frameLocalResults);
            }


            // Get Local Displacements




        }
        #endregion
    }
}
