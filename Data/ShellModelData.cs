using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject.Loads;
using ThesisProject.Sections;
using ThesisProject.Structural_Members;

namespace Data
{
    public class ShellModelData
    {
        public ShellModelData()
        {

        }




        #region Private Fields

        private List<Node> _ListOfNodes;
        private List<Support> _ListOfSupports;

        private List<ILoad> _ListOfLoads = new List<ILoad>();
        private List<QuadShellMember> _ListOfMembers = new List<QuadShellMember>();

        private double _Width;
        private double _Height;
        private double _MeshSize;

        #endregion

        #region Public Properties

        public List<Node> ListOfNodes { get => _ListOfNodes; set => _ListOfNodes = value; }
        public List<Support> ListOfSupports { get => _ListOfSupports; set => _ListOfSupports = value; }
        public List<ILoad> ListOfLoads { get => _ListOfLoads; set => _ListOfLoads = value; }
        public List<QuadShellMember> ListOfMembers { get => _ListOfMembers; set => _ListOfMembers = value; }
        public double Width { get => _Width; set => _Width = value; }
        public double Height { get => _Height; set => _Height = value; }
        public double MeshSize { get => _MeshSize; set => _MeshSize = value; }

        #endregion

        #region Public Methods
        public void FillNodeInfo()
        {
            var listOfNodes = new List<Node>();
            var nx = (this.Width / this.MeshSize + 1);
            var ny = (this.Height / this.MeshSize + 1);

            var nodeIDCounter = 1;
            for (int i = 0; i < ny; i++)
            {
                for (int j = 0; j < nx; j++)
                {
                    var node = new Node();
                    node.Point = new ModelInfo.Point();
                    node.Point.Z = 0; // Level of system, not necessary at the moment. 

                    node.Point.Y = i * this.MeshSize;
                    node.Point.X = j * this.MeshSize;

                    node.SupportCondition = new Support(eSupportType.Free);
                    node.ID = nodeIDCounter;
                    listOfNodes.Add(node);
                    nodeIDCounter++;
                }

            }
            this.ListOfNodes = listOfNodes;
        }
        private List<Node> GetBorderNodes()
        {
            var ret = new List<Node>();
            if (this.ListOfNodes != null)
            {
                ret = this.ListOfNodes.Where(x => x.Point.X == 0 ||
                                            x.Point.X == _Width ||
                                             x.Point.Y == 0 ||
                                             x.Point.Y == _Height).ToList();

            }
            return ret;
        }
        public void AssignLoadToMiddle()
        {
            if (ListOfNodes != null)
            {
                for (int i = 0; i < ListOfNodes.Count; i++)
                {
                    var node = ListOfNodes[i];
                    if (node.Point.X == Width / 2 &&
                        node.Point.Y == Height / 2)
                    {
                        //TODO: Tolerance ekle
                        _ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -1, Node = node, DofID = 3 });
                    }
                }
            }
        }
        public void SetBorderNodesSupportCondition(eSupportType supportType)
        {
            var borderNodes = GetBorderNodes();
            _ListOfSupports = new List<Support>();

            for (int i = 0; i < borderNodes.Count; i++)
            {
                var node = borderNodes[i];
                node.SupportCondition = new Support(supportType);
            }

            foreach (var n 
                in this.ListOfNodes)
            {
                
                if (!borderNodes.Contains(n))
                {
                     n.SupportCondition = new Support(eSupportType.Fixed);


                    n.SupportCondition.Restraints[0] = eRestrainedCondition.Restrained;
                    n.SupportCondition.Restraints[1] = eRestrainedCondition.Restrained;
                    n.SupportCondition.Restraints[5] = eRestrainedCondition.Restrained;

                    n.SupportCondition.Restraints[2] = eRestrainedCondition.NotRestrained;
                    n.SupportCondition.Restraints[3] = eRestrainedCondition.NotRestrained;
                    n.SupportCondition.Restraints[4] = eRestrainedCondition.NotRestrained;
                }
            }


        }
        public void FillMemberInfoList()
        {
            int nElmX = (int)(this.Width / this.MeshSize);
            int nElmY = (int)(this.Height / this.MeshSize);

            int nx = (int)(nElmX + 1);
            for (int i = 0; i < nElmY; i++)
            {
                for (int j = 0; j < nElmX; j++)
                {
                    int idx = (i * nElmX) + j + 1;
                    int iNodeIdx = (i * nx) + j;
                    int jNodeIdx = iNodeIdx + 1;
                    int kNodeIdx = jNodeIdx + nx;
                    int lNodeIdx = kNodeIdx - 1;
                    var member = new QuadShellMember() { IEndNode = ListOfNodes[iNodeIdx], JEndNode = ListOfNodes[jNodeIdx], KEndNode = ListOfNodes[kNodeIdx], LEndNode = ListOfNodes[lNodeIdx], ID = idx,
                    Section = new ShellSection(), Thickness = 0.1 };
               
                    idx++;

                    ListOfMembers.Add(member);
                }
            }
        }
    }
    #endregion

}
