using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput input;

    private void Start()
    {
        input = new PlayerInput();
        input.Enable();
    }

    private void Update()
    {
        Debug.Log(input.Default.Move.ReadValue<Vector2>());
    }
}
