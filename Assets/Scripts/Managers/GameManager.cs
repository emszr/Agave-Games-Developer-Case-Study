using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private IsColumnSpawnableButton isColumnSpawnableButtonPrefab;
    [SerializeField] private Transform isColumnSpawnableButtonsParent;
    
    private List<TileObject> matchedVerticalTileObjects = new List<TileObject>();
    private List<TileObject> matchedHorizontalTileObjects = new List<TileObject>();
    private List<TileObject> movedTileObjects = new List<TileObject>();
    private List<TileObject> tilesToDestroy = new List<TileObject>();

    public const int DelayTime = 300;
    
    public bool inputEnabled = true;

    public List<bool> spawnableColumns = new List<bool>();
    
    private void Start()
    {
        SwipeManager.OnSwipePerformed += OnSwipePerformed;
        for (var i = 0; i < GridManager.HorizontalTileCount; i++)
        {
            spawnableColumns.Add(true);
            var button = Instantiate(isColumnSpawnableButtonPrefab, isColumnSpawnableButtonsParent);
            button.Initialize(true, i + 1);
        }
    }
    
    private void OnDestroy()
    {
        SwipeManager.OnSwipePerformed -= OnSwipePerformed;
    }

    private async UniTask HandleInvalidSwipe(TileObject tileObject)
    {
        tileObject.transform.DOShakePosition(DelayTime / 1000f, .4f);
        await UniTask.Delay(DelayTime);
        inputEnabled = true;
    }
    
    private void OnSwipePerformed(Vector2 startPosition, Vector2 endPosition)
    {
        if (!GridManager.instance.IsInsideGrid(startPosition))
        {
            Debug.Log("Start touch is out Of grid");
            return;
        }

        inputEnabled = false;
        
        var startedCellVerticalCoordinate = GridManager.instance.GetRowOfGrid(startPosition);
        var startedCellHorizontalCoordinate = GridManager.instance.GetColumnOfGird(startPosition);

        var startedTileObject = GridManager.instance.GetTileObject(startedCellVerticalCoordinate, startedCellHorizontalCoordinate);

        if (startedTileObject == null)
        {
            inputEnabled = true;
            return;
        }
        
        var endedCellVerticalCoordinate = GridManager.instance.GetRowOfGrid(endPosition);
        var endedCellHorizontalCoordinate = GridManager.instance.GetColumnOfGird(endPosition);
        var endedTileObject = GridManager.instance.GetTileObject(endedCellVerticalCoordinate, endedCellHorizontalCoordinate);
        
        if (!GridManager.instance.IsInsideGrid(endPosition) || endedTileObject == null)
        {
            HandleInvalidSwipe(startedTileObject);
            Debug.Log("End cell out Of grid");
            return;
        }
        
        if (startedCellHorizontalCoordinate != endedCellHorizontalCoordinate && startedCellVerticalCoordinate != endedCellVerticalCoordinate)
        {
            Debug.Log("Diagonal Swipe Detected");
            inputEnabled = true;
            return;
        }

        if (Math.Abs(endedCellHorizontalCoordinate - startedCellHorizontalCoordinate) > 1 || Math.Abs(endedCellVerticalCoordinate - startedCellVerticalCoordinate) > 1)
        {
            Debug.Log("Cells are not neighbor");
            inputEnabled = true;
            return;
        }
        
        Debug.Log("Started TileBackground: " + startedCellVerticalCoordinate + "-" + startedCellHorizontalCoordinate);
        Debug.Log("Ended TileBackground: " + endedCellVerticalCoordinate + "-" + endedCellHorizontalCoordinate);
        
        startedTileObject = GridManager.instance.GetTileObject(startedCellVerticalCoordinate, startedCellHorizontalCoordinate);
        HandleMatch(startedTileObject, endedTileObject);
    }
    
    private async UniTask HandleMatch(TileObject startedTileObject, TileObject endedTileObject)
    {
        var startedTileObjectPosition = startedTileObject.transform.position;
        var endedTileObjectPosition = endedTileObject.transform.position;
        var startedTileObjectHorizontalCoordinate = startedTileObject.HorizontalCoordinate;
        var startedTileObjectVerticalCoordinate = startedTileObject.VerticalCoordinate;
        var endedTileObjectHorizontalCoordinate = endedTileObject.HorizontalCoordinate;
        var endedTileObjectVerticalCoordinate = endedTileObject.VerticalCoordinate;

        startedTileObject.transform.DOKill();
        endedTileObject.transform.DOKill();
        endedTileObject.transform.DOMove(startedTileObjectPosition, DelayTime / 1000f).SetEase(Ease.Linear);
        startedTileObject.transform.DOMove(endedTileObjectPosition, DelayTime / 1000f).SetEase(Ease.Linear);
        await UniTask.Delay(DelayTime);

        startedTileObject.SetGridCoordinates(endedTileObjectVerticalCoordinate, endedTileObjectHorizontalCoordinate);
        endedTileObject.SetGridCoordinates(startedTileObjectVerticalCoordinate, startedTileObjectHorizontalCoordinate);

        var matchHappened1 = CheckIfMatchHappened(startedTileObject);
        if (matchHappened1)
        {
            GridManager.instance.DestroyTileObject(startedTileObject);
            GridManager.instance.DestroyTileObjects(matchedHorizontalTileObjects);
            GridManager.instance.DestroyTileObjects(matchedVerticalTileObjects);
        }

        var matchHappened2 = CheckIfMatchHappened(endedTileObject);
        if (matchHappened2)
        {
            GridManager.instance.DestroyTileObject(endedTileObject);
            GridManager.instance.DestroyTileObjects(matchedHorizontalTileObjects);
            GridManager.instance.DestroyTileObjects(matchedVerticalTileObjects);
        }

        if (!matchHappened1 && !matchHappened2)
        {
            startedTileObject.transform.DOKill();
            endedTileObject.transform.DOKill();
            endedTileObject.transform.DOMove(endedTileObjectPosition, DelayTime / 1000f).SetEase(Ease.Linear);
            startedTileObject.transform.DOMove(startedTileObjectPosition, DelayTime / 1000f).SetEase(Ease.Linear);
            startedTileObject.SetGridCoordinates(startedTileObjectVerticalCoordinate, startedTileObjectHorizontalCoordinate);
            endedTileObject.SetGridCoordinates(endedTileObjectVerticalCoordinate, endedTileObjectHorizontalCoordinate);
            await UniTask.Delay(DelayTime);
            inputEnabled = true;
            return;
        }
        
        await UniTask.Delay(DelayTime);
        HandleMatchLoop();
    }
    
    private async UniTask HandleEffectedColumns()
    {
        movedTileObjects.Clear();
        foreach (var columnMatchInfo in GridManager.instance.columnMatchInfos)
        {
            columnMatchInfo.MatchedRows.Sort();
            var tileObjects = GridManager.instance.GetColumn(columnMatchInfo.Column);
            
            foreach (var tileObject in tileObjects)
            {
                var rowCounter = 0;
                foreach (var row in columnMatchInfo.MatchedRows)
                {
                    if (tileObject.VerticalCoordinate - row <= 0)
                    {
                        rowCounter++;
                    }
                }

                if (rowCounter == 0)
                {
                    continue;
                }
                    
                tileObject.SetGridCoordinates(tileObject.VerticalCoordinate + rowCounter, tileObject.HorizontalCoordinate);
                movedTileObjects.Add(tileObject);
                var targetPosition = tileObject.transform.position + GridManager.instance.CalculateDestination(rowCounter);
                tileObject.transform.DOKill();
                tileObject.transform.DOMove(targetPosition, DelayTime / 1000f);
            }

            if (!spawnableColumns[columnMatchInfo.Column])
            {
                continue;
            }
                
            var previouslyEmptyTileCount = GridManager.VerticalTileCount - (columnMatchInfo.NumberOfTiles + tileObjects.Count);
            for (var i = columnMatchInfo.NumberOfTiles - 1 + previouslyEmptyTileCount; i >= 0; i--)
            {
                var tileObject = GridManager.instance.SpawnTileObject(i, columnMatchInfo.Column);
                movedTileObjects.Add(tileObject);
                var targetPosition = tileObject.transform.position + GridManager.instance.CalculateDestination(i + 1);
                tileObject.transform.DOKill();
                tileObject.transform.DOMove(targetPosition, DelayTime / 1000f);
            }
        }
    }

    private async UniTask HandleMovedTileObjects()
    {
        tilesToDestroy.Clear();
        foreach (var tileObject in movedTileObjects)
        {
            if (tileObject == null || tilesToDestroy.Contains(tileObject))
            {
                continue;
            }

            var matchHappened = CheckIfMatchHappened(tileObject);
            if (matchHappened)
            {
                tilesToDestroy.Add(tileObject);
                foreach (var tile in matchedHorizontalTileObjects)
                {
                    if (!tilesToDestroy.Contains(tile))
                    {
                        tilesToDestroy.Add(tile);
                    }
                }
                    
                foreach (var tile in matchedVerticalTileObjects)
                {
                    if (!tilesToDestroy.Contains(tile))
                    {
                        tilesToDestroy.Add(tile);
                    }
                }
            }
        }
            
        GridManager.instance.columnMatchInfos.Clear();
        GridManager.instance.DestroyTileObjects(tilesToDestroy);
    }
    
    private async UniTask HandleMatchLoop()
    {
        while (GridManager.instance.columnMatchInfos.Count > 0)
        {
            HandleEffectedColumns();
            await UniTask.Delay(DelayTime);
            HandleMovedTileObjects();
            await UniTask.Delay(DelayTime);
        }

        inputEnabled = true;
    }
    
    private bool CheckIfMatchHappened(TileObject targetTileObject)
    {
        var targetCellIObjectHorizontalCoordinate = targetTileObject.HorizontalCoordinate;
        var targetCellIObjectVerticalCoordinate = targetTileObject.VerticalCoordinate;
        var targetCellType = targetTileObject.GetTileType();
        var horizontalResult = false;
        var verticalResult = false;

        // Check Horizontal Matches
        
        matchedHorizontalTileObjects.Clear();
        var i = targetCellIObjectHorizontalCoordinate - 1;
        while (i >= 0)
        {
            var checkedTileObject = GridManager.instance.GetTileObject(targetCellIObjectVerticalCoordinate, i);
            if (checkedTileObject == null || (checkedTileObject != null && checkedTileObject.GetTileType() != targetCellType))
            {
                break;
            }

            matchedHorizontalTileObjects.Add(checkedTileObject);
            i--;
        }

        i = targetCellIObjectHorizontalCoordinate + 1;
        while (i < GridManager.HorizontalTileCount)
        {
            var checkedTileObject = GridManager.instance.GetTileObject(targetCellIObjectVerticalCoordinate, i);
            if (checkedTileObject == null || (checkedTileObject != null && checkedTileObject.GetTileType() != targetCellType))
            {
                break;
            }

            matchedHorizontalTileObjects.Add(checkedTileObject);
            i++;
        }

        horizontalResult = matchedHorizontalTileObjects.Count >= 2;
        if (!horizontalResult)
        {
            matchedHorizontalTileObjects.Clear();
        }
        
        // Check Vertical Matches
        
        matchedVerticalTileObjects.Clear();
        i = targetCellIObjectVerticalCoordinate - 1;
        while (i >= 0)
        {
            var checkedTileObject = GridManager.instance.GetTileObject(i, targetCellIObjectHorizontalCoordinate);
            if (checkedTileObject == null || (checkedTileObject != null && checkedTileObject.GetTileType() != targetCellType))
            {
                break;
            }

            matchedVerticalTileObjects.Add(checkedTileObject);
            i--;
        }

        i = targetCellIObjectVerticalCoordinate + 1;
        while (i < GridManager.VerticalTileCount)
        {
            var checkedTileObject = GridManager.instance.GetTileObject(i, targetCellIObjectHorizontalCoordinate);
            if (checkedTileObject == null || (checkedTileObject != null && checkedTileObject.GetTileType() != targetCellType))
            {
                break;
            }

            matchedVerticalTileObjects.Add(checkedTileObject);
            i++;
        }
        
        verticalResult = matchedVerticalTileObjects.Count >= 2;
        if (!verticalResult)
        {
            matchedVerticalTileObjects.Clear();
        }
        
        return horizontalResult || verticalResult;
    }
}

public class InfoOfColumnEffectedFromMatch
{
    public readonly int Column;
    public readonly List<int> MatchedRows = new List<int>();
    
    public int NumberOfTiles => MatchedRows.Count;
    
    public InfoOfColumnEffectedFromMatch(int column, int lowestRow)
    {
        Column = column;
        MatchedRows.Add(lowestRow);
    }
    
    public void TryToUpdateHighestRow(int lowestRow)
    {
        if (!MatchedRows.Contains(lowestRow))
        {
            MatchedRows.Add(lowestRow);
        }
    }
}