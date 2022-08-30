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
    ///     A simple astract layer that provides access to the rainfall data.
    /// </summary>
	
    public class PrecipitationLayer : AbstractLayer
    {
		public IAgentManager AgentManager { get; private set; }
		public GeoHashEnvironment<Precipitation> Rainfall { get; private set; }
		
        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
            UnregisterAgent unregisterAgentHandle)
        {
			
            base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too
			Rainfall = GeoHashEnvironment<Precipitation>.BuildByBBox( 24.107791,-33.681584, 24.207791, -33.745409, 35.0);
            AgentManager = layerInitData.Container.Resolve<IAgentManager>();
			AgentManager.Spawn<Precipitation, PrecipitationLayer>().ToList();
            return true;
        } //initlayer
		
    }//PatchLayer
}