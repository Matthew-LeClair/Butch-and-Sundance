using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class PuzzleSwitch : MonoBehaviour
{
    public enum SwitchColor { Red, Green, Blue, White }

    [SerializeField] SwitchColor currentColor = SwitchColor.White;
    [SerializeField] Renderer visual;

    [SerializeField] SwitchColor[] cycleOrder;

    public SwitchColor CurrentColor => currentColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateVisual();
    }

    private void OnMouseDown()
    {
        CycleColor();
        
    }

    void CycleColor()
    {
        int index = System.Array.IndexOf(cycleOrder, currentColor);
        index = (index +1) % cycleOrder.Length;

        currentColor = cycleOrder[index];
        UpdateVisual();

        PuzzleManager.Instance.CheckPuzzle();
    }

    // Update is called once per frame
    void UpdateVisual()
    {
        if (visual == null) return;        
        
        switch (currentColor)
        {
            case SwitchColor.Red: 
                visual.material.SetColor("_BaseColor", Color.red);
                visual.material.color = Color.red;
                break;
            case SwitchColor.Green: 
                visual.material.SetColor("_BaseColor", Color.green);
                visual.material.color = Color.green;
                break;
            case SwitchColor.Blue: 
                visual.material.SetColor("_BaseColor", Color.blue);
                visual.material.color = Color.blue;
                break;
        }
        
    }
}
