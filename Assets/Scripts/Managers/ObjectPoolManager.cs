using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
     [SerializeField] private Transform tileObjectParent;
     [SerializeField] private TileObject tileObjectPrefab;
     [SerializeField] private Transform tileParent;
     [SerializeField] private TileBackground tileBackgroundPrefab;
     [SerializeField] private TileObjectData tileObjectData;

     private List<TileObject> availableTileObjects = new List<TileObject>();
     private List<TileObject> currentlyUsedTileObjects = new List<TileObject>();
     
     private List<TileBackground> availableTileBackgrounds = new List<TileBackground>();
     private List<TileBackground> currentlyUsedTileBackgrounds = new List<TileBackground>();
    
     public TileObject GetTileObject(TileType tileType, Transform parent = null)
     {
          var tileObject = availableTileObjects.Count > 0 ? availableTileObjects[^1] : Instantiate(tileObjectPrefab, tileObjectParent);
          tileObject.transform.SetParent(parent == null ? tileObjectParent : parent);
          tileObject.Set(tileType, tileObjectData.GetSprite(tileType));
          currentlyUsedTileObjects.Add(tileObject);
          availableTileObjects.Remove(tileObject);
          return tileObject;
     }

     public TileBackground GetTileBackground()
     {
          var tile = availableTileBackgrounds.Count > 0 ? availableTileBackgrounds[^1] : Instantiate(tileBackgroundPrefab, tileParent);
          tile.gameObject.SetActive(true);
          var tileTransform = tile.transform;
          tileTransform.localPosition = Vector3.zero;
          tileTransform.localScale = Vector3.one;
          currentlyUsedTileBackgrounds.Add(tile);
          availableTileBackgrounds.Remove(tile);
          return tile;
     }

     public void ResetTileObject(TileObject tileObject)
     {
          tileObject.Reset();
          tileObject.transform.SetParent(tileObjectParent);
          currentlyUsedTileObjects.Remove(tileObject);
          availableTileObjects.Add(tileObject);
     }
     
     public void ResetTileBackground(TileBackground tileBackground)
     {
          tileBackground.Reset();
          tileBackground.transform.SetParent(tileParent);
          currentlyUsedTileBackgrounds.Remove(tileBackground);
          availableTileBackgrounds.Add(tileBackground);
     }
     
     private void ResetAllCellObjects()
     {
          foreach (var cellObject in currentlyUsedTileObjects)
          {
               ResetTileObject(cellObject);
          }
     }
     
     private void ResetAllTiles()
     {
          foreach (var tile in currentlyUsedTileBackgrounds)
          {
               ResetTileBackground(tile);
          }
     }

     public void ResetAllPoolObjects()
     {
          ResetAllCellObjects();
          ResetAllTiles();
     }
}