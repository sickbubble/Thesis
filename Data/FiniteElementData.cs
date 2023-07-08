using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;
using ThesisProject.AssemblyInfo;
using ThesisProject.Loads;
using ThesisProject.Structural_Members;

namespace Data
{
    public interface IHaveAssemblyData
    {
        void SetNodeAssemblyData();
    }


    public interface IFiniteElementModel
    {
        List<Node> ListOfNodes { get; set; }
        List<Support> ListOfSupports { get; set; }
        List<ILoad> ListOfLoads { get; set; }
        List<IStructuralMember> ListOfMembers { get; set; }
        AssemblyDataContainer AssemblyData { get; set; }
        MatrixCS GetGlobalStiffness();
        MatrixCS GetRightHandSide();
        MatrixCS GetMassMatrix();
    }

    public abstract class FiniteElementModel : IFiniteElementModel, IHaveAssemblyData
    {
        public List<Support> ListOfSupports { get => _ListOfSupports; set => _ListOfSupports = value; }
        public List<IStructuralMember> ListOfMembers { get => _ListOfMembers; set => _ListOfMembers = value; }
        public List<ILoad> ListOfLoads { get => _ListOfLoads; set => _ListOfLoads = value; }
        public AssemblyDataContainer AssemblyData { get => _AssemblyData; set => _AssemblyData = value; }
        public List<Node> ListOfNodes { get => _ListOfNodes; set => _ListOfNodes = value; }


        #region Private Fields

        private List<Node> _ListOfNodes;
        private List<Support> _ListOfSupports;

        private List<ILoad> _ListOfLoads = new List<ILoad>();
        private List<IStructuralMember> _ListOfMembers = new List<IStructuralMember>();
        private AssemblyDataContainer _AssemblyData;


        #endregion

        public abstract bool SetModelData(RunData modelRunData);

        public abstract MatrixCS GetGlobalStiffness();

        public MatrixCS GetRightHandSide()
        {
            MatrixCS rightHandSide = new MatrixCS(AssemblyData.NumberOfUnknowns, 1);
            foreach (var load in this.ListOfLoads)
            {
                switch (load.LoadType)
                {
                    case ThesisProject.Loads.eLoadType.Point:
                        var pLoad = (PointLoad)load;
                        var eqnNumber = AssemblyData.NodeEquationData[pLoad.Node.ID][pLoad.DofID];
                        if (eqnNumber != -1)
                        {
                            // eqnNumber is one based. 
                            rightHandSide.Matrix[eqnNumber, 0] = pLoad.Magnitude;
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


        public abstract MatrixCS GetMassMatrix();

        public void SetNodeAssemblyData()
        {
            int numberOfUnknowns = 0;

            var nodeEquationData = new Dictionary<int, List<int>>();


            for (int i = 0; i < this.ListOfNodes.Count; i++)
            {
                var node = this.ListOfNodes[i];
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
            this.AssemblyData = new AssemblyDataContainer(numberOfUnknowns, nodeEquationData);
        }
    }

}
