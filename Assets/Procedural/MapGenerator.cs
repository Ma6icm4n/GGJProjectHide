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
    public int tableThreshold = 50;
    public GameObject[] objects;
    public GameObject[] doorPrefab;
    public GameObject[] assetWallCorner;
    public GameObject[] assetWall;
    public GameObject[] assetCenter;
    public GameObject[] assetFloor;
    private int tilesCount = 0;

    // Room container
    private int[,] rooms;
    private List<(int X, int Y, int direction)> doors;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);

        initializeRooms();
        extendRooms();
        placeDoors();
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

    private void placeDoors()
    {
        doors = new List<(int X, int Y, int direction)>();

        // Loop over all the rooms
        for (int room = 0; room < roomQuantity; room++)
        {
            List<(int room, List<(int X, int Y, int direction)> tiles)> roomWalls = new List<(int room, List<(int X, int Y, int direction)> tiles)>();

            // Loop over each tiles
            for (int coordX = 0; coordX < levelSizeX; coordX++)
            {
                for (int coordY = 0; coordY < levelSizeY; coordY++)
                {
                    // If the tile is not the current room, we skip
                    if (rooms[coordX, coordY] != room) { continue; }

                    List<int> relativePoses = new List<int>();
                    List<int> neighbourRoomIDs = getNeighbours(coordX, coordY, ref relativePoses);

                    // Loop over each neighbours
                    for (int neighbour = 0; neighbour < neighbourRoomIDs.Count; neighbour++)
                    {
                        // If the neighbour is in the same room as us, skip it
                        if (neighbourRoomIDs[neighbour] == rooms[coordX, coordY]) { continue; }

                        bool isNewNeighbour = true;

                        // For each already appended
                        for(int roomWall = 0; roomWall < roomWalls.Count; roomWall++)
                        {
                            // If the room as already been appended
                            if(roomWalls[roomWall].room != neighbourRoomIDs[neighbour]) { continue; }
                                
                            roomWalls[roomWall].tiles.Add((coordX, coordY, relativePoses[neighbour]));
                            isNewNeighbour = false;
                            break;
                        }

                        if(isNewNeighbour)
                        {
                            roomWalls.Add((neighbourRoomIDs[neighbour], new List<(int X, int Y, int direction)> { (coordX, coordY, relativePoses[neighbour]) }));
                        }
                    }
                }
            }

            roomWalls.ForEach(delegate ((int room, List<(int X, int Y, int direction)> tiles) roomWall)
            {
                int chosenTile = Random.Range(0, roomWall.tiles.Count - 1);
                doors.Add((roomWall.tiles[chosenTile].X, roomWall.tiles[chosenTile].Y, roomWall.tiles[chosenTile].direction));
            });
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

                // Check if the neighbour is a door
                bool isDoor = false;
                doors.ForEach(delegate ((int X, int Y, int direction) door)
                {
                    if (coordX == door.X && coordY == door.Y) { isDoor = true; }
                });

                // Loop over each neighbours
                for (int i = 0; i < neighbourRoomIDs.Count; i++)
                {
                    edgeCheck.Remove(relativePoses[i]);

                    // If the neighbour has a difference ID
                    if (neighbourRoomIDs[i] != rooms[coordX, coordY])
                    {
                        int neighbourCoordX = coordX;
                        int neighbourCoordY = coordY;

                        if (relativePoses[i] == 0) { neighbourCoordX -= 1; }
                        if (relativePoses[i] == 1) { neighbourCoordY += 1; }
                        if (relativePoses[i] == 2) { neighbourCoordX += 1; }
                        if (relativePoses[i] == 3) { neighbourCoordY -= 1; }

                        doors.ForEach(delegate ((int X, int Y, int direction) door)
                        {
                            if (neighbourCoordX == door.X && neighbourCoordY == door.Y) { isDoor = true; }
                        });


                        if (!isDoor)
                        {
                            // Place a wall between them
                            Instantiate(objects[0], new Vector3(coordX * scale, 0, coordY * scale), Quaternion.AngleAxis(90 * relativePoses[i], Vector3.up));
                        }
                        else
                        {
                            // Place a wall between them
                            Instantiate(doorPrefab[0], new Vector3(coordX * scale, 0, coordY * scale), Quaternion.AngleAxis(90 * relativePoses[i], Vector3.up));
                        }
                    }
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
                // Find all the neighbours of the subTile
                List<int> relativePoses = new List<int>();
                List<int> neighbourRoomIDs = getNeighbours(coordX, coordY, ref relativePoses);

                List<int> edgeCheck = new List<int>(new int[] { 0, 2, 3, 1 });

                // Check if we are in a coridor and if we are against a wall
                bool isAgainstWall = false;
                bool isCorridor = false;
                int horizontalCount = 0;
                int verticalCount = 0;
                for (int neighbour = 0; neighbour < neighbourRoomIDs.Count; neighbour++)
                {
                    if(neighbourRoomIDs[neighbour] != rooms[coordX, coordY])
                    {
                        isAgainstWall = true;
                        if (relativePoses[neighbour] == 0 || relativePoses[neighbour] == 2) { horizontalCount++; }
                        if (relativePoses[neighbour] == 3 || relativePoses[neighbour] == 1) { verticalCount++; }
                    }
                    edgeCheck.Remove(relativePoses[neighbour]);
                }

                // For all the edges
                edgeCheck.ForEach(delegate (int edgeDirection)
                {
                    isAgainstWall = true;
                    if (edgeDirection == 0 || edgeDirection == 2) { horizontalCount++; }
                    if (edgeDirection == 3 || edgeDirection == 1) { verticalCount++; }
                });

                if (horizontalCount == 2 || verticalCount == 2) { isCorridor = true; }

                bool isAtCenter = true;
                List<int> relativeCornerPoses = new List<int>();
                List<int> neighbourCornerRoomIDs = getCornerNeighbours(coordX, coordY, ref relativePoses);

                for (int cornerNeighbour = 0; cornerNeighbour < neighbourCornerRoomIDs.Count; cornerNeighbour++)
                {
                    if(neighbourCornerRoomIDs[cornerNeighbour] != rooms[coordX, coordY])
                    {
                        isAtCenter = false;
                    }
                }

                bool isDoor = false;
                doors.ForEach(delegate ((int X, int Y, int direction) door)
                {
                    if (coordX == door.X && coordY == door.Y) { isDoor = true; }
                });

                // Loop over each neighbours
                for (int i = 0; i < neighbourRoomIDs.Count; i++)
                {
                    // If the neighbour has the same ID skip it
                    if (neighbourRoomIDs[i] == rooms[coordX, coordY]) { continue; }

                    int neighbourCoordX = coordX;
                    int neighbourCoordY = coordY;

                    if (relativePoses[i] == 0) { neighbourCoordX -= 1; }
                    if (relativePoses[i] == 1) { neighbourCoordY += 1; }
                    if (relativePoses[i] == 2) { neighbourCoordX += 1; }
                    if (relativePoses[i] == 3) { neighbourCoordY -= 1; }

                    doors.ForEach(delegate ((int X, int Y, int direction) door)
                    {
                        if (neighbourCoordX == door.X && neighbourCoordY == door.Y) { isDoor = true; }
                    });
                }

                edgeCheck = new List<int>(new int[] { 0, 2, 3, 1 });

                if (!isCorridor && !isAgainstWall && isAtCenter)
                {
                    // Loop over all the subTiles
                    for (int i = 0; i < 4; i++)
                    {
                        // Get the subCoordinate
                        int subCoordX = i % 2;
                        int subCoordY = i / 2;

                        bool isFullTable = false;

                        if (coordX % 2 == 0 && coordY % 2 == 1 && Random.Range(0, 1) == 0)
                        {
                            if (Random.Range(0, 100) < tableThreshold) { isFullTable = true; }
                        }
                        else if (coordX % 2 == 1 && coordY % 2 == 0)
                        {
                            if (Random.Range(0, 100) < tableThreshold) { isFullTable = true; }
                        }

                        if(isFullTable)
                        {
                            float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                            float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                            Instantiate(assetCenter[0], new Vector3(positionX, 0.0f, positionZ), Quaternion.identity);
                        }
                    }
                }

                if (!isCorridor && isAgainstWall && !isDoor)
                {
                    // Loop over all the subTiles
                    for (int i = 0; i < 4; i++)
                    {
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
                            int angle = 90;
                            if (subCoordY == 1) { angle *= subCoordX; }
                            if (subCoordY == 0) { angle *= (1 - subCoordX) + 2; }
                            Instantiate(assetWallCorner[0], new Vector3(positionX, 0.0f, positionZ), Quaternion.AngleAxis(angle, Vector3.up));
                        }

                        // If the subtile is along a wall
                        else if (subtileType == 1 || subtileType == 2 || subtileType == 4 || subtileType == 8)
                        {
                            if (subtileType == 1)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.0f, positionZ), Quaternion.AngleAxis(0, Vector3.up));
                            }
                            if (subtileType == 2)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.0f, positionZ), Quaternion.AngleAxis(180, Vector3.up));
                            }
                            if (subtileType == 4)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.0f, positionZ), Quaternion.AngleAxis(-90, Vector3.up));
                            }
                            if (subtileType == 8)
                            {
                                float positionX = (float)coordX * scale + (subCoordX - 0.5f);
                                float positionZ = (float)coordY * scale + (subCoordY - 0.5f);
                                Instantiate(assetWall[0], new Vector3(positionX, 0.0f, positionZ), Quaternion.AngleAxis(90, Vector3.up));
                            }
                        }
                    }
                }

                // Place the floor
                Instantiate(assetFloor[0], new Vector3(coordX * scale, 0, coordY * scale), Quaternion.identity);
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

    private List<int> getCornerNeighbours(int coordX, int coordY, ref List<int> relativePoses)
    {
        // Initialize the corner neighbours ID
        List<int> neighbourCornerRoomIDs = new List<int>();

        if (coordX > 0 && coordY > 0)
        {
            if (rooms[coordX - 1, coordY - 1] >= 0)
            {
                neighbourCornerRoomIDs.Add(rooms[coordX - 1, coordY - 1]);
                relativePoses.Add(0);
            }
        }
        if (coordX > 0 && coordY < levelSizeY - 1)
        {
            if (rooms[coordX - 1, coordY + 1] >= 0)
            {
                neighbourCornerRoomIDs.Add(rooms[coordX - 1, coordY + 1]);
                relativePoses.Add(1);
            }
        }
        if (coordX < levelSizeX - 1 && coordY > 0)
        {
            if (rooms[coordX + 1, coordY - 1] >= 0)
            {
                neighbourCornerRoomIDs.Add(rooms[coordX + 1, coordY - 1]);
                relativePoses.Add(2);
            }
        }
        if (coordX < levelSizeX - 1 && coordY < levelSizeY - 1)
        {
            if (rooms[coordX + 1, coordY + 1] >= 0)
            {
                neighbourCornerRoomIDs.Add(rooms[coordX + 1, coordY + 1]);
                relativePoses.Add(3);
            }
        }

        return neighbourCornerRoomIDs;
    }
}
