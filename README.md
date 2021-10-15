# QuakeNavSharp

A .NET Core library to handle Quake 2021 (Enhanced) bot navigation files.

## Features

* **.nav file support**: Manipulate .nav files directly if you wish with a 1:1 representation of the binary structures. Supports reading and writing.
* **OOP navigation graph representation**: Edit, view and create the navigation graph with references instead of ids and logical hierarchy.
* **NavJson support**: Convert navigation graphs from and to NavJson format. A git friendly format that represents a navigation graph.
* **Multiple version**: Supports NAV 14 and 15


## Quickstart
**Nuget**:  
[![NuGet](https://img.shields.io/nuget/dt/QuakeNavSharp.svg)](https://www.nuget.org/packages/QuakeNavSharp/)  
`Install-Package QuakeNavSharp`

## Usage example

```csharp
// Loading a nav file
NavFile navFile;
using(var fs = new FileStream("mymap.nav",FileMode.Open))
    navFile = await NavFile.LoadAsync(fs);

// Convert it to a navigation graph
var graph = navFile.ToNavigationGraph();

// Adding a new node
var node1 = graph.NewNode();
node1.Origin = new Vector3(100,100,100);

// Adding a link to another node
var node2 = graph.NewNode();
var linkToNode1 = node2.NewLink();
linkToNode1.Target = node1;

// Save a .nav file with the modified nodes
using(var fs = new FileStream("mymap.nav",FileMode.Create))
    graph.ToNavFile().SaveAsync(fs);

// Convert the graph to NavJson json
var json = graph.ToNavJson().ToJson();

// Load any NAV version
using(var fs = new FileStream("mymap_v14.nav",FileMode.Open))
  graph = NavFileUtils.LoadAnyVersionAsync(fs);
```


## NavJson

NavJson is a JSON representation of the navigation graph. It was created with Git in mind to allow incremental changes to bot navigation in plain text and easily diff'ed. 

It is closer to the OOP navigation graph than the binary structures in terms of look.

The main difference is that Links are connected via Node coordinates instead of Node Ids. This allows nodes to be deleted without having to apply a change to every other node / link with an id above it.

It also includes some information about the map, contributors and possible urls where to download a map from.

Here's a snippet:
```json
{
  "Version": 2,
  "Map": {
    "Name": "The Slipgate Complex",
    "Author": "John Romero",
    "Filename": "e1m1",
    "Urls": []
  },
  "Contributors": [
      "JPiolho"
  ],
  "Comments": "Deathmatch",
  "Nodes": [
	{
      "Origin": [684.62695,104.976776,48.53125],
      "Flags": 0,
      "Links": [
        {
          "Target": [607.29736,129.47595,16.531242],
          "Type": 0
        },
        {
          "Target": [688.049,-3.7063046,48.53125
          ],
          "Type": 0,
          "Edict": {
            "Mins": [719,31,47],
            "Maxs": [785,49,113],
            "EntityId": -34
          }
        },
        {
          "Target": [681.66376,180.51585,48.53125],
          "Type": 0
        }
      ],
      "Radius": 20
    },
    ...
```
(Unfortunately it doesn't look that pretty due to limitations from System.Text.Json serializer, but feel free to roll your own if you wish!)
