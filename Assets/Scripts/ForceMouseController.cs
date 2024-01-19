using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ForceMouseController : MonoBehaviour
{
    public AttractorForce target;

    public enum MouseButton
    {
        LMB,
        RMB
    };

    public MouseButton activationButton;

    void Update()
    {
        Mouse mouse = InputSystem.GetDevice<Mouse>();
        if (activationButton == MouseButton.LMB)
        {
            target.enabled = mouse.leftButton.IsPressed();
        }
        else if (activationButton == MouseButton.RMB)
        {
            target.enabled = mouse.rightButton.IsPressed();
        }

        if (Camera.main)
        {
            target.targetPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        }
        else
        {
            target.targetPos = mouse.position.ReadValue();
        }
    }
}
