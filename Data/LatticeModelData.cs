using ModelInfo;
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

        public double GetTotalMass()
        {
            var members = this.ListOfMembers;
            double totalMass = 0;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];

                totalMass += member.GetMass();

            }
            return totalMass;
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
                        _ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -1, Node = node, DofID = 2 });
                    }
                }
            }
        }
        public void AssignLoadToMiddle2()
        {
            if (ListOfNodes != null )
            {
            for (int i = 0; i < ListOfNodes.Count; i++)
            {
                    var node = ListOfNodes[i];
                    if (node.Point.X == Width/2 &&
                        node.Point.Z == Height/ 2)
                    {
                        _ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -1, Node = node,DofID = 1 });
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
                    member.IEndCondition.IsReleaseMx = true;
                    member.JEndCondition.IsReleaseMx = true;

                }

            }
            
        }
        public void SetBorderNodesSupportCondition(eSupportType supportType,Point LShapeInstPt = null)
        {
            var borderNodes = GetBorderNodes();
            _ListOfSupports = new List<Support>();

            for (int i = 0; i < borderNodes.Count; i++)
            {
                var node = borderNodes[i];
                node.SupportCondition = new Support(supportType);
            }


            if (LShapeInstPt != null)
            {
                var limitX = LShapeInstPt.X;
                var limitY = LShapeInstPt.Y;
                var extraLNodes = _ListOfNodes.Where(n => (n.Point.X >= limitX && n.Point.Y == limitY) ||
                                                         (n.Point.Y >= limitY && n.Point.X == limitX)).ToList();

                if (extraLNodes != null)
                {
                    for (int i = 0; i < extraLNodes.Count; i++)
                    {
                        var node = extraLNodes[i];
                        node.SupportCondition = new Support(supportType);
                    }
                }

            }



        }
        public void SetBorderNodesSupportCondition2(eSupportType supportType, Point LShapeInstPt = null)
        {
            var borderNodes = GetBorderNodes2();
            //var borderNodes = GetBorderNodes();
            _ListOfSupports = new List<Support>();

            for (int i = 0; i < borderNodes.Count; i++)
            {
                var node = borderNodes[i];
                node.SupportCondition = new Support(supportType);
            }


            if (LShapeInstPt != null)
            {
                var limitX = LShapeInstPt.X;
                var limitY = LShapeInstPt.Y;
                var extraLNodes = _ListOfNodes.Where(n => (n.Point.X >= limitX && n.Point.Y == limitY) ||
                                                         (n.Point.Y >= limitY && n.Point.X == limitX)).ToList();

                if (extraLNodes != null)
                {
                    for (int i = 0; i < extraLNodes.Count; i++)
                    {
                        var node = extraLNodes[i];
                        node.SupportCondition = new Support(supportType);
                    }
                }

            }



        }

        public void SetModelGeometryType(eModelGeometryType geometryType,Point pt = null,double gapSize = 0)
        {

            switch (geometryType)
            {
                case eModelGeometryType.Rectangular:
                    break;
                case eModelGeometryType.LShape:
                    SetModelShapeAsL(pt);
                    break;
                case eModelGeometryType.WithOpening:
                    SetModelShapeAsGapped(pt,gapSize);
                    break;
                default:
                    break;
            }

        }

        public void FillMemberInfo(double horizon, double sectionHeight)
        {
            var labelCounter = 1;
            for (int i = 0; i < _ListOfNodes.Count; i++)
            {
                for (int j = i+1; j < _ListOfNodes.Count; j++)
                {
                    var lengthOfMember = Math.Sqrt(Math.Pow(_ListOfNodes[j].Point.X - _ListOfNodes[i].Point.X, 2) + Math.Pow(_ListOfNodes[j].Point.Y - _ListOfNodes[i].Point.Y, 2) + Math.Pow(_ListOfNodes[j].Point.Z - _ListOfNodes[i].Point.Z, 2));

                    if (lengthOfMember < horizon * _MeshSize)
                    {
                        var frameMember = new FrameMember() { IEndNode = _ListOfNodes[i], JEndNode = _ListOfNodes[j], ID = labelCounter };
                        //frameMember.SetAsTrussMember();

                        if (_ListOfMembers.Any(x => x.IEndNode == frameMember.JEndNode && x.JEndNode == frameMember.IEndNode))
                            continue;

                        frameMember.Section = new FrameSection(1, sectionHeight);
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
            this.ListOfNodes = listOfNodes;
        }
        public void FillNodeInfo2()
        {
            var listOfNodes = new List<Node>();
            var nx = (this.Width / this.MeshSize + 1);
            var nZ = (this.Height / this.MeshSize + 1);

            var nodeIDCounter = 1;
            for (int i = 0; i < nZ; i++)
            {
                for (int j = 0; j < nx; j++)
                {
                    var node = new Node();
                    node.Point = new ModelInfo.Point();
                    node.Point.Y = 0; // Level of system, not necessary at the moment. 

                    node.Point.Z = i * this.MeshSize;
                    node.Point.X = j * this.MeshSize;

                    node.SupportCondition = new Support(eSupportType.Free);
                    node.ID = nodeIDCounter;
                    listOfNodes.Add(node);
                    nodeIDCounter++;
                }

            }
            this.ListOfNodes = listOfNodes;
        }
        #endregion


        #region Private Methods

        private void SetModelShapeAsL(Point pointToCut)
        {
            //  ________  . . . . . . 
            // |        | . . . . . .
            // |        | . . . . . .
            // |        |_____________
            // |      pt              |
            // |______________________|

            var limitX = pointToCut.X;
            var limitY = pointToCut.Y;
            
            // Remove Nodes
                var nodesToDelete = _ListOfNodes.Where(x => x.Point.X > limitX &&
                                                            x.Point.Y > limitY).ToList();
            if (nodesToDelete == null  )
            {
                return;
            }

            for (int i = 0; i < nodesToDelete.Count; i++)
            {
                _ListOfNodes.Remove(nodesToDelete[i]);
            }

            //Remove Members

            var membersToDelete = _ListOfMembers.Where(m => (m.IEndNode.Point.X > limitX &&
                                                            m.IEndNode.Point.Y > limitY) ||
                                                            (m.JEndNode.Point.X > limitX &&
                                                            m.JEndNode.Point.Y > limitY)).ToList();


            for (int i = 0; i < membersToDelete.Count; i++)
            {
                _ListOfMembers.Remove(membersToDelete[i]);
            }




        }

        private void SetModelShapeAsGapped(Point instPoint,double gapSize)
        {
            //  ______________________ 
            // |                      |
            // |         ______       |
            // |        |      |      |
            // |        |______|      |
            // |      pt              |
            // |______________________|

            var limitBotX = instPoint.X;
            var limitBotY = instPoint.Y;
            var limitTopX = instPoint.X + gapSize;
            var limitTopY = instPoint.Y + gapSize;

            // Remove Nodes
            var nodesToDelete = _ListOfNodes.Where(x => x.Point.X > limitBotX && x.Point.X < limitTopX &&
                                                        x.Point.Y > limitBotY && x.Point.Y < limitTopY).ToList();
            if (nodesToDelete == null)
            {
                return;
            }

            for (int i = 0; i < nodesToDelete.Count; i++)
            {
                _ListOfNodes.Remove(nodesToDelete[i]);
            }

            //Remove Members

            var membersToDelete = _ListOfMembers.Where(m => (m.IEndNode.Point.X > limitBotX &&
                                                            m.IEndNode.Point.X < limitTopX &&
                                                            m.IEndNode.Point.Y > limitBotY &&
                                                            m.IEndNode.Point.Y < limitTopY) ||
                                                            (m.JEndNode.Point.X > limitBotX &&
                                                            m.JEndNode.Point.X < limitTopX &&
                                                            m.JEndNode.Point.Y > limitBotY &&
                                                            m.JEndNode.Point.Y < limitTopY)).ToList();


            for (int i = 0; i < membersToDelete.Count; i++)
            {
                _ListOfMembers.Remove(membersToDelete[i]);
            }

        }



        


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

        private List<Node> GetBorderNodes2()
        {
            var ret = new List<Node>();
            if (this.ListOfNodes != null)
            {
                ret = this.ListOfNodes.Where(x => x.Point.X == 0 ||
                                            x.Point.X == _Width ||
                                             x.Point.Z == 0 ||
                                             x.Point.Z == _Height).ToList();

            }
            return ret;
        }

        #endregion
    }

}
