
// [Summary] (By Wessel)
//
// This script is for testing action subscriptions.
//

using System;
using UnityEngine;

public class InputTester : MonoBehaviour
{
    // Changeable
    [Header("Debugging")]
    [SerializeField] private string _inputText = "Input Received";

    // Input Handler
    private InputHandler _inputHandler => InputHandler.Instance;
    private bool _isRegistered = false;


    // ----------------- Functions -----------------


    #region OnEnable Functions

    private void OnEnable()
    {
        RegisterInputHandlerRelated();
    }

    private void RegisterInputHandlerRelated()
    {
        // Safety check.
        if (_isRegistered)
            return;
        _isRegistered = true;

        // Safely register input handler related things.
        _inputHandler.OnMousePositionPerformed += InputResult; // Rename this testing!
    }

    #endregion

    #region OnDisable Functions

    private void OnDisable()
    {
        UnregisterInputHandlerRelated();
    }

    private void UnregisterInputHandlerRelated()
    {
        // Safety check.
        if (!_isRegistered)
            return;
        _isRegistered = false;

        // Safely unregister input handler related things.
        _inputHandler.OnMousePositionPerformed -= InputResult; // Rename this testing!
    }

    #endregion

    #region Subscription Functions

    // The input result.
    private void InputResult(Vector2 position)
    {
        Debug.Log(_inputText + " (" + this.gameObject.name + position + ")");
    }

    #endregion

}