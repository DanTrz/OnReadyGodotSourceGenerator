# OnReadyGodotSourceGenerator
 Generate code to Resolve Fields and Attributes to initiate variables on _Ready() similar to GDSCRIPT. This solution uses a Source Generator to create code.


This repo contains a simplified solution to resolve fields in Godot using C# by applying a [OnReady] attribute to the fields we want to load, similar to using @onready in GDScript. 

To resolve the Nodes, you can use one of the approaches below:

* Approach #1 - Using AutoLoaad  That should auto-resolve each node, as they are added to the SceneTree. This is done by 




Example:
![image](https://github.com/user-attachments/assets/02c96839-d3d8-4a8c-9676-e0ec06ca247f)



This solution was done using my own code and the approach is a simple one that only does one purpose.

This was inspired by other solutions from:

* Using Reflections from Romain Mouillard: https://medium.com/@romain.mouillard.fr/bringing-gdscripts-onready-magic-to-c-a-quick-custom-solution-5bae074ce799

* Using Source Generator from firebelley: https://github.com/firebelley/GodotUtilities
