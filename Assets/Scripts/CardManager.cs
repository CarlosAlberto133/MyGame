using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    [SerializeField] private GameObject[] cardPrefabs;
    [SerializeField] private Transform cardPanel;
    [SerializeField] private int initialHandSize = 5;
    [SerializeField] private float cardSpacing = 2f;
    [SerializeField] private Vector2 cardSize = new Vector2(350f, 500f);

    private List<GameObject> currentHand = new List<GameObject>();

    void Start()
    {
        DrawInitialHand();
    }

    void DrawInitialHand()
    {
        foreach (GameObject card in currentHand)
        {
            Destroy(card);
        }
        currentHand.Clear();

        for (int i = 0; i < initialHandSize; i++)
        {
            DrawCard();
        }
    }

    void DrawCard()
    {
        if (cardPrefabs.Length == 0)
        {
            Debug.LogError("Não há prefabs de cartas configurados no CardManager!");
            return;
        }

        int randomIndex = Random.Range(0, cardPrefabs.Length);
        GameObject cardPrefab = cardPrefabs[randomIndex];

        if (cardPrefab == null)
        {
            Debug.LogError("Prefab de carta é null no índice: " + randomIndex);
            return;
        }

        GameObject newCard = Instantiate(cardPrefab, cardPanel);
        if (newCard == null)
        {
            Debug.LogError("Falha ao instanciar carta!");
            return;
        }

        float xPos = (currentHand.Count - (initialHandSize - 1) / 2f) * cardSpacing;
        Vector3 cardPosition = new Vector3(xPos, 0, 0);
        newCard.transform.localPosition = cardPosition;

        // Adiciona os componentes necessários
        if (!newCard.GetComponent<RectTransform>())
        {
            newCard.AddComponent<RectTransform>();
        }

        // Ajusta o tamanho da carta - ADICIONE ESTE BLOCO
        RectTransform rectTransform = newCard.GetComponent<RectTransform>();
        if (rectTransform)
        {
            rectTransform.sizeDelta = cardSize;
        }

        if (!newCard.GetComponent<CanvasGroup>())
        {
            newCard.AddComponent<CanvasGroup>();
        }

        if (!newCard.GetComponent<DraggableCard>())
        {
            newCard.AddComponent<DraggableCard>();
        }

        currentHand.Add(newCard);
    }
}