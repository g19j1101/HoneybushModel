using System;
using System.Linq;
using Mars.Interfaces;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace Honeybush.Model;

/// <summary>
///     A simple agent stub that has an Init() method for initialization and a
///     Tick() method for acting in every tick of the simulation.
/// </summary>
public class Plant : IAgent<PatchLayer>, IPositionable
{
	#region Attributes

    [PropertyDescription] public UnregisterAgent UnregisterHandle { get; set; }

    [PropertyDescription] public double adult_growth { get; set; } //growth rate parameter for a mature bush

    [PropertyDescription] public double seedling_growth { get; set; } //growth rate parameter for a seedling 
	
	[PropertyDescription] public PrecipitationLayer Precipitation { get; set; }

    public int Age, Decay, age_inc; //@4 years since harvest -> can be harvested 
    public Position Position { get; set; }
    public string Stem_Colour, State, MonthsToHarvest;
    private readonly Random rand = new(42); //seed the randomness 
    private int Buds, Flowers;
    private bool Harvested = false, Burnt = false;
    public double Height;

    public int Patch_ID_plant;

    public long Tick_counter;
    public int Current_year { get; set; }
    public int Month { get; set; } 

    private PatchLayer _plants;

    private ISimulationContext Context;
    
	public Guid ID { get; set; } // identifies the agent
    #endregion
	
    #region Init

    public void Init(PatchLayer layer)
    {
        _plants = layer;
		Position = _plants.FindRandomPlantPosition();
        Context = _plants.Context;
        Tick_counter = Context.CurrentTick;
        var date = (DateTime) Context.CurrentTimePoint; //GetValueorDefault
        Month = date.Month;
        Current_year = date.Year;
		age_inc = 0;
		_plants.PlantEnvironment.Insert(this); 
    } //intialise method

    #endregion

    #region Tick

    public void Tick()
    {
        //depending on the month, call a phenophase method to update attributes
        //decide in every tick if the plant is burnable or harvestable
        //affect patch's total population and crop yield 

        var date = (DateTime) Context.CurrentTimePoint;
        Month = date.Month;
        Current_year = date.Year;

        //when start spawing more/1 patch -> spawn 37 plants -> each one executes mass spawn for patch 
        if (Tick_counter == 0)
        {
            var init_patch = _plants.PatchEnvironment.Explore(Position, -1D, 1, agentInEnvironment
                => agentInEnvironment.havePlants == false).FirstOrDefault();
            for (var i = 0; i < init_patch.Patch_Population; i++) SpawnAdult(init_patch); //it works!
            init_patch.havePlants = true;
        }
		
		var patch = _plants.FindPatchForID(Patch_ID_plant); 
	
        if (Age > 10) //need to build in decay factors in budding, flowering, etc. -> to add to this condition
            // some bushes do live much longer than 10 years
        {
            Die(patch);
            return;
        } //avoid further tick if dead 

        //does the state need to be updated?
        if (State == "seed" && Age > 5)
            State = "mature";
		
		//reset age_inc so age can be incremented again 
		if(Month == 1 && age_inc > 0)
			age_inc = 0; 
		
        //does the Harvested flag need to be reset?
        if (Harvested && Current_year != patch.LastHarvest)
            Harvested = false;
		
		//does the Burnt flag need to be reset? 
		if(Burnt && Current_year != patch.LastBurnt)
			Burnt = false; 
		
        switch (Month)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                var precipitation = Precipitation.FindAgentForYear(Current_year);
                Grow(patch, precipitation);
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
				if (Age < 10 && State == "mature")
					Set_Seed(patch);
                //only increment age once 
                if (age_inc == 0)
                {
                    Age++;
                    age_inc++;
					Decay++; 
                }
                break;
            default:
                Console.WriteLine("error");
                break;
        }

		var moisture = Precipitation.FindAgentForYear(Current_year);
		if (Burnable(patch, moisture) && Burnt == false)//&& Month > 8 && Burnt == false)
		{ 
			reduceBiomassByFire(patch); 
			Console.WriteLine($"{patch.Patch_ID} has been burnt in {Current_year}");
			Burnt = true; 
			Decay--; 
		}
		
        if (Harvestable(patch) && Harvested == false && HarvestMonth() && patch.harvestCount > 0) //executes when deltaT = 1
        {
			//Console.WriteLine("will harvest"); 
            reduceBiomassAddYield(patch);
            Harvested = true;
			Decay++; 
			patch.harvestCount--; 
			//Console.WriteLine($"{patch.Patch_ID} has been harvested in {Current_year}");
        }
		
