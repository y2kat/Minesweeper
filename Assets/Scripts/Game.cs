using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=HBrF8LJ0Hfg
//https://www.youtube.com/watch?v=wAJyWmyA0ug
public class Game : MonoBehaviour
{
    public int width = 16; 
    public int height = 16; 
    public int mineCount = 32; // n�mero de minas en el tablero

    private Board board; // referencia al tablero de juego
    private CellGrid grid; // referencia a la cuadr�cula de celdas del tablero
    private bool gameover; // bandera que indica si el juego ha terminado
    private bool generated; // bandera que indica si el tablero de juego ha sido generado

    public Timer timer;

    public TextMeshProUGUI minesText;
    private int minesLeft; // N�mero de minas que quedan

    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI highscoreText; // Referencia al objeto de texto del puntaje alto
    public GameObject gameOverPanel; // Referencia al panel de fin de juego
    private float timeElapsed; // Tiempo transcurrido en segundos
    private bool timerRunning; // Indica si el temporizador est� corriendo

    private static float highscore = float.MaxValue;
    private bool hasWon;

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height); // asegura que el n�mero de minas no exceda el n�mero de celdas en el tablero
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
        PlayerPrefs.DeleteKey("Highscore");
    }

    private void NewGame()
    {
        // detiene todas las corutinas, establece la posici�n de la c�mara, reinicia las banderas del juego y genera un nuevo tablero de juego

        StopAllCoroutines();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

        gameover = false;
        generated = false;

        grid = new CellGrid(width, height);
        board.Draw(grid);

        timer.StartTimer();

        minesLeft = mineCount;
        UpdateMinesText();

        timer.StartTimer(); // Inicia el temporizador
        timeElapsed = 0; // Reinicia el tiempo transcurrido
        gameOverPanel.SetActive(false); // Oculta el panel de fin de juego

        hasWon = false;
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
            timeElapsed += Time.deltaTime;

            if (Input.GetMouseButtonDown(0)) {
                Reveal();
            } else if (Input.GetMouseButtonDown(1)) {
                Flag();
            } else if (Input.GetMouseButton(2)) {
                Chord();
            } else if (Input.GetMouseButtonUp(2)) {
                Unchord();
            }

            timerText.text = "Time: " + timeElapsed.ToString("0.00");
        }
    }

    private void Reveal()
    {
        if (TryGetCellAtMousePosition(out Cell cell)) // intenta obtener una celda en la posici�n del mouse y, si tiene �xito, revela la celda
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
        // revela una celda espec�fica, dependiendo de su tipo

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

        if (gameover) yield break; // si el juego ha terminado, detiene la funci�n
        if (cell.revealed) yield break; // si la celda ya ha sido revelada, detiene la funci�n
        if (cell.type == Cell.Type.Mine) yield break; //si la celda es una mina, detiene la funci�n

        // marca la celda como revelada y dibuja el tablero.
        cell.revealed = true;
        board.Draw(grid);

        // espera un frame antes de continuar.
        yield return null;

        // si la celda es vac�a, inicia el "flood" en todas las celdas adyacentes (revela autom�ticamente todas las celdas vac�as adyacentes a una celda dada)
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
        // intenta obtener una celda en la posici�n del mouse
        if (!TryGetCellAtMousePosition(out Cell cell)) return; // si no puede obtener una celda, detiene la funci�n

        // verifica si la celda ya ha sido revelada
        if (cell.revealed) return; // si la celda ya ha sido revelada, detiene la funci�n

        // cambia el estado de marcado de la celda (si estaba marcada, la desmarca; si no estaba marcada, la marca)
        cell.flagged = !cell.flagged;

        minesLeft += cell.flagged ? -1 : 1;
        UpdateMinesText();

        //dibuja el tablero con las celdas actualizadas
        board.Draw(grid);
    }

    //cuando un jugador �acuerda� una celda, est� indicando que cree que todas las minas adyacentes a esa celda ya han sido marcadas correctamente con banderas
    //si el n�mero en la celda acordada coincide con el n�mero de banderas en las celdas adyacentes,
    //entonces todas las celdas adyacentes no marcadas se revelan autom�ticamente
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

        // luego, si se puede obtener una celda en la posici�n del mouse, acuerda las celdas adyacentes a esa celda
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
        // desacuerda una celda espec�fica y luego verifica las celdas adyacentes
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

        timer.StopTimer(); // Detiene el temporizador
        hasWon = false;
        ShowGameOver();
    }

    private void UpdateMinesText()
    {
        // Actualiza el texto de la UI para mostrar el n�mero de minas que quedan
        minesText.text = "Flags: " + minesLeft;
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

        if (gameover)
        {
            Debug.Log("Winner!");

            timer.StopTimer(); // Detiene el temporizador

            // Actualiza y guarda el puntaje m�s alto si el tiempo transcurrido es menor que el puntaje m�s alto actual
            UpdateHighscore(); // Actualiza el puntaje m�s alto

            hasWon = true;
            ShowGameOver(); // Muestra el panel de fin de juego
        }
    }

    private void ShowGameOver()
    {
        // Muestra el panel de fin de juego
        gameOverPanel.SetActive(true);

        if (hasWon)
        {
            Debug.Log("Winner!");
        }
        else
        {
            Debug.Log("Game Over!");
        }

        // Obtiene el puntaje m�s alto de PlayerPrefs
        float highscore = PlayerPrefs.GetFloat("Highscore", float.MaxValue);

        // Verifica si el puntaje m�s alto es igual a float.MaxValue
        if (highscore == float.MaxValue)
        {
            // Si es igual a float.MaxValue, muestra un mensaje que indica que no hay un puntaje m�s alto a�n
            highscoreText.text = "Highscore: No highscore yet";
        }
        else
        {
            // Si no es igual a float.MaxValue, muestra el puntaje m�s alto
            highscoreText.text = "Highscore: " + highscore.ToString("0.00");
        }
    }



    private void UpdateHighscore()
    {
        // Obtiene el puntaje m�s alto de PlayerPrefs
        float currentHighscore = PlayerPrefs.GetFloat("Highscore", float.MaxValue);

        Debug.Log("Highscore antes de la comparaci�n: " + currentHighscore);
        Debug.Log("Tiempo transcurrido: " + timeElapsed);

        if (timeElapsed < currentHighscore)
        {
            highscore = timeElapsed;
            PlayerPrefs.SetFloat("Highscore", highscore);
            PlayerPrefs.Save(); // Guarda los cambios en PlayerPrefs

            Debug.Log("Nuevo highscore: " + highscore);
        }

        highscoreText.text = "Highscore: " + highscore.ToString("0.00");
    }




    public void RestartGame()
    {
        NewGame();
    }

    public void Retry()
    {
        NewGame();
    }

    private bool TryGetCellAtMousePosition(out Cell cell) //parecido a lo que sucede con inputsystem
    {
        //convierte la posici�n del mouse de coordenadas de pantalla a coordenadas del mundo
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //convierte la posici�n del mundo a una posici�n de celda en la cuadr�cula del tablero
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        //intenta obtener una celda en la posici�n de celda y devuelve el resultado
        return grid.TryGetCell(cellPosition.x, cellPosition.y, out cell);
    } 

}
