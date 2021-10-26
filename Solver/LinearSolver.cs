using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;
using ThesisProject.Structural_Members;
using MathNet.Numerics.LinearAlgebra;

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
            var _AssemblyData = GetNodeAssemblyData();

        }
        #endregion

        #region Private Fields

        private LatticeModelData _LatticeModelData;
        private AssemblyDataContainer _AssemblyData;


        #endregion

        #region Public Methods

        public void RunAnalysis()
        {
            var KG = GetGlobalStiffness();
            var RHS = GetRightHandSide();


            var size = KG.Matrix.Length;


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
                var ıNode = frmMember.IEndNode;
                var jNode = frmMember.JEndNode;

                var g = new List<int>();
                g.AddRange(nodeEquationData[ıNode.ID]);
                g.AddRange(nodeEquationData[jNode.ID]);

                var matrixLength = K.Matrix.Length;//should be 12 for frame

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
                        numberOfUnknowns++;
                        listOfEquationNumbers.Add(numberOfUnknowns);
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


        #endregion
    }
}
