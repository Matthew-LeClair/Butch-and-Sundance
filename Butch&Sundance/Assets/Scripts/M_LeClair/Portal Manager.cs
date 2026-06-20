using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    [SerializeField] Portals portalA;
    [SerializeField] Portals portalB;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
