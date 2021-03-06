using Data;
using ModelInfo;
using Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject.Structural_Members;
using System.Text.Json;
using System.IO;

namespace TestConsol
{
    class Program
    {
        static void Main(string[] args)
        {

            //RunAndSaveProcess();

            var runInfo = new RunData();
            runInfo.IsTorsionalRelease = true;
            runInfo.Horizon = 1.51;
            runInfo.MemberDim = 6;
            runInfo.LatticeMeshSize = 0.5;
            runInfo.ShellMeshSize = 1;
            runInfo.FrameHeight = 0.5;
            runInfo.AlphaRatio = 0.6;

            var incr = 0.1;

            for (int i = 0; i < 10; i++)
            {

            RunAnalysis(runInfo, 1);

                runInfo.FrameHeight += incr; 
            }



            ReadResultsFromFile();
        }

        static void RunAnalysis(RunData runInfo,double shellThickness )
        {

            var memberDim = runInfo.MemberDim;
            var latticeMeshSize = runInfo.LatticeMeshSize;
            var horizon = runInfo.Horizon;
            var latticeFrameHeight = runInfo.FrameHeight;
            


            var latticeModelData = new LatticeModelData();
            latticeModelData.Width = memberDim;
            latticeModelData.Height = memberDim;
            latticeModelData.MeshSize = latticeMeshSize;
            latticeModelData.FillNodeInfo();
            latticeModelData.FillMemberInfo(horizon, latticeFrameHeight);

            latticeModelData.SetModelGeometryType(eModelGeometryType.Rectangular);



            latticeModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
            latticeModelData.AssignLoadToMiddle();
            latticeModelData.SetTorsionalReleaseToAllMembers();
            var listOfNodes = latticeModelData.ListOfNodes;
            var latticeMass = latticeModelData.GetTotalMass();


            var shellMeshSize = runInfo.ShellMeshSize;

            var shellModelData = new ShellModelData();
            shellModelData.IsOnlyPlate = false;
            shellModelData.Width = memberDim;
            shellModelData.Height = memberDim;
            shellModelData.MeshSize = shellMeshSize;
            shellModelData.FillNodeInfo();
            shellModelData.FillMemberInfoList();

            var shellMemberCount = shellModelData.ListOfMembers.Count;
            var singleMemberMass = latticeMass / shellMemberCount;
            var shellUw = singleMemberMass / (shellMeshSize * shellMeshSize * shellThickness);





            shellModelData.SetModelGeometryType(eModelGeometryType.Rectangular);

            shellModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
            shellModelData.AssignLoadToMiddle();

            foreach (var item in shellModelData.ListOfMembers)
            {
                item.Thickness = shellThickness;
                item.Section.Material.Uw = shellUw;
            }


            var linearSolver = new LinearSolver();
            var latticeModelResultData = linearSolver.RunAnalysis_Lattice(latticeModelData);

            var shellModelResultData = linearSolver.RunAnalysis_Shell(shellModelData);
            var kg_Shell = linearSolver.GetGlobalStiffness_Shell();
            var shellMassMatrix = linearSolver.GetMassMatrix_Shell();


            var alphaRatio = runInfo.AlphaRatio;
            var isTorsionalRelease = runInfo.IsTorsionalRelease;

            var runData = linearSolver.EqualizeSystems(shellModelResultData, latticeModelResultData, latticeModelData, shellModelData, alphaRatio);
            runData.FrameHeight = latticeFrameHeight;
            runData.AlphaRatio = alphaRatio;
            runData.Horizon = horizon;
            runData.LatticeMeshSize = latticeMeshSize;
            runData.ShellMeshSize = shellMeshSize;
            runData.IsTorsionalRelease = isTorsionalRelease;
            var ratio = runData.EnergyRatio;

            var kg_LatticeNew = linearSolver.GetGlobalStiffness_Latttice();
            var latticeMassMatrix = linearSolver.GetMassMatrix_Latttice();

            //double lastEigen = 0;

            //var latticePeriods = linearSolver.GetPeriodsOfTheSystem(kg_LatticeNew, latticeMassMatrix, ref lastEigen);
            //var shellPeriods = linearSolver.GetPeriodsOfTheSystem(kg_Shell, shellMassMatrix, ref lastEigen);
            runData.ID = 1;
            //listOfRunData.Add(runData);

            //Console.Clear();
        }


