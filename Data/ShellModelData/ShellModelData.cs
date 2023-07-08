using ModelInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;
using ThesisProject.Loads;
using ThesisProject.Sections;
using ThesisProject.Structural_Members;



namespace Data
{
    public enum eModelGeometryType
    {
        Rectangular=0,
        LShape = 1,
        WithOpening =2
    }

   
    public class ShellModelData : FiniteElementModel
    {
      
        #region Singleton Implementation

        private ShellModelData() { }
        private static ShellModelData instance = null;
        public static ShellModelData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShellModelData();
                }
                return instance;
            }
        }
        public static void KillInstance()
        {
            instance = null;
        }

        public static bool IsInstanceValid()
        {
            return (instance != null);
        }


        #endregion


        #region Private Fields

        private double _Width;
        private double _Height;
        private double _MeshSize;
        private bool _IsOnlyPlate;
        private double _ShellThickness;

        private MatrixCS _GlobalStiffness;
        private MatrixCS _MassMatrix;
        private double _TotalMass;
        double _Area;


        #endregion

        #region Public Properties
        public double Width { get => _Width; set => _Width = value; }
        public double Height { get => _Height; set => _Height = value; }
        public double MeshSize { get => _MeshSize; set => _MeshSize = value; }
        public bool IsOnlyPlate { get => _IsOnlyPlate; set => _IsOnlyPlate = value; }
        public double ShellThickness { get => _ShellThickness; set => _ShellThickness = value; }
        public double Area { get => _Width * _Height; }


        #endregion

        #region Public Methods
        public void SetModelGeometryType(eModelGeometryType geometryType, Point pt = null, double gapSize = 0)
        {

            switch (geometryType)
            {
                case eModelGeometryType.Rectangular:
                    break;
                case eModelGeometryType.LShape:
                    SetModelShapeAsL(pt);
                    break;
                case eModelGeometryType.WithOpening:
                    SetModelShapeAsGapped(pt, gapSize);
                    break;
                default:
                    break;
            }

        }
        public void SetModelAsPlate()
        {
            foreach (var node in ListOfNodes) node.SupportCondition.SetAsPlate();
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
            var nodesToDelete = this.ListOfNodes.Where(x => x.Point.X > limitX &&
                                                        x.Point.Y > limitY).ToList();
            if (nodesToDelete == null)
            {
                return;
            }

            for (int i = 0; i < nodesToDelete.Count; i++)
            {
                ListOfNodes.Remove(nodesToDelete[i]);
            }

            //Remove Members

            //var membersToDelete = ListOfMembers.Where(m => (m.IEndNode.Point.X > limitX &&
            //                                                m.IEndNode.Point.Y > limitY) ||
            //                                                (m.JEndNode.Point.X > limitX &&
            //                                                m.JEndNode.Point.Y > limitY)||
            //                                                (m.KEndNode.Point.X > limitX &&
            //                                                m.KEndNode.Point.Y > limitY)||
            //                                                (m.LEndNode.Point.X > limitX &&
            //                                                m.LEndNode.Point.Y > limitY)).ToList();


            //for (int i = 0; i < membersToDelete.Count; i++)
            //{
            //    _ListOfMembers.Remove(membersToDelete[i]);
            //}




        }

        private void SetModelShapeAsGapped(Point instPoint, double gapSize)
        {
            //  ______________________ 
            // |                      |
            // |         ______       |
            // |        |      |      |
            // |        |______|      |
            // |      pt              |
            // |______________________|

            //var limitBotX = instPoint.X;
            //var limitBotY = instPoint.Y;
            //var limitTopX = instPoint.X + gapSize;
            //var limitTopY = instPoint.Y + gapSize;

            //// Remove Nodes
            //var nodesToDelete = _ListOfNodes.Where(x => x.Point.X > limitBotX && x.Point.X < limitTopX &&
            //                                            x.Point.Y > limitBotY && x.Point.Y < limitTopY).ToList();
            //if (nodesToDelete == null)
            //{
            //    return;
            //}

            //for (int i = 0; i < nodesToDelete.Count; i++)
            //{
            //    _ListOfNodes.Remove(nodesToDelete[i]);
            //}

            ////Remove Members

            //var membersToDelete = _ListOfMembers.Where(m => (m.IEndNode.Point.X > limitBotX &&
            //                                                m.IEndNode.Point.X < limitTopX &&
            //                                                m.IEndNode.Point.Y > limitBotY &&
            //                                                m.IEndNode.Point.Y < limitTopY) ||
            //                                                (m.JEndNode.Point.X > limitBotX &&
            //                                                m.JEndNode.Point.X < limitTopX &&
            //                                                m.JEndNode.Point.Y > limitBotY &&
            //                                                m.JEndNode.Point.Y < limitTopY)||
            //                                                (m.KEndNode.Point.X > limitBotX &&
            //                                                m.KEndNode.Point.X < limitTopX &&
            //                                                m.KEndNode.Point.Y > limitBotY &&
            //                                                m.KEndNode.Point.Y < limitTopY)||
            //                                                (m.LEndNode.Point.X > limitBotX &&
            //                                                m.LEndNode.Point.X < limitTopX &&
            //                                                m.LEndNode.Point.Y > limitBotY &&
            //                                                m.LEndNode.Point.Y < limitTopY)).ToList();


            //for (int i = 0; i < membersToDelete.Count; i++)
            //{
            //    _ListOfMembers.Remove(membersToDelete[i]);
            //}

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
        public void SetLoad(RunInfo runInfo) 
        {
            switch (runInfo.LoadingType)
            {
                case eLoadingType.PointLoad:
                    AssignLoadToMiddle(runInfo.LoadMagnitude);
                    break;
                case eLoadingType.FullAreaLoad:
                    AssignDistributedLoads(runInfo.LoadMagnitude);
                    break;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="magnitude">in per/m2</param>
        private void AssignDistributedLoads(double magnitude)
        {
            var totalLoad = magnitude * this.Area;
            var eachLoad = totalLoad / this.ListOfNodes.Count;

            if (this.ListOfNodes != null)
            {
                foreach (var node in this.ListOfNodes) ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -eachLoad, Node = node, DofID = 2 });

            }
        }
        private void AssignLoadToMiddle(double magnitude)
        {

            if(this.ListOfNodes != null)
            {
                var midNode = this.ListOfNodes.FirstOrDefault(x => x.Point.X == Width / 2 && x.Point.Y == Height / 2);
                if (midNode !=null)
                {
                    ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -magnitude, Node = midNode, DofID = 2 });
                }
            }
        }
        public void SetBorderNodesSupportCondition(eSupportType supportType,Point LShapeInstPt = null)
        {
            var borderNodes = GetBorderNodes();
            ListOfSupports = new List<Support>();

            for (int i = 0; i < borderNodes.Count; i++)
            {
                var node = borderNodes[i];
                node.SupportCondition = new Support(supportType);
            }


            if (LShapeInstPt != null)
            {
                var limitX = LShapeInstPt.X;
                var limitY = LShapeInstPt.Y;
                var extraLNodes = ListOfNodes.Where(n => (n.Point.X >= limitX && n.Point.Y == limitY) ||
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
            //foreach (var n 
            //    in this.ListOfNodes)
            //{

            //    if (!borderNodes.Contains(n))
            //    {
            //         n.SupportCondition = new Support(eSupportType.Fixed);


            //        n.SupportCondition.Restraints[0] = eRestrainedCondition.Restrained;
            //        n.SupportCondition.Restraints[1] = eRestrainedCondition.Restrained;
            //        n.SupportCondition.Restraints[5] = eRestrainedCondition.Restrained;

            //        n.SupportCondition.Restraints[2] = eRestrainedCondition.NotRestrained;
            //        n.SupportCondition.Restraints[3] = eRestrainedCondition.NotRestrained;
            //        n.SupportCondition.Restraints[4] = eRestrainedCondition.NotRestrained;
            //    }
            //}


        }
        public void FillMemberInfoList(RunInfo runInfo)
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
                    Section = new ShellSection(runInfo.PoissonsRatio,runInfo.Shell_E) { Thickness = this.ShellThickness }};
                    member.IsOnlyPlate = false;
                    idx++;

                    ListOfMembers.Add(member);
                }
            }
        }

        public override MatrixCS GetGlobalStiffness()
        {
            if (_GlobalStiffness == null) SetGlobalStiffness(); 
            return _GlobalStiffness;
        }

        private  void SetGlobalStiffness()
        {
            MatrixCS KG = new MatrixCS(AssemblyData.NumberOfUnknowns, AssemblyData.NumberOfUnknowns);
            foreach (var mem in ListOfMembers)
            {
                var shellMember = (QuadShellMember)mem;
                var K = shellMember.GetGlobalStiffnessMatrix();
                var iNode = shellMember.IEndNode;
                var jNode = shellMember.JEndNode;
                var kNode = shellMember.KEndNode;
                var lNode = shellMember.LEndNode;

                var g = new List<int>();

                g.AddRange(AssemblyData.NodeEquationData[iNode.ID]);
                g.AddRange(AssemblyData.NodeEquationData[jNode.ID]);
                g.AddRange(AssemblyData.NodeEquationData[kNode.ID]);
                g.AddRange(AssemblyData.NodeEquationData[lNode.ID]);

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
            _GlobalStiffness =  KG;
        }

        public void SetMemberMass(double mass) 
        {
            foreach (var item in this.ListOfMembers)
            {
                (item as QuadShellMember).SetMass();
            }
        }

    private void SetMassMatrix()
        {
          
            MatrixCS massMatrix = new MatrixCS(AssemblyData.NumberOfUnknowns, AssemblyData.NumberOfUnknowns);
            foreach (var mem in ListOfMembers)
            {
                var shellMember = mem as QuadShellMember;
                var m = shellMember.GetGlobalMassMatrix();
                var iNode = shellMember.IEndNode;
                var jNode = shellMember.JEndNode;
                var kNode = shellMember.KEndNode;
                var lNode = shellMember.LEndNode;

                var g = new List<int>();

                g.AddRange(AssemblyData.NodeEquationData[iNode.ID]);
                g.AddRange(AssemblyData.NodeEquationData[jNode.ID]);
                g.AddRange(AssemblyData.NodeEquationData[kNode.ID]);
                g.AddRange(AssemblyData.NodeEquationData[lNode.ID]);

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

            _MassMatrix = massMatrix;
        }

        public override MatrixCS GetMassMatrix()
        {
            if (_MassMatrix == null) SetMassMatrix();
            return _MassMatrix;
        }

      

        public bool SetShellMemberUwByValue(double uwValue)
        {
         
            foreach (var item in this.ListOfMembers)
            {
                var shellMem = item as QuadShellMember;
                shellMem.Section.Material.Uw = uwValue;
                shellMem.SetMass();
            }

            return true;

        }

        public double GetTotalMass()
        {
            if (_TotalMass == 0) SetTotalMass();
            return _TotalMass;
        }

        public void SetTotalMass()
        {
            double totalMass = 0;
            foreach (var item in this.ListOfMembers)
            {
                totalMass += (item as QuadShellMember).MemberMass;
            }
            _TotalMass = totalMass;
        }


        public override bool SetModelData(RunData runData)
        {

            if (!(runData is RunInfo)) return false;

            var modelRunInfo = runData as RunInfo;

            this.IsOnlyPlate = false;
            this.Width = modelRunInfo.MemberDim;
            this.Height = modelRunInfo.MemberDim;
            this.MeshSize = modelRunInfo.ShellMeshSize;
            this.ShellThickness = modelRunInfo.ShellThickness;
            this.FillNodeInfo();
            this.FillMemberInfoList(modelRunInfo);
            SetModelGeometryType(modelRunInfo.GeometryType);
            SetShellMemberUwByValue(modelRunInfo.ShelllUnitWeigth);

            this.SetBorderNodesSupportCondition(modelRunInfo.BorderSupportType);
            if (modelRunInfo.IsShellOnlyPlate) this.SetModelAsPlate();


            return true;
        }
    }
    #endregion

}
