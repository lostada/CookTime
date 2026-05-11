using Fusion;
using UnityEngine;

public class Ingredient : NetworkBehaviour
{
    public string ingredientName;

    private void OnMouseEnter()
    {
        // Opcional: mudar cor quando olhar
        GetComponent<Renderer>().material.color = Color.green;
    }

    private void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
}