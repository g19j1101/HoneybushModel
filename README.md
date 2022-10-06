# Documentation for HoneybushModel

This ABM attempts to model the growing cycle of honeybush (specifically the bergtee species) in the Kouga Mountains at a Melmont Farm, Nooitgedacht. 

## Project structure

Below is a brief description of each component of the project structure:

- `Program.cs`: the entry point of the model from which the model can be started. 
- `config.json`: a JavaScript Object Notation (JSON) with which the model can be configured to run certain scenarios. 
- `Model`: a directory that holds the agent types, layer types, entity types, and other classes of the model. 
- `Resources`: a directory that holds the external resources, such as data csv files, of the model that are integrated into the model at runtime. 

## Model description

The model consists of the following agent types and layer types:

- Agent types:
  - `Patch`: an agent that manages certain attributes and crop yield values, restting them, updating or outputting them in a harvest year. 
  - `Plant`: an agent that remains static but grows, buds, flowers, sets seed and decides if it is harvestable or burnable.
  - `Precipitation`: an agent that is implemented only for technical reasons. It simply holds annual rainfall values from a timeseries csv.
- Layer types:
  - `PatchLayer`: the layer on which the `Plant` and `Patch` agents live and behave.
- Other classes:
  - `PrecipitationLayer`: a layer that `Precipitation` lives on. This layer also provides the ability to query rainfall values for a particular year.

## Model configuration

The model can be configured via a JavaScript Object Notation (JSON) file called `config.json`. Below are some of the main configurable parameters:

- `startTime` and `endTime`: the start time and end time, respectively, of the simulation
- `deltaT`: the length of a single time step. The simulation time is given by the number of `deltaT` time steps that fit into the range defined by `startTime` and `endTime`
- `console`: a boolean flag that, if set to `true`, prompts the simulation framework to send output a progress bar.
- `agents`: the agent types that should be included in the simulation
  - The number of agents can be changed here by updating the value of the `count` key of each agent type. 
  - This should not be changed as 44 patches will always be simulated as there will always be at least 44 patches with relevant data in this model.
  - This is also where certain parameters of an agent are given values. 

## Model setup and execution

The following tools are required on your machine to run a full simulation of this model:

- A C# Interactive Development Environment (IDE), or any other text editer. 
- [.NET Core](https://dotnet.microsoft.com/en-us/download) 6.0 or higher

To set up and run the simulation, please follow these steps:

1. Open a terminal.
2. Navigate into the HoneybushModel/Honeybush folder.
3. Type the command: 'dotnet run -project Honeybush.csproj'
4. Press enter. 
