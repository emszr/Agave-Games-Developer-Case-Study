using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private Transform tileBackgroundsParent;
    [SerializeField] private Transform tileObjectsParent;
    [SerializeField] private TileObjectData tileObjectData;

    private const int DefaultScreenWidth = 1152;
    private const int DefaultScreenHeight = 2048;
    private const int DefaultTileCount = 8;
    
    public const int HorizontalTileCount = 8;
    public const int VerticalTileCount = 8;
    private const int TileSize = 256;

    private float scaleFactor = 1f;
    private Vector2 topLeftCornerOfGrid;
    private List<TileObject> tileObjects = new List<TileObject>();
    public List<InfoOfColumnEffectedFromMatch> columnMatchInfos = new List<InfoOfColumnEffectedFromMatch>();
    
    private int MinimumTileCountOnAnyAxis => Math.Min(HorizontalTileCount, VerticalTileCount);
    private float DefaultScreenResolution => (float) DefaultScreenWidth / DefaultScreenHeight;
    private float ScreenResolution => (float) Screen.width / Screen.height;
    private float CalculatedTileSize => TileSize / 100f * scaleFactor;
    
    private void Start()
    {
        PlaceTiles();
        ScaleGrid();
        SetGridPosition();
        InitializeLevel();
        SwipeManager.instance.CalculateTilePixelDistance(GetTileObject(0, 0), GetTileObject(0, 1));
    }

    # region Grid Initialization
    
    private void PlaceTiles()
    {
        const float calculatedTileSize = TileSize / 100f;
        const float firstHorizontalTilePlacementPosition = HorizontalTileCount % 2 == 0 ? calculatedTileSize / 2 + (HorizontalTileCount / 2 - 1) * calculatedTileSize : HorizontalTileCount / 2 * calculatedTileSize;
        const float firstVerticalTilePlacementPosition = VerticalTileCount % 2 == 0 ? - calculatedTileSize / 2 - (VerticalTileCount / 2 - 1) * calculatedTileSize : - VerticalTileCount / 2 * calculatedTileSize;
        
        for (var i = 0; i < HorizontalTileCount; i++)
        {
            for (var j = 0; j < VerticalTileCount; j++)
            {
                var tileBackground = ObjectPoolManager.instance.GetTileBackground();
                tileBackground.name = "TileBackground: " + i + "-" + j;
                var tileTransform = tileBackground.transform;
                tileTransform.SetParent(tileBackgroundsParent);
                var tilePosition = new Vector3(firstVerticalTilePlacementPosition + calculatedTileSize * j, firstHorizontalTilePlacementPosition - calculatedTileSize * i, 0f);
                tileTransform.position = tilePosition;
                var tileObject = ObjectPoolManager.instance.GetTileObject(TileType.None);
                tileObject.SetGridCoordinates(i, j);
                var tileObjectTransform = tileObject.transform;
                tileObjectTransform.SetParent(tileObjectsParent);
                tileObjectTransform.position = tilePosition;
                tileObject.name = "TileObject: " + i + "-" + j;
                tileObjects.Add(tileObject);
            }
        }
    }
    
    private void ScaleGrid()
    {
        scaleFactor = ScreenResolution / DefaultScreenResolution * ((float) DefaultTileCount / MinimumTileCountOnAnyAxis);
        if (ScreenResolution > 1f)
        {
            scaleFactor /= ScreenResolution;
        }
        
        gridParent.localScale = Vector3.one * scaleFactor;
    }

    private void SetGridPosition()
    {
        gridParent.position = Vector3.zero;
        topLeftCornerOfGrid = new Vector3(GetTileObject(0,0).transform.position.x - CalculatedTileSize / 2f, GetTileObject(0,0).transform.position.y + CalculatedTileSize / 2f);
    }

    private void InitializeLevel()
    {
        var cellTypes = new List<TileType>();
        for (var i = 0; i < HorizontalTileCount; i++)
        {
            for (var j = 0; j < VerticalTileCount; j++)
            {
                var cellType = (TileType) Extensions.random.Next(4);
                if (!IsCellTypeEligible(cellType, i, j))
                {
                    cellTypes.Clear();
                    for (var k = 0; k < 4; k++)
                    {
                        var temporaryCellType = (TileType) k;
                        if (IsCellTypeEligible(temporaryCellType, i, j) && cellType != temporaryCellType)
                        {
                            cellTypes.Add(temporaryCellType);
                        }
                    }

                    if (cellTypes.Count == 0)
                    {
                        Debug.Log("Grid cannot be created");
                        ResetTileObjectTypes();
                        InitializeLevel();
                        return;
                    }
                    
                    cellTypes.Shuffle();
                    cellType = cellTypes[Extensions.random.Next(cellTypes.Count - 1)];
                }                    
                
                tileObjects[ConvertCoordinates(i, j)].Set(cellType, tileObjectData.GetSprite(cellType));
            }
        }
    }

    private void ResetTileObjectTypes()
    {
        foreach (var tileObject in tileObjects)
        {
            tileObject.SetTileType(TileType.None);
        }
    }

    private bool IsCellTypeEligible(TileType tileType, int x, int y)
    {
        if (x > 1 && tileObjects[ConvertCoordinates(x - 1, y)].GetTileType() == tileType && tileObjects[ConvertCoordinates(x - 2, y)].GetTileType() == tileType)
        {
            return false;
        }
        
        if (x < HorizontalTileCount - 1 && tileObjects[ConvertCoordinates(x + 1, y)].GetTileType() == tileType && tileObjects[ConvertCoordinates(x + 2, y)].GetTileType() == tileType)
        {
            return false;
        }
        
        if (y > 1 && tileObjects[ConvertCoordinates(x, y - 1)].GetTileType() == tileType && tileObjects[ConvertCoordinates(x, y - 2)].GetTileType() == tileType)
        {
            return false;
        }
        
        if (y < VerticalTileCount - 1 && tileObjects[ConvertCoordinates(x, y + 1)].GetTileType() == tileType && tileObjects[ConvertCoordinates(x, y + 2)].GetTileType() == tileType)
        {
            return false;
        }
        
        return true;
    }
    
    private int ConvertCoordinates(int x, int y)
    {
        return x * VerticalTileCount + y;
    }
    
    #endregion

    public TileObject GetTileObject(int verticalCoordinate, int horizontalCoordinate)
    {
        return tileObjects.Find(tileObject => tileObject.HorizontalCoordinate == horizontalCoordinate && tileObject.VerticalCoordinate == verticalCoordinate);
    }

    public List<TileObject> GetColumn(int column)
    {
        return tileObjects.FindAll(tileObject => tileObject.HorizontalCoordinate == column);
    }
    
    public List<TileObject> GetRow(int row)
    {
        return tileObjects.FindAll(tileObject => tileObject.VerticalCoordinate == row);
    }
    
    public bool IsInsideGrid(Vector2 position)
    {
        return !(position.x <= topLeftCornerOfGrid.x || position.y >= topLeftCornerOfGrid.y || position.x >= topLeftCornerOfGrid.x + VerticalTileCount * CalculatedTileSize || position.y <= topLeftCornerOfGrid.y - HorizontalTileCount * CalculatedTileSize);
    }

    public int GetRowOfGrid(Vector2 position)
    {
        return (int)((topLeftCornerOfGrid.y - position.y) / CalculatedTileSize);
    }

    public int GetColumnOfGird(Vector2 position)
    {
        return (int)((position.x - topLeftCornerOfGrid.x) / CalculatedTileSize);
    }

    public Vector3 CalculateDestination(int i)
    {
        return Vector3.down * CalculatedTileSize * i;
    }
    
    public void DestroyTileObjects(List<TileObject> tileObjects)
    {
        foreach (var tileObject in tileObjects)
        {
            DestroyTileObject(tileObject);
        }
    }

    public async UniTask DestroyTileObject(TileObject tileObject)
    {
        tileObjects.Remove(tileObject);
        TryToAddColumnMatchInfos(tileObject);
        await tileObject.Destroy();
    }
    
    public TileObject SpawnTileObject(int verticalCoordinate, int horizontalCoordinate)
    {
        var tileType = (TileType) Extensions.random.Next(4);
        var tileObject = ObjectPoolManager.instance.GetTileObject(tileType, tileObjectsParent);
        tileObject.SetGridCoordinates(verticalCoordinate, horizontalCoordinate);
        tileObject.transform.position = topLeftCornerOfGrid + new Vector2((horizontalCoordinate + .5f) * CalculatedTileSize, .5f * CalculatedTileSize);
        tileObjects.Add(tileObject);
        return tileObject;
    }
    
    private void TryToAddColumnMatchInfos(TileObject tileObject)
    {
        var columnMatchInfo = columnMatchInfos.Find(info => info.Column == tileObject.HorizontalCoordinate);
        if (columnMatchInfo == null)
        {
            columnMatchInfos.Add(new InfoOfColumnEffectedFromMatch(tileObject.HorizontalCoordinate, tileObject.VerticalCoordinate));
        }
        else
        {
            columnMatchInfo.TryToUpdateHighestRow(tileObject.VerticalCoordinate);
        }
    }
}