using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardsObjProgram card;

    public Text nameText;
    public Text descriptionText;
    public Image ArtImage;
    public Text manaOrGoldCostText;
    public Text attackText;
    public Text shieldText;
    public Text healthText;

    // Start is called before the first frame update
    void Start()
    {
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        ArtImage.sprite = card.Art;
        manaOrGoldCostText.text = card.manaOrGoldCost.ToString();
        attackText.text = card.attack.ToString();
        shieldText.text = card.shield.ToString();
        healthText.text = card.health.ToString();
    }
}
