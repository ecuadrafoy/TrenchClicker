using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputActions inputActions;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create the input actions
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        // Enable the Gameplay action map
        inputActions.Gameplay.Enable();

        // Subscribe to the SendSoldiers action
        inputActions.Gameplay.SendSoldiers.performed += OnSendSoldiersPerformed;
    }

    void OnDisable()
    {
        // Unsubscribe
        inputActions.Gameplay.SendSoldiers.performed -= OnSendSoldiersPerformed;

        // Disable the action map
        inputActions.Gameplay.Disable();
    }

    private void OnSendSoldiersPerformed(InputAction.CallbackContext context)
    {
        // Only allow sending soldiers if assault is active
        if (GameManager.Instance != null && GameManager.Instance.IsAssaultActive())
        {
            // Trigger the same method as clicking the button
            GameManager.Instance.OnSoldierClick();

            // Trigger the button animation
            UIManager.Instance?.TriggerClickButtonAnimation();
        }
    }
}