        static void ReadResultsFromFile()
        {

            string path = GetFilePath();

            var file = File.ReadAllText(path);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunData>>(file);



            var accaptableResultRUns = new Dictionary<Point, List<RunData>>();

            var nCOmpData = listOfRunDataDeserialized.FirstOrDefault().NodeCompareData;
            var nPointList = new List<Point>();


            foreach (var item in nCOmpData) nPointList.Add(item.Point);

            foreach (Point item in nPointList)
            {
                var pointAccaptableRuns = new List<RunData>();
                foreach (var res in listOfRunDataDeserialized)
                {
                    if (res.NodeCompareData.FirstOrDefault(p => p.Point.X == item.X && p.Point.Y == item.Y).PercentDiff<3)
                        pointAccaptableRuns.Add(res);
                    
                }
                accaptableResultRUns.Add(item, pointAccaptableRuns);

            }

            var nodeMeshSize = new Dictionary<Point, List<double>>();
            var nodeAlphaRatio = new Dictionary<Point, List<double>>();


            foreach (var item in accaptableResultRUns)
            {
                var meshSizeList = new List<double>();
                var alphaList = new List<double>();
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var meshSize =  item.Value[i].LatticeMeshSize;
                    var alphaRatio= item.Value[i].AlphaRatio;
                    meshSizeList.Add(meshSize);
                    alphaList.Add(alphaRatio);
                }
                nodeMeshSize.Add(item.Key, meshSizeList);
                nodeAlphaRatio.Add(item.Key, alphaList);

            }


            Console.ReadLine();

        }








        public static string GetFilePath()
        {
            string path = "C:\\Users\\Burak T\\Desktop\\ders\\Tez\\Analiz Results";

            string fileName = "\\ListOFRunData.json";

            return path + fileName;
        }





