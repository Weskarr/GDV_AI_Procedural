
// [Summary] (By Wessel)
//
// This script is the center of all inputs,
// by turning them into events which is more performance friendly.
//
// Great tutorial explaining most of this amazing script!
// https://youtu.be/lclDl-NGUMg
//

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // Changeable
    [Header("Required Asset")]
    [SerializeField, Tooltip("The Input Action Asset that's being used")]
    private InputActionAsset _playerControls;

    [Header("Required Map Reference")]
    [SerializeField, Tooltip("The Reference to the input action map name")]
    private string _actionMapName = "User";

    [Header("Required References")]
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _mousePosition = "MousePosition";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _mouseDeltaName = "MouseDelta";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _leftMouseName = "LeftMouse";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _rightMouseName = "RightMouse";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _middleMouseName = "MiddleMouse";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _scrollWheelDeltaName = "ScrollWheelDelta";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _backspaceName = "Backspace";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _enterName = "Enter";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _spaceName = "Space";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _deleteName = "Delete";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _leftArrowName = "LeftArrow";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _rightArrowName = "RightArrow";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _upArrowName = "UpArrow";
    [SerializeField, Tooltip("The Reference to the input action name")]
    private string _downArrowName = "DownArrow";

    // Singleton reference
    public static InputHandler Instance { get; private set; }

    // Inputs
    private InputAction _onMousePositionInput;
    private InputAction _onLeftMouseInput;
    private InputAction _onRightMouseInput;
    private InputAction _onMiddleMouseInput;
    private InputAction _onMouseDeltaInput;
    private InputAction _onScrollWheelDeltaInput;
    private InputAction _onBackspaceInput;
    private InputAction _onEnterInput;
    private InputAction _onSpaceInput;
    private InputAction _onDeleteInput;
    private InputAction _onLeftArrowInput;
    private InputAction _onRightArrowInput;
    private InputAction _onUpArrowInput;
    private InputAction _onDownArrowInput;

    // Actions (on TextInput)
    public event Action<char> OnCharTyped;

    // Actions (on Started)
    public event Action OnLeftMouseStarted;
    public event Action OnRightMouseStarted;
    public event Action OnMiddleMouseStarted;
    public event Action OnBackspaceStarted;
    public event Action OnEnterStarted;
    public event Action OnSpaceStarted;
    public event Action OnDeleteStarted;
    public event Action OnLeftArrowStarted;
    public event Action OnRightArrowStarted;
    public event Action OnUpArrowStarted;
    public event Action OnDownArrowStarted;
    public event Action OnScrollWheelStarted;

    // Actions (on Canceled)
    public event Action OnLeftMouseCanceled;
    public event Action OnRightMouseCanceled;
    public event Action OnMiddleMouseCanceled;
    public event Action OnBackspaceCanceled;
    public event Action OnEnterCanceled;
    public event Action OnSpaceCanceled;
    public event Action OnDeleteCanceled;
    public event Action OnLeftArrowCanceled;
    public event Action OnRightArrowCanceled;
    public event Action OnUpArrowCanceled;
    public event Action OnDownArrowCanceled;

    // Actions (on Performed)
    public event Action<Vector2> OnMouseDeltaPerformed;
    public event Action<Vector2> OnScrollWheelDeltaPerformed;
    public event Action<Vector2> OnMousePositionPerformed;

    // Actions (Combined)
    public event Action OnAnyMouseStarted;

    // Action (MousePosition)
    public Vector2 GetMousePosition => _onMousePositionInput.ReadValue<Vector2>();


    // ----------------- Functions -----------------


    #region Awake Functions

    // Absolute first thing this script does.
    private void Awake()
    {
        Singleton();
        AssignActions();
        RegisterInputActions();

        // Expand..
    }

    // Sets up singleton logic..
    private void Singleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persist across scenes.
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicate instances.
        }
    }

    // Assigns the actions based on the given references.
    private void AssignActions()
    {
        InputActionMap actionMap = _playerControls.FindActionMap(_actionMapName);

        _onMousePositionInput = actionMap.FindAction(_mousePosition);
        _onLeftMouseInput = actionMap.FindAction(_leftMouseName);
        _onRightMouseInput = actionMap.FindAction(_rightMouseName);
        _onMiddleMouseInput = actionMap.FindAction(_middleMouseName);
        _onMouseDeltaInput = actionMap.FindAction(_mouseDeltaName);
        _onScrollWheelDeltaInput = actionMap.FindAction(_scrollWheelDeltaName);
        _onBackspaceInput = actionMap.FindAction(_backspaceName);
        _onEnterInput = actionMap.FindAction(_enterName);
        _onSpaceInput = actionMap.FindAction(_spaceName);
        _onDeleteInput = actionMap.FindAction(_deleteName);
        _onLeftArrowInput = actionMap.FindAction(_leftArrowName);
        _onRightArrowInput = actionMap.FindAction(_rightArrowName);
        _onUpArrowInput = actionMap.FindAction(_upArrowName);
        _onDownArrowInput = actionMap.FindAction(_downArrowName);

        // Expand Assignments..
    }

    // Register the actions like a event.
    private void RegisterInputActions()
    {
        Keyboard.current.onTextInput += context => OnCharTyped?.Invoke(context);

        _onLeftMouseInput.started += context => OnLeftMouseStarted?.Invoke();
        _onRightMouseInput.started += context => OnRightMouseStarted?.Invoke();
        _onMiddleMouseInput.started += context => OnMiddleMouseStarted?.Invoke();
        _onBackspaceInput.started += context => OnBackspaceStarted?.Invoke();
        _onEnterInput.started += context => OnEnterStarted?.Invoke();
        _onSpaceInput.started += context => OnSpaceStarted?.Invoke();
        _onDeleteInput.started += context => OnDeleteStarted?.Invoke();
        _onLeftArrowInput.started += context => OnLeftArrowStarted?.Invoke();
        _onRightArrowInput.started += context => OnRightArrowStarted?.Invoke();
        _onUpArrowInput.started += context => OnUpArrowStarted?.Invoke();
        _onDownArrowInput.started += context => OnDownArrowStarted?.Invoke();
        _onScrollWheelDeltaInput.started += context => OnScrollWheelStarted?.Invoke();

        _onLeftMouseInput.canceled += context => OnLeftMouseCanceled?.Invoke();
        _onRightMouseInput.canceled += context => OnRightMouseCanceled?.Invoke();
        _onMiddleMouseInput.canceled += context => OnMiddleMouseCanceled?.Invoke();
        _onBackspaceInput.canceled += context => OnBackspaceCanceled?.Invoke();
        _onEnterInput.canceled += context => OnEnterCanceled?.Invoke();
        _onSpaceInput.canceled += context => OnSpaceCanceled?.Invoke();
        _onDeleteInput.canceled += context => OnDeleteCanceled?.Invoke();
        _onLeftArrowInput.canceled += context => OnLeftArrowCanceled?.Invoke();
        _onRightArrowInput.canceled += context => OnRightArrowCanceled?.Invoke();
        _onUpArrowInput.canceled += context => OnUpArrowCanceled?.Invoke();
        _onDownArrowInput.canceled += context => OnDownArrowCanceled?.Invoke();

        _onMouseDeltaInput.performed += context => OnMouseDeltaPerformed?.Invoke(context.ReadValue<Vector2>());
        _onScrollWheelDeltaInput.performed += context => OnScrollWheelDeltaPerformed?.Invoke(context.ReadValue<Vector2>());
        _onMousePositionInput.performed += context => OnMousePositionPerformed?.Invoke(context.ReadValue<Vector2>());

        // Special Case Combined:
        _onLeftMouseInput.started += context => OnAnyMouseStarted?.Invoke();
        _onRightMouseInput.started += context => OnAnyMouseStarted?.Invoke();
        _onMiddleMouseInput.started += context => OnAnyMouseStarted?.Invoke();
        //_onScrollWheelDeltaInput.started += context => OnAnyMouseStarted?.Invoke();

        // Expand Invokes..
    }

    #endregion

    #region OnEnable Functions

    // On enable changes.
    private void OnEnable()
    {
        EnableAllActions();

        // Expand..
    }

    // Enables all actions.
    private void EnableAllActions()
    {
        _onLeftMouseInput.Enable();
        _onRightMouseInput.Enable();
        _onMiddleMouseInput.Enable();
        _onMouseDeltaInput.Enable();
        _onScrollWheelDeltaInput.Enable();
        _onBackspaceInput.Enable();
        _onEnterInput.Enable();
        _onDeleteInput.Enable();
        _onLeftArrowInput.Enable();
        _onRightArrowInput.Enable();
        _onUpArrowInput.Enable();
        _onDownArrowInput.Enable();
        _onMousePositionInput.Enable();

        // Expand Inputs..
    }

    #endregion

    #region OnDisable Functions

    // On disable changes.
    private void OnDisable()
    {
        DisableAllActions();

        // Expand..
    }

    // Disables all actions.
    private void DisableAllActions()
    {
        _onLeftMouseInput.Disable();
        _onRightMouseInput.Disable();
        _onMiddleMouseInput.Disable();
        _onMouseDeltaInput.Disable();
        _onScrollWheelDeltaInput.Disable();
        _onBackspaceInput.Disable();
        _onEnterInput.Disable();
        _onDeleteInput.Disable();
        _onLeftArrowInput.Disable();
        _onRightArrowInput.Disable();
        _onUpArrowInput.Disable();
        _onDownArrowInput.Disable();
        _onMousePositionInput.Disable();

        // Expand Inputs..
    }

    #endregion

}