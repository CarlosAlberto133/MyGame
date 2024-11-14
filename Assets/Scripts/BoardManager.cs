using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Adiciona o namespace UI necessário

public class BoardManager : MonoBehaviour
{
    [System.Serializable]
    public class TeamMaterialsData
    {
        public Material tileMaterial;
        public int size;
    }

    [Header("Board Setup")]
    [SerializeField] private int boardWidth = 5;
    [SerializeField] private int boardHeight = 5;
    [SerializeField] private float spacing = 0.1f;
    
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private GameObject victoryScreen;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private TeamMaterialsData teamMaterials;
    
    private GameObject[,] tiles;
    private GameObject[,] placedCards;
    
    void Start()
    {
        tiles = new GameObject[boardWidth, boardHeight];
        placedCards = new GameObject[boardWidth, boardHeight];
        GenerateBoard();
    }
    
    private void GenerateBoard()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab não foi atribuído! Por favor, arraste um prefab para o campo Tile Prefab no Inspector.");
            return;
        }

        float totalWidth = (boardWidth * tileSize) + ((boardWidth - 1) * spacing);
        float totalHeight = (boardHeight * tileSize) + ((boardHeight - 1) * spacing);
        
        float startX = boardCenter.x - (totalWidth / 2f);
        float startZ = boardCenter.z - (totalHeight / 2f);
        
        for (int x = 0; x < boardWidth; x++)
        {
            for (int z = 0; z < boardHeight; z++)
            {
                float xPos = startX + (x * (tileSize + spacing));
                float zPos = startZ + (z * (tileSize + spacing));
                
                Vector3 tilePosition = new Vector3(xPos, boardCenter.y + yOffset, zPos);
                
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = $"Tile_{x}_{z}";
                
                if (tileMaterial != null)
                {
                    var renderer = tile.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = tileMaterial;
                    }
                }
                else
                {
                    var renderer = tile.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        Material defaultMaterial = new Material(Shader.Find("Standard"));
                        defaultMaterial.color = new Color(0.8f, 0.8f, 0.8f);
                        renderer.material = defaultMaterial;
                    }
                }
                
                tile.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
                tile.layer = LayerMask.NameToLayer("Tile");
                
                tiles[x, z] = tile;
            }
        }
    }
    
    public bool CanPlaceCardAt(int x, int z)
    {
        if (x < 0 || x >= boardWidth || z < 0 || z >= boardHeight)
            return false;
        return placedCards[x, z] == null;
    }
    
    public void PlaceCard(GameObject card, int x, int z)
    {
        if (!CanPlaceCardAt(x, z))
            return;

        Vector3 position = tiles[x, z].transform.position;
        position.y += 0.4f; // Ajusta a altura da carta acima do tile

        // Configura a posição e rotação da carta
        card.transform.position = position;
        card.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Inclina a carta para melhor visualização
        card.transform.SetParent(transform);

        // Configura o Canvas para World Space
        Canvas cardCanvas = card.GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = card.AddComponent<Canvas>();
        }
        cardCanvas.renderMode = RenderMode.WorldSpace;
        cardCanvas.worldCamera = Camera.main;

        // Ajusta a escala
        float cardScale = tileSize * 0.01f; // Um pouco menor que o tile
        card.transform.localScale = new Vector3(cardScale, cardScale, 1f);

        // Configura o RectTransform
        RectTransform rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(100f, 100f);
        }

        // Configura o CanvasScaler
        CanvasScaler scaler = card.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = card.AddComponent<CanvasScaler>();
        }
        scaler.scaleFactor = 1f;
        scaler.dynamicPixelsPerUnit = 100f;

        // Ajusta o CanvasGroup
        CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // Garante que os renderers estão ativos
        var renderers = card.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        // Registra a carta na posição
        placedCards[x, z] = card;

        var cardMovement = card.GetComponent<CardMovement>();
        if (cardMovement != null)
        {
            cardMovement.OnCardPlaced(x, z);
        }
    }
    
    public Vector2Int GetTileCoordinatesFromPosition(Vector3 worldPosition)
    {
        float totalWidth = (boardWidth * tileSize) + ((boardWidth - 1) * spacing);
        float totalHeight = (boardHeight * tileSize) + ((boardHeight - 1) * spacing);
        
        float startX = boardCenter.x - (totalWidth / 2f);
        float startZ = boardCenter.z - (totalHeight / 2f);
        
        float xPos = (worldPosition.x - startX) / (tileSize + spacing);
        float zPos = (worldPosition.z - startZ) / (tileSize + spacing);
        
        return new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(xPos), 0, boardWidth - 1),
            Mathf.Clamp(Mathf.RoundToInt(zPos), 0, boardHeight - 1)
        );
    }

    // Método auxiliar para debug
    public void DebugCardPlacement(GameObject card, int x, int z)
    {
        if (placedCards[x, z] != null)
        {
            Debug.Log($"Carta na posição [{x}, {z}]: {placedCards[x, z].name}");
            Debug.Log($"Posição: {placedCards[x, z].transform.position}");
            Debug.Log($"Rotação: {placedCards[x, z].transform.rotation.eulerAngles}");
            Debug.Log($"Escala: {placedCards[x, z].transform.localScale}");
            
            var canvas = placedCards[x, z].GetComponent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"Canvas Render Mode: {canvas.renderMode}");
                Debug.Log($"Canvas World Camera: {canvas.worldCamera}");
            }
        }
    }
    public Vector3 GetTilePosition(int x, int y) {
    return tiles[x, y].transform.position;
    }

    public Material GetDefaultTileMaterial() {
        return tileMaterial;
    }

    public GameObject GetTileAtPosition(int x, int y)
    {
        if (x >= 0 && x < boardWidth && y >= 0 && y < boardHeight)
        {
            return tiles[x, y];
        }
        return null;
    }
    public int GetBoardWidth()
    {
        return boardWidth;
    }

    public int GetBoardHeight()
    {
        return boardHeight;
    }

    public void RemoveCard(int x, int z)
    {
        if (x >= 0 && x < boardWidth && z >= 0 && z < boardHeight)
        {
            placedCards[x, z] = null;
        }
    }
}