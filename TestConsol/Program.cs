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

            double alphaRatio = 0.2;
            double increment = 0.1;
            double thickness = 1;

            var gapInstPt = new Point(6, 6, 0);
            double gapSize = 2;
            double meshSize = 0.5;
            double memberDim = 4;


            var horizonList = new List<double>() { 1.5, 3.01 };
            var listOfRunData = new List<RunData>();
            var isTorsionalRelase = new List<bool>() { true, false };


            for (int t = 0; t < isTorsionalRelase.Count; t++)
            {
                var isTorsionalRelease = isTorsionalRelase[t];
            for (int h = 0; h < horizonList.Count; h++)
            {
                var horizon = horizonList[h];
                for (int j = 0; j < 20; j++)
                {

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

                        res.Add(alphaRatio, ratio);
                        Console.Clear();
                        alphaRatio = alphaRatio + increment;
                    }

                    thickness += increment;
                }
            }
            }

            var listOfControlValues = new List<double>();
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
