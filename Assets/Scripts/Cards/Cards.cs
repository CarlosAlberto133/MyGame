using UnityEngine;

public enum CardPieceType
{
    None = 0,
    Rook = 1,
    Bishop = 2,
    Queen = 3,
    King = 4,
    Knight = 5,
    Pawn = 6

}

public class Cards : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public CardPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if(force)
            transform.position = desiredPosition;
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if(force)
            transform.localScale = desiredScale;
    }
}
