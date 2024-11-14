using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    [SerializeField] private float handYPosition = -4f;
    [SerializeField] private float handZPosition = 0f;
    [SerializeField] private float handCardSpacing = 1.5f;
    [SerializeField] private float cardMoveSpeed = 10f;
    [SerializeField] private int maxCardsInHand = 7;

    private List<GameObject> cardsInHand = new List<GameObject>();
    private bool isCardMoving = false;

    public bool CanAddCardToHand()
    {
        return cardsInHand.Count < maxCardsInHand;
    }

    public void AddCardToHand(GameObject card)
    {
        if (!CanAddCardToHand() || cardsInHand.Contains(card))
            return;

        cardsInHand.Add(card);

        // Adiciona os componentes necessários para UI
        if (!card.GetComponent<RectTransform>())
            card.AddComponent<RectTransform>();
            
        if (!card.GetComponent<CanvasGroup>())
            card.AddComponent<CanvasGroup>();

        // Adiciona o componente de drag and drop
        if (!card.GetComponent<CardInHand>())
            card.AddComponent<CardInHand>();

        // Configura a carta como filha do canvas
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            card.transform.SetParent(canvas.transform, false);
        }

        RepositionCardsInHand();
    }

    public void RemoveCardFromHand(GameObject card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            RepositionCardsInHand();
        }
    }

    private void RepositionCardsInHand()
    {
        int cardCount = cardsInHand.Count;
        float totalWidth = handCardSpacing * (cardCount - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = cardsInHand[i];
            if (card != null)
            {
                Vector3 targetPosition = new Vector3(
                    startX + (handCardSpacing * i),
                    handYPosition,
                    handZPosition
                );

                StartCoroutine(MoveCardSmoothly(card, targetPosition));
            }
        }
    }

    private System.Collections.IEnumerator MoveCardSmoothly(GameObject card, Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = card.transform.localPosition;
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cardMoveSpeed;
            card.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            yield return null;
        }

        card.transform.localPosition = targetPosition;
    }
}

// Componente para controlar o drag and drop das cartas
public class CardInHand : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private CardHandManager handManager;
    private Board board;
    private Camera mainCamera;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        handManager = FindObjectOfType<CardHandManager>();
        board = FindObjectOfType<Board>();
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Começou arrasto");
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        RaycastHit hit;
        LayerMask tileMask = LayerMask.GetMask("Tile");
        if (Physics.Raycast(ray, out hit, 100f, tileMask))
        {
            Debug.Log($"Hovering tile at {hit.point}, Layer: {LayerMask.LayerToName(hit.transform.gameObject.layer)}");
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Tile")))
        {
            // Usa a posição do hit para encontrar as coordenadas do tile
            Vector2Int tileCoords = board.GetTileCoordinatesFromPosition(hit.point);
            Debug.Log($"Tentando colocar carta nas coordenadas: {tileCoords}");

            if (board.CanPlaceCardAt(tileCoords.x, tileCoords.y))
            {
                Debug.Log("Posição válida, colocando carta...");

                // Remove da mão
                handManager.RemoveCardFromHand(gameObject);

                // Remove componentes UI
                Destroy(canvasGroup);
                Destroy(GetComponent<RectTransform>());

                // Configura para o mundo 3D
                transform.SetParent(board.transform);
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.identity;

                // Coloca no tabuleiro
                board.PlaceCard(gameObject, tileCoords.x, tileCoords.y);
                
                // Remove este componente
                Destroy(this);
                return;
            }
            else
            {
                Debug.Log($"Não pode colocar carta na posição {tileCoords}");
            }
        }

        // Se não conseguiu colocar, volta para a posição original
        rectTransform.anchoredPosition = originalPosition;
    }
}