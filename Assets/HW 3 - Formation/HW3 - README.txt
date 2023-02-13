Ivan Ho

When running the project, select "Formation" in the Main Menu to go to Assignment 3. You may need 
to scroll down to find the option. Within that scene, you can move the player (predator) around with 
arrow keys or WASD and change the number of agents or formation mode using the respective buttons. 
The player is located at the bottom left at the start of the scenario (white dot) and can collide 
with the agents as specified in the homework instructions. Changing the number of agents or the 
Formation Mode will reset the formation and the number of agents currently active in the scene.

All C# scripts for this assignment can be found in "*.zip\Assets\HW 3 - Formation\Scripts". 

FormationManager.cs
 - Manages the positions (slots) of each agents and the type of formation.
 - ScalableFormation()
   - Computes the destination of each agent such that they form a circle
     around the formation lead.
 - TwoLevelFormation()
   - Computes the destination of each agent such that they form a grid
	 formation around the formation lead.

FormationLead.cs
 - Controls the movement of the formation lead.
 - The formation lead moves along a fixed path and cannot interact with agents or obstacles.
 - FollowPath()
   - Moves the formation lead along a path of waypoints.
   - While in Scalable Formation, the lead moves at a constant speed that is slightly slower
     than the agents' max speed.
   - While in Two-Level Formation, the lead moves dynamically based on the average speed of the 
    agents and the distance between each agent and their formation destination.
 - ScalableFormation()
   - While in Scalable Formation, the lead will FollowPath() and check around itself for any 
     obstacles with a radius slightly larger than the formation's radius. If there is an obstacle, 
     the lead will shrink the formation and detection radius, and vise versa if there is no obstacle 
     (with a max radius).
 - TwoLevelFormation()
   - While in Two-Level Formation, the lead will simply follow the path as specified in 
     FollowPath().

FormationAgent.cs
 - Controls the movement of an agent.
 - While in Scalable Formation mode, the agent will seek its respective formation position/slot.
 - While in Two-Level Formation mode, the agent will seek its respective formation position/slot if 
   no obstacles are within its field of view, otherwise it will use obstacle avoidance to navigate 
   around the obstacle (or stay still if its formation slot is unreachable).
 - FollowFormation()
   - Moves to its respective formation position/slot. If the slot is unreachable (i.e. in an 
     obstacle), it moves to the nearest reachable point unless otherwise specified.
 - AvoidObstacles()
   - Raycasts in front and to both sides of the agent that collide with obstacles, and reacts 
     accordingly:
     1. If there is an obstacle in front and to both sides, the agent simply seeks the lead to 
        reduce idling as much as possible.
     2. If there is an obstacle in front and on one side, the agent steers in the direction in 
        which an obstacle was not found.
     3. If there is no obstacle in front but there is an obstacle to either side, the agent moves 
        straight ahead.
     4. If there is no obstacle at all, then the agent moves to its formation slot (this function 
        should never be called then).
 - CheckForObstacles()
   - Checks for any obstacles within the agent's field of view.
