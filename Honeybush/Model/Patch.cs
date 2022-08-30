using System;
using Mars.Interfaces.Agents;
using Mars.Interfaces; 
using System.Linq;
using Mars.Common;
using Mars.Components.Agents; 
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace Honeybush.Model
{
    /// <summary>
    ///  A simple agent stub that has an Init() method for initialization and a
    ///  Tick() method for acting in every tick of the simulation.
    /// </summary>
    public class Patch : IAgent<PatchLayer>, IPositionable
    {
		[PropertyDescription]
		public int Patch_ID{get; set;} //the FID 
		
		[PropertyDescription]
        public double Latitude { get; set; }
        
        [PropertyDescription]
        public double Longitude { get; set; }
		
		[PropertyDescription]
		public double Area {get; set;}
		
		[PropertyDescription]
		public string Camp{get; set;} //for realism purposes
		
		[PropertyDescription]
		public string Harvest_Data{get; set;} //get from history data
		
		[PropertyDescription]
		public string Fire_Data{get; set;} //get from history data
		
		public int Patch_Population{get; set;} //@determined by area, model output
		
		public double Crop_YieldA{get; set;}// model output, method A is individual plant contribition
		
		public double Crop_YieldB{get; set;}// model output, method B is average below
		
		public bool havePlants = false; //flag for determining if a patch has been intialised with plants yet
		
		public int LastHarvest = 0, LastBurnt = 0;
		
		public int countAge = 0, checkColour = 0; 
		
		private int Tick_counter = 0, Current_year = 2000; 
		
        public void Init(PatchLayer layer)
        {
            Layer = layer; // store layer for access within agent class
			Position = Position.CreateGeoPosition(Longitude, Latitude);
            Layer.PatchEnvironment.Insert(this);
			Patch_Population = Convert.ToInt32(Area)*GetRandomNumber(42, 800, 3500); //800-3500 plants per hectre 
			
        }//intialise method
        
        public void Tick()
        {
            //do something in every tick of the simulation
			//decide in every tick if the Patch is burnable or harvestable
			//Harvestable()and Burnable() will probably be public methods that indicate what the behaviour
			//of the fire and harvestor agent is 
			//fire and harvest acts upon a patch
			// crop yield and pop size is detmerined by plant agents
			//although - will also compare with the average calc eq
	
			Crop_YieldB = CalculateCropYieldEq(Patch_Population); 
			if(HarvestFireYear(Harvest_Data, Current_year))
				LastHarvest = Current_year;
			
			if(HarvestFireYear(Fire_Data, Current_year))
				LastBurnt = Current_year;  
			
			Tick_counter += 1; 
			if (Tick_counter % 52 == 0)
				Current_year++; 
        }
		
		private int GetRandomNumber(int seed, int min, int max)
		{
			Random rand = new Random(seed);
			return rand.Next(min, max); 
		}
		
		//equation determined by field guide - McGregor 2018 
		private double CalculateCropYieldEq(int numPlants)
		{
			return numPlants*0.45; //0.45kg = ave weight of plant
		}
		
		public void GetPopulationAltered(int change, int id)
		{
			if(id == Patch_ID) 
				Patch_Population += change; 
		}
		
		
		private bool HarvestFireYear(string data, int year)
		{
			if (data.Contains(year.ToString()))
				return true; 
			return false; 
		}
		// public int GetRainfall()
		// {
			
		// }

		
		// public bool IsBurnable()
		// {
			
		// }
		
        private PatchLayer Layer { get; set; } // provides access to the main layer of this agent
        public Guid ID { get; set; } // identifies the agent
		public Position Position { get; set; }
    }
}