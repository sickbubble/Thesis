using Data;
using ModelInfo;
using Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject.Structural_Members;

namespace TestConsol
{
    class Program
    {
        static void Main(string[] args)
        {
            var res = new Dictionary<double, double>();

            double increment = 0.1;
            double thicknessIncr = 0.05;

            var gapInstPt = new Point(6, 6, 0);
            double gapSize = 2;
            double meshSize =0.5;
            double memberDim = 5;

            var listOfNodes = new List<Node>();


            var listOfRunData = new List<RunData>();
            var isTorsionalRelase = new List<bool>() { true, false };

            var horizonList = new List<double>() { 1.5, 3.01 };

            for (int t = 0; t < isTorsionalRelase.Count; t++)
            {
                var isTorsionalRelease = isTorsionalRelase[t];

                for (int h = 0; h < horizonList.Count; h++)
                {
                    var horizon = horizonList[h];
                    double thickness = 0.3;
                    for (int j = 0; j < 20; j++)
                    {

                        double alphaRatio = 0.2;
                        for (int i = 0; i < 20; i++)
                        {



                            var latticeModelData = new LatticeModelData();
                            latticeModelData.Width = memberDim;
                            latticeModelData.Height = memberDim;
                            latticeModelData.MeshSize = meshSize;
                            latticeModelData.FillNodeInfo();
                            latticeModelData.FillMemberInfoList(horizon);

                            latticeModelData.SetModelGeometryType(eModelGeometryType.Rectangular, gapInstPt, gapSize);



                            latticeModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
                            latticeModelData.AssignLoadToMiddle();
                            latticeModelData.SetTorsionalReleaseToAllMembers();
                            listOfNodes = latticeModelData.ListOfNodes;


                            var latticeMass = latticeModelData.GetTotalMass();

                            var shellModelData = new ShellModelData();
                            shellModelData.IsOnlyPlate = false;
                            shellModelData.Width = memberDim;
                            shellModelData.Height = memberDim;
                            shellModelData.MeshSize = meshSize;
                            shellModelData.FillNodeInfo();
                            shellModelData.FillMemberInfoList();

                            var shellMemberCount = shellModelData.ListOfMembers.Count;
                            var singleMemberMass = latticeMass / shellMemberCount;
                            var shellUw = singleMemberMass / (meshSize * meshSize * thickness);





                            shellModelData.SetModelGeometryType(eModelGeometryType.Rectangular, gapInstPt, gapSize);

                            shellModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
                            shellModelData.AssignLoadToMiddle();

                            foreach (var item in shellModelData.ListOfMembers)
                            {
                                item.Thickness = thickness;
                                item.Section.Material.Uw = shellUw;
                            }


                            var linearSolver = new LinearSolver();
                            var latticeModelResultData = linearSolver.RunAnalysis_Lattice(latticeModelData);

                            var shellModelResultData = linearSolver.RunAnalysis_Shell(shellModelData);
                            var kg_Shell = linearSolver.GetGlobalStiffness_Shell();
                            var shellMassMatrix = linearSolver.GetMassMatrix_Shell();

                            var runData = linearSolver.EqualizeSystems(shellModelResultData, latticeModelResultData, latticeModelData, alphaRatio);
                            runData.ShellThickness = thickness;
                            runData.AlphaRatio = alphaRatio;
                            runData.Horizon = horizon;
                            runData.IsTorsionalRelease = isTorsionalRelease;
                            var ratio = runData.EnergyRatio;

                            var kg_LatticeNew = linearSolver.GetGlobalStiffness_Latttice();
                            var latticeMassMatrix = linearSolver.GetMassMatrix_Latttice();

                            double lastEigen = 0;

                            var latticePeriods = linearSolver.GetPeriodsOfTheSystem(kg_LatticeNew, latticeMassMatrix, ref lastEigen);
                            var shellPeriods = linearSolver.GetPeriodsOfTheSystem(kg_Shell, shellMassMatrix, ref lastEigen);

                            listOfRunData.Add(runData);

                            Console.Clear();
                            alphaRatio = alphaRatio + increment;
                        }

                        thickness += thicknessIncr;
                    }
                }
            }



            var listOfControlValues = new List<double>();

            var minRunDataList = new Dictionary<int, RunData>();

            for (int i = 0; i < listOfNodes.Count; i++)
            {
                var controlNode = listOfNodes[i];


                var minValue = listOfRunData.Min(x => Math.Abs(x.NodeCompareData[controlNode.ID].PercentDiff));

                if (Double.IsNaN(minValue))
                    continue;


                var minValueRunRef = listOfRunData.FirstOrDefault(x => Math.Abs(x.NodeCompareData[controlNode.ID].PercentDiff) == minValue);
                var minvalueRun = new RunData();

                minvalueRun.PercentDiff = minValue;
                minvalueRun.ShellThickness = minValueRunRef.ShellThickness;
                minvalueRun.Horizon = minValueRunRef.Horizon;
                minvalueRun.IsTorsionalRelease = minValueRunRef.IsTorsionalRelease;
                minvalueRun.AlphaRatio = minValueRunRef.AlphaRatio;
                minvalueRun.EnergyRatio = minValueRunRef.EnergyRatio;
                minvalueRun.MinControlNode = null;
                minvalueRun.MinControlNode = controlNode;
                minRunDataList.Add(controlNode.ID, minvalueRun);
            }

            Console.Clear();
            Console.WriteLine("Node ID" + ";" + "X" + ";" + "Y" + ";" + "Percent Diff" + ";" + "Shell Thickness" + ";" + "Horizon" + ";" + "Torsional Release" + ";" + "Alpha Ratio" + ";" + "Enegy Ratio");
            //minRunDataList.OrderBy(x => x.MinControlNode.ID).ToList();


            for (int i = 0; i < minRunDataList.Count; i++)
            {
                var runData = minRunDataList.Values.ElementAt(i);

                var node = runData.MinControlNode;
                var strNodeId = node.ID.ToString();
                var strPx = node.Point.X.ToString();
                var strPy = node.Point.Y.ToString();
                var strPercentDiff = runData.PercentDiff.ToString();
                var strThickness = runData.ShellThickness.ToString();
                var strHorizon = runData.Horizon.ToString();
                var strRelease = runData.IsTorsionalRelease ? "Yes" : "No";
                var strEnergyRatio = runData.EnergyRatio.ToString();
                var strAlphaRatio = runData.AlphaRatio.ToString();


                Console.WriteLine(strNodeId + ";" + strPx + ";" + strPy + ";" + strPercentDiff + ";" + strThickness + ";" + strHorizon + ";" + strRelease + ";" + strAlphaRatio + ";" + strEnergyRatio);

            }

            for (int i = 0; i < res.Count; i++)
            {
                listOfControlValues.Add(Math.Pow(res.Keys.ElementAt(i), 3) / res.Values.ElementAt(i));
            }

            for (int i = 0; i < res.Count; i++)
            {
                Console.WriteLine(res.Keys.ElementAt(i).ToString() + ", " + res.Values.ElementAt(i).ToString() + "," + listOfControlValues[i].ToString());
            }

            Console.ReadLine();

        }
    }
}
