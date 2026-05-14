using UnityEngine;
using UnityEngine.EventSystems;

public class FancyButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale")]
    public float hoverScale = 1.1f;
    public float clickScale = 0.95f;
    public float smoothSpeed = 10f;

    Vector3 originalScale;
    Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            smoothSpeed * Time.deltaTime
        );
    }

    // Mouse entrou
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    // Mouse saiu
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    // Clique apertado
    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScale;
    }

    // Clique solto
    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }
}