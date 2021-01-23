using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeGenerator3D : MonoBehaviour
{
    public bool withWalls;
    public int row = 10;
    public int column = 10;
    public int height = 10;
    public Transform Wall, Tile;

    public int wallThickness = 1;
    public class Cell
    {
        public bool Visited;
        public bool VisitedE, VisitedS, VisitedW, VisitedN, VisitedUp, VisitedDown;

        public Cell()
        // Constructor, used to initialize a Cell, since at the start none of them are visited, we set them all to be false
        {
            Visited = false;
            VisitedN = false;
            VisitedS = false;
            VisitedW = false;
            VisitedE = false;
            VisitedUp = false;
            VisitedDown = false;
        }
    }
    private int cellArea = 1;
    private Cell[] maze;  // maze is a collection of cells
    private int visitedCells = 1;  // Number of visited cells
    private Stack<ValueTuple<int, int, int>> mazeStack = new Stack<ValueTuple<int, int, int>>();
    private GameObject[] allTiles;
    Dictionary<Vector3, GameObject> posNobj = new Dictionary<Vector3, GameObject>();
    System.Random rnd = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        maze = new Cell[row * column * height];
        for (int i = 0; i < maze.Length; i++)
            maze[i] = new Cell();
        int x = rnd.Next(row);
        int y = rnd.Next(column);
        int z = rnd.Next(height);
        mazeStack.Push(ValueTuple.Create(x, y, z));
        maze[z * column * row + y * row + x].Visited = true;
        if (withWalls)
            Maze_Initialize();
        else
            Maze_Initialize_noWalls();
        Maze_Generator_Combined();

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Maze_Initialize()
    {
        int totalColumn = column * (wallThickness + 1);
        int totalRow = row * (wallThickness + 1);
        int totalHeight = height * (wallThickness + 1);
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < row; x = x + 1)
            {
                for (int y = 0; y < column; y = y + 1)
                {
                    for (int wy = 0; wy < cellArea; wy = wy + 1)
                    {
                        for (int wx = 0; wx < cellArea; wx = wx + 1)
                        {
                            // Generating Tiles
                            Vector3 v1 = new Vector3(x * (cellArea + wallThickness), z * (cellArea + wallThickness), y * (cellArea + wallThickness));
                            Instantiate(Tile, v1, Quaternion.identity);
                            for (int i = 0; i < wallThickness; i++)
                            {
                                Vector3 v2 = new Vector3(x * (cellArea + wallThickness), z * (cellArea + wallThickness), y * (cellArea + wallThickness) + i + cellArea);
                                Instantiate(Wall, v2, Quaternion.identity);
                                Vector3 v3 = new Vector3(x * (cellArea + wallThickness) + i + cellArea, z * (cellArea + wallThickness), y * (cellArea + wallThickness));
                                Instantiate(Wall, v3, Quaternion.identity);
                                Vector3 v1_up = new Vector3(x * (cellArea + wallThickness), z * (cellArea + wallThickness) + i + cellArea, y * (cellArea + wallThickness));
                                Instantiate(Wall, v1_up, Quaternion.identity);
                                for (int k = 0; k < wallThickness; k++)
                                {
                                    Vector3 v_up = new Vector3(x * (cellArea + wallThickness) + k + 1, z * (cellArea + wallThickness) + i + 1, y * (cellArea + wallThickness));
                                    Instantiate(Wall, v_up, Quaternion.identity);
                                    Vector3 v_up2 = new Vector3(x * (cellArea + wallThickness), z * (cellArea + wallThickness) + i + 1, y * (cellArea + wallThickness) + k + 1);
                                    Instantiate(Wall, v_up2, Quaternion.identity);
                                }
                            }
                        }
                    }
                }
            }
        }
        // Fill in corner
        for (int k = 0; k < totalHeight; k++)
        {
            int currentx = 0;
            for (int i = 0; i < totalColumn; i++)
            {
                currentx++;
                int currenty = 0;
                for (int j = 0; j < totalRow; j++)
                {
                    currenty++;
                    if (currenty % (wallThickness + 1) != 0 && currentx % (wallThickness + 1) != 0)
                    {
                        Instantiate(Wall, new Vector3(currenty, k, currentx), Quaternion.identity);
                    }
                }
            }
        }
        //Fill in sides
        for (int k = 0; k < height * (wallThickness + 1); k++)  // Fillin Lower Surface
        {
            for (int i = 0; i < wallThickness; i++)  // TODO: Revise this for wall thickness consideration
            {
                for (int j = 0; j < row * (wallThickness + 1); j++)
                {
                    Instantiate(Wall, new Vector3(j, k, -1 - i), Quaternion.identity);
                }
            }
        }

        for (int k = 0; k < height * (wallThickness + 1); k++)  // Fill in Left Surface
        {
            for (int i = 0; i < wallThickness; i++)  // TODO: Revise this for wall thickness consideration
            {
                for (int j = -wallThickness; j < column * (wallThickness + 1); j++)
                {
                    Instantiate(Wall, new Vector3(-1 - i, k, j), Quaternion.identity);
                }
            }
        }


        for (int i = 0; i < wallThickness; i++)  // Fill the underground level
        {
            for (int k = 0; k < wallThickness + row * (wallThickness + 1); k++)
            {
                for (int j = 0; j < wallThickness + column * (wallThickness + 1); j++)
                {
                    Instantiate(Wall, new Vector3(k - wallThickness, i - wallThickness, j - wallThickness), Quaternion.identity);
                }
            }

        }
        //label each object in initial maze with their vector3;
        allTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject t in allTiles)
            posNobj.Add(t.transform.position, t);
    }
    private void Maze_Generator_Combined()
    {
        Func<int, int, int, int> lookAt = (px, py, pz) => ((mazeStack.Peek().Item1 + px) + (mazeStack.Peek().Item2 + py) * row) + ((mazeStack.Peek().Item3 + pz) * row * column);
        while (visitedCells < row * column * height)
        {
            List<int> neighbors = new List<int>();
            // North neighbor
            if (mazeStack.Peek().Item2 > 0 && maze[lookAt(0, -1, 0)].Visited == false)
                neighbors.Add(0); // meaning the north neighbour exists and unvisited
            // East neighbor
            if (mazeStack.Peek().Item1 < row - 1 && maze[lookAt(+1, 0, 0)].Visited == false)
                neighbors.Add(1);
            // South neighbor
            if (mazeStack.Peek().Item2 < column - 1 && maze[lookAt(0, +1, 0)].Visited == false)
                neighbors.Add(2);
            // West neighbor
            if (mazeStack.Peek().Item1 > 0 && maze[lookAt(-1, 0, 0)].Visited == false)
                neighbors.Add(3);
            // Up neighbor
            if (mazeStack.Peek().Item3 < height - 1 && maze[lookAt(0, 0, +1)].Visited == false)
                neighbors.Add(4);
            // Down neighbor
            if (mazeStack.Peek().Item3 > 0 && maze[lookAt(0, 0, -1)].Visited == false)
                neighbors.Add(5);
            if (neighbors.Count > 0)
            {
                int nextCellDir = neighbors[rnd.Next(neighbors.Count)];
                switch (nextCellDir)
                {
                    case 0: // North
                        maze[lookAt(0, -1, 0)].Visited = true;
                        maze[lookAt(0, -1, 0)].VisitedS = true;
                        maze[lookAt(0, 0, 0)].VisitedN = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 0), (mazeStack.Peek().Item2 - 1), (mazeStack.Peek().Item3 + 0)));
                        break;

                    case 1: // East
                        maze[lookAt(+1, 0, 0)].Visited = true;
                        maze[lookAt(+1, 0, 0)].VisitedW = true;
                        maze[lookAt(0, 0, 0)].VisitedE = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 1), (mazeStack.Peek().Item2 + 0), (mazeStack.Peek().Item3 + 0)));
                        break;

                    case 2: // South
                        maze[lookAt(0, +1, 0)].Visited = true;
                        maze[lookAt(0, +1, 0)].VisitedN = true;
                        maze[lookAt(0, 0, 0)].VisitedS = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 0), (mazeStack.Peek().Item2 + 1), (mazeStack.Peek().Item3 + 0)));
                        break;

                    case 3: // West
                        maze[lookAt(-1, 0, 0)].Visited = true;
                        maze[lookAt(-1, 0, 0)].VisitedE = true;
                        maze[lookAt(0, 0, 0)].VisitedW = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 - 1), (mazeStack.Peek().Item2 + 0), (mazeStack.Peek().Item3 + 0)));
                        break;
                    case 4: // Up
                        maze[lookAt(0, 0, +1)].Visited = true;
                        maze[lookAt(0, 0, +1)].VisitedDown = true;
                        maze[lookAt(0, 0, 0)].VisitedUp = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 0), (mazeStack.Peek().Item2 + 0), (mazeStack.Peek().Item3 + 1)));
                        break;
                    case 5: // Down
                        maze[lookAt(0, 0, -1)].Visited = true;
                        maze[lookAt(0, 0, -1)].VisitedUp = true;
                        maze[lookAt(0, 0, 0)].VisitedDown = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 0), (mazeStack.Peek().Item2 + 0), (mazeStack.Peek().Item3 - 1)));
                        break;
                }
                visitedCells++;
            }
            else
            {
                mazeStack.Pop();
            }
        }
        if (withWalls)
            Draw();
        else
            Draw_noWalls();
    }
    private void Draw()
    {
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < column; y++)
                {
                    for (int p = 0; p < 1; p++)
                    {
                        if (maze[z * column * row + y * row + x].VisitedS)
                        {
                            for (int i = 0; i < wallThickness; i++)// Destroy all the walls on that direction
                            {
                                Vector3 v3 = new Vector3(x * 2 + (wallThickness - 1) * x, z * 2 + (wallThickness - 1) * z, y * 2 + 1 + i + (wallThickness - 1) * y);
                                Destroy(posNobj[v3]);
                                Instantiate(Tile, v3, Quaternion.identity);
                            }

                        }
                        if (maze[z * column * row + y * row + x].VisitedE)
                        {
                            for (int i = 0; i < wallThickness; i++)
                            {
                                Vector3 v3_1 = new Vector3(x * 2 + 1 + i + (wallThickness - 1) * x, z * 2 + (wallThickness - 1) * z, y * 2 + (wallThickness - 1) * y);
                                Destroy(posNobj[v3_1]);
                                Instantiate(Tile, v3_1, Quaternion.identity);
                            }
                        }
                        if (maze[z * column * row + y * row + x].VisitedUp)
                        {
                            for (int i = 0; i < wallThickness; i++)
                            {
                                Vector3 v3_2 = new Vector3(x * 2 + (wallThickness - 1) * x, z * 2 + 1 + i + (wallThickness - 1) * z, y * 2 + (wallThickness - 1) * y);
                                Destroy(posNobj[v3_2]);
                                Instantiate(Tile, v3_2, Quaternion.identity);
                            };
                        }
                    }
                }
            }
        }

    }
    private void Maze_Initialize_noWalls()
    {
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < row; x = x + 1)
            {
                for (int y = 0; y < column; y = y + 1)
                {
                    // Generating Tiles
                    Vector3 v1 = new Vector3(x * (cellArea + wallThickness), z * (cellArea + wallThickness), y * (cellArea + wallThickness));
                    Instantiate(Tile, v1, Quaternion.identity);

                }
            }
        }
    }
    private void Draw_noWalls()
    {
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < column; y++)
                {
                    for (int p = 0; p < 1; p++)
                    {
                        if (maze[z * column * row + y * row + x].VisitedS)
                        {
                            for (int i = 0; i < wallThickness; i++)// Destroy all the walls on that direction
                            {
                                Vector3 v3 = new Vector3(x * 2 + (wallThickness - 1) * x, z * 2 + (wallThickness - 1) * z, y * 2 + 1 + i + (wallThickness - 1) * y);
                                Instantiate(Tile, v3, Quaternion.identity);
                            }

                        }
                        if (maze[z * column * row + y * row + x].VisitedE)
                        {
                            for (int i = 0; i < wallThickness; i++)
                            {
                                Vector3 v3_1 = new Vector3(x * 2 + 1 + i + (wallThickness - 1) * x, z * 2 + (wallThickness - 1) * z, y * 2 + (wallThickness - 1) * y);
                                Instantiate(Tile, v3_1, Quaternion.identity);
                            }
                        }
                        if (maze[z * column * row + y * row + x].VisitedUp)
                        {
                            for (int i = 0; i < wallThickness; i++)
                            {
                                Vector3 v3_2 = new Vector3(x * 2 + (wallThickness - 1) * x, z * 2 + 1 + i + (wallThickness - 1) * z, y * 2 + (wallThickness - 1) * y);
                                Instantiate(Tile, v3_2, Quaternion.identity);
                            };
                        }
                    }
                }
            }
        }
    }
}
