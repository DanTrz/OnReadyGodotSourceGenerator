# OnReadyGodotSourceGenerator
 Generate code to Resolve Fields and Attributes to initiate variables on _Ready() similar to GDSCRIPT. This solution uses a Source Generator to create code.


This repo contains a simplified solution to resolve fields in Godot using C# by applying a [OnReady] attribute to the fields we want to load, similar to using @onready in GDScript. 

To resolve the Nodes, you can use one of the approaches below:

* Approach #1 - Using AutoLoaad  That should auto-resolve each node, as they are added to the SceneTree. This is done by following two steps:
    * - Step 1 => Make sure you add the script "GlobalAutoLoad" as an AutoLoad in Godot.
      - Step 2 => Use a decorator Root scene to instantiate the real main scene of the game. You should add the decorator root scene to your project Run > Main Scene.
            - this approach is needed due to a Godot Issue => @see  https://github.com/godotengine/godot/issues/37813
        Example on how your decorator Root Scene should look:
        ![image](https://github.com/user-attachments/assets/3e99e206-9820-42d9-92fc-0a80023a3453)


* Approach #2 - Manually add this code, "this.OnReady(this)" to each node within the _Ready() method.
         Example:
         ![image](https://github.com/user-attachments/assets/02c96839-d3d8-4a8c-9676-e0ec06ca247f)


To test this, you can get the entire code in this repo and there is a test Godot Game Project in the folder: [GodotGameSampleProject](https://github.com/DanTrz/OnReadyGodotSourceGenerator/tree/main/samplegodotproject_onreadysourcegenerator)

At the moment, the Godot Game Project needs to manually reference the Source Generator Project: [OnReadySourceGenerator](https://github.com/DanTrz/OnReadyGodotSourceGenerator/tree/main/OnReadySourceGenerator)
Example:
![image](https://github.com/user-attachments/assets/cc61bca9-1117-4c2d-92aa-787c8a3faa02)


If you downaload or get the entire code with all folders, it should all be setup for testing. I will create a NuGet Package in the future.

This solution was done using my own code and the approach is a simple one that only does one purpose.

This was inspired by other solutions from:

* Using Reflections from Romain Mouillard: https://medium.com/@romain.mouillard.fr/bringing-gdscripts-onready-magic-to-c-a-quick-custom-solution-5bae074ce799

* Using Source Generator from firebelley: https://github.com/firebelley/GodotUtilities
