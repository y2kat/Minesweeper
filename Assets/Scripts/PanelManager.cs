using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject[] panels;
    GameObject currentPanel;

    public void EnablePanel(int index)
    {
        if (index != 0) panels[0].SetActive(false);
        panels[index].SetActive(true);
        currentPanel = panels[index];
    }

    public void Return()
    {
        if (currentPanel == null) return;
        currentPanel.SetActive(false);
        panels[0].SetActive(true);
    }
}
