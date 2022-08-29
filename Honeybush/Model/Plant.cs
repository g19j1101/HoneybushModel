using System;
using System.Linq;
using Mars.Common;
using Mars.Components.Layers;
using System.Collections.Generic;
using Mars.Common.Core;
using Mars.Interfaces;
using Mars.Interfaces.Data;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using Mars.Interfaces.Agents;

namespace Honeybush.Model
{
    /// <summary>
    ///  A simple agent stub that has an Init() method for initialization and a
    ///  Tick() method for acting in every tick of the simulation.
    /// </summary>
    public class Plant : IAgent<PatchLayer>, IPositionable
    {
		[PropertyDescription]
		public UnregisterAgent UnregisterHandle { get; set; }
		
		[PropertyDescription]
		public double adult_growth{get;set;}
		
		[PropertyDescription]
		public double seedling_growth{get;set;}
		
		public int Age; //@3 years since harvest -> can be harvested 
		public Position Position { get; set; }
		public string Stem_Colour, State; 
		private Random rand = new Random(42);
		private int Buds, Flowers; 
		
		public double Height;
		
		public int Patch_ID_plant{get; set;}  //potentiially give by the locality CSV   
		
		public int  Tick_counter = 0, Current_year = 2000;
		public int Month{get;set;} //only an output for now to see if it's working...it's not
		
		private PatchLayer _plants;
		
        public void Init(PatchLayer layer)
        {
			_plants = layer;
        }//intialise method? 
		
        public void Tick()
        {
			
			
            //do something in every tick of the simulation
			//depending on the month, call a phenophase method to update attributes
			//decide in every tick if the plant is burnable or harvestable
			//Harvestable()and Burnable() will probably be public methods that indicate what the behaviour
			//of the fire and harvestor agent is 
			
			//when start spawing more/1 patch -> put this in a for loop for 37 patches --> ensure all get inialsed
			if(Tick_counter == 0)
			{
				var init_patch = _plants.PatchEnvironment.Explore(Position, -1D, 1, agentInEnvironment
															=> agentInEnvironment.havePlants == false).FirstOrDefault();
				for(int i = 0; i < init_patch.Patch_Population; i++)
					SpawnAdult(init_patch); //it works!
				init_patch.havePlants = true; 
			}
			var patch = _plants.PatchEnvironment.Explore(Position, -1D, 1, agentInEnvironment
															=> agentInEnvironment.havePlants == true 
															&& agentInEnvironment.Patch_ID==Patch_ID_plant).FirstOrDefault();
			if (Age > 10)//need to build in decay factors in budding, flowering, etc. -> to add to this condition
						 // some bushes do live much longer than 10 years
			{	Die(patch); return;} //avoid further tick if dead 
			
			//does the state need to be updated?
			if (State == "seed" && Age > 5)
				State = "mature"; 
			switch(Month)
			{
				case 1:
				case 2:
				case 3: 
					Grow(patch);
					break;
				case 4:
				case 5: 
				case 6: 
				case 7:
				case 8:
					Bud();
					break;
				case 9:
				case 10: 
				case 11:
					Flower();
					break;
				case 12: 
					Set_Seed(patch);
					break;	
			}	
			checkHarvestable(patch);
			bool isHarvestable = Harvestable(patch);
			if (Harvestable(patch) && Current_year == patch.LastHarvest)
			{
				reduceBiomassAddYield(patch);
			}
			Tick_counter++; 
			if (Tick_counter % 4 == 0)
				Month++;
			if (Tick_counter % 52 == 0)
			{
				Age++; 
				Current_year++; 
			}
        }
		
		private void Die(Patch patch)
        {
            UnregisterHandle.Invoke(_plants, this);
			_plants.PlantEnvironment.Remove(this);
			patch.GetPopulationAltered(-1, Patch_ID_plant);
        }
		/*Much of these ranges comes from an analysis of field assessments.*/
		private void SpawnAdult(Patch patch)
		{
			_plants.AgentManager.Spawn<Plant, PatchLayer>(null, agent =>
			{
				agent.Patch_ID_plant = patch.Patch_ID;
				agent.Age = rand.Next(4, 6); //let it be in a similar range 
											       //  -> in the tick, harvest/fire data will change it via patch control
				agent.Height = rand.NextDouble() * (100 - 30) + 30;
				agent.State = "mature"; 
				agent.Position = Position.CreatePosition(patch.Longitude, patch.Latitude);
			}).Take(1).First();
			patch.GetPopulationAltered(1, Patch_ID_plant);
		} //SpawnAdult 
		
