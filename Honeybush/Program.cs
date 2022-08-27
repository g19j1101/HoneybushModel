using System;
using System.IO;
using Honeybush.Model;
using Mars.Components.Starter;
using Mars.Interfaces.Model;

namespace Honeybush
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // the scenario consist of the model (represented by the model description)
            // an the simulation configuration (see config.json)
            
            // Create a new model description that holds all parts of the model (agents, entities, layers)
            var description = new ModelDescription();
            description.AddLayer<PatchLayer>();// Register the layer for patch and plant
            description.AddAgent<Patch, PatchLayer>();// Register agent Patch
			description.AddAgent<Plant, PatchLayer>(); // register agent plant
            // scenario definition
            // use config.json that holds the specification of the scenario
            var file = File.ReadAllText("config.json");
            var config = SimulationConfig.Deserialize(file);
            
            // Create simulation task
            var task = SimulationStarter.Start(description, config);
            
            // Run simulation
            var loopResults = task.Run();
            
            // Feedback to user that simulation run was successful
            Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps");
			
        }
    }
}