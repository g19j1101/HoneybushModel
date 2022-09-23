using System;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace Honeybush.Model;

/// <summary>
///     A simple agent stub that has an Init() method for initialization and a
///     Tick() method for acting in every tick of the simulation.
/// </summary>
public class Precipitation : IAgent<PrecipitationLayer>, IPositionable
{
    [PropertyDescription] public int Year { get; set; }

    [PropertyDescription] public double Annual { get; set; }

    public PrecipitationLayer _rain { get; set; } // provides access to the main layer of this agent

    public void Init(PrecipitationLayer layer)
    {
        _rain = layer; // store layer for access within agent class
    } //intialise method

    public void Tick()
    {
        //do nothing
        //Precipitation agent is just a data holder
    }

    public Guid ID { get; set; } // identifies the agent
    public Position Position { get; set; }
}