		private void SpawnSeed(Patch patch)
		{
			_plants.AgentManager.Spawn<Plant, PatchLayer>(null, agent =>
			{
				agent.Patch_ID_plant = patch.Patch_ID;
				agent.State = "seed";
				agent.Height = 0; 
				agent.Age = 0; 
				agent.Position = Position.CreatePosition(Position.X, Position.Y);
			}).Take(1).First(); 
			patch.GetPopulationAltered(1, Patch_ID_plant);
		} //SpawnSeed
		
		public void checkHarvestable(Patch patch)
		{
			
			if(Patch_ID_plant == patch.Patch_ID)
				patch.countAge += Age;
			if(Stem_Colour != "green")
				patch.checkColour += 1; 	
		}
		
		public bool Harvestable(Patch patch)
		{
			int aveAge = patch.countAge / patch.Patch_Population; 
			double percent = 0.75*patch.Patch_Population; 
			if(patch.checkColour >= Convert.ToInt32(percent) && aveAge >= 4 && Current_year - patch.LastHarvest >= 4 && Current_year - patch.LastBurnt>=5)
				return true;
			return false; 
		} 
		
		public void reduceBiomassAddYield(Patch patch)
		{
			if (Height > 40.0 && Stem_Colour != "green" && Patch_ID_plant == patch.Patch_ID)
			{
				double reduce = Height - 15.0; //cut above 15cm 
				Height -= reduce; 
				patch.Crop_YieldA += (rand.NextDouble()*(10 - 5) + 5) * Height; //cm to grams conversion 
			}
		}
		
		/*In Bud(), initialse #buds, stem colour + correspond this to height and age -> field assessment
		 *Essentially, a lot of randomness since this is how variable wild honeybush is. */
		private void getBudsAndColour(string[] colour, int maxBuds)
		{
			Buds = rand.Next(0, maxBuds/4); // divide by 4 to disperse budding over 4 months 
			if (Age > 8) {
				Stem_Colour = colour[3]; 
				Buds -= rand.Next(0, maxBuds);// old therefore will have few buds
			}
			Stem_Colour =  colour[rand.Next(1, 2)];
		}
		
		private void Bud()
		{
			string [] colour = new string[]{"green", "yellow", "orange", "brown"};
			
			if (Height > 30 && Height < 50) //small, mature plant
				getBudsAndColour(colour, 200);
			else if (Height > 50 && Height < 75) // medium mature plant
				getBudsAndColour(colour, 400);
			else if (Height > 75) // large mature plant
				getBudsAndColour(colour, 600);
			else
			{//is a seedling 
				if (State == "seed") {
					Stem_Colour = "green";
					Buds = 0; 
				}
			} 
		} //Bud()
		/*Get num of flowers from num of buds -> abortion rate*/
		private void Flower()
		{
			if (Buds < 0)
				Flowers = 0; //negative translates to no buds 
			Flowers = (int)0.9*Buds; //some buds won't flower 
		}
		
		/*num seeds from num flowers, spawn a seed, include abortion rate of 90% somewhere.
		 * note an abstraction -> skipped phase of forming pods 
		 * not necessary really as pods == flowers almost exactly always 
		 *1st strategy: spawn 10% of initial seeds, assume these will grow successfully, age is 0
		 *Abortion rate is something that can be tuned according to rainfall.*/
		private void Set_Seed(Patch patch)
		{
			int seeds = (int)(rand.NextDouble()*(0.2 - 0.1) + 0.1)*Flowers; 
			for(int i = 0; i < seeds/4; i++)
				SpawnSeed(patch); 
		}
		/*December to March: growth*/
		private void Grow(Patch patch)
		{
			// int growthFactor = precipatation;  // moisture_level;
			// double growth factor if theres been fire and rainfall
			// increase growth slightly if there's been harvest
			// std : low rainfall, no harvest , no fire 
			// increase if high rainfall 
			// Height += growthFactor; 
			int lastHarvestOrFire = 0; 
			int rain = 1; //for now -> still need to find a way of incorporating Precipitation agent 
			if (patch.LastHarvest > patch.LastBurnt)
				lastHarvestOrFire = patch.LastHarvest; //perhaps need to adust growth parameter
			else 
				lastHarvestOrFire = patch.LastBurnt; // better resprout rate if burnt
			if (State == "mature")
			{
				Height += adult_growth*(rain/(Current_year - lastHarvestOrFire));
			}
			else //seedling/seed 
			{
				Height += seedling_growth*Height;
			}
			
		}
		
		// private bool Burnable()
		// {
			
		// }
		
        public Guid ID { get; set; } // identifies the agent
    }
}