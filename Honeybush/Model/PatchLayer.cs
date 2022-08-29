using System;
using System.Linq;
using Mars.Common.Core.Collections;
using Mars.Common.Core.Random;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using Mars.Numerics.Statistics;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace Honeybush.Model
{
    /// <summary>
    ///     A simple geo layer that provides access to the x,y values of a csv input.
    /// </summary>
	// [PropertyDescription]
    // public PrecipitationLayer PrecipitationLayer { get; set; }
	//investigate the above
	
    public class PatchLayer : AbstractLayer
    {
		public IAgentManager AgentManager { get; private set; }
		public SpatialHashEnvironment<Plant> PlantEnvironment { get; private set; }
		public GeoHashEnvironment<Patch> PatchEnvironment { get; private set; }
		public PrecipitationLayer PrecipitationLayer { get; set; }
		
		private int Height = 1, Width = 1; 
		
        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
            UnregisterAgent unregisterAgentHandle)
        {
			
            base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too

            // the agent manager can create agents and initializes them like defined in the sim config
			PatchEnvironment = GeoHashEnvironment<Patch>.BuildByBBox( 24.107791,-33.681584, 24.207791, -33.745409, 35.0);
			PlantEnvironment = new SpatialHashEnvironment<Plant>(Height,Width);
			
            AgentManager = layerInitData.Container.Resolve<IAgentManager>();
			AgentManager.Spawn<Patch, PatchLayer>().ToList();
			AgentManager.Spawn<Plant, PatchLayer>().ToList();
            return true;
        } //initlayer
		
		// public Position FindRandomPlantPosition()
        // {
            // var random = RandomHelper.Random;
            // return Position.CreatePosition(random.Next(Width), random.Next(Height));
        // }
    }//PatchLayer
}