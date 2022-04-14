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
    public enum eAssemblyDataType
    {
        Lattice = 1, 
        Shell = 2
    }
    public class AssemblyDataContainer
    {

        public AssemblyDataContainer(int numberOfUnknows, Dictionary<int, List<int>> nodeEquationData)
        {
            _NumberOfUnknowns = numberOfUnknows;
            _NodeEquationData = nodeEquationData;
        }

        private int _NumberOfUnknowns;
        private Dictionary<int, List<int>> _NodeEquationData;
        private eAssemblyDataType _DataType;

        public int NumberOfUnknowns { get => _NumberOfUnknowns; set => _NumberOfUnknowns = value; }
        public Dictionary<int, List<int>> NodeEquationData { get => _NodeEquationData; set => _NodeEquationData = value; }
        public eAssemblyDataType DataType { get => _DataType; set => _DataType = value; }
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

            var solver = new GeneralizedEigenvalueDecomposition(kG.Matrix,mass.Matrix,false);
            var w2 = solver.RealEigenvalues;


            var w = new List<double>();

            for (int i = 0; i < w2.Length; i++)
            {
                w.Add(Math.Sqrt(w2[i]));
            }

            var T = new List<double>();

            for (int i = w.Count - 1; i>=0; i--)
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
            var latticeModelResultData = new LatticeModelResultData();
            
            _LatticeModelData = latticemodeldata;
            _AssemblyData = GetNodeAssemblyData(latticemodeldata.ListOfNodes);
            _AssemblyData.DataType = eAssemblyDataType.Lattice;

            var KG = GetGlobalStiffness_Latttice();
            var RHS = GetRightHandSide();


            var mass = GetMassMatrix_Latttice();
            var res = GetPeriodsOfTheSystem(KG, mass);

            //for (int i = 0; i < 6; i++)
            //{
            //    latticeModelResultData.ListOfPeriods.Add(res[i]);
            //}
            //Console.WriteLine("Lattice Model Periods");

            //Console.WriteLine("--------------------------");
            //for (int i = 0; i < res.Count; i++)
            //{
            //    Console.WriteLine("Mode No: " + (i + 1).ToString() + " Period:" + res[i].ToString());
            //}
            //Console.WriteLine("--------------------------");



            var externalKG = Matrix<double>.Build.SparseOfArray(KG.Matrix);
            var externalRHS = Matrix<double>.Build.SparseOfArray(RHS.Matrix);

            var dispRes = externalKG.Solve(externalRHS);




            var dispResAsArray = dispRes.ToArray();
            var dispResMatrix = new MatrixCS(dispResAsArray.Length, 1);

            dispResMatrix.Matrix = dispResAsArray;

            latticeModelResultData.DispRes = dispResMatrix;
            latticeModelResultData.NodeResults = new Dictionary<int, List<double>>();
            latticeModelResultData.FrameResults = new Dictionary<int, FrameMemberResults>();

            GetNodeResults(dispResAsArray,latticeModelResultData.NodeResults);
            FillFrameMemberResults(latticeModelResultData);

            var midNodeID = latticemodeldata.ListOfNodes.FirstOrDefault(x => x.Point.X == 1 && x.Point.Y == 1).ID;

            var midDisp = latticeModelResultData.NodeResults[midNodeID][2];

            return latticeModelResultData;
        }


    

        public double EqualizeSystems(ShellModelResultData shellModelRes,LatticeModelResultData latticeModelRes,LatticeModelData latticeModel,double alphaRatio)
        {
            var shellInternalEnergy = GetShellModelInternalEnergy(shellModelRes.DispRes);
            var latticeInternalEnergy = GetLatticeModelInternalEnergy(latticeModelRes.DispRes, latticeModel);
            var ratio = latticeInternalEnergy / shellInternalEnergy;

            var shortMemberLength = latticeModel.ListOfMembers.Min(x => x.GetLength());

            for (int i = 0; i < latticeModel.ListOfMembers.Count; i++)
            {
                var mem = latticeModel.ListOfMembers[i];
                if (mem.GetLength() == shortMemberLength )
                {
                    mem.Section.Material.E *= ratio; 
                }
                else
                {
                    mem.Section.Material.E *=(alphaRatio  * ratio);

                }
            }

            var newLatticeRes = RunAnalysis_Lattice(latticeModel);

            var latticeInternalEnergyNew = GetLatticeModelInternalEnergy(newLatticeRes.DispRes, latticeModel);

            var latticeMidPoint = latticeModel.ListOfNodes.FirstOrDefault(x => x.Point.X == latticeModel.Width / 2 && x.Point.Y == latticeModel.Height / 2);
            
            var midPointDispsLatt =newLatticeRes.NodeResults[latticeMidPoint.ID];
            var midPointDispsShell =shellModelRes.NodeResults[latticeMidPoint.ID];

            //shellModelRes.DispRes.Print();
            //newLatticeRes.DispRes.Print();

            var latticeNodeRes = newLatticeRes.NodeResults;
            var shellNodeRes = shellModelRes.NodeResults;
            Console.WriteLine("Lattice/Shell Node Vertical Deflections");
            for (int i = 0; i < latticeNodeRes.Count; i++)
            {
                var nodeID = latticeNodeRes.Keys.ElementAt(i);
                var nodeRes = latticeNodeRes[nodeID];
                var nodeResShell = shellNodeRes[nodeID];
                var nodePoint = latticeModel.ListOfNodes.FirstOrDefault(x => x.ID == nodeID).Point;
                var verticalDef = nodeRes[2];
                var verticalDefShell = nodeResShell[2];

                Console.WriteLine(nodeID.ToString() + "; " + nodePoint.X.ToString() + ";" + nodePoint.Y.ToString() + "  ;  " + verticalDef.ToString() + " ; " + verticalDefShell.ToString());
            }


            //Console.WriteLine("Shell Node Vertical Deflections");
            //for (int i = 0; i < shellNodeRes.Count; i++)
            //{
            //    var nodeID = shellNodeRes.Keys.ElementAt(i);
            //    var nodeRes = shellNodeRes[nodeID];
            //    var verticalDef = nodeRes[2];
            //    var nodePoint =  latticeModel.ListOfNodes.FirstOrDefault(x => x.ID == nodeID).Point;


            //    Console.WriteLine(nodeID.ToString() + "; " + nodePoint.X.ToString() + ";" + nodePoint.Y.ToString() + "  ;  " + verticalDef.ToString());
            //}


            return ratio;
        }

        public ShellModelResultData RunAnalysis_Shell(ShellModelData shellModelData)
        {
            var shellModelResultData = new ShellModelResultData();
            _ShellModelData = shellModelData;
            _AssemblyData = GetNodeAssemblyData(shellModelData.ListOfNodes);
            _AssemblyData.DataType = eAssemblyDataType.Shell;

            var KG = GetGlobalStiffness_Shell();
            var RHS = GetRightHandSide();

            var externalKG = Matrix<double>.Build.SparseOfArray(KG.Matrix);
            var externalRHS = Matrix<double>.Build.SparseOfArray(RHS.Matrix);

            var dispRes = externalKG.Solve(externalRHS);

            var dispResAsArray = dispRes.ToArray();
            var dispResMatrix = new MatrixCS(dispResAsArray.Length, 1);

            dispResMatrix.Matrix = dispResAsArray;
            var mass = GetMassMatrix_Shell();
            var res = GetPeriodsOfTheSystem(KG, mass);


            //for (int i = 0; i < 6; i++)
            //{
            //    shellModelResultData.ListOfPeriods.Add(res[i]);
            //}

            //Console.WriteLine("Shell Model Periods");

            //Console.WriteLine("--------------------------");
            //for (int i = 0; i < res.Count; i++)
            //{
            //    Console.WriteLine("Mode No: " + (i + 1).ToString() + " Period:" + res[i].ToString());
            //}
            //Console.WriteLine("--------------------------");

            var internalEnergy = GetShellModelInternalEnergy(dispResMatrix);


            shellModelResultData.DispRes = dispResMatrix;
            shellModelResultData.NodeResults = new Dictionary<int, List<double>>();

            GetNodeResults(dispResAsArray,shellModelResultData.NodeResults);



            return shellModelResultData;
        }

        public double GetShellModelInternalEnergy(MatrixCS dispMatrix)
        {
            double ret;
            var KG = GetGlobalStiffness_Shell();

            var firstPart = (dispMatrix.Transpose()).Multiply(KG);
            ret = firstPart.Multiply(dispMatrix).Matrix[0,0];


            return ret;
        }

        public double GetLatticeModelInternalEnergy(MatrixCS dispMatrix,LatticeModelData latticeModelData,double coeff = 1)
        {
            double ret;
            _LatticeModelData = latticeModelData;
            _AssemblyData = GetNodeAssemblyData(latticeModelData.ListOfNodes);
            _AssemblyData.DataType = eAssemblyDataType.Lattice;
            var KG = GetGlobalStiffness_Latttice();
            KG = KG.Multiply(coeff);
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

        public MatrixCS GetGlobalStiffness_Latttice()
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


        public MatrixCS GetGlobalStiffness_Shell()
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
