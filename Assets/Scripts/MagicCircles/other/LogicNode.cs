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

    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if( leftHandValue.Value() != null && rightHandValue.Value() != null )
        {
            print(" comparison result: " + GetResult() + " | " + leftHandValue.Value().ToString() + " " + compareType.ToString() + " " + rightHandValue.Value().ToString() );
        }
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

        if( compareType == LogicType.Equals )
        {
            return leftHandValue.Value().Equals( rightHandValue.Value() );
        }
        else if( compareType == LogicType.NotEquals )
        {
            return !leftHandValue.Value().Equals( rightHandValue.Value() );
        }
        else if( compareType == LogicType.And )
        {
            return ((bool) leftHandValue.Value()) && ((bool)rightHandValue.Value());
        }
        else if( compareType == LogicType.Or )
        {
            return ((bool) leftHandValue.Value()) && ((bool)rightHandValue.Value());
        }
        else
        {
            try
            {
                IComparable comparer = (IComparable) leftHandValue.Value();
                switch( compareType )
                {
                    case LogicType.GreaterThan:
                    {
                        return comparer.CompareTo(rightHandValue.Value()) > 0;
                    }
                    case LogicType.LessThan:
                    {
                        return comparer.CompareTo(rightHandValue.Value()) < 0;
                    }
                    case LogicType.GreaterThanOrEqual:
                    {
                        return comparer.CompareTo(rightHandValue.Value()) >= 0;
                    }
                    case LogicType.LessThanOrEqual:
                    {
                        return comparer.CompareTo(rightHandValue.Value()) <= 0;
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
