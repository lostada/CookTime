using UnityEngine;

public class PanelChecker : MonoBehaviour
{
    public GameObject rotatingObjects;

    void OnEnable()
    {
        rotatingObjects.SetActive(false);
    }

    void OnDisable()
    {
        rotatingObjects.SetActive(true);
    }
}