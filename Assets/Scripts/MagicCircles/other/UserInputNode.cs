using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UserInputNode : SpellNode
{
    public UnityEvent onPressedEvent;
    public UnityEvent onReleasedEvent;

    public string inputCharacter;
    public float holdValue;
    public bool resetAfterHold = true;
    public bool isPressed = false;
    private const string space = " ";

    void Awake()
    {
        if (onPressedEvent == null)
            onPressedEvent = new UnityEvent();

        if (onReleasedEvent == null)
            onReleasedEvent = new UnityEvent();
    }

    void Update()
    {
        if( inputCharacter != null && inputCharacter.Length > 0 && GetKey() != null )
        {
            KeyCode currentKey = GetKey() ?? KeyCode.Space;
            if( Input.GetKeyDown( currentKey ) )
            {
                if( !isPressed )
                {
                    Debug.Log("Key: " + currentKey.ToString() + " was pressed");
                    onPressedEvent.Invoke();
                }
                isPressed = true;
            }
            else if( Input.GetKeyUp( currentKey ) )
            {
                if( isPressed )
                {
                    Debug.Log("Key: " + currentKey.ToString() + " was released");
                    onReleasedEvent.Invoke();
                }
                isPressed = false;
            }
        }
    }

    void FixedUpdate()
    {
        if( inputCharacter != null && inputCharacter.Length > 0 )
        {
            if( isPressed )
            {
                holdValue += Time.fixedDeltaTime;
            }
            else
            {
                if( resetAfterHold )
                {
                    holdValue = 0;
                }
            }
        }
    }

    public override MagicCircleType GetMcType()
    {
        return MagicCircleType.Input;
    }

    public Vector3 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public bool IsPressed()
    {
        return isPressed;
    }

    public float PressedDuration()
    {
        return holdValue;
    }

    public string GetKeyString()
    {
        return inputCharacter != null ? GetKey().ToString() : null;
    }

    KeyCode? GetKey()
    {
        if( inputCharacter == null )
            return null;

        string returnKey = inputCharacter.ToLower();
        if( returnKey == "tab" || returnKey == "enter" || returnKey == "ctrl"
            || returnKey == "f1" || returnKey == "f2" || returnKey == "f3" || returnKey == "f4" || returnKey == "f5"
            || returnKey == "f6" || returnKey == "f7" || returnKey == "f8" || returnKey == "f9" || returnKey == "f10"
            || returnKey == "f11" || returnKey == "f12" )
        {
            return (KeyCode) Enum.Parse( typeof(KeyCode), returnKey, true );
        }
        else if( returnKey == "1" || returnKey == "2" || returnKey == "3" || returnKey == "4" || returnKey == "5"
             || returnKey == "6" || returnKey == "7" || returnKey == "8" || returnKey == "9" || returnKey == "0" )
        {
            return (KeyCode) Enum.Parse( typeof(KeyCode), "Alpha" + returnKey, true );
        }
        else if( returnKey.Contains(" ") || returnKey.Contains("space") )
        {
            return KeyCode.Space;
        }
        else
        {
            return (KeyCode) Enum.Parse( typeof(KeyCode), returnKey[0].ToString(), true );
        }
    }

}
