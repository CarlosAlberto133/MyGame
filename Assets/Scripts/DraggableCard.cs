using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Transform handTransform;
    private BoardManager board;
    private Camera mainCamera;
    private bool isInHand = true;
    
    void Awake()
    {
        // Procura o Canvas e verifica se encontrou
        var canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            canvas = canvasObj.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas encontrado mas não tem o componente Canvas!");
            }
        }
        else
        {
            Debug.LogError("Não foi possível encontrar o objeto Canvas!");
        }

        // Procura e configura o RectTransform
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform não encontrado na carta!");
        }

        // Procura e configura o CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Procura o BoardManager usando FindObjectOfType
        board = FindObjectOfType<BoardManager>();
        if (board == null)
        {
            Debug.LogError("Não foi possível encontrar o componente BoardManager!");
        }

        // Procura a câmera principal
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Câmera principal não encontrada!");
        }

        // Procura o Hand
        var handObj = GameObject.Find("Hand");
        if (handObj != null)
        {
            handTransform = handObj.transform;
        }
        else
        {
            Debug.LogError("Não foi possível encontrar o objeto Hand!");
        }

        // Garante que a carta é interativa
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        GetComponent<CanvasGroup>().interactable = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isInHand) return;

        Debug.Log("Começou a arrastar");
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Move para o canvas principal durante o arrasto
        transform.SetParent(canvas.transform);
        
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Garante que a carta fique visível durante o arrasto
        var images = GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            image.raycastTarget = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isInHand) return;

        Debug.Log("Arrastando");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
        // Faz um raycast para ver se está sobre o tabuleiro
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Tile")))
        {
            Debug.Log("Sobre o tabuleiro: " + hit.transform.name);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isInHand) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Tile")))
        {
            Vector2Int tilePosition = board.GetTileCoordinatesFromPosition(hit.point);
            
            if (board.CanPlaceCardAt(tilePosition.x, tilePosition.y))
            {
                // Coloca a carta no tabuleiro
                board.PlaceCard(gameObject, tilePosition.x, tilePosition.y);
                isInHand = false;
                
                // Mantém os componentes visuais ativos
                var images = GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    image.raycastTarget = true;
                }
                
                return;
            }
        }
        
        // Se não colocou no tabuleiro, volta para a Hand
        transform.SetParent(handTransform);
        rectTransform.anchoredPosition = originalPosition;
    }
}