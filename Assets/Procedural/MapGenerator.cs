using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Room generation parameters
    public int roomQuantity = 1;
    public float scale = 1.0f;
    public int levelSizeX = 10;
    public int levelSizeY = 10;
    public int maxIteration = 50;
    public int seed = 42;
    public GameObject[] objects;
    public GameObject[] assetWallCorner;
    public GameObject[] assetWall;
    private int tilesCount = 0;

    // Room container
    private int[,] rooms;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);
        initializeRooms();
        extendRooms();
        placeWalls();
        placeAsset();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Room generation functions
    private void initializeRooms()
    {
        // Initialize the room at the given size
        rooms = new int[levelSizeX, levelSizeY];

        // Initialize all the tiles
        for (int coordX = 0; coordX < levelSizeX; coordX++)
        {
            for (int coordY = 0; coordY < levelSizeY; coordY++)
            {
                rooms[coordX, coordY] = -1;
            }
        }

        int roomCreated = 0;
        // Loop over all the rooms we have to create
        for (int i = 0; i < roomQuantity; i++)
        {
            bool roomFound = false;
            int iterationCount = 0;
            // Loop until we find an available tile
            while (roomFound == false && iterationCount < maxIteration)
            {
                // Get a random tile
                int selectedRoomX = Random.Range(0, levelSizeX - 1);
                int selectedRoomY = Random.Range(0, levelSizeY - 1);

                // If the tile is available
                if (rooms[selectedRoomX, selectedRoomY] < 0)
                {
                    // Set a room to it
                    rooms[selectedRoomX, selectedRoomY] = i;
                    // Increment the tile count
                    tilesCount++;
                    // A room has ben found
                    roomFound = true;
                    roomCreated++;
                }
                iterationCount++;
            }
        }

        roomQuantity = roomCreated;
    }

    private void extendRooms()
    {
        int iterationCount = 0;
        while (iterationCount < maxIteration && tilesCount < levelSizeX * levelSizeY)
        {
            // We have to create a roomsTemp variable to make sure each iterations are independant
            int[,] roomsTemp = rooms.Clone() as int[,];

            // Loop over each tiles to initialize the rooms starting points
            for (int coordX = 0; coordX < levelSizeX; coordX++)
            {
                for (int coordY = 0; coordY < levelSizeY; coordY++)
                {
                    // If the tile already has a room, skip it
                    if (rooms[coordX, coordY] != -1) { continue; }

                    // Store all the neighbours of the tile
                    List<int> relativePoses = new List<int>();
                    List<int> neighbourRoomIDs = getNeighbours(coordX, coordY, ref relativePoses);

                    // If some neighbours has been found
                    if (neighbourRoomIDs.Count > 0 && rooms[coordX, coordY] == -1)
                    {
                        // Get a random value from all the available values
                        int randomIndex = Random.Range(0, neighbourRoomIDs.Count - 1);

                        // Store the chosen neighbours
                        roomsTemp[coordX, coordY] = neighbourRoomIDs[randomIndex];
                        // Increment the tile count
                        tilesCount++;
                    }
                }
            }
            // Replace the old rooms with the new rooms
            rooms = roomsTemp.Clone() as int[,];
            iterationCount++;
        }
    }

    private void placeWalls()
    {
        // Loop over each tiles to check if any walls to place
        for (int coordX = 0; coordX < levelSizeX; coordX++)
        {
            for (int coordY = 0; coordY < levelSizeY; coordY++)
            {
                List<int> relativePoses = new List<int>();
                List<int> neighbourRoomIDs = getNeighbours(coordX, coordY, ref relativePoses);

                List<int> edgeCheck = new List<int>(new int[] { 0, 2, 3, 1 });

                // Loop over each neighbours
                for (int i = 0; i < neighbourRoomIDs.Count; i++)
                {
                    // If the neighbour has a difference ID
                    if (neighbourRoomIDs[i] != rooms[coordX, coordY])
                    {
                        // Place a wall between them
                        Instantiate(objects[0], new Vector3(coordX * scale, 0, coordY * scale), Quaternion.AngleAxis(90 * relativePoses[i], Vector3.up));
                    }

                    edgeCheck.Remove(relativePoses[i]);
                }

                // Place a wall at every edge
                edgeCheck.ForEach(delegate (int edgeDirection)
                {
                    Instantiate(objects[0], new Vector3(coordX * scale, 0, coordY * scale), Quaternion.AngleAxis(90 * edgeDirection, Vector3.up));
                });
            }
        }
    }

    private void placeAsset()
    {
        // Loop over each tiles to check if any walls to place
        for (int coordX = 0; coordX < levelSizeX; coordX++)
        {
            for (int coordY = 0; coordY < levelSizeY; coordY++)
            {
                if (true)
                {
                    // Loop over all the subTiles
                    for (int i = 0; i < 4; i++)
                    {
                        // Find all the neighbours of the subTile
                        List<int> relativePoses = new List<int>();
                        List<int> neighbourRoomIDs = getNeighbours(coordX, coordY, ref relativePoses);

                        List<int> edgeCheck = new List<int>(new int[] { 0, 2, 3, 1 });

                        // Get the subCoordinate
                        int subCoordX = i % 2;
                        int subCoordY = i / 2;

                        int subtileType = 0;

                        // Check the two possible neighbours
                        for (int neighbour = 0; neighbour < neighbourRoomIDs.Count; neighbour++)
                        {
                            if (relativePoses[neighbour] == 0 && subCoordX == 0 && neighbourRoomIDs[neighbour] != rooms[coordX, coordY]) { subtileType += 1; }
                            if (relativePoses[neighbour] == 2 && subCoordX == 1 && neighbourRoomIDs[neighbour] != rooms[coordX, coordY]) { subtileType += 2; }
                            if (relativePoses[neighbour] == 3 && subCoordY == 0 && neighbourRoomIDs[neighbour] != rooms[coordX, coordY]) { subtileType += 4; }
                            if (relativePoses[neighbour] == 1 && subCoordY == 1 && neighbourRoomIDs[neighbour] != rooms[coordX, coordY]) { subtileType += 8; }

                            edgeCheck.Remove(relativePoses[neighbour]);
                        }

                        // Append every edges
                        edgeCheck.ForEach(delegate (int edgeDirection)
                        {
                            if (edgeDirection == 0 && subCoordX == 0) { subtileType += 1; }
                            if (edgeDirection == 2 && subCoordX == 1) { subtileType += 2; }
                            if (edgeDirection == 3 && subCoordY == 0) { subtileType += 4; }
                            if (edgeDirection == 1 && subCoordY == 1) { subtileType += 8; }
                        });

                        // If the subtile is a corner
                        if (subtileType != 1 && subtileType != 2 && subtileType != 4 && subtileType != 8 && subtileType != 0)
                        {
                            float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                            float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                            Instantiate(assetWallCorner[0], new Vector3(positionX, 0.5f, positionZ), Quaternion.identity);
                        }

                        // If the subtile is along a wall
                        else if (subtileType == 1 || subtileType == 2 || subtileType == 4 || subtileType == 8)
                        {
                            if (subtileType == 1)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.5f, positionZ), Quaternion.identity);
                            }
                            if (subtileType == 2)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.5f, positionZ), Quaternion.identity);
                            }
                            if (subtileType == 4)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.5f, positionZ), Quaternion.identity);
                            }
                            if (subtileType == 8)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.5f, positionZ), Quaternion.identity);
                            }
                        }
                    }
                }
            }
        }
    }

    private List<int> getNeighbours(int coordX, int coordY, ref List<int> relativePoses)
    {
        // Initialize the neighbourd ID
        List<int> neighbourRoomIDs = new List<int>();

        // Get the neighbour tiles
        if (coordX > 0)
        {
            if (rooms[coordX - 1, coordY] >= 0)
            {
                neighbourRoomIDs.Add(rooms[coordX - 1, coordY]);
                relativePoses.Add(0);
            }
        }
        if (coordX < levelSizeX - 1)
        {
            if (rooms[coordX + 1, coordY] >= 0)
            {
                neighbourRoomIDs.Add(rooms[coordX + 1, coordY]);
                relativePoses.Add(2);
            }
        }
        if (coordY > 0)
        {
            if (rooms[coordX, coordY - 1] >= 0)
            {
                neighbourRoomIDs.Add(rooms[coordX, coordY - 1]);
                relativePoses.Add(3);
            }
        }
        if (coordY < levelSizeY - 1)
        {
            if (rooms[coordX, coordY + 1] >= 0)
            {
                neighbourRoomIDs.Add(rooms[coordX, coordY + 1]);
                relativePoses.Add(1);
            }
        }

        return neighbourRoomIDs;
    }
}
