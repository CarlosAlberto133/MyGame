using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardsObjProgram : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite Art;
    public int manaOrGoldCost;
    public int attack;
    public int shield;
    public int health;

    public void Print()
    {
        Debug.Log(cardName + ": " + description + "The card costs: " + manaOrGoldCost);
    }
}
