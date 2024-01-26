using UnityEngine;

public class TileBackground : MonoBehaviour
{
    public void Set()
    {
        gameObject.SetActive(true);
        var tileBackgroundTransform = transform;
        tileBackgroundTransform.localPosition = Vector3.zero;
        tileBackgroundTransform.localScale = Vector3.one;
    }
    
    public void Reset()
    {
        gameObject.SetActive(false);
        var tileBackgroundTransform = transform;
        tileBackgroundTransform.localScale = Vector3.zero;
        tileBackgroundTransform.localPosition = Vector3.zero;
    }
}