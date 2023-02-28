Ivan Ho

When running the project, select "Pathfinding" in the Main Menu to go to Assignment 4. In the scene, you can move 
the camera around with WASD or arrow keys, click anywhere on the map to set the destination, and adjust values 
or change map using the UI elements on the right. 

The UI elements are as follows:
 - Map
   - Select the map to load
 - Heuristic
   - Select the heuristic to use
     - Distance: euclidean distance from node to target
     - ManhattanDistance: manhattan distance from node to target
 - Heuristic Weight
   - Adjust the weight of the heuristic (0-1). The greater the weight, the less impact the cost of the path has
     on the function cost.
 - Search Delay
   - Time delay between each search step (in seconds)

Scripts used in this assignment can be found in "*.zip\Assets\HW 4 - Pathfinding\Scripts". For A* implementation, 
look in "PlayerController.cs" and the FindPath() function.

Notes:
 - Heuristic functions are quite trivial as I could not come up with a good one within a reasonable amount of time.
   As a result, finding paths in long distances can be quite slow and inefficient.
 - Aside from the algorithm, the scene has not been optimized greatly, so expect frame drops and lots of lag when 
   loading large maps.
 - Changing maps while the agent is in the middle of pathfinding could potentially break future pathfinding. If 
   this happens, simply restart the scene (return to menu then back to the scene).