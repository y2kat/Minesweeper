using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // referencia al objeto de texto de la UI
    private float timeElapsed = 0; // tiempo transcurrido en segundos
    private bool timerRunning = false; // indica si el temporizador está corriendo

    void Update()
    {
        // Si el temporizador está corriendo, incrementa el tiempo transcurrido
        if (timerRunning)
        {
            timeElapsed += Time.deltaTime;
            timerText.text = "Time: " + timeElapsed.ToString("0.00");
        }
    }

    public void StartTimer()
    {
        // reeinicia el tiempo transcurrido y comienza el temporizador
        timeElapsed = 0;
        timerRunning = true;
    }

    public void StopTimer()
    {
        // detiene el temporizador
        timerRunning = false;
    }
}
