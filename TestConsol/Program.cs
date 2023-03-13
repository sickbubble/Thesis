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
using Adapters;

namespace TestConsol
{
    class Program
    {

        static void Main(string[] args)
        {

            //RunAndSaveProcess();

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

            //RunAnalysis(runInfo, 0.1); // shellThickness
            //RunNew(runInfo);
            RunAndEqualizeSystems(runInfo);


        }

        /// <summary>
        /// This method runs two models with given info and equalizes systems
        /// </summary>
        /// <param name="runInfo"></param>
        static void RunAndEqualizeSystems(RunInfo runInfo)
        {
            // Initialize shell model

            ShellModelData.Instance.SetModelData(runInfo);
            ShellModelData.Instance.AssignLoadToMiddle();
            // Update Shell unit weight to have equal mass system
            ShellModelData.Instance.SetShellMemberUwByValue(1);
            //ShellModelData.Instance.setmass(1, ShellModelData.Instance.ShellThickness);
            LinearSolver.Instance.RunAnalysis_Shell();
            var shellTotalMass = ShellModelData.Instance.GetTotalMass();

            // Initialize lattice model
            var latticeModelData = new LatticeModelData();
            latticeModelData.SetModelData(runInfo);
            latticeModelData.SetFramesUwByTotalMass(shellTotalMass);
            latticeModelData.AssignLoadToMiddle();
            double eqnRatio = LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData, true).EqnRatio;
            Console.WriteLine($"Alpha Ratio = {runInfo.AlphaRatio}");
            Console.WriteLine($"Frame Thickness = {runInfo.FrameHeight}");
            Console.WriteLine($"Shell Thickness = {runInfo.ShellThickness}");
            Console.WriteLine($"Equalization Ratio = {eqnRatio}");


            var aa = "0";

        }



        static void RunAndSaveProcess()
        {
            var res = new Dictionary<double, double>();

            int runID = 1;

            double increment = 0.1;
            double heightIncr = 0.05;
            var latticeMeshSizeList = new List<double>() { 0.5, 1 };
            var listOfNodes = new List<Node>();


            var listOfRunData = new List<RunData>();
            var isTorsionalRelase = new List<bool>() { true, false };
            var horizonList = new List<double>() { 1.5 ,3.01};
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
                            for (int i = 0; i < 10; i++)
                            {
                              
                                alphaRatio = alphaRatio + increment;
                                runID++;
                            }

                            latticeFrameHeight += heightIncr;
                        }
                    }
                }

            }
            var path = GetFilePath();
            string jsonString = JsonSerializer.Serialize(listOfRunData);
            File.WriteAllText(path, jsonString);
            Console.WriteLine(jsonString);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunData>>(jsonString);
        }




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
            // Update Shell unit weight to have equal mass system
            ShellModelData.Instance.SetShellMemberUwByValue(1);
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
