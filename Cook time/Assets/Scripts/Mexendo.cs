using UnityEngine;

public class Mexendo : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] float speed;
    Vector2 offset;
    void Start()
    {
        offset = Vector2.one;
    }
    void Update()
    {
        offset.x += speed * Time.deltaTime;
        material.mainTextureOffset = offset;
    }
}
