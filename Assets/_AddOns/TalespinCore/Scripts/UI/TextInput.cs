using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Talespin
{
    public class TextInput : MonoBehaviour
    {
        public UnityEngine.UI.InputField InputField;
        public bool IsListeningForHotKeys;

        public delegate void OnFocusDelegate();
        public delegate void OnUnFocusDelegate(string username);
        public OnFocusDelegate OnFocus;
        public OnUnFocusDelegate OnUnfocus;

        public delegate bool InputHandlerDelegate(string inputString);
        public InputHandlerDelegate InputValidityHandlerMethod;
        public bool IsValidInput;

        public void Enable(OnUnFocusDelegate unFocusCallback)
        {
            OnUnfocus = unFocusCallback;

            if (OnFocus != null)
            {
                OnFocus();
            }

            InputField.onEndEdit.AddListener((string s) => Disable());

            InputField.Select();
            InputField.ActivateInputField();

            InputField.onValueChanged.AddListener(ValueChangeCheck);
        }

        public void ValueChangeCheck(string inputString)
        {
            IsValidInput = true;

            if (InputValidityHandlerMethod != null)
            {
                IsValidInput = InputValidityHandlerMethod(inputString);
            }
        }

        public void Disable()
        {
            if (!IsValidInput)
            {
                return;
            }

            if (OnUnfocus != null)
            {
                OnUnfocus(InputField.text);
                OnUnfocus = null;
            }

            InputField.onEndEdit.RemoveAllListeners();
            InputField.DeactivateInputField();
        }
    }
}