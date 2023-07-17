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


            //RunAndSaveProcess_ShellConvergenceTest();
            //RunAndSaveProcess();

            //RunAndSaveProcess_MeshCount();
            //RunAndSaveProcess_MeshCountWEqualize();
            //RunAndSaveProcess_ConstantSizeDiffMesh();
            ////RunAnalysis(runInfo, 0.1); // shellThickness
            ////RunNew(runInfo);
            //var runData = RunAndEqualizeSystems(runInfo,true);
            //runData.SetShellAnalyticalResults();
            //runData.PrepareCSVFilesForGnuplot();
            ReadResults();

        }

    


        static RunResult RunAndEqualizeSystems(RunInfo runInfo,bool isStandAlone = false)
        {
            if (isStandAlone)
            {
            //Initialize shell model
            ShellModelData.Instance.SetModelData(runInfo);
                
            ShellModelData.Instance.SetLoad(runInfo);
            // Update Shell unit weight to have equal mass system
            //ShellModelData.Instance.setmass(1, ShellModelData.Instance.ShellThickness);
            LinearSolver.Instance.RunAnalysis_Shell();
            }


            var shellTotalMass = ShellModelData.Instance.GetTotalMass();

            // Initialize lattice model
            var latticeModelData = new LatticeModelData();
            latticeModelData.SetModelData(runInfo);
            latticeModelData.SetFramesUwByTotalMass(shellTotalMass);
            latticeModelData.SetLoad(runInfo);
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

            foreach (var item in listOfRunDataDeserialized)
            {
                item.SetShellAnalyticalResults();
            }


            var runData = listOfRunDataDeserialized.LastOrDefault();
            //runData.Shell_E = 1;
            //runData.MemberDim = 10;
            //runData.PoissonsRatio = 0.3;
            //runData.LoadMagnitude = 1;
            //runData.ShellThickness = 1;


            var midDefs1 = listOfRunDataDeserialized.FirstOrDefault().NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs2 = listOfRunDataDeserialized.ElementAt(1).NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs4 = listOfRunDataDeserialized.ElementAt(2).NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs8 = listOfRunDataDeserialized.ElementAt(3).NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            Console.WriteLine($"Lattice VerticalDisp : {midDefs1.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs1.ShellVerticalDisp} ");
            Console.WriteLine($"Lattice VerticalDisp : {midDefs2.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs2.ShellVerticalDisp} ");
            Console.WriteLine($"Lattice VerticalDisp : {midDefs4.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs4.ShellVerticalDisp} ");
            Console.WriteLine($"Lattice VerticalDisp : {midDefs8.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs8.ShellVerticalDisp} ");


            var listOFRunData = new List<RunResult>();

            var runInfo = new RunInfo();
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.FrameHeight = 0.3;
            runInfo.AlphaRatio = 0.8;
            runInfo.ShellThickness = 0.6;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            runInfo.ShelllUnitWeigth = 1.0;
            runInfo.PoissonsRatio = 0.30;
            runInfo.Shell_E = 25 * Math.Pow(10, 9);

            runInfo.LoadMagnitude = 10;
            runInfo.LoadingType = ThesisProject.Loads.eLoadingType.FullAreaLoad;

            runInfo.BorderSupportType = eSupportType.Fixed;
            runInfo.IsShellOnlyPlate = true;
            ShellModelData.Instance.SetModelData(runInfo);
            var shellTotalMass = ShellModelData.Instance.GetTotalMass();
            for (int i = 0; i < listOfRunDataDeserialized.Count; i++)
            {
                var latticeModelData = new LatticeModelData();
                latticeModelData.SetModelData(runInfo);
                latticeModelData.SetFramesUwByTotalMass(shellTotalMass);
                latticeModelData.SetLoad(runInfo);


                 var midData = listOfRunDataDeserialized[i].NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);



                //latticeModelData.AssignRatio(runResult.EqualizationRatio, runInfo.AlphaRatio);
                //var latticeResults = LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData);


            }

            //Console.WriteLine(jsonString);

           


            runData.PrepareCSVFilesForGnuplot();

            listOfRunDataDeserialized.FirstOrDefault().PrepareCSVFilesForGnuplot();
            listOfRunDataDeserialized.LastOrDefault().PrepareCSVFilesForGnuplot();

            var horizonGroups = validResults.GroupBy(x => x.Horizon);

            var alphaRatioGroups = validResults.GroupBy(x => x.AlphaRatio);
            var latticeMeshSize = validResults.GroupBy(x => x.LatticeMeshSize);

            var minMeanPercentDiff = listOfRunDataDeserialized.Min(x => x.PercentDiff);
            var minPercentDiffRuns = listOfRunDataDeserialized.Where(x => x.PercentDiff == minMeanPercentDiff);

            (minPercentDiffRuns.ElementAt(0) as RunResult).PrepareCSVFilesForGnuplot();
            (minPercentDiffRuns.ElementAt(1) as RunResult).PrepareCSVFilesForGnuplot();


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

            var latticeMeshSizeList = new List<double>() { 0.25, 0.5, 1 };
            var horizonList = new List<int>() { (int)eHorizon.LightMesh, (int)eHorizon.DenseMesh };

            var runInfo = new RunInfo();
            runInfo.EndConditionValue = 0;
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.MemberDim = 10;
            runInfo.LatticeMeshSize = 1;
            runInfo.ShellMeshSize = 0.25;
            runInfo.FrameHeight = 0.2;
            runInfo.AlphaRatio = 1.1;
            runInfo.ShellThickness = 1.0;
            runInfo.PoissonsRatio = 0.30;
            runInfo.Shell_E = 1;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            runInfo.ShelllUnitWeigth = 1.0;
            runInfo.LoadMagnitude = 1;
            runInfo.IsShellOnlyPlate = true;

            // Initialize shell model

            ShellModelData.Instance.SetModelData(runInfo);
            ShellModelData.Instance.SetLoad(runInfo);
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

                            var runResult = RunAndEqualizeSystems(runInfo,false);
                            runResult.ID = runID;
                            listOfRunData.Add(runResult);
                            stopWatch.Stop();

                            Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
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


        /// <summary>
        /// Runs analysis for given values and save results for different mesh sizes
        /// </summary>
        static void RunAndSaveProcess_ShellConvergenceTest()
        {

            int runID = 1;

            var listOfRunData = new List<RunResult>();


            var runInfo = new RunInfo();
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.MemberDim = 10;


            var nElmX = 32;
            
            // Lattice Input
            runInfo.LatticeMeshSize = runInfo.MemberDim/nElmX;
            runInfo.FrameHeight = 0.2;
            runInfo.AlphaRatio = 1.1;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            
            runInfo.PoissonsRatio = 0.30;
            
            //Shell Input Card
            runInfo.ShellMeshSize = 0.5;
            runInfo.ShellThickness = 0.6;
            runInfo.ShelllUnitWeigth = 1.0;
            runInfo.Shell_E = 25 * Math.Pow(10,9);
            runInfo.BorderSupportType = eSupportType.Pinned;
            
            runInfo.LoadMagnitude = 1;
            runInfo.IsShellOnlyPlate = true;
            runInfo.LoadingType = ThesisProject.Loads.eLoadingType.FullAreaLoad;
            
                //// Initialize shell model

            //ShellModelData.Instance.SetModelData(runInfo);
            //ShellModelData.Instance.AssignLoadToMiddle(runInfo.LoadMagnitude);
            //LinearSolver.Instance.RunAnalysis_Shell();

            var stopWatch = new Stopwatch();

                stopWatch.Start();

                var runResult = RunAndEqualizeSystems(runInfo,true);
                runResult.ID = runID;
                listOfRunData.Add(runResult);
                stopWatch.Stop();
                Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
                stopWatch.Reset();

                runID++;
            runResult.SetShellAnalyticalResults();
            listOfRunData.Add(runResult);            
            string jsonString2 = JsonSerializer.Serialize(listOfRunData);
            File.WriteAllText(GetFilePath(), jsonString2);
            //Console.WriteLine(jsonString);
            var  fineal_listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunResult>>(jsonString2);
        }

     

        /// <summary>
        /// Runs analysis for given values and save results for different mesh sizes
        /// </summary>
  
      static void RunAndSaveProcess_MeshCountWEqualize()
        {

            int runID = 1;

            var listOfRunData = new List<RunResult>();

            var memberDims = new List<double>() { 1, 2, 4,8};

            var runInfo = new RunInfo();
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.FrameHeight = 0.3;
            runInfo.AlphaRatio = 0.8;
            runInfo.ShellThickness = 0.6;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            runInfo.ShelllUnitWeigth = 1.0;
            runInfo.PoissonsRatio = 0.30;
            runInfo.Shell_E = 25*Math.Pow(10,9);
            
            runInfo.LoadMagnitude = 10;
            runInfo.LoadingType = ThesisProject.Loads.eLoadingType.FullAreaLoad;
            
            runInfo.BorderSupportType = eSupportType.Fixed;
            runInfo.IsShellOnlyPlate = true;


            var stopWatch = new Stopwatch();

            for (int m = 0; m < memberDims.Count; m++)
            {
                runInfo.MemberDim = memberDims[m];

                runInfo.ShellMeshSize = runInfo.MemberDim/32.0;
                runInfo.LatticeMeshSize = runInfo.MemberDim / 32.0;
                stopWatch.Start();

                ShellModelData.KillInstance();
                ShellModelResultData.KillInstance();
                ShellModelData.Instance.SetModelData(runInfo);
                
                ShellModelData.Instance.SetLoad(runInfo);
                LinearSolver.Instance.RunAnalysis_Shell();

                var shellTotalMass = ShellModelData.Instance.GetTotalMass();

                // Initialize lattice model
                var latticeModelData = new LatticeModelData();
                latticeModelData.SetModelData(runInfo);
                latticeModelData.SetFramesUwByTotalMass(shellTotalMass);
                latticeModelData.SetLoad(runInfo);
                var latticeResults = LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData);

                var runResult = LinearSolver.Instance.EqualizeSystems(latticeResults, latticeModelData, runInfo); ;
                runResult.ID = runID;
                //Get first results


                listOfRunData.Add(runResult);
                stopWatch.Stop();
                Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
                stopWatch.Reset();

                runID++;
            }
            var path = GetFilePath();
            string jsonString = JsonSerializer.Serialize(listOfRunData);
            File.WriteAllText(path, jsonString);

            var midDefs1 = listOfRunData.FirstOrDefault().NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs2 = listOfRunData.ElementAt(1).NodeCompareData.FirstOrDefault(x => x.Point.X == 1 && x.Point.Y == 1);
            var midDefs4 = listOfRunData.ElementAt(2).NodeCompareData.FirstOrDefault(x => x.Point.X == 2 && x.Point.Y == 2);
            var midDefs8 = listOfRunData.ElementAt(3).NodeCompareData.FirstOrDefault(x => x.Point.X == 4 && x.Point.Y == 4);
            Console.WriteLine($"Lattice VerticalDisp : {midDefs1.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs1.ShellVerticalDisp} " );
            Console.WriteLine($"Lattice VerticalDisp : {midDefs2.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs2.ShellVerticalDisp} " );
            Console.WriteLine($"Lattice VerticalDisp : {midDefs4.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs4.ShellVerticalDisp} " );
            Console.WriteLine($"Lattice VerticalDisp : {midDefs8.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs8.ShellVerticalDisp} " );
            //Console.WriteLine(jsonString);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunResult>>(jsonString);
        }
    static void RunAndSaveProcess_MeshCount()
        {

            int runID = 1;

            var listOfRunData = new List<RunResult>();

            var memberDims = new List<double>() { 1, 2, 4,8};

            var runInfo = new RunInfo();
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.FrameHeight = 0.3;
            runInfo.AlphaRatio = 0.8;
            runInfo.ShellThickness = 0.6;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            runInfo.ShelllUnitWeigth = 1.0;
            runInfo.PoissonsRatio = 0.30;
            runInfo.Shell_E = 25*Math.Pow(10,9);
            
            runInfo.LoadMagnitude = 10;
            runInfo.LoadingType = ThesisProject.Loads.eLoadingType.FullAreaLoad;
            
            runInfo.BorderSupportType = eSupportType.Fixed;
            runInfo.IsShellOnlyPlate = true;


            var stopWatch = new Stopwatch();

            for (int m = 0; m < memberDims.Count; m++)
            {
                runInfo.MemberDim = memberDims[m];

                runInfo.ShellMeshSize = runInfo.MemberDim/32.0;
                runInfo.LatticeMeshSize = runInfo.MemberDim / 32.0;
                stopWatch.Start();

                ShellModelData.KillInstance();
                ShellModelResultData.KillInstance();
                ShellModelData.Instance.SetModelData(runInfo);
                ShellModelData.Instance.SetLoad(runInfo);
                LinearSolver.Instance.RunAnalysis_Shell();


                var latticeModelData = new LatticeModelData();
                latticeModelData.SetModelData(runInfo);
                latticeModelData.SetFramesUwByTotalMass(ShellModelData.Instance.GetTotalMass());

                latticeModelData.SetLoad(runInfo);

                var resultData = new RunResult();
                resultData.ID = runID;
                //Get first results
                resultData.NodeCompareData = LinearSolver.Instance.GetNodeCompareDataList(LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData));


                listOfRunData.Add(resultData);
                stopWatch.Stop();
                Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
                stopWatch.Reset();

                runID++;
            }
            var path = GetFilePath();
            string jsonString = JsonSerializer.Serialize(listOfRunData);
            File.WriteAllText(path, jsonString);

            var midDefs1 = listOfRunData.FirstOrDefault().NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs2 = listOfRunData.ElementAt(1).NodeCompareData.FirstOrDefault(x => x.Point.X == 1 && x.Point.Y == 1);
            var midDefs4 = listOfRunData.ElementAt(2).NodeCompareData.FirstOrDefault(x => x.Point.X == 2 && x.Point.Y == 2);
            var midDefs8 = listOfRunData.ElementAt(3).NodeCompareData.FirstOrDefault(x => x.Point.X == 4 && x.Point.Y == 4);
            Console.WriteLine($"Lattice VerticalDisp : {midDefs1.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs1.ShellVerticalDisp} " );
            Console.WriteLine($"Lattice VerticalDisp : {midDefs2.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs2.ShellVerticalDisp} " );
            Console.WriteLine($"Lattice VerticalDisp : {midDefs4.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs4.ShellVerticalDisp} " );
            Console.WriteLine($"Lattice VerticalDisp : {midDefs8.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs8.ShellVerticalDisp} " );
            //Console.WriteLine(jsonString);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunResult>>(jsonString);
        }
        static void RunAndSaveProcess_ConstantSizeDiffMesh()
        {

            int runID = 1;

            var listOfRunData = new List<RunResult>();

            var memberDims = new List<double>() { 32, 16, 8, 4 };

            var runInfo = new RunInfo();
            runInfo.Horizon = eHorizon.LightMesh;
            runInfo.FrameHeight = 0.3;
            runInfo.AlphaRatio = 0.8;
            runInfo.ShellThickness = 0.6;
            runInfo.EndConditionValue = eEndConditionSet.TorsionalRelease;
            runInfo.ShelllUnitWeigth = 1.0;
            runInfo.PoissonsRatio = 0.30;
            runInfo.Shell_E = 25 * Math.Pow(10, 9);

            runInfo.LoadMagnitude = 10;
            runInfo.LoadingType = ThesisProject.Loads.eLoadingType.FullAreaLoad;

            runInfo.BorderSupportType = eSupportType.Fixed;
            runInfo.IsShellOnlyPlate = true;

                runInfo.MemberDim = 1;

            var stopWatch = new Stopwatch();

            runInfo.ShellMeshSize = runInfo.MemberDim /32.0;
            ShellModelData.Instance.SetModelData(runInfo);
            ShellModelData.Instance.SetLoad(runInfo);
            LinearSolver.Instance.RunAnalysis_Shell();

            for (int m = 0; m < memberDims.Count; m++)
            {

                runInfo.LatticeMeshSize = runInfo.MemberDim / memberDims[m];
                stopWatch.Start();

                var latticeModelData = new LatticeModelData();
                latticeModelData.SetModelData(runInfo);
                latticeModelData.SetFramesUwByTotalMass(ShellModelData.Instance.GetTotalMass());

                latticeModelData.SetLoad(runInfo);

                var runResult = LinearSolver.Instance.EqualizeSystems(LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData), latticeModelData, runInfo); ;

                runResult.ID = runID;
                //Get first results
                runResult.NodeCompareData = LinearSolver.Instance.GetNodeCompareDataList(LinearSolver.Instance.RunAnalysis_Lattice(latticeModelData));


                listOfRunData.Add(runResult);
                stopWatch.Stop();
                Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
                stopWatch.Reset();

                runID++;
            }
            var path = GetFilePath();
            string jsonString = JsonSerializer.Serialize(listOfRunData);
            File.WriteAllText(path, jsonString);

            var midDefs1 = listOfRunData.FirstOrDefault().NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs2 = listOfRunData.ElementAt(1).NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs4 = listOfRunData.ElementAt(2).NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            var midDefs8 = listOfRunData.ElementAt(3).NodeCompareData.FirstOrDefault(x => x.Point.X == 0.5 && x.Point.Y == 0.5);
            Console.WriteLine($"Lattice VerticalDisp : {midDefs1.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs1.ShellVerticalDisp} ");
            Console.WriteLine($"Lattice VerticalDisp : {midDefs2.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs2.ShellVerticalDisp} ");
            Console.WriteLine($"Lattice VerticalDisp : {midDefs4.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs4.ShellVerticalDisp} ");
            Console.WriteLine($"Lattice VerticalDisp : {midDefs8.LatticeVerticalDisp }    Shell Vertical Dips : {midDefs8.ShellVerticalDisp} ");
            //Console.WriteLine(jsonString);
            var listOfRunDataDeserialized = JsonSerializer.Deserialize<List<RunResult>>(jsonString);
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
            ShellModelData.Instance.SetLoad(runInfo);
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

                latticeModelData.SetLoad(runInfo);


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
            string path = "C:\\Users\\Burak T\\Desktop\\RunResults";

            //string fileName = "\\MeshCount32ComparisonConstantSize.json";
            string fileName = "\\ListOfRunData3.json";

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
