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
using OptimizationAlgorithms.PSOObjects.Particles;
using OptimizationAlgorithms.Particles;
using OptimizationAlgorithms.Swarms;
using OptimizationAlgorithms.Types;
using OptimizationAlgorithms;
using OptimizationAlgorithms.FitnessFunction;
using System.Diagnostics;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Series;


namespace TestConsol
{
    class Program
    {

        static void Main(string[] args)
        {



            //var runInfo = new RunInfo();
            //runInfo.EndConditionValue = 0;
            //runInfo.Horizon = eHorizon.LightMesh;
            //runInfo.MemberDim = 20;
            //runInfo.LatticeMeshSize = 1;
            //runInfo.ShellMeshSize = 1;
            //runInfo.FrameHeight = 0.85;
            //runInfo.AlphaRatio = 0.77;
            //runInfo.ShellThickness = 1.0;
            //runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            //runInfo.ShelllUnitWeigth = 1.0;

            ////RunAndSaveProcess();


            ////RunAnalysis(runInfo, 0.1); // shellThickness
            ////RunNew(runInfo);
            //RunAndEqualizeSystems(runInfo);

            //ReadResults();
            ReadResults();
            //RunAndSaveProcess();


        }


        static RunResult RunAndEqualizeSystems(RunInfo runInfo)
        {
            // Initialize shell model
            //ShellModelData.Instance.SetModelData(runInfo);
            //ShellModelData.Instance.AssignLoadToMiddle();
            //// Update Shell unit weight to have equal mass system
            ////ShellModelData.Instance.setmass(1, ShellModelData.Instance.ShellThickness);
            //LinearSolver.Instance.RunAnalysis_Shell();


            var shellTotalMass = ShellModelData.Instance.GetTotalMass();

            // Initialize lattice model
            var latticeModelData = new LatticeModelData();
            latticeModelData.SetModelData(runInfo);
            latticeModelData.SetFramesUwByTotalMass(shellTotalMass);
            latticeModelData.AssignLoadToMiddle();
            var latticeResults = LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData);

            var runResult = LinearSolver.Instance.EqualizeSystems(latticeResults, latticeModelData, runInfo); ;

            Console.WriteLine($"Alpha Ratio = {runResult.AlphaRatio}");
            Console.WriteLine($"Frame Thickness = {runResult.FrameHeight}");
            Console.WriteLine($"Shell Thickness = {runInfo.ShellThickness}");
            Console.WriteLine($"Equalization Ratio = {runResult.EqualizationRatio}");


            return runResult;

        }


        static void ReadResults()
        {
            var path = GetFilePath();
            string jsonString = File.ReadAllText(path);
            //Console.WriteLine(jsonString);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunResult>>(jsonString);

            foreach (var item in listOfRunDataDeserialized)
            {
                double totalPercentDiff = 0;
                int percentDiffCounter = 0;
                foreach (var compData in item.NodeCompareData)
                {
                    if (compData.PercentDiff != 0)
                    {
                        totalPercentDiff += Math.Abs(compData.PercentDiff);
                        percentDiffCounter++;
                    }
                }

                item.PercentDiff = totalPercentDiff / percentDiffCounter;
            }

            var validResults = listOfRunDataDeserialized.Where(x => x.PercentDiff < 10);

            validResults.FirstOrDefault().PrepareCSVFilesForGnuplot();

            var horizonGroups = validResults.GroupBy(x => x.Horizon);

            var alphaRatioGroups = validResults.GroupBy(x => x.AlphaRatio);
            var latticeMeshSize = validResults.GroupBy(x => x.LatticeMeshSize);

            var minMeanPercentDiff = listOfRunDataDeserialized.Min(x => x.PercentDiff);
            var minPercentDiffRuns = listOfRunDataDeserialized.Where(x => x.PercentDiff == minMeanPercentDiff);

            (minPercentDiffRuns.ElementAt(0) as RunResult).PrepareCSVFilesForGnuplot();


        }



