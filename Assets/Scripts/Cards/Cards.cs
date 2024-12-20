﻿using System.Collections.Generic;
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

    // Função para rotacionar as cartas pretas para ficarem de frente para as brancas
    // o erro ocorre por que no vídeo ele gira as peças pretas, no caso das cartas está ocorrendo um bug ao girar
    // então deixar comentado para lembrar de girar as cartas pretas.
    // private void Start()
    // {
    //   transform.rotation = Quaternion.Euler((team == 0) ? Vector3.zero : new Vector3(0, 180, 0));
    // }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Cards[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
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