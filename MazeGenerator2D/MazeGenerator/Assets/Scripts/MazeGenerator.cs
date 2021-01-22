using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeGenerator : MonoBehaviour
{
    public bool draw = true;
    public int row = 10;
    public int column = 10;
    public int cellArea = 1;
    public int wallThickness = 1;
    public Transform unbreakablewalls, walls, Tile_regular, Tile_current;
    public class Cell
    {
        public bool Visited;
        public bool VisitedE, VisitedS, VisitedW, VisitedN;

        public Cell()  
        // Constructor, used to initialize a Cell, since at the start none of them are visited, we set them all to be false
        { 
            Visited = false;
            VisitedN = false;
            VisitedS = false;
            VisitedW = false;
            VisitedE = false;
        }
    }

    private Cell[] maze;  // maze is a collection of cells
    private int visitedCells = 1;  // Number of visited cells
    private Stack<ValueTuple<int, int>> mazeStack = new Stack<ValueTuple<int, int>>();
    private List<Transform> previousCurrent = new List<Transform>();
    private GameObject[] allTiles;
    Dictionary<Vector3, GameObject> posNobj = new Dictionary<Vector3, GameObject>();
    System.Random rnd = new System.Random();
    // Start is called before the first frame update
    void Start()
    {   
        maze = new Cell[row * column];
        for (int i = 0; i < maze.Length; i++)
            maze[i] = new Cell();

        int x = rnd.Next(row);
        int y = rnd.Next(column);

        mazeStack.Push(ValueTuple.Create(x, y));
        maze[y * row + x].Visited = true;


        Maze_Initialize();
        Maze_Generator_Combined();
        // allTiles = GameObject.FindGameObjectsWithTag("Tile");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Maze_Initialize()
    {
        int totalobjects = 0;  // record the number of objects in the initial maze
        int totalColumn = column * (wallThickness + 1);
        int totalRow = row * (wallThickness + 1);
        for (int x = 0; x < row; x = x+1)
        {
            for (int y = 0; y < column; y = y+1)
            {
                for (int wy = 0; wy < cellArea; wy = wy + 1)
                {
                    for (int wx = 0; wx < cellArea; wx = wx + 1)
                    {
                        // Generating Tiles
                        Vector3 v1 = new Vector3(x * (cellArea + wallThickness) + wx, 0, y * (cellArea + wallThickness) + wy);
                        Instantiate(Tile_regular, v1, Quaternion.identity);
                        totalobjects++;
                        for (int i = 0; i < wallThickness; i++)
                        {
                            Vector3 v2 = new Vector3(x * (cellArea + wallThickness) + wx, 0, y * (cellArea + wallThickness) + i + cellArea);
                            Instantiate(walls, v2, Quaternion.identity);
                            totalobjects++;
                            Vector3 v3 = new Vector3(x * (cellArea + wallThickness) + i + cellArea, 0, y * (cellArea + wallThickness) + wx);
                            Instantiate(walls, v3, Quaternion.identity);
                            totalobjects++;
                        }
                    }
                }
            }
        }

        //Fill in corner
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
                    Instantiate(walls, new Vector3(currenty, 0, currentx), Quaternion.identity);
                    totalobjects++;
                }
            }
            
        }

        //Fill in sides
        for (int i = 0; i < wallThickness; i++)
        {
            for (int j = 0; j < row * (wallThickness + 1); j++)
            {
                Instantiate(walls, new Vector3(j, 0, -1 - i), Quaternion.identity);
                totalobjects++;
            }
        }
        for (int i = 0; i < wallThickness; i++)
        {
            for (int j = 0; j < wallThickness + column * (wallThickness + 1); j++)
            {
                Instantiate(walls, new Vector3(-1-i, 0, j-wallThickness), Quaternion.identity);
                totalobjects++;
            }
        }

        // label each object in initial maze with their vector3;
        allTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject t in allTiles)
            posNobj.Add(t.transform.position, t);
    }
    private void Draw()
    {
        for (int x = 0; x < row; x++)
        {
             for (int y = 0; y < column; y++)
             {
                for (int p = 0; p < cellArea; p++)
                {
                    Vector3 v3 = new Vector3(x * 2 , 0, y * 2 + 1);
                    Vector3 v3_1 = new Vector3(x * 2 + 1, 0, y * 2);
                    if (maze[y * row + x].VisitedS)
                    {
                         //DestroyTile(v3);
                        Destroy(posNobj[v3]);
                        Instantiate(Tile_regular, v3, Quaternion.identity);
                    }
                    if (maze[y * row + x].VisitedE)
                    {
                         //DestroyTile(v3_1);
                        Destroy(posNobj[v3_1]);
                        Instantiate(Tile_regular, v3_1, Quaternion.identity);
                    }
                }
             }
        }
    }
    private void Maze_Generator_Combined()
    {
        Func<int, int, uint> lookAt = (px, py) => (uint)((mazeStack.Peek().Item1 + px) + (mazeStack.Peek().Item2 + py) * row);
        while (visitedCells < row * column)
        {
            List<int> neighbors = new List<int>();
            // North neighbor
            if (mazeStack.Peek().Item2 > 0 && maze[lookAt(0, -1)].Visited == false)
                neighbors.Add(0); // meaning the north neighbour exists and unvisited
            // East neighbor
            if (mazeStack.Peek().Item1 < row - 1 && maze[lookAt(+1, 0)].Visited == false)
                neighbors.Add(1);
            // South neighbor
            if (mazeStack.Peek().Item2 < column - 1 && maze[lookAt(0, +1)].Visited == false)
                neighbors.Add(2);
            // West neighbor
            if (mazeStack.Peek().Item1 > 0 && maze[lookAt(-1, 0)].Visited == false)
                neighbors.Add(3);

            if (neighbors.Count > 0)
            {
                int nextCellDir = neighbors[rnd.Next(neighbors.Count)];
                switch (nextCellDir)
                {
                    case 0: // North
                        maze[lookAt(0, -1)].Visited = true;
                        maze[lookAt(0, -1)].VisitedS = true;
                        // maze[lookAt(0, 0)].VisitedN = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 0), (mazeStack.Peek().Item2 - 1)));
                        break;

                    case 1: // East
                        maze[lookAt(+1, 0)].Visited = true;
                        //maze[lookAt(+1, 0)].VisitedW = true;
                        maze[lookAt(0, 0)].VisitedE = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 1), (mazeStack.Peek().Item2 + 0)));
                        break;

                    case 2: // South
                        maze[lookAt(0, +1)].Visited = true;
                        //maze[lookAt(0, +1)].VisitedN = true;
                        maze[lookAt(0, 0)].VisitedS = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 + 0), (mazeStack.Peek().Item2 + 1)));
                        break;

                    case 3: // West
                        maze[lookAt(-1, 0)].Visited = true;
                        maze[lookAt(-1, 0)].VisitedE = true;
                        //maze[lookAt(0, 0)].VisitedW = true;
                        mazeStack.Push(ValueTuple.Create((mazeStack.Peek().Item1 - 1), (mazeStack.Peek().Item2 + 0)));
                        break;
                }
                visitedCells++;
            }
            else
            {
                mazeStack.Pop();
            }
        }
        Draw();
    }
}
