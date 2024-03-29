﻿using Adapters;
using ModelInfo;
using OptimizationAlgorithms.Particles;
using OptimizationAlgorithms.PSOObjects.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;
using ThesisProject.AssemblyInfo;
using ThesisProject.Loads;
using ThesisProject.Sections;
using ThesisProject.Structural_Members;

namespace Data
{


    public class LatticeModelData : FiniteElementModel
    {
        #region Ctor
        public LatticeModelData()
        {

        }
        #endregion

        #region Private Fields

         double _Width;
         double _Height;
         double _FrameHeight;
         double _MeshSize;

         double _LatticeMeshRatio;
        eHorizon _Horizon;
        double _AlphaRatio;
        double _Area;


        #endregion

        #region Public Properties


        public double Width { get => _Width; set => _Width = value; }
        public double Height { get => _Height; set => _Height = value; }
        public double FrameHeight { get => _FrameHeight; set => _FrameHeight = value; }
        public double MeshSize { get => _MeshSize; set => _MeshSize = value; }
        public double LatticeMeshRatio { get => _LatticeMeshRatio; set => _LatticeMeshRatio = value; }
        public eHorizon Horizon { get => _Horizon; set => _Horizon = value; }
        public double AlphaRatio { get => _AlphaRatio; set => _AlphaRatio = value; }
        public double Area { get => _Width * _Height; }


        #endregion

        #region Public Methods

        public double GetTotalMass()
        {
            double totalMass = 0;
            foreach (var mem in this.ListOfMembers)
            {
                totalMass += (mem as FrameMember).GetMass();
            }

            return totalMass;
        }
        private void AssignLoadToMiddle(double magnitude)
        {
            if (this.ListOfNodes != null)
            {
                var midNode = this.ListOfNodes.FirstOrDefault(x => x.Point.X == Width / 2 && x.Point.Y == Height / 2);
                if (midNode != null)
                {
                    ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -magnitude, Node = midNode, DofID = 2 });
                }
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

