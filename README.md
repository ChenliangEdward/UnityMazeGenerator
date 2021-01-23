# 2D/3D Maze Generator

![alt text](https://github.com/ChenliangEdward/UnityMazeGenerator/blob/main/Pics/3DMazeExample.png?raw=true)
![alt text](https://github.com/ChenliangEdward/UnityMazeGenerator/blob/main/Pics/WallthicknessAdjustmentexample.png?raw=true)
This algorithm generates maze on Unity3D and can be used to provide some inspirations on Level Design. Maze generated with this algorithm ensures that there is one and only one path between two reachable points.

Special thanks to YuzhouGuo for providing her  [Maze Generator Algorithm](https://github.com/YuzhouGuo/MazeGeneration_RecursiveBacktracking), this repo contains an improved version for generating a maze.

# New Features
  - Improved Algorithm complexity from O(n^4) to O(n^2), so larger maze generation is possible.
  - Added 3d feature, you can generate maze in 3 directions now.
  - You can now adjust the thickness of the wall.
  - You can hide the walls by unchecking the "With Wall" checkbox.
