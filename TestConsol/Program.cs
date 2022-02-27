﻿using Data;
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
            latticeModelData.Width = 16;
            latticeModelData.Height = 16;
            latticeModelData.MeshSize = 2;
            latticeModelData.FillNodeInfo();
            latticeModelData.FillMemberInfoList();
            latticeModelData.SetBorderNodesSupportCondition(eSupportType.Pinned);
            latticeModelData.AssignLoadToMiddle();
            latticeModelData.SetTorsionalReleaseToAllMembers();


            var shellModelData = new ShellModelData();
            shellModelData.Width = 16;
            shellModelData.Height = 16;
            shellModelData.MeshSize = 2;
            shellModelData.FillNodeInfo();
            shellModelData.FillMemberInfoList();
            shellModelData.SetBorderNodesSupportCondition(eSupportType.Fixed);
            shellModelData.AssignLoadToMiddle();



            var linearSolver = new LinearSolver();
            var latticeModelResultData = linearSolver.RunAnalysis_Lattice(latticeModelData);
            var shellModelResultData = linearSolver.RunAnalysis_Shell(shellModelData);

             linearSolver.EqualizeSystems(shellModelResultData, latticeModelResultData, latticeModelData);



            Console.ReadLine();



        }
    }
}