        static void RunAndSaveProcess()
        {
            var res = new Dictionary<double, double>();

            int runID = 1;

            double increment = 0.1;
            double heightIncr = 0.05;

            var gapInstPt = new Point(6, 6, 0);
            double gapSize = 2;
            double shellMeshSize = 1;
            var latticeMeshSizeList = new List<double>() { 0.5, 1 };
            double memberDim = 6;

            double shellThickness = 0.6;

            var listOfNodes = new List<Node>();


            var listOfRunData = new List<RunData>();
            var isTorsionalRelase = new List<bool>() { true, false };

            var horizonList = new List<double>() { 1.5 };
            //var horizonList = new List<double>() { 1.5, 3.01 };
            for (int m = 0; m < latticeMeshSizeList.Count; m++)
            {
                var latticeMeshSize = latticeMeshSizeList[m];
                for (int t = 0; t < isTorsionalRelase.Count; t++)
                {
                    var isTorsionalRelease = isTorsionalRelase[t];

                    for (int h = 0; h < horizonList.Count; h++)
                    {
                        var horizon = horizonList[h];
                        double latticeFrameHeight = 0.3;
                        for (int j = 0; j < 20; j++)
                        {
                            double alphaRatio = 0.2;
                            for (int i = 0; i < 20; i++)
                            {
                                var latticeModelData = new LatticeModelData();
                                latticeModelData.Width = memberDim;
                                latticeModelData.Height = memberDim;
                                latticeModelData.MeshSize = latticeMeshSize;
                                latticeModelData.FillNodeInfo();
                                latticeModelData.FillMemberInfo(horizon, latticeFrameHeight);

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
                                shellModelData.MeshSize = shellMeshSize;
                                shellModelData.FillNodeInfo();
                                shellModelData.FillMemberInfoList();

                                var shellMemberCount = shellModelData.ListOfMembers.Count;
                                var singleMemberMass = latticeMass / shellMemberCount;
                                var shellUw = singleMemberMass / (shellMeshSize * shellMeshSize * shellThickness);





                                shellModelData.SetModelGeometryType(eModelGeometryType.Rectangular, gapInstPt, gapSize);

                                shellModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
                                shellModelData.AssignLoadToMiddle();

                                foreach (var item in shellModelData.ListOfMembers)
                                {
                                    item.Thickness = shellThickness;
                                    item.Section.Material.Uw = shellUw;
                                }


                                var linearSolver = new LinearSolver();
                                var latticeModelResultData = linearSolver.RunAnalysis_Lattice(latticeModelData);

                                var shellModelResultData = linearSolver.RunAnalysis_Shell(shellModelData);
                                var kg_Shell = linearSolver.GetGlobalStiffness_Shell();
                                var shellMassMatrix = linearSolver.GetMassMatrix_Shell();

                                var runData = linearSolver.EqualizeSystems(shellModelResultData, latticeModelResultData, latticeModelData, shellModelData, alphaRatio);
                                runData.FrameHeight = latticeFrameHeight;
                                runData.AlphaRatio = alphaRatio;
                                runData.Horizon = horizon;
                                runData.LatticeMeshSize = latticeMeshSize;
                                runData.ShellMeshSize = shellMeshSize;
                                runData.IsTorsionalRelease = isTorsionalRelease;
                                var ratio = runData.EnergyRatio;

                                var kg_LatticeNew = linearSolver.GetGlobalStiffness_Latttice();
                                var latticeMassMatrix = linearSolver.GetMassMatrix_Latttice();

                                //double lastEigen = 0;

                                //var latticePeriods = linearSolver.GetPeriodsOfTheSystem(kg_LatticeNew, latticeMassMatrix, ref lastEigen);
                                //var shellPeriods = linearSolver.GetPeriodsOfTheSystem(kg_Shell, shellMassMatrix, ref lastEigen);
                                runData.ID = runID;
                                listOfRunData.Add(runData);

                                Console.Clear();
                                alphaRatio = alphaRatio + increment;
                                runID++;
                            }

                            latticeFrameHeight += heightIncr;
                        }
                    }
                }

            }


            var listOfControlValues = new List<double>();

            var minRunDataList = new Dictionary<int, RunData>();

            foreach (var item in listOfRunData)
            {
                var shellPeriods = item.ShellPeriods;
                for (int i = 0; i < shellPeriods.Count; i++)
                {
                    if (Double.IsNaN(shellPeriods[i]))
                    {
                        shellPeriods[i] = 0;
                    }
                }
            }

            var path = GetFilePath();
            string jsonString = JsonSerializer.Serialize(listOfRunData);
            File.WriteAllText(path, jsonString);

            Console.WriteLine(jsonString);



            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunData>>(jsonString);






            var accaptableResultRUns = new Dictionary<Point, List<RunData>>();

            var nCOmpData = listOfRunDataDeserialized.FirstOrDefault().NodeCompareData;
            var nPointList = new List<Point>();




            foreach (var item in nCOmpData) nPointList.Add(item.Point);

            foreach (Point item in nPointList)
            {
                var listOfAccaptableRes = listOfRunData.Where(x => Math.Abs(x.NodeCompareData.FirstOrDefault(p => p.Point.X == item.X && p.Point.Y == item.Y ).PercentDiff)<3 ).ToList();

                accaptableResultRUns.Add(item, listOfAccaptableRes);
            }


            for (int i = 0; i < listOfNodes.Count; i++)
            {
                var controlNode = listOfNodes[i];


                var minValue = listOfRunData.Min(x => Math.Abs(x.NodeCompareData.FirstOrDefault(n => n.Point == controlNode.Point).PercentDiff));

                if (Double.IsNaN(minValue))
                    continue;


                var minValueRunRef = listOfRunData.FirstOrDefault(x => Math.Abs(x.NodeCompareData.FirstOrDefault(n => n.NodeID == controlNode.ID).PercentDiff) == minValue);
                var minvalueRun = new RunData();

                minvalueRun.PercentDiff = minValue;
                minvalueRun.FrameHeight = minValueRunRef.FrameHeight;
                minvalueRun.Horizon = minValueRunRef.Horizon;
                minvalueRun.IsTorsionalRelease = minValueRunRef.IsTorsionalRelease;
                minvalueRun.AlphaRatio = minValueRunRef.AlphaRatio;
                minvalueRun.EnergyRatio = minValueRunRef.EnergyRatio;
                minvalueRun.MinControlNode = null;
                minvalueRun.MinControlNode = controlNode;
                minRunDataList.Add(controlNode.ID, minvalueRun);
            }

            Console.Clear();
            Console.WriteLine("Node ID" + ";" + "X" + ";" + "Y" + ";" + "Percent Diff" + ";" + "Frame Height" + ";" + "Horizon" + ";" + "Torsional Release" + ";" + "Alpha Ratio" + ";" + "Enegy Ratio");
            //minRunDataList.OrderBy(x => x.MinControlNode.ID).ToList();


            for (int i = 0; i < minRunDataList.Count; i++)
            {
                var runData = minRunDataList.Values.ElementAt(i);

                var node = runData.MinControlNode;
                var strNodeId = node.ID.ToString();
                var strPx = node.Point.X.ToString();
                var strPy = node.Point.Y.ToString();
                var strPercentDiff = runData.PercentDiff.ToString();
                var strThickness = runData.FrameHeight.ToString();
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
