using Data;
using Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlotUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {



            var runInfo = new RunInfo();
            runInfo.EndConditionValue = 0;
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.MemberDim = 10;
            runInfo.LatticeMeshSize = 0.5;
            runInfo.ShellMeshSize = 0.5;
            runInfo.FrameHeight = 0.85;
            runInfo.AlphaRatio = 0.77;
            runInfo.ShellThickness = 1.0;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            runInfo.ShelllUnitWeigth = 1.0;

            //RunAndSaveProcess();


            //RunAnalysis(runInfo, 0.1); // shellThickness
            //RunNew(runInfo);
            RunAndEqualizeSystems(runInfo);




         


        }


        static RunResult RunAndEqualizeSystems(RunInfo runInfo)
        {
            // Initialize shell model
            ShellModelData.Instance.SetModelData(runInfo);
            ShellModelData.Instance.AssignLoadToMiddle(runInfo.LoadMagnitude);
            // Update Shell unit weight to have equal mass system
            //ShellModelData.Instance.setmass(1, ShellModelData.Instance.ShellThickness);
            LinearSolver.Instance.RunAnalysis_Shell();


            var shellTotalMass = ShellModelData.Instance.GetTotalMass();

            // Initialize lattice model
            var latticeModelData = new LatticeModelData();
            latticeModelData.SetModelData(runInfo);
            latticeModelData.SetFramesUwByTotalMass(shellTotalMass);
            latticeModelData.AssignLoadToMiddle(runInfo.LoadMagnitude);
            var latticeResults = LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData);

            var runResult = LinearSolver.Instance.EqualizeSystems(latticeResults, latticeModelData, runInfo); ;

            Console.WriteLine($"Alpha Ratio = {runResult.AlphaRatio}");
            Console.WriteLine($"Frame Thickness = {runResult.FrameHeight}");
            Console.WriteLine($"Shell Thickness = {runInfo.ShellThickness}");
            Console.WriteLine($"Equalization Ratio = {runResult.EqualizationRatio}");


            return runResult;

        }
    }
}
