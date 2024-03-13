using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //https://www.youtube.com/watch?v=mRkFj8J7y_I

    private PlayerInputActions playerInputActions;
    private Camera _mainCamera;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        _mainCamera = Camera.main;
    }

    void Start()
    {
        playerInputActions.Enable();
        playerInputActions.Standard.Clicked.performed += Click;
        Debug.Log("ola");
    }

    public void Click(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;

        //rayHit.collider.gameObject.GetComponent<Enemy>().OnKilled();
    }
}
