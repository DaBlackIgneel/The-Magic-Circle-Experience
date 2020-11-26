using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// public enum LogicType
// {
//     Equals,
//     NotEquals,
//     GreaterThan,
//     LessThan,
//     GreaterThanOrEqual,
//     LessThanOrEqual,
//     And,
//     Or
// }

public enum InputDataType
{
    Automatic,
    Int,
    Float,
    Vector,
    Bool,
    Object
}

public class LogicNode : SpellNode
{
    public LogicType compareType;
    public InputDataType inputDataType;
    public LinkableData<object> leftHandValue = new LinkableData<object>(null);
    public LinkableData<object> rightHandValue = new LinkableData<object>(null);

    public object defaultLeftHandValue;
    public object defaultRightHandValue;

    void Update()
    {

    }

    void FixedUpdate()
    {
        // if( leftHandValue.Value() != null && rightHandValue.Value() != null )
        // {
        //     print(" comparison result: " + GetResult() + " | " + leftHandValue.Value().ToString() + " " + compareType.ToString() + " " + rightHandValue.Value().ToString() );
        // }
    }

    public override MagicCircleType GetMcType()
    {
        return MagicCircleType.Logic;
    }

    public bool GetResult()
    {
        if( leftHandValue.Value() == null && rightHandValue.Value() == null )
        {
            return false;
        }
        Type conversionType;
        // if( inputDataType != InputDataType.Automatic )
        // {
        switch( inputDataType )
        {
            case InputDataType.Int:
            {
                conversionType = typeof(int);
                break;
            }
            case InputDataType.Float:
            {
                conversionType = typeof(float);
                break;
            }
            case InputDataType.Vector:
            {
                conversionType = typeof(Vector3);
                break;
            }
            case InputDataType.Bool:
            {
                conversionType = typeof(bool);
                break;
            }
            case InputDataType.Object:
            {
                conversionType = typeof(object);
                break;
            }
            default:
            {
                if( leftHandValue.Value() == null || rightHandValue.Value() == null )
                {
                    conversionType = null;
                    break;
                }
                else if( leftHandValue.Value() != null && leftHandValue.Value().GetType().Equals( typeof(string) ) )
                {
                    float dummy;
                    if( float.TryParse( (string)leftHandValue.Value(), out dummy ) )
                    {
                        conversionType = typeof( float );
                        break;
                    }
                    else if( ((string)leftHandValue.Value()).ToLower() == "true" || ((string)leftHandValue.Value()).ToLower() == "false" )
                    {
                        conversionType = typeof( bool );
                        break;
                    }
                    print("COULD NOT PARSE LEFT HAND");
                }
                else if( rightHandValue.Value() != null && rightHandValue.Value().GetType().Equals( typeof(string) ) )
                {
                    float dummy;
                    if( float.TryParse( (string)rightHandValue.Value(), out dummy ) )
                    {
                        conversionType = typeof( float );
                        break;
                    }
                    else if( ((string)rightHandValue.Value()).ToLower() == "true" || ((string)rightHandValue.Value()).ToLower() == "false" )
                    {
                        conversionType = typeof( bool );
                        break;
                    }
                    print("COULD NOT PARSE RIGHT HAND");
                }
                // else
                // {
                //     conversionType = leftHandValue.Value().GetType();
                //     print( "setting left hand value to be " + conversionType );
                //     break;
                // }
                conversionType = null;
                break;
            }
        }
            // print(" Conversion to type: " + conversionType );

        var tempLeftHandValue = conversionType != null ? Convert.ChangeType( leftHandValue.Value(), conversionType ) : leftHandValue.Value();
        var tempRightHandValue = conversionType != null ? Convert.ChangeType( rightHandValue.Value(), conversionType ) : rightHandValue.Value();

        if( tempLeftHandValue == null )
        {
            // print( "LEFTHANDVALUE IS NULL");
            return false;
        }
        else
        {
            // print("Left hand type: " + tempLeftHandValue.GetType().ToString() );
            // print("Right hand type: " + tempRightHandValue.GetType().ToString() );
        }

        if( compareType == LogicType.Equals )
        {
            return tempLeftHandValue.Equals( tempRightHandValue );
        }
        else if( compareType == LogicType.NotEquals )
        {
            return !tempLeftHandValue.Equals( tempRightHandValue );
        }
        else if( compareType == LogicType.And )
        {
            return ((bool) tempLeftHandValue) && ((bool) tempRightHandValue);
        }
        else if( compareType == LogicType.Or )
        {
            return ((bool) tempLeftHandValue) && ((bool)tempRightHandValue);
        }
        else
        {
            try
            {
                IComparable comparer = (IComparable) ((float)tempLeftHandValue);
                IComparable rightComparer = (IComparable) ((float)tempRightHandValue);
                switch( compareType )
                {
                    case LogicType.GreaterThan:
                    {
                        return comparer.CompareTo(rightComparer) > 0;
                    }
                    case LogicType.LessThan:
                    {
                        return comparer.CompareTo(tempRightHandValue) < 0;
                    }
                    case LogicType.GreaterThanOrEqual:
                    {
                        return comparer.CompareTo(tempRightHandValue) >= 0;
                    }
                    case LogicType.LessThanOrEqual:
                    {
                        return comparer.CompareTo(tempRightHandValue) <= 0;
                    }
                    default:
                    {
                        Debug.LogWarning(" Logic Type not implemented: " + compareType );
                        return false;
                    }
                }
            }
            catch( Exception e )
            {
                Debug.Log( "Failed to compare values: " + e.ToString() );
                return false;
            }
        }
    }
}

// public class ComparableGroup
// {
//     public ComparisonType compareType;
//
// }
