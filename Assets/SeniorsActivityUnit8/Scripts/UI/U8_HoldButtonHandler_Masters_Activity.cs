using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Googolplex.Unit8
{
    /// <summary>
    /// Reliable Hold-to-Play button handler supporting mouse in Unity Editor and touch on Android/mobile.
    /// Uses IPointerDownHandler, IPointerUpHandler, IDragHandler, with an input polling fallback in Update
    /// to guarantee release is always detected even if the pointer moves slightly or drifts.
    /// </summary>
    public class U8_HoldButtonHandler_Masters_Activity : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public Action onPointerDown;
        public Action onPointerUp;

        private bool isPointerDown = false;

        public bool IsPointerDown => isPointerDown;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isPointerDown) return;

            isPointerDown = true;
            onPointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isPointerDown)
            {
                EndHold();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Implementing IDragHandler ensures Unity EventSystem keeps pointerPress locked to this object
            // even if the mouse or finger moves slightly while holding.
        }

        private void EndHold()
        {
            if (isPointerDown)
            {
                isPointerDown = false;
                onPointerUp?.Invoke();
            }
        }

        private void Update()
        {
            if (isPointerDown)
            {
                // Fallback check: if pointer is considered down but no mouse button or touch is active, release immediately
                bool isAnyInputActive = false;

                #if UNITY_EDITOR || UNITY_STANDALONE
                isAnyInputActive = Input.GetMouseButton(0);
                #elif UNITY_ANDROID || UNITY_IOS
                isAnyInputActive = Input.touchCount > 0;
                #else
                isAnyInputActive = Input.GetMouseButton(0) || Input.touchCount > 0;
                #endif

                if (!isAnyInputActive)
                {
                    EndHold();
                }
            }
        }

        private void OnDisable()
        {
            if (isPointerDown)
            {
                EndHold();
            }
        }
    }
}
