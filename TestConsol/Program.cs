using Data;
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

            double thicknes = 0.1;
            double increment = 0.01;

            for (int i = 0; i < 20; i++)
            {

            var latticeModelData = new LatticeModelData();
            latticeModelData.Width = 10;
            latticeModelData.Height = 10;
            latticeModelData.MeshSize = 1;
            latticeModelData.FillNodeInfo();
            latticeModelData.FillMemberInfoList();
            latticeModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
                latticeModelData.AssignLoadToMiddle();
                latticeModelData.SetTorsionalReleaseToAllMembers();


                var shellModelData = new ShellModelData();
            shellModelData.Width = 10;
            shellModelData.Height = 10;
            shellModelData.MeshSize = 1;
            shellModelData.FillNodeInfo();
            shellModelData.FillMemberInfoList();
            shellModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
            shellModelData.AssignLoadToMiddle();

                foreach (var item in shellModelData.ListOfMembers)
                    item.Thickness = thicknes;
                

            var linearSolver = new LinearSolver();
            var latticeModelResultData = linearSolver.RunAnalysis_Lattice(latticeModelData);
                var kg_Lattice = linearSolver.GetGlobalStiffness_Latttice();

                var shellModelResultData = linearSolver.RunAnalysis_Shell(shellModelData);
                var kg_Shell = linearSolver.GetGlobalStiffness_Latttice();

                var shellMassMatrix = linearSolver.GetMassMatrix_Shell();


                var ratio =  linearSolver.EqualizeSystems(shellModelResultData, latticeModelResultData, latticeModelData,kg_Shell,shellMassMatrix);
                 kg_Lattice = linearSolver.GetGlobalStiffness_Latttice();



                var latticePeriods = linearSolver.GetPeriodsOfTheSystem(kg_Lattice, shellMassMatrix);
                var shellPeriods = linearSolver.GetPeriodsOfTheSystem(kg_Shell, shellMassMatrix);


                res.Add(thicknes, ratio);
                thicknes = thicknes + increment;
            }

            var listOfControlValues = new List<double>();
            for (int i = 0; i < res.Count; i++)
            {
                listOfControlValues.Add(Math.Pow(res.Keys.ElementAt(i), 3) / res.Values.ElementAt(i));
            }

            for (int i = 0; i < res.Count; i++)
            {
                Console.WriteLine(res.Keys.ElementAt(i).ToString() + ", " + res.Values.ElementAt(i).ToString() + "," + listOfControlValues[i].ToString() );
            }

            Console.ReadLine();

        }
    }
}
