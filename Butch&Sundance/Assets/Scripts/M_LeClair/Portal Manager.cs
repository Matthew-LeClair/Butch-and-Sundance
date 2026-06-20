using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    [SerializeField] Portals portalA;
    [SerializeField] Portals portalB;
    
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if(portalA != null && portalB != null)
        {
            portalA.LinkedPortal = portalB;
            portalB.LinkedPortal = portalA;
        }
    }

    public bool BothPortalsActive()
    {
        return portalA != null && portalB != null && portalA.gameObject.activeInHierarchy && portalB.gameObject.activeInHierarchy;
    }
}
