using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [System.Serializable]
    public struct SwitchGoal
    {
        public PuzzleSwitch switchRef;
        public PuzzleSwitch.SwitchColor requiredColor;
    }

    [SerializeField] SwitchGoal[] goals;
    [SerializeField] DoorBehavior linkedDoor;

    public bool IsSolved;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckPuzzle()
    {
        foreach (var g in goals)
        {
            if (g.switchRef.CurrentColor != g.requiredColor)
            {
                linkedDoor.SetExternalOpen(false);
                return;
            }
        }
        linkedDoor.SetExternalOpen(true);
        IsSolved = true;
    }
}
