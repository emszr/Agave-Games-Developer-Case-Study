using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileObjectData")]
public class TileObjectData : ScriptableObject
{
    [SerializeField] private List<Sprite> sprites;

    public Sprite GetSprite(TileType tileType)
    {
        return tileType == TileType.None ? null : sprites[(int) tileType];
    }
}