        /// <summary>
        /// Runs analysis for given values and save results
        /// </summary>
        static void RunAndSaveProcess()
        {

            int runID = 1;

            double increment = 0.05;
            double heightIncr = 0.05;

            var listOfRunData = new List<RunResult>();

            var latticeMeshSizeList = new List<double>() { 0.5, 1 };
            var horizonList = new List<int>() { (int)eHorizon.LightMesh, (int)eHorizon.DenseMesh };

            var runInfo = new RunInfo();
            runInfo.EndConditionValue = 0;
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.MemberDim = 10;
            runInfo.LatticeMeshSize = 1;
            runInfo.ShellMeshSize = 1;
            runInfo.FrameHeight = 0.2;
            runInfo.AlphaRatio = 1.1;
            runInfo.ShellThickness = 1.0;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            runInfo.ShelllUnitWeigth = 1.0;

            // Initialize shell model

            ShellModelData.Instance.SetModelData(runInfo);
            ShellModelData.Instance.AssignLoadToMiddle();
            LinearSolver.Instance.RunAnalysis_Shell();

            var stopWatch = new Stopwatch();

            for (int m = 0; m < latticeMeshSizeList.Count; m++)
            {
                var latticeMeshSize = latticeMeshSizeList[m];

                runInfo.LatticeMeshSize = latticeMeshSize;

                for (int h = 0; h < horizonList.Count; h++)
                {
                    runInfo.Horizon = (eHorizon)horizonList[h];
                    double latticeFrameHeight = 0.5;
                    for (int j = 0; j < 10; j++)
                    {
                        runInfo.FrameHeight = latticeFrameHeight;
                        runInfo.AlphaRatio = 0.7; // set inital value again for every loop
                        for (int i = 0; i < 8; i++)
                        {
                            stopWatch.Start();

                            var runResult = RunAndEqualizeSystems(runInfo);
                            runResult.ID = runID;
                            listOfRunData.Add(runResult);
                            stopWatch.Stop();

                            Console.WriteLine(stopWatch.Elapsed.TotalSeconds) ;
                            stopWatch.Reset();
                            runInfo.AlphaRatio += increment;
                            runID++;
                        }
                        latticeFrameHeight += heightIncr;
                    }
                }
            }
            var path = GetFilePath();
            string jsonString = JsonSerializer.Serialize(listOfRunData);
            File.WriteAllText(path, jsonString);
            //Console.WriteLine(jsonString);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunResult>>(jsonString);
        }

        #region data
        
        #endregion


        /// <summary>
        /// Run analyisis with PSO optimization algorithm
        /// </summary>
        /// <param name="runInfo"></param>
        static void RunNew(RunInfo runInfo)
        {
            int numParticles = 10;
            int numDimensions = 2;

            // Initialize shell model
            ShellModelData.Instance.SetModelData(runInfo);
            ShellModelData.Instance.AssignLoadToMiddle();
            LinearSolver.Instance.RunAnalysis_Shell();


            var shellTotalMass = ShellModelData.Instance.GetTotalMass();

            ThesisDataContainer.Instance.LatticeModels = new Dictionary<int, LatticeModelData>();


            // Initialize the particles
            ParticleFactory particleFactory = new ParticleFactory();
            var particles = new List<IParticle>();
            Random rand = new Random();
            var resultData = new RunResult();
            for (int i = 0; i < numParticles; i++)
            {
                // Initialize lattice model
                var latticeModelData = new LatticeModelData();
                latticeModelData.SetModelData(runInfo);
                latticeModelData.SetFramesUwByTotalMass(shellTotalMass);

                latticeModelData.AssignLoadToMiddle();



                //Get first results
                resultData.NodeCompareData = LinearSolver.Instance.GetNodeCompareDataList(LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData));


                var position = new double[numDimensions];
                position[0] = 0.5 + rand.NextDouble();
                position[1] = rand.NextDouble();
                var particle = particleFactory.CreateLatticleParticle(position); // check position
                particle.Result = resultData.GetDisplacementProfile();
                particle.ID = i + 1;
                ThesisDataContainer.Instance.LatticeModels.Add(particle.ID, latticeModelData);
                particles.Add(particle);
            }






            // Initialize the SwarmFactory
            SwarmFactory swarmFactory = new SwarmFactory();

            // Initialize the Swarm
            ISwarm swarm = swarmFactory.CreateLatticeModelSwarm(particles, resultData.GetBestSolution());


            // Initialize the PSOAlgorithm
            IOptimizationAlgorithm<PSOAlgorithm> algorithm = new PSOAlgorithm();
            var instance = algorithm.GetInstance();
            instance.RegisterObserver(LinearSolver.Instance);
            IFitnessFunction fitnessFunction = new SumofSquaredDeviations();
            instance.SetAlgorithmParams(fitnessFunction, swarm, 0.5, 2.0, 2.0);
            // Run the algorithm
            instance.Run();
        }

        static void ReadResultsFromFile()
        {

            string path = GetFilePath();

            var file = File.ReadAllText(path);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunResult>>(file);



            var accaptableResultRUns = new Dictionary<Point, List<RunResult>>();

            var nCOmpData = listOfRunDataDeserialized.FirstOrDefault().NodeCompareData;
            var nPointList = new List<Point>();


            foreach (var item in nCOmpData) nPointList.Add(item.Point);

            foreach (Point item in nPointList)
            {
                var pointAccaptableRuns = new List<RunResult>();
                foreach (var res in listOfRunDataDeserialized)
                {
                    if (res.NodeCompareData.FirstOrDefault(p => p.Point.X == item.X && p.Point.Y == item.Y).PercentDiff < 3)
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
                    var meshSize = item.Value[i].LatticeMeshSize;
                    var alphaRatio = item.Value[i].AlphaRatio;
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

        public void NotifyConvergence(double fitness)
        {
            //throw new NotImplementedException();
        }

        public void NotifyNewGlobalBest(double fitness, double[] position)
        {
            //throw new NotImplementedException();
        }
    }
}
