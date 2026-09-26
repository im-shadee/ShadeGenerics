using System;
using UnityEngine.InputSystem;

namespace Shade.Generics
{
    [Serializable]
    public class InputButton : IDisposable
    {
        private readonly InputAction m_Action = null;
        
        /// <summary>
        /// True only during the frame the button was pressed.
        /// </summary>
        public bool Pressed => m_Action.WasPressedThisFrame();

        /// <summary>
        /// True as long as the button is held down.
        /// </summary>
        public bool Held => m_Action.IsPressed();

        /// <summary>
        /// True only during the frame the button was released.
        /// </summary>
        public bool Released => m_Action.WasReleasedThisFrame();

        /// <summary>
        /// Fires when this button is pressed.
        /// </summary>
        public event Action OnPressed = null;

        /// <summary>
        /// Fires when this button is released.
        /// </summary>
        public event Action OnReleased = null;

        public InputButton(InputAction action)
        {
            // Shade: Cache the action corresponding to this button, else throw an exception
            m_Action = action ?? throw new ArgumentNullException(nameof(action));
            
            m_Action.started += InvokePressed;
            m_Action.canceled += InvokeReleased;
        }
        
        private void InvokePressed(InputAction.CallbackContext ctx) => OnPressed?.Invoke();
        private void InvokeReleased(InputAction.CallbackContext ctx) => OnReleased?.Invoke();
        
        /// <summary>
        /// Called when destroying or releasing the wrapper to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            if (m_Action != null)
            {
                m_Action.started -= InvokePressed;
                m_Action.canceled -= InvokeReleased;
            }
        }
    }
}
