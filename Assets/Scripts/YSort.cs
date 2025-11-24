using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int baseOrder = 1000;   
    public int multiplier = 100;   

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // mientras más arriba en Y, menor orden, pero siempre alrededor de baseOrder
        sr.sortingOrder = baseOrder + Mathf.RoundToInt(-transform.position.y * multiplier);
    }
}
