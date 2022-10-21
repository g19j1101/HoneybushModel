using System;
using System.Linq;
using System.Collections.Generic;
using Mars.Common.Core.Collections;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Layers;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace Honeybush.Model;

public class PatchLayer : AbstractLayer
{
    private readonly int Height = 2;
    private readonly int Width = 2;
    public IAgentManager AgentManager { get; private set; }
    public SpatialHashEnvironment<Plant> PlantEnvironment { get; private set; }
    public GeoHashEnvironment<Patch> PatchEnvironment { get; private set; }
    public PrecipitationLayer PrecipitationLayer { get; set; }
	public List<Patch> Patches;
	public List<Plant> Plants; 
    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too

        // the agent manager can create agents and initializes them like defined in the sim config
        PatchEnvironment = GeoHashEnvironment<Patch>.BuildByBBox(24.107791, -33.681584, 24.207791, -33.745409, 35.0);
        PlantEnvironment = new SpatialHashEnvironment<Plant>(Height, Width);
        AgentManager = layerInitData.Container.Resolve<IAgentManager>();
        Patches = AgentManager.Spawn<Patch, PatchLayer>().ToList();
        Plants = AgentManager.Spawn<Plant, PatchLayer>().ToList();
        return true;
    } //initlayer
	
	//method to find a random position in the grid environment for plants
	public Position FindRandomPlantPosition()
	{
		var random = new Random();
		return Position.CreatePosition(random.Next(Width), random.Next(Height));
	}
	
	//find an appropriate patch -> used for intialisation
	public Patch FindPatchForID(int plant_id)
    {
		var target = Patches.Where(patch => patch.havePlants
               && patch.Patch_ID == plant_id).FirstOrDefault();
		if (target != null)
			return target;
        throw new ArgumentException($"No patch for ID {plant_id}");
    }
	
	//check that a patch is healthy and old enough overall
	public void checkHarvestable(Patch patch)
    {
		foreach (var plant in Plants)
		{
			if (plant.Patch_ID_plant == patch.Patch_ID)
			{
				patch.countAge += plant.Age;
				if (plant.Stem_Colour != "green" && plant.State == "mature")
					patch.checkColour += 1;
			}
		}
    }
}