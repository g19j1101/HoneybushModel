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
		#region Attributes
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
		
		[PropertyDescription]
		public int Harvest_Days{get; set;}		
		
		public int Patch_Population{get; set;} //@determined by area, model output
		
		public double Crop_YieldA{get; set;}// model output, method A is individual plant contribition
		
		public double Crop_YieldB{get; set;}// model output, method B is average below
		
		
		public bool havePlants = false; //flag for determining if a patch has been intialised with plants yet
		
		public int LastHarvest = 0, LastBurnt = 0;
		
		public int countAge = 0, checkColour = 0; 
		
		private long Tick_counter = 0;
		
		private int Current_year = 2000, Month = 0; 
		
		private ISimulationContext Context;
		#endregion
		
		#region Init
        public void Init(PatchLayer layer)
        {
            _patches = layer; // store layer for access within agent class
			Position = Position.CreateGeoPosition(Longitude, Latitude);
            _patches.PatchEnvironment.Insert(this);
			Patch_Population = Convert.ToInt32(Area)*rand.Next(800, 3500); //800-3500 plants per hectre 
			Crop_YieldA = 0;
			Context = _patches.Context;
			Tick_counter = Context.CurrentTick; 
			DateTime date = (DateTime)Context.CurrentTimePoint;
			Month = date.Month; 
			Current_year = date.Year;
        }//intialise method
        #endregion
		
		#region Tick
        public void Tick()
        {
            //do something in every tick of the simulation
			//decide in every tick if the Patch is burnable or harvestable
			//Harvestable()and Burnable() will probably be public methods that indicate what the behaviour
			//of the fire and harvestor agent is 
			//fire and harvest acts upon a patch
			// crop yield and pop size is detmerined by plant agents
			//although - will also compare with the average calc eq
			DateTime date = (DateTime)Context.CurrentTimePoint;
			Month = date.Month; 
			Current_year = date.Year; 
			
			if(HarvestFireYear(Harvest_Data, Current_year))
				LastHarvest = Current_year; //definitely working as it should 
			
			if(HarvestFireYear(Fire_Data, Current_year))
				LastBurnt = Current_year;  
			
			if(Current_year != LastHarvest)
			{
				Crop_YieldA = 0.0; 
				Crop_YieldB = 0.0; 
			}
			if (Current_year == LastHarvest)
				Crop_YieldB = CalculateCropYieldEq(Patch_Population);
			
			Tick_counter = Context.CurrentTick; 
        }
		#endregion
		
		#region Methods
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
			if (data.Contains(year.ToString()) && year != 0)
				return true; 
			return false; 
		}
		#endregion
		
        private PatchLayer _patches { get; set; } // provides access to the main layer of this agent
		private Random rand = new Random(42); 
        public Guid ID { get; set; } // identifies the agent
		public Position Position{get; set;}
    }
}