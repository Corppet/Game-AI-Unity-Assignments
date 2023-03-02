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

World Representation
 - The map is a 1-to-1 tile-based representation of the map in the text file (one char in the map file is a tile in the world).
 - Corners graphs are used as the waypoint representation for pathfinding. If a normal ground tile has an out-of-bounds 
   or obstacle tile, it is considered a corner and a node is created at that point.
 - A node will be connected to another node if they have line of sight with each other (raycast to other node, and check if it 
   collides with anything else) obstacles and out-of-bounds block lines of sight.
 - Obstacles and Out-of-Bounds tiles are effectively treated the same way. The agent will try to avoid these tiles by 
   pathfinding around them.
 - Different maps or map sizes may warrant a change in the waypoint representation, if the map has a lot of open space and not
   a lot of corners. Although corner graphs are effective and greatly reduces the number of waypoints in a world representation,
   it does not allow the agent to utilize the open space in the map, and also does not provide optimal pathfinding in the beginning
   and end of the path (as the agent would first be pathing towards a corner, then pathing towards the destination).

Limitations
 - A path must exist between the agent and the destination. If the destination is unreachable, the agent will search almost the 
   entire map before giving up (which will taka a long time).
 - Waypoint representation does not allow the agent to utilize open space in the map. This can be fixed by using a different 
   waypoint representation, such as a grid-based representation.

Advantages
 - There are no waypoints in open spaces (except for start and destination) so the graph-represenation is much smaller and simpler 
   than typical graph representations (i.e. grid-based).
 - The agent will follow the path exactly, as there are no waypoints in open spaces. This is a good thing as the agent will not 
   deviate from the path, and will not get stuck in corners.

Notes:
 - Heuristic functions are quite trivial as I could not come up with a good one within a reasonable amount of time.
   As a result, finding paths in long distances can be quite slow and inefficient.
 - Aside from the algorithm, the scene has not been optimized greatly, so expect frame drops and lots of lag when 
   loading large maps.
 - Changing maps while the agent is in the middle of pathfinding could potentially break future pathfinding. If 
   this happens, simply restart the scene (return to menu then back to the scene).