using UnityEngine;

public class Cell
{
    public enum Type //determina qué tipo de mina es:
    {
        Empty,
        Mine,
        Number,
    }

    public Vector3Int position; //posición en el board
    public Type type;
    public int number; //int que representa qué número es
    public bool revealed;
    public bool flagged;
    public bool exploded;
    public bool chorded;
}
