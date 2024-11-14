using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardSpawner : MonoBehaviour
{
    [SerializeField] private float cardSpacing = 2f; // Espaçamento entre as cartas
    [SerializeField] private float yPosition = 0f; // Altura das cartas
    [SerializeField] private float zPosition = 0f; // Profundidade das cartas

    private CardHandManager handManager;

    void Awake()
    {
        handManager = FindObjectOfType<CardHandManager>();
    }

    private void Start()
    {
        ShowRandomCards(5);
    }

    private void ShowRandomCards(int numberOfCards)
    {
        // Pega todas as cartas
        List<GameObject> allCards = new List<GameObject>();
        foreach (Transform child in transform)
        {
            allCards.Add(child.gameObject);
        }

        // Embaralha a lista
        allCards = allCards.OrderBy(x => Random.value).ToList();

        // Desativa todas as cartas primeiro
        foreach (GameObject card in allCards)
        {
            card.SetActive(false);
        }

        // Calcula a posição inicial para centralizar as cartas
        float totalWidth = cardSpacing * (numberOfCards - 1);
        float startX = -totalWidth / 2f;

        // Ativa e posiciona as cartas selecionadas
        int cardsToShow = Mathf.Min(numberOfCards, allCards.Count);
        for (int i = 0; i < cardsToShow; i++)
        {
            GameObject card = allCards[i];
            card.SetActive(true);

            // Calcula a nova posição
            float xPos = startX + (cardSpacing * i);
            Vector3 newPosition = new Vector3(xPos, yPosition, zPosition);
            
            // Aplica a nova posição
            card.transform.localPosition = newPosition;
            
            // Garante que a rotação está correta
            card.transform.localRotation = Quaternion.identity;

            // Verifica se já tem algum collider
            Collider boxCollider = card.GetComponent<BoxCollider>();
            Collider2D boxCollider2D = card.GetComponent<BoxCollider2D>();

            // Se não tem nenhum collider, adiciona um BoxCollider2D
            if (!boxCollider && !boxCollider2D)
            {
                card.AddComponent<BoxCollider2D>();
            }
            // Se tem um BoxCollider 3D, vamos usá-lo em vez de adicionar um 2D
            else if (boxCollider)
            {
                // Garante que o collider está ativo
                boxCollider.enabled = true;
            }

            // Adiciona o componente de clique
            if (!card.GetComponent<CardClickHandler>())
            {
                CardClickHandler clickHandler = card.AddComponent<CardClickHandler>();
                clickHandler.handManager = handManager;
            }
        }
    }

    public class CardClickHandler : MonoBehaviour
    {
        public CardHandManager handManager;
        
        private void OnMouseDown()
        {
            if (handManager && handManager.CanAddCardToHand())
            {
                handManager.AddCardToHand(gameObject);
                // Remove o próprio componente após ser adicionado à mão
                Destroy(this);
            }
        }
    }

    // Método público para reembaralhar as cartas quando necessário
    public void ShuffleCards()
    {
        ShowRandomCards(5);
    }
}