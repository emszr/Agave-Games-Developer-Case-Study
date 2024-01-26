using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IsColumnSpawnableButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI columnNUmberText;

    public bool isSpawnable;
    public int columnNUmber;

    public void Initialize(bool isSpawnable, int columnNUmber)
    {
        this.isSpawnable = isSpawnable;
        this.columnNUmber = columnNUmber;
        SetColor();
        columnNUmberText.text = columnNUmber.ToString();
    }
    
    public void OnClick()
    {
        isSpawnable = !isSpawnable;
        SetColor();
        GameManager.instance.spawnableColumns[columnNUmber - 1] = isSpawnable;
    }

    private void SetColor()
    {
        image.color = isSpawnable ? Color.green : Color.red;
    }
    
}
