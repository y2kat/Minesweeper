using UnityEngine;
using UnityEngine.Tilemaps;

// https://www.youtube.com/watch?v=HBrF8LJ0Hfg
//https://www.youtube.com/watch?v=wAJyWmyA0ug
[RequireComponent(typeof(Tilemap))] //asegura que el objeto al que se adjunta este script tenga un componente Tilemap
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; } //referencia al componente Tilemap del objeto. Es público para que otros scripts puedan acceder a él, pero su valor no puede ser modificado desde fuera


    //referencias a los diferentes tipos de tiles que se pueden dibujar en el tablero
    public Tile tileUnknown;
    public Tile tileEmpty;
    public Tile tileMine;
    public Tile tileExploded;
    public Tile tileFlag;
    public Tile tileNum1;
    public Tile tileNum2;
    public Tile tileNum3;
    public Tile tileNum4;
    public Tile tileNum5;
    public Tile tileNum6;
    public Tile tileNum7;
    public Tile tileNum8;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    //dibuja el tablero en función de la cuadrícula de celdas proporcionada
    public void Draw(CellGrid grid)
    {
        int width = grid.Width;
        int height = grid.Height;

        //recorre todas las celdas de la cuadrícula
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //obtiene la celda actual y establece el tile correspondiente en la posición de la celda en el tilemap
                Cell cell = grid[x, y];
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    //devuelve el tile que corresponde a una celda dada
    private Tile GetTile(Cell cell)
    {
        if (cell.revealed) //si la celda ha sido revelada, devuelve el tile correspondiente al tipo de celda
        {
            return GetRevealedTile(cell);
        }
        else if (cell.flagged) //si la celda ha sido marcada con una bandera, devuelve el tile de bandera
        {
            return tileFlag;
        }
        else if (cell.chorded)  //si la celda ha sido acordonada, devuelve el tile vacío
        {
            return tileEmpty;
        }
        else  //si la celda no ha sido revelada, devuelve el tile desconocido
        {
            return tileUnknown;
        }
    }

    //devuelve el tile que corresponde a una celda revelada
    private Tile GetRevealedTile(Cell cell)
    {
        switch (cell.type)
        {
            case Cell.Type.Empty: return tileEmpty;
            case Cell.Type.Mine: return cell.exploded ? tileExploded : tileMine;
            case Cell.Type.Number: return GetNumberTile(cell);
            default: return null;
        }
    }

    //devuelve el tile que corresponde a una celda con un número
    private Tile GetNumberTile(Cell cell)
    {
        switch (cell.number)
        {
            // dependiendo del número de la celda, devuelve el tile correspondiente
            case 1: return tileNum1;
            case 2: return tileNum2;
            case 3: return tileNum3;
            case 4: return tileNum4;
            case 5: return tileNum5;
            case 6: return tileNum6;
            case 7: return tileNum7;
            case 8: return tileNum8;
            default: return null;
        }
    }

}
