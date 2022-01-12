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
            var latticeModelData = new LatticeModelData();
            latticeModelData.Width = 2;
            latticeModelData.Height = 2;
            latticeModelData.MeshSize = 1;
            latticeModelData.FillNodeInfo();
            latticeModelData.FillMemberInfoList();
            latticeModelData.SetBorderNodesSupportCondition(eSupportType.Pinned);
            latticeModelData.AssignLoadToMiddle();


            var shellModelData =new ShellModelData();
            shellModelData.Width = 2;
            shellModelData.Height = 2;
            shellModelData.MeshSize = 1;
            shellModelData.FillNodeInfo();
            shellModelData.FillMemberInfoList();
            shellModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
            shellModelData.AssignLoadToMiddle();



            var solverLattice = new LinearSolver(latticeModelData);
var solverShell = new LinearSolver( shellModelData);
            solverLattice.RunAnalysis_Lattice();

            solverShell.RunAnalysis_Shell();


            Console.ReadLine();



        }
    }
}
