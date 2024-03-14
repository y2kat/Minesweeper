using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int width = 16; 
    public int height = 16; 
    public int mineCount = 32; // número de minas en el tablero

    private Board board; // referencia al tablero de juego
    private CellGrid grid; // referencia a la cuadrícula de celdas del tablero
    private bool gameover; // bandera que indica si el juego ha terminado
    private bool generated; // bandera que indica si el tablero de juego ha sido generado

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height); // asegura que el número de minas no exceda el número de celdas en el tablero
    }

    private void Awake()
    {
        // establece la tasa de fotogramas objetivo y obtiene una referencia al tablero de juego

        Application.targetFrameRate = 60;

        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        // detiene todas las corutinas, establece la posición de la cámara, reinicia las banderas del juego y genera un nuevo tablero de juego

        StopAllCoroutines();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

        gameover = false;
        generated = false;

        grid = new CellGrid(width, height);
        board.Draw(grid);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NewGame();
            return;
        }

        if (!gameover)
        {
            if (Input.GetMouseButtonDown(0)) {
                Reveal();
            } else if (Input.GetMouseButtonDown(1)) {
                Flag();
            } else if (Input.GetMouseButton(2)) {
                Chord();
            } else if (Input.GetMouseButtonUp(2)) {
                Unchord();
            }
        }
    }

    private void Reveal()
    {
        if (TryGetCellAtMousePosition(out Cell cell)) // intenta obtener una celda en la posición del mouse y, si tiene éxito, revela la celda
        {
            if (!generated)
            {
                grid.GenerateMines(cell, mineCount);
                grid.GenerateNumbers();
                generated = true;
            }

            Reveal(cell);
        }
    }

    private void Reveal(Cell cell)
    {
        // revela una celda específica, dependiendo de su tipo

        if (cell.revealed) return;
        if (cell.flagged) return;

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;

            case Cell.Type.Empty:
                StartCoroutine(Flood(cell));
                CheckWinCondition();
                break;

            default:
                cell.revealed = true;
                CheckWinCondition();
                break;
        }

        board.Draw(grid);
    }

    private IEnumerator Flood(Cell cell)
    {
        // verifica si el juego ha terminado, si la celda ya ha sido revelada o si la celda es una mina

        if (gameover) yield break; // si el juego ha terminado, detiene la función
        if (cell.revealed) yield break; // si la celda ya ha sido revelada, detiene la función
        if (cell.type == Cell.Type.Mine) yield break; //si la celda es una mina, detiene la función

        // marca la celda como revelada y dibuja el tablero.
        cell.revealed = true;
        board.Draw(grid);

        // espera un frame antes de continuar.
        yield return null;

        // si la celda es vacía, inicia el "flood" en todas las celdas adyacentes (revela automáticamente todas las celdas vacías adyacentes a una celda dada)
        if (cell.type == Cell.Type.Empty)
        {
            if (grid.TryGetCell(cell.position.x - 1, cell.position.y, out Cell left))
            {
                StartCoroutine(Flood(left)); //a la izquierda
            }
            if (grid.TryGetCell(cell.position.x + 1, cell.position.y, out Cell right))
            {
                StartCoroutine(Flood(right)); //a la derecha
            }
            if (grid.TryGetCell(cell.position.x, cell.position.y - 1, out Cell down))
            {
                StartCoroutine(Flood(down)); //la celda de abajo
            }
            if (grid.TryGetCell(cell.position.x, cell.position.y + 1, out Cell up))
            {
                StartCoroutine(Flood(up)); //la celda de arriba
            }
        }
    }

    private void Flag()
    {
        // intenta obtener una celda en la posición del mouse
        if (!TryGetCellAtMousePosition(out Cell cell)) return; // si no puede obtener una celda, detiene la función

        // verifica si la celda ya ha sido revelada
        if (cell.revealed) return; // si la celda ya ha sido revelada, detiene la función

        // cambia el estado de marcado de la celda (si estaba marcada, la desmarca; si no estaba marcada, la marca)
        cell.flagged = !cell.flagged;

        //dibuja el tablero con las celdas actualizadas
        board.Draw(grid);
    }

    //cuando un jugador “acuerda” una celda, está indicando que cree que todas las minas adyacentes a esa celda ya han sido marcadas correctamente con banderas
    //si el número en la celda acordada coincide con el número de banderas en las celdas adyacentes,
    //entonces todas las celdas adyacentes no marcadas se revelan automáticamente
    private void Chord()
    {
        // primero, desacuerda todas las celdas existentes
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].chorded = false;
            }
        }

        // luego, si se puede obtener una celda en la posición del mouse, acuerda las celdas adyacentes a esa celda
        if (TryGetCellAtMousePosition(out Cell chord))
        {
            for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
            {
                for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
                {
                    int x = chord.position.x + adjacentX;
                    int y = chord.position.y + adjacentY;

                    if (grid.TryGetCell(x, y, out Cell cell)) {
                        cell.chorded = !cell.revealed && !cell.flagged;
                    }
                }
            }
        }
        // finalmente, dibuja el tablero con las celdas actualizadas
        board.Draw(grid);
    }

    private void Unchord()
    {
        // "desacuerda" todas las celdas que estaban acordadas
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                if (cell.chorded) {
                    Unchord(cell);
                }
            }
        }

        board.Draw(grid);
    }

    private void Unchord(Cell chord)
    {
        // desacuerda una celda específica y luego verifica las celdas adyacentes
        chord.chorded = false;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) {
                    continue;
                }

                int x = chord.position.x + adjacentX;
                int y = chord.position.y + adjacentY;

                if (grid.TryGetCell(x, y, out Cell cell))
                {
                    if (cell.revealed && cell.type == Cell.Type.Number)
                    {
                        if (grid.CountAdjacentFlags(cell) >= cell.number)
                        {
                            Reveal(chord);
                            return;
                        }
                    }
                }
            }
        }
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game Over!");
        gameover = true;

        //establece la mina como explotada y revelada
        cell.exploded = true;
        cell.revealed = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = grid[x, y];

                if (cell.type == Cell.Type.Mine) {
                    cell.revealed = true;
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        //verifica si todas las celdas que no son minas han sido reveladas
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                if (cell.type != Cell.Type.Mine && !cell.revealed) {
                    return; // no win!!
                }
            }
        }

        //si todas las celdas que no son minas han sido reveladas, imprime un mensaje de "Winner!" y establece la bandera de gameover a verdadero
        Debug.Log("Winner!");
        gameover = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                if (cell.type == Cell.Type.Mine) {
                    cell.flagged = true;
                }
            }
        }
    }

    private bool TryGetCellAtMousePosition(out Cell cell) //parecido a lo que sucede con inputsystem
    {
        //convierte la posición del mouse de coordenadas de pantalla a coordenadas del mundo
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //convierte la posición del mundo a una posición de celda en la cuadrícula del tablero
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        //intenta obtener una celda en la posición de celda y devuelve el resultado
        return grid.TryGetCell(cellPosition.x, cellPosition.y, out cell);
    }

}
