using UnityEngine;

public enum CardPieceType
{
    None = 0,
    Rook = 1,
    Bishop = 2,
    Queen = 3,
    King = 4
}

public class Cards : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public CardPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale;
}
