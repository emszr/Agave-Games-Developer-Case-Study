using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private TileType tileType;
    public int VerticalCoordinate { get; private set; }
    public int HorizontalCoordinate { get; private set; }
    
    public TileType GetTileType()
    {
        return tileType;
    }
    
    public void SetTileType(TileType tileType)
    {
        this.tileType = tileType;
    }
    
    public void SetGridCoordinates(int verticalCoordinate, int horizontalCoordinate)
    {
        VerticalCoordinate = verticalCoordinate;
        HorizontalCoordinate = horizontalCoordinate;
    }
    
    public void Set(TileType tileType, Sprite sprite)
    {
        this.tileType = tileType;
        spriteRenderer.sprite = sprite;
        gameObject.SetActive(true);
        var cellObjectTransform = transform;
        cellObjectTransform.localScale = Vector3.one;
    }

    public async UniTask Destroy()
    {
        transform.DOScale(0f, GameManager.DelayTime / 1000f);
        await UniTask.Delay(GameManager.DelayTime);
        ObjectPoolManager.instance.ResetTileObject(this);
    }
    
    public void Reset()
    {
        tileType = TileType.None;
        VerticalCoordinate = -1;
        HorizontalCoordinate = -1;
        spriteRenderer.sprite = null;
        gameObject.SetActive(false);
        var cellObjectTransform = transform;
        cellObjectTransform.localScale = Vector3.zero;
        cellObjectTransform.localPosition = Vector3.zero;
    }
}

public enum TileType
{
    Blue,
    Green,
    Red,
    Yellow,
    None
}