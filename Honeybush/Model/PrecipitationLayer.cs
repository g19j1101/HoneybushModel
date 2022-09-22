using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Components.Agents;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;

namespace Honeybush.Model;

/// <summary>
///     A simple astract layer that provides access to the rainfall data.
/// </summary>
public class PrecipitationLayer : AbstractLayer
{
    public IAgentManager AgentManager { get; private set; }

    public List<Precipitation> Agents;
    public GeoHashEnvironment<Precipitation> Rainfall { get; set; }

    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too
        Rainfall = GeoHashEnvironment<Precipitation>.BuildByBBox(24.107791, -33.681584, 24.207791, -33.745409, 35.0);
        
        AgentManager = layerInitData.Container.Resolve<IAgentManager>();
        Agents = AgentManager.Spawn<Precipitation, PrecipitationLayer>().ToList();

        return true;
    }

    public Precipitation FindAgentForYear(int year)
    {
        foreach (var precipitation in Agents)
        {
            if(precipitation.Year == year)
            {
                return precipitation;
            }
        }

        throw new ArgumentException($"No precipitation data for year {year}");
    }
    
} //PrecipitationLayer