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
   public class LatticeModelData
    {
        #region Ctor
        public LatticeModelData()
        {

        }
        #endregion


        #region Private Fields

        private List<Node> _ListOfNodes;
        private List<Support> _ListOfSupports;
        
        private List<ILoad> _ListOfLoads = new List<ILoad>();
        private List<FrameMember> _ListOfMembers = new List<FrameMember>();

        private double _Width;
        private double _Height;
        private double _MeshSize;

        private double _LatticeMeshRatio;


        #endregion

        #region Public Properties

        public List<Node> ListOfNodes { get => _ListOfNodes; set => _ListOfNodes = value; }
        public List<Support> ListOfSupports { get => _ListOfSupports; set => _ListOfSupports = value; }
        public List<FrameMember> ListOfMembers { get => _ListOfMembers; set => _ListOfMembers = value; } 
        public double Width { get => _Width; set => _Width = value; }
        public double Height { get => _Height; set => _Height = value; }
        public double MeshSize { get => _MeshSize; set => _MeshSize = value; }
        public double LatticeMeshRatio { get => _LatticeMeshRatio; set => _LatticeMeshRatio = value; }
        public List<ILoad> ListOfLoads { get => _ListOfLoads; set => _ListOfLoads = value; }

        #endregion



        #region Public Methods
        public void AssignLoadToMiddle()
        {
            if (ListOfNodes != null )
            {
            for (int i = 0; i < ListOfNodes.Count; i++)
            {
                    var node = ListOfNodes[i];
                    if (node.Point.X == Width/2 &&
                        node.Point.Y == Height/ 2)
                    {
                        _ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -1, Node = node,DofID = 2 });
                    }
            }
            }
        }
        public void SetTorsionalReleaseToAllMembers()
        {
            if (this.ListOfMembers != null)
            {
                
                for (int i = 0; i < this.ListOfMembers.Count; i++)
                {
                    var member = this.ListOfMembers[i];

                    member.IEndNode.SupportCondition.Restraints[5] = eRestrainedCondition.NotRestrained;
                    member.JEndNode.SupportCondition.Restraints[5] = eRestrainedCondition.NotRestrained;
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

        }

        public void FillMemberInfoList()
        {
            var labelCounter = 1;
            for (int i = 0; i < _ListOfNodes.Count; i++)
            {
                for (int j = i+1; j < _ListOfNodes.Count; j++)
                {
                    var lengthOfMember = Math.Sqrt(Math.Pow(_ListOfNodes[j].Point.X - _ListOfNodes[i].Point.X, 2) + Math.Pow(_ListOfNodes[j].Point.Y - _ListOfNodes[i].Point.Y, 2) + Math.Pow(_ListOfNodes[j].Point.Z - _ListOfNodes[i].Point.Z, 2));

                    if (lengthOfMember < 1.42 * _MeshSize)
                    {
                        var frameMember = new FrameMember() { IEndNode = _ListOfNodes[i], JEndNode = _ListOfNodes[j], ID = labelCounter };
                        //frameMember.SetAsTrussMember();
                        frameMember.Section = new FrameSection();
                        _ListOfMembers.Add(frameMember);
                        labelCounter ++;
                    }
                }
            }
        }

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
            this.ListOfNodes =listOfNodes;
        }

        #endregion


        #region Private Methods
        
        private List<Node> GetBorderNodes()
        {
            var ret = new List<Node>();
            if (this.ListOfNodes !=null)
            {
                ret = this.ListOfNodes.Where(x => x.Point.X == 0 ||
                                            x.Point.X == _Width ||
                                             x.Point.Y == 0 ||
                                             x.Point.Y == _Height).ToList();

            }
            return ret;
        }
        
        #endregion
    }

}
