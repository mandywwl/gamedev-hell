using UnityEngine;

public class RoofZoneSimple : MonoBehaviour
{
    [SerializeField] Renderer[] roofRenderers;  // drag the Roof's TilemapRenderer here

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SetVisible(false);
        Debug.Log("[RoofZone] Hide roof");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SetVisible(true);
        Debug.Log("[RoofZone] Show roof");
    }

    void SetVisible(bool visible)
    {
        foreach (var r in roofRenderers)
            if (r) r.enabled = visible;
    }
}