        Tick_counter = Context.CurrentTick;
    }

    #endregion

    #region Methods

    private void Die(Patch patch)
    {
		_plants.PlantEnvironment.Remove(this);
        UnregisterHandle.Invoke(_plants, this);
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
			age_inc = 0; 
            agent.Height = rand.NextDouble() * (150 - 30) + 30;
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
            agent.Height = 1;
            agent.Age = 0;
			age_inc = 0; 
            agent.Position = Position.CreatePosition(patch.Longitude, patch.Latitude);
        }).Take(1).First();
        patch.GetPopulationAltered(1, Patch_ID_plant);
    } //SpawnSeed

    public void reduceBiomassAddYield(Patch patch)
    {
        if (Height > 40.0 && Stem_Colour != "green" && State == "mature")
        {
            double reduce = Height - 15.0; //cut above 15cm 
            Height -= reduce;
            patch.Crop_YieldA += (0.45*reduce)*reduce*0.001;//((rand.NextDouble() * (10 - 5) + 5) * reduce)*0.001; //cm to grams conversion 
        }
    }
	
	public void reduceBiomassByFire(Patch patch)
	{
		Height = rand.Next(2,4); //almost zero height
		Age = 1;
	}
	
    /*In Bud(), initialse #buds, stem colour + correspond this to height and age -> field assessment
     *Essentially, a lot of randomness since this is how variable wild honeybush is. */
    private void getBudsAndColour(string[] colour, int maxBuds)
    {
        Buds = rand.Next(0, maxBuds / 4); // divide by 4 to disperse budding over 4 months 
        if (Age > 10)
        {
            Stem_Colour = colour[3];
            Buds -= rand.Next(maxBuds/2, maxBuds/4); // old therefore will have few buds
        }
        Stem_Colour = colour[rand.Next(1, 2)];
    }

    private void Bud()
    {
        string[] colour = {"green", "yellow", "orange", "brown"};

        if (Height > 30 && Height < 50) //small, mature plant
        {
            getBudsAndColour(colour, 200);
        }
        else if (Height > 50 && Height < 75) // medium mature plant
        {
            getBudsAndColour(colour, 400);
        }
        else if (Height > 75) // large mature plant
        {
            getBudsAndColour(colour, 600);
        }
        else
        {
            //is a seedling 
            if (State == "seed")
            {
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
        Flowers = Convert.ToInt32(0.9 * Buds); //some buds won't flower 
    }

    /*num seeds from num flowers, spawn a seed, include abortion rate of 90% somewhere.
     * note an abstraction -> skipped phase of forming pods 
     * not necessary really as pods == flowers almost exactly always 
     *1st strategy: spawn 10% of initial seeds, assume these will grow successfully, age is 0
     *Abortion rate is something that can be tuned according to rainfall.*/
    private void Set_Seed(Patch patch)
    {
        var seeds = 0.1 * Flowers;
		//Console.WriteLine(seeds); 
        for (int i = 0; i < Convert.ToInt32(seeds) / 4; i++)
            SpawnSeed(patch);
    }

    /*December to March: growth*/
    /*Source for logic and equation: Lucas et al. */
    private void Grow(Patch patch, Precipitation moisture)
    {
        // double growth factor if theres been fire and rainfall
        // increase growth slightly if there's been harvest
        // std : low rainfall, no harvest , no fire 
        // increase if high rainfall 
        // Height += growthFactor; 
        int lastHarvestOrFire = 0;
        int rain = Convert.ToInt32(moisture.Annual);
        if (patch.LastHarvest > patch.LastBurnt)
        {
            lastHarvestOrFire = patch.LastHarvest; //perhaps need to adust growth parameter
			adult_growth = 0.03;
			seedling_growth = 0.2; 
        }
        else
        {
            lastHarvestOrFire = patch.LastBurnt; // better resprout rate if burnt
            adult_growth += 0.2; //adjustment of growth parameter to actuate high resprout rate 
			if (State == "seed" && Age > 1)
				seedling_growth++;
        }  
		
		if (Age > 8 && Decay > 10)
			adult_growth = 0.01; //wont't grow as much
		
        if (State == "mature")
            Height += adult_growth*(rain / (12*(Current_year - lastHarvestOrFire)));
        else //seedling/seed 
            Height += seedling_growth * Height;
		//Console.WriteLine(Height); 
    }
	
	private bool Harvestable(Patch patch)
    {
        if (Current_year == patch.LastHarvest) 
		{
			patch.harvestCount = Convert.ToInt32(0.75*patch.Patch_Population); 
			return true;
		}
		
        if (Current_year > 2020)
        {
            _plants.checkHarvestable(patch); //every single plant in a patch needs to have done this before moving on 
            int aveAge = patch.countAge / patch.Patch_Population;
            var percent = 0.75 * patch.Patch_Population;
            if (aveAge >= 4 && patch.checkColour >= Convert.ToInt32(percent)
				&& Current_year - patch.LastHarvest >= 4 && Current_year - patch.LastBurnt >= 5)
            {
                Console.WriteLine($"This year ({Current_year}) is a good time to harvest patch {patch.Patch_ID} in Camp {patch.Camp}.");
				patch.LastHarvest = Current_year; 
				patch.harvestCount = Convert.ToInt32(0.75*patch.Patch_Population);
                return true;
            }
        }
// && aveAge >= 4 && 
        return false;
    }
	
	private bool HarvestMonth()
	{
		if (Current_year <= 2020)
			return true;
		else if (MonthsToHarvest.Contains(Month.ToString()))
			return true;
		return false; 
	}
	
    private bool Burnable(Patch patch, Precipitation moisture)
    {
        if (Current_year <= 2020)
        {
            if (Current_year == patch.LastBurnt)
                return true; //need to use history to get timeline correct 
        }
        else
        { //very simplistic condition, but is observable from data
            if (moisture.Annual < 550.0 && Current_year - patch.LastBurnt >= 4) 
			{
				patch.LastBurnt = Current_year; 
                return true;
			}
        }

        return false;
    }

    #endregion
}