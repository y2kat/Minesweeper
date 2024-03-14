using UnityEngine;

// https://www.youtube.com/watch?v=HBrF8LJ0Hfg
//https://www.youtube.com/watch?v=wAJyWmyA0ug
public class CellGrid
{
    // matriz de celdas que representa el tablero del juego
    private readonly Cell[,] cells;

    // propiedades para obtener el ancho y alto del tablero
    public int Width => cells.GetLength(0);
    public int Height => cells.GetLength(1);

    // indexador para acceder a una celda espec�fica en el tablero
    public Cell this[int x, int y] => cells[x, y];

    // constructor que inicializa el tablero con celdas vac�as
    public CellGrid(int width, int height)
    {
        cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new Cell
                {
                    position = new Vector3Int(x, y, 0),
                    type = Cell.Type.Empty
                };
            }
        }
    }

    // generar minas en el tablero
    public void GenerateMines(Cell startingCell, int amount)
    {
        int width = Width;
        int height = Height;

        for (int i = 0; i < amount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            Cell cell = cells[x, y];

            //si la celda ya es una mina o es adyacente a la celda de inicio, busca otra celda!!!
            while (cell.type == Cell.Type.Mine || IsAdjacent(startingCell, cell))
            {
                x++;

                if (x >= width)
                {
                    x = 0;
                    y++;

                    if (y >= height) {
                        y = 0;
                    }
                }

                cell = cells[x, y];
            }

            cell.type = Cell.Type.Mine; //asigna el tipo de celda como mina
        }
    }

    // m�todo para generar los n�meros en las celdas que indican la cantidad de minas adyacentes
    public void GenerateNumbers()
    {
        int width = Width;
        int height = Height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];

                if (cell.type == Cell.Type.Mine) { //si la celda es una mina, no hace nada
                    continue;
                }

                //cuenta las minas adyacentes y asigna el n�mero a la celda
                cell.number = CountAdjacentMines(cell);
                cell.type = cell.number > 0 ? Cell.Type.Number : Cell.Type.Empty;
            }
        }
    }


    //m�todo para contar las minas adyacentes a una celda
    public int CountAdjacentMines(Cell cell)
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) {
                    continue;
                }

                int x = cell.position.x + adjacentX;
                int y = cell.position.y + adjacentY;

                if (TryGetCell(x, y, out Cell adjacent) && adjacent.type == Cell.Type.Mine) {
                    count++;
                }
            }
        }

        return count;
    }

    //m�todo para contar las banderas adyacentes a una celda
    public int CountAdjacentFlags(Cell cell)
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) {
                    continue;
                }

                int x = cell.position.x + adjacentX;
                int y = cell.position.y + adjacentY;

                if (TryGetCell(x, y, out Cell adjacent) && !adjacent.revealed && adjacent.flagged) {
                    count++;
                }
            }
        }

        return count;
    }

    // m�todo para obtener una celda en una posici�n espec�fica
    public Cell GetCell(int x, int y)
    {
        if (InBounds(x, y)) {
            return cells[x, y];
        } else {
            return null;
        }
    }

    // m�todo para INTENTAR obtener una celda en una posici�n espec�fica
    public bool TryGetCell(int x, int y, out Cell cell)
    {
        cell = GetCell(x, y);
        return cell != null;
    }

    //m�todo para verificar si una posici�n est� dentro de los l�mites del tablero
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    //m�todo para verificar si dos celdas son adyacentes
    public bool IsAdjacent(Cell a, Cell b)
    {
        return Mathf.Abs(a.position.x - b.position.x) <= 1 &&
               Mathf.Abs(a.position.y - b.position.y) <= 1;
    }

}
