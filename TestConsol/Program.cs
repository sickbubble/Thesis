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

            for (int i = 0; i < 20; i++)
            {

                var gapInstPt = new Point(6, 6, 0);
                double gapSize = 2;
                double meshSize = 1;
                double memberDim = 6;

                var latticeModelData = new LatticeModelData();
                latticeModelData.Width =memberDim;
                latticeModelData.Height =memberDim;
                latticeModelData.MeshSize = meshSize;
                latticeModelData.FillNodeInfo();
                latticeModelData.FillMemberInfoList();

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
                var shellUw =singleMemberMass / (meshSize * meshSize * thickness);





                shellModelData.SetModelGeometryType(eModelGeometryType.Rectangular, gapInstPt, gapSize);

                shellModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
                shellModelData.AssignLoadToMiddle();

                foreach (var item in shellModelData.ListOfMembers)
                {
                    //item.Thickness = thickness;
                    item.Section.Material.Uw = shellUw;
                }


                var linearSolver = new LinearSolver();
                var latticeModelResultData = linearSolver.RunAnalysis_Lattice(latticeModelData);

                var shellModelResultData = linearSolver.RunAnalysis_Shell(shellModelData);
                var kg_Shell = linearSolver.GetGlobalStiffness_Shell();
                var shellMassMatrix = linearSolver.GetMassMatrix_Shell();

                var ratio = linearSolver.EqualizeSystems(shellModelResultData, latticeModelResultData, latticeModelData,alphaRatio);
                
                var kg_LatticeNew = linearSolver.GetGlobalStiffness_Latttice();
                var latticeMassMatrix = linearSolver.GetMassMatrix_Latttice();
        
                var latticePeriods = linearSolver.GetPeriodsOfTheSystem(kg_LatticeNew, latticeMassMatrix);
                var shellPeriods = linearSolver.GetPeriodsOfTheSystem(kg_Shell, shellMassMatrix);


                res.Add(alphaRatio, ratio);
                alphaRatio = alphaRatio + increment;
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
