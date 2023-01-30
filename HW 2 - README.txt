Ivan Ho

When running the project, select "Obstacle Avoidance" in the Main Menu to go to Assignment 2. 
Within that scene, you can move the prey around with arrow keys or WASD.

All C# scripts for this assignment can be found in "*.zip\Assets\HW 2 - Obstacle Avoidance". 
Behavior algorithms are in "ObstacleAvoidanceAgent.cs", where you can find the code for all the 
required behaviors. The following functions are used to implement these behaviors:
 - Ray Cast: AvoidWalls()
 - Cone Check and Collision Prediction: ConeCheck()

Keep in mind that the agent's (predator) dynamic behavior is not very intuitive. If it detects an 
obstacle it will react to it according to whether the obstacle is moving or not, then it will 
enter a "cooldown" period before it resumes seeking the prey. This is to prevent the agent from
reacting to the same obstacle over and over again.