using UnityEngine;
using System.Collections.Generic;

public class CardMovement : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float moveSpeed = 5f;
    
    private BoardManager boardManager;
    private Vector2Int currentPosition;
    private List<GameObject> highlightedTiles = new List<GameObject>();
    private bool isMovementMode = false;
    private bool hasBeenPlaced = false;
    
    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
    }
    
    public void OnCardPlaced(int x, int y)
    {
        currentPosition = new Vector2Int(x, y);
        hasBeenPlaced = true;
        ClearHighlights(); // Limpa qualquer highlight existente
    }
    
    private void OnMouseDown()
    {
        if (!hasBeenPlaced) return;
        
        if (!isMovementMode)
        {
            ShowPossibleMoves();
        }
        else
        {
            ClearHighlights();
        }
    }
    
    private void ShowPossibleMoves()
    {
        ClearHighlights();
        isMovementMode = true;
        
        // Verifica os 4 tiles adjacentes
        if (currentPosition.x > 0) // Esquerda
            CheckAndHighlightTile(currentPosition.x - 1, currentPosition.y);
            
        if (currentPosition.x < 4) // Direita (assumindo tabuleiro 5x5)
            CheckAndHighlightTile(currentPosition.x + 1, currentPosition.y);
            
        if (currentPosition.y > 0) // Baixo
            CheckAndHighlightTile(currentPosition.x, currentPosition.y - 1);
            
        if (currentPosition.y < 4) // Cima (assumindo tabuleiro 5x5)
            CheckAndHighlightTile(currentPosition.x, currentPosition.y + 1);
    }
    
    private void CheckAndHighlightTile(int x, int y)
    {
        if (boardManager.CanPlaceCardAt(x, y))
        {
            GameObject tile = boardManager.GetTileAtPosition(x, y);
            if (tile != null)
            {
                HighlightTile(tile);
                tile.AddComponent<MovementTileHandler>().Initialize(this, new Vector2Int(x, y));
            }
        }
    }
    
    private void HighlightTile(GameObject tile)
    {
        MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = highlightMaterial;
            highlightedTiles.Add(tile);
        }
    }
    
    private void ClearHighlights()
    {
        foreach (var tile in highlightedTiles)
        {
            if (tile != null)
            {
                var handler = tile.GetComponent<MovementTileHandler>();
                if (handler != null)
                    Destroy(handler);
                
                MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.material = boardManager.GetDefaultTileMaterial();
            }
        }
        highlightedTiles.Clear();
        isMovementMode = false;
    }
    
    public void MoveToPosition(Vector2Int newPosition)
    {
        if (!boardManager.CanPlaceCardAt(newPosition.x, newPosition.y))
            return;

        boardManager.RemoveCard(currentPosition.x, currentPosition.y);
        StartCoroutine(MoveCardCoroutine(newPosition));
    }
    
    private System.Collections.IEnumerator MoveCardCoroutine(Vector2Int newPosition)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = boardManager.GetTilePosition(newPosition.x, newPosition.y);
        targetPos.y = startPos.y;
        
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            yield return null;
        }
        
        boardManager.PlaceCard(gameObject, newPosition.x, newPosition.y);
        currentPosition = newPosition;
        ShowPossibleMoves();
    }
}

public class MovementTileHandler : MonoBehaviour
{
    private CardMovement cardMovement;
    private Vector2Int targetPosition;
    
    public void Initialize(CardMovement movement, Vector2Int pos)
    {
        cardMovement = movement;
        targetPosition = pos;
    }
    
    private void OnMouseDown()
    {
        if (cardMovement != null)
            cardMovement.MoveToPosition(targetPosition);
    }
}