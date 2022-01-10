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


            var solver = new LinearSolver(latticeModelData);
            solver.RunAnalysis();



        }
    }
}
