# Unity Advanced Navmesh

An API based on the custom pathfinding code I keep having to write over and over. I thought it would be nice to 
have my base pathfinding code in one place, where other people can also benefit from it.

This is not a replacement for the Navmesh System, it just lets you work with it at a "lower level". 

You are probably still going to need a NavmeshAgent, but this system can help you be smarter about where your NavmeshAgent goes.

## What this does
Provides a API to help you do more advanced pathfinding such as:
- Working out what triangles in the navmesh have cover from the player
- Custom pathfinding such as flanking

The two most important classes are AdvancedNavmeshBase and NavmeshMetaDataProviderBase:
- AdvancedNavmeshBase - takes the triangulation of the scene and extracts a Octree of navmesh triangles you can query. Use queries based on axis-aligned bounding boxes to efficiently find things such as "what triangle is the player standing on?"
- NavmeshMetaDataProviderBase - lets you generate MetaData or "notes" about each Navmesh triangle e.g. whether player can see it

I've included an example scene to show how to use these types. 

WASD controls your character, the mouse pointer controls where your character looks.

There is a debug mesh to show you in real time what parts of the navmesh are "visible" to the player and which are not.

## What this does not do (yet)

I may add these features if people ask (or, even better, make a pull request):
- Off mesh links
- Multiple agent sizes
- Dynamic navmesh changes
- Crowd behaviours, braking etc

You may also suggest other features or contributions I didn't think of.

If you do add one of these features for your own game, consider contributing it to the repo for others to benefit.