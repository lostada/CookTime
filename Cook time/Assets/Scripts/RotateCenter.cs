using UnityEngine;

public class RotateCenter : MonoBehaviour
{
    [Header("Rotation")]
    public float speed = 50f;

    [Header("Pulse Effect")]
    public bool pulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;

    Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Rotação do grupo inteiro
        transform.Rotate(0f, 0f, speed * Time.deltaTime);

        // Efeito de pulsar
        if (pulse)
        {
            float scale =
                1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

            transform.localScale = originalScale * scale;
        }
    }
}