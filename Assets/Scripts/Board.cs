using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffSet = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private GameObject victoryScreen;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // LOGIC
    private Cards[,] Cards;
    private Cards currentlyDragging;
    private List<Vector2Int> avaiableMoves = new List<Vector2Int>();
    private List<Cards> deadWhites = new List<Cards>();
    private List<Cards> deadBlacks = new List<Cards>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn;

    private void Awake()
    {
        isWhiteTurn = true;

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        //SpawnAllCards();
        //PositionAllCards();
    }

    private void Update()
    {
        if(!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Hightlight")))
        {
            // Get the indexes of the tile i've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // If we're hovering a tile after not hovering any tiles
            if(currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we were already hovering a tile, change the previous one
            if(currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref avaiableMoves, currentHover)) ? LayerMask.NameToLayer("Hightlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we press down on the mouse
            if(Input.GetMouseButtonDown(0))
            {
                if(Cards[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn?
                    if((Cards[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (Cards[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        currentlyDragging = Cards[hitPosition.x, hitPosition.y];

                        // Get a list of where i can go, hightlight tiles as well
                        avaiableMoves = currentlyDragging.GetAvailableMoves(ref Cards, TILE_COUNT_X, TILE_COUNT_Y);
                        HightlightTiles();
                    }
                }
            }

            // If we are releasing the mouse button
            if(currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if(!validMove)
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));

                currentlyDragging = null;
                RemoveHightlightTiles();
            }
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref avaiableMoves, currentHover)) ? LayerMask.NameToLayer("Hightlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if(currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHightlightTiles();
            }
        }

        // If we're dragging a card
        if(currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffSet);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }

    // Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffSet += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x,y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffSet, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffSet, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffSet, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffSet, (y+1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Spawning of the cards
    /*private void SpawnAllCards()
    {
        Cards = new Cards[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        // White team
        Cards[0,0] = SpawnSingleCard(CardPieceType.Rook, whiteTeam);
        Cards[1,0] = SpawnSingleCard(CardPieceType.Knight, whiteTeam);
        Cards[2,0] = SpawnSingleCard(CardPieceType.Bishop, whiteTeam);
        Cards[3,0] = SpawnSingleCard(CardPieceType.King, whiteTeam);
        Cards[4,0] = SpawnSingleCard(CardPieceType.Queen, whiteTeam);
        Cards[5,0] = SpawnSingleCard(CardPieceType.Bishop, whiteTeam);
        Cards[6,0] = SpawnSingleCard(CardPieceType.Knight, whiteTeam);
        Cards[7,0] = SpawnSingleCard(CardPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            Cards[i,1] = SpawnSingleCard(CardPieceType.Pawn, whiteTeam);

        // Black team
        Cards[0,7] = SpawnSingleCard(CardPieceType.Rook, blackTeam);
        Cards[1,7] = SpawnSingleCard(CardPieceType.Knight, blackTeam);
        Cards[2,7] = SpawnSingleCard(CardPieceType.Bishop, blackTeam);
        Cards[3,7] = SpawnSingleCard(CardPieceType.King, blackTeam);
        Cards[4,7] = SpawnSingleCard(CardPieceType.Queen, blackTeam);
        Cards[5,7] = SpawnSingleCard(CardPieceType.Bishop, blackTeam);
        Cards[6,7] = SpawnSingleCard(CardPieceType.Knight, blackTeam);
        Cards[7,7] = SpawnSingleCard(CardPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            Cards[i,6] = SpawnSingleCard(CardPieceType.Pawn, blackTeam);
    }*/

    private Cards SpawnSingleCard(CardPieceType type, int team)
    {
        // Instancia o prefab da carta
        Cards cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Cards>();

        // Define o tipo e o time da carta
        cp.type = type;
        cp.team = team;

        // Pega o SpriteRenderer
        SpriteRenderer spriteRenderer = cp.GetComponent<SpriteRenderer>();

        return cp;
    }

    // Positioning
    private void PositionAllCards()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if(Cards[x, y] != null)
                    PositionSingleCard(x, y, true);
    }

    private void PositionSingleCard(int x, int y, bool force = false)
    {
        if (Cards[x, y] != null)  // Adicione esta verificação
        {
            Cards[x, y].currentX = x;
            Cards[x, y].currentY = y;
            Cards[x, y].SetPosition(GetTileCenter(x, y), force);
        }
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffSet, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Hightlight Tiles
    private void HightlightTiles()
    {
        for (int i = 0; i < avaiableMoves.Count; i ++)
            tiles[avaiableMoves[i].x, avaiableMoves[i].y].layer = LayerMask.NameToLayer("Hightlight");
    }

    private void RemoveHightlightTiles()
    {
        for (int i = 0; i < avaiableMoves.Count; i ++)
            tiles[avaiableMoves[i].x, avaiableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        avaiableMoves.Clear();
    }

    // Checkmate - função de checkmate para terminar o jogo
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }

    private void DisplayVictory(int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    public void OnResetButton()
    {
        // UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        // Fields reset
        currentlyDragging = null;
        avaiableMoves = new List<Vector2Int>();

        // Clean up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(Cards[x, y] != null)
                    Destroy(Cards[x, y].gameObject);

                Cards[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);

        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        //SpawnAllCards();
        //PositionAllCards();
        isWhiteTurn = true;
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if(moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if(tiles[x,y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one; // Invalid
    }

    private bool MoveTo(Cards cp, int x, int y)
    {
        if(!ContainsValidMove(ref avaiableMoves, new Vector2(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        // Is ther another card on the target position?
        if(Cards[x, y] != null)
        {
            Cards ocp = Cards[x, y];

            if(cp.team == ocp.team)
                return false;

            // If its the enemy team
            if(ocp.team == 0)
            {
                if(ocp.type == CardPieceType.King) // Esse if termina o jogo caso a carta king morra
                    CheckMate(1);                  // Mudar a lógica para acabar o jogo quando o hp chegar a 0

                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                    new Vector3(8 * tileSize, yOffSet, -1 * tileSize)
                    -bounds 
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count);
            }
            else
            {
                if(ocp.type == CardPieceType.King)  // Esse if termina o jogo caso a carta king morra
                    CheckMate(0);                   // Mudar a lógica para acabar o jogo quando o hp chegar a 0

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                    new Vector3(-1 * tileSize, yOffSet, 8 * tileSize)
                    -bounds 
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }
        }

        Cards[x,y] = cp;
        Cards[previousPosition.x, previousPosition.y] = null;

        PositionSingleCard(x, y);

        isWhiteTurn = !isWhiteTurn;

        return true;
    }
}
