Ivan Ho

All related project files are located in "Assets/HW 5 - RL Practice". Source code can be found in the "Scripts" 
folder.

ML Agents is utilized in the "ML Plane" scene. You can test the code by simply playing the scene and modifying 
the agent's model in the "Player" game object. Results about the training can be found in the "ML-Agents" folder.
the specific ML Agent can be found in "PlaneAgent.cs".

Tabular Q-Learning is utilized in the "RL Plane" scene. You can test the code by simply playing the scene and 
modifying the hyperparameters. Data is recorded in the "Assets/StreamingAssets/Data" folder, where cumulative 
rewards are recorded at the end of every episode. The specific RL Agent can be found in "QLearningAgent.cs" 
with the Q-Learning Table managed in "QLearningManager.cs".

Overall ML Agents performs much better and more efficiently than Q-Learning, as Plane Navigation contains 
continuous actions with floating point values. Q-Learning is much more suited for discrete actions with integer 
values, and as such is not as efficient as training with ML Agents. For the sake of simplicity, the states and 
actions are represented in "simplified values" (i.e. float --> int), but as such the accuracy of the result has 
been greatly compromised. Additionally, the Q-Learning algorithm is not as efficient as ML Agents, as it is not 
able to take advantage of the GPU to train the model. As such, the training time is much longer than ML Agents 
and is currently incomplete. As such, the training time is much longer than ML Agents and is currently incomplete
and the current data of Q-Learning does not display any convergence.

Notes
 - Training on both ML Agents and Q-Learning is extremely slow and thus the recorded data and models are 
   incomplete due to time constraints.
 - "results/Plane_01_Plane.csv"  contains graphs for ML Agents results.
 - "StreamingAssets/Data/PlaneNavigation-QLearning-15.csv" contains a graph for Q-Learning results.