        public void AssignLoadToMiddle2()
        {
            if (ListOfNodes != null)
            {
                for (int i = 0; i < ListOfNodes.Count; i++)
                {
                    var node = ListOfNodes[i];
                    if (node.Point.X == Width / 2 &&
                        node.Point.Z == Height / 2)
                    {
                        this.ListOfLoads.Add(new PointLoad() { LoadType = eLoadType.Point, Magnitude = -1, Node = node, DofID = 1 });
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
                    var member = (FrameMember)this.ListOfMembers[i];

                    member.IEndCondition.IsReleaseMx = true;
                    member.JEndCondition.IsReleaseMx = true;
                }

            }

        }

        public void SetEndConditon(eEndConditionSet endCondition)
        {
            if (this.ListOfMembers != null)
            {

                for (int i = 0; i < this.ListOfMembers.Count; i++)
                {
                    var member = (FrameMember)this.ListOfMembers[i];
                     member.SetAllFixed();
                    switch (endCondition)
                    {
                        case eEndConditionSet.AllFixed:
                            break;
                        case eEndConditionSet.TorsionalRelease:
                            member.IEndCondition.IsReleaseMx = true;
                            member.JEndCondition.IsReleaseMx = true;
                            break;
                        case eEndConditionSet.WeakSideRotation:
                            member.IEndCondition.IsReleaseMz = true;
                            member.JEndCondition.IsReleaseMz = true;
                            break;
                        case eEndConditionSet.TorsionalReleaseAndWeakSide:
                            member.IEndCondition.IsReleaseMz = true;
                            member.JEndCondition.IsReleaseMz = true;
                            member.IEndCondition.IsReleaseMx = true;
                            member.JEndCondition.IsReleaseMx = true;
                            break;

                    }

                }

            }

        }


        public void AssignRatio(double ratio, double alphaRatio)
        {


            for (int i = 0; i < this.ListOfMembers.Count; i++)
            {
                var mem = (FrameMember)this.ListOfMembers[i];

                mem.Section.Material.SetBaseE();

                mem.Section.Material.SetE(mem.Section.Material.E * (mem.FrameType == eFrameMemberType.Rectangle ? ratio : (alphaRatio * ratio)));

                

            }
        }


        public void SetReleaseAllRotations()
        {
            if (this.ListOfMembers != null)
            {

                for (int i = 0; i < this.ListOfMembers.Count; i++)
                {
                    var member = (FrameMember)this.ListOfMembers[i];

                    member.IEndCondition.IsReleaseMx = true;
                    member.JEndCondition.IsReleaseMx = true;

                    member.IEndCondition.IsReleaseMy = true;
                    member.JEndCondition.IsReleaseMy = true;

                    member.IEndCondition.IsReleaseMz = true;
                    member.JEndCondition.IsReleaseMz = true;
                }

            }

        }

        public override MatrixCS GetGlobalStiffness()
        {
            MatrixCS KG = new MatrixCS(this.AssemblyData.NumberOfUnknowns, this.AssemblyData.NumberOfUnknowns);

            var listOfFrameMembers = this.ListOfMembers;

            for (int i = 0; i < listOfFrameMembers.Count; i++)
            {
                var frmMember = (FrameMember)listOfFrameMembers[i];
                var K = frmMember.GetGlobalStiffnessMatrix();
                var iNode = frmMember.IEndNode;
                var jNode = frmMember.JEndNode;

                var g = new List<int>();
                g.AddRange(this.AssemblyData.NodeEquationData[iNode.ID]);
                g.AddRange(this.AssemblyData.NodeEquationData[jNode.ID]);

                var matrixLength = K.NRows;//should be 12 for frame

                for (int k = 0; k < matrixLength; k++)
                {
                    for (int l = 0; l < matrixLength; l++)
                    {
                        var gk = g[k];
                        var gl = g[l];
                        if (gk != -1 && gl != -1) // -1 means free a.k.a unknown
                        {
                            KG.Matrix[gk, gl] = KG.Matrix[gk, gl] + K.Matrix[k, l];
                        }
                    }
                }
            }

            return KG;
        }


        public override MatrixCS GetMassMatrix()
        {
            MatrixCS globalMass = new MatrixCS(AssemblyData.NumberOfUnknowns, AssemblyData.NumberOfUnknowns);
            foreach (var mem in this.ListOfMembers)
            {
                var frmMember = (FrameMember)mem;
                var m = frmMember.GetGlobalMassMatrix();
                var iNode = frmMember.IEndNode;
                var jNode = frmMember.JEndNode;

                var g = new List<int>();
                g.AddRange(AssemblyData.NodeEquationData[iNode.ID]);
                g.AddRange(AssemblyData.NodeEquationData[jNode.ID]);

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

        public void SetBorderNodesSupportCondition(eSupportType supportType, Point LShapeInstPt = null)
        {
            var borderNodes = GetBorderNodes();
            this.ListOfSupports = new List<Support>();

            for (int i = 0; i < borderNodes.Count; i++)
            {
                var node = borderNodes[i];
                node.SupportCondition = new Support(supportType);
            }


            if (LShapeInstPt != null)
            {
                var limitX = LShapeInstPt.X;
                var limitY = LShapeInstPt.Y;
                var extraLNodes = this.ListOfNodes.Where(n => (n.Point.X >= limitX && n.Point.Y == limitY) ||
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
            this.ListOfSupports = new List<Support>();

            for (int i = 0; i < borderNodes.Count; i++)
            {
                var node = borderNodes[i];
                node.SupportCondition = new Support(supportType);
            }


            if (LShapeInstPt != null)
            {
                var limitX = LShapeInstPt.X;
                var limitY = LShapeInstPt.Y;
                var extraLNodes = this.ListOfNodes.Where(n => (n.Point.X >= limitX && n.Point.Y == limitY) ||
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

        public void FillMemberInfo( double sectionHeight)
        {
            var labelCounter = 1;
            ListOfMembers.Clear();
            var horizon = this.Horizon == eHorizon.DenseMesh ? 3.01 : 1.51;
            for (int i = 0; i < ListOfNodes.Count; i++)
            {
                for (int j = i + 1; j < ListOfNodes.Count; j++)
                {
                    var lengthOfMember = Math.Sqrt(Math.Pow(ListOfNodes[j].Point.X - ListOfNodes[i].Point.X, 2) + Math.Pow(ListOfNodes[j].Point.Y - ListOfNodes[i].Point.Y, 2) + Math.Pow(ListOfNodes[j].Point.Z - ListOfNodes[i].Point.Z, 2));


                    if (lengthOfMember < horizon * _MeshSize)
                    {
                        var frameMember = new FrameMember() { IEndNode = ListOfNodes[i], JEndNode = ListOfNodes[j], ID = labelCounter };
                        frameMember.FrameType = lengthOfMember == _MeshSize ? eFrameMemberType.Rectangle : eFrameMemberType.DiagonalFirst;

                        if (ListOfMembers.Any(x => (x as FrameMember).IEndNode == frameMember.JEndNode && (x as FrameMember).JEndNode == frameMember.IEndNode)) continue;

                        frameMember.Section = new FrameSection(1, sectionHeight);
                        ListOfMembers.Add(frameMember);
                        labelCounter++;
                    }
                }
            }
        }
        public void SetFramesUwByTotalMass(double totalMass) 
        {
            var singleFrameMass = totalMass / this.ListOfMembers.Count;

            foreach (var frmMember in this.ListOfMembers)
            {
                (frmMember as FrameMember).SetUwByMass(singleFrameMass);
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

            //var limitX = pointToCut.X;
            //var limitY = pointToCut.Y;

            //// Remove Nodes
            //var nodesToDelete = _ListOfNodes.Where(x => x.Point.X > limitX &&
            //                                            x.Point.Y > limitY).ToList();
            //if (nodesToDelete == null)
            //{
            //    return;
            //}

            //for (int i = 0; i < nodesToDelete.Count; i++)
            //{
            //    _ListOfNodes.Remove(nodesToDelete[i]);
            //}

            ////Remove Members

            //var membersToDelete = _ListOfMembers.Where(m => (m.IEndNode.Point.X > limitX &&
            //                                                m.IEndNode.Point.Y > limitY) ||
            //                                                (m.JEndNode.Point.X > limitX &&
            //                                                m.JEndNode.Point.Y > limitY)).ToList();


            //for (int i = 0; i < membersToDelete.Count; i++)
            //{
            //    _ListOfMembers.Remove(membersToDelete[i]);
            //}




        }

        private void SetModelShapeAsGapped(Point instPoint, double gapSize)
        {
            ////  ______________________ 
            //// |                      |
            //// |         ______       |
            //// |        |      |      |
            //// |        |______|      |
            //// |      pt              |
            //// |______________________|

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
            //                                                m.JEndNode.Point.Y < limitTopY)).ToList();


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

        public override bool SetModelData(RunData runData)
        {
            if (!(runData is RunInfo)) return false;

            var modelRunInfo = runData as RunInfo;

            this.AlphaRatio = runData.AlphaRatio;
            this.Width = modelRunInfo.MemberDim;
            this.Height = modelRunInfo.MemberDim;
            this.MeshSize = modelRunInfo.LatticeMeshSize;
            this.FrameHeight = modelRunInfo.FrameHeight;
            this.FillNodeInfo();
            this.FillMemberInfo(modelRunInfo.FrameHeight);
            this.SetModelGeometryType(modelRunInfo.GeometryType);
            this.SetBorderNodesSupportCondition(modelRunInfo.BorderSupportType);

            this.SetEndConditon(modelRunInfo.EndConditionValue);


            return true;
        }
        public bool UpdateModelForOptimization(double frameHeight,double alphaRatio) 
        {
            this.FrameHeight = Math.Abs(frameHeight);
            this.FillMemberInfo(this.FrameHeight);
            this.AlphaRatio = Math.Abs(alphaRatio) ;
            //this.SetEndConditon(eEndConditionSet);

            return true;
        }

        public IParticle GetOptimizationObject()
        {

            var e = (this.ListOfMembers.FirstOrDefault() as FrameMember).Section.Material.E;

            var latticeParticle = new LatticeParticle(new double[0]);


            return latticeParticle;
        }



        #endregion
    }

}
