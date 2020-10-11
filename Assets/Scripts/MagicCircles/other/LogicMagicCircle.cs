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

public class LogicMagicCircle : MagicCircle
{
    public LogicType compareType;
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
            print(" comparison result: " + GetResult() );
        }
    }

    public bool GetResult()
    {
        if( leftHandValue.Value() == null && rightHandValue.Value() == null )
        {
            return false;
        }

        if( compareType == LogicType.Equals )
        {
            if( leftHandValue.Value() == null )
            {
                return defaultLeftHandValue == rightHandValue.Value();
            }
            else if( rightHandValue.Value() == null )
            {
                return leftHandValue.Value() == defaultRightHandValue;
            }
            else
            {
                return leftHandValue.Value() == rightHandValue.Value();
            }
        }
        else if( compareType == LogicType.NotEquals )
        {
            if( leftHandValue.Value() == null )
            {
                return defaultLeftHandValue != rightHandValue.Value();
            }
            else if( rightHandValue.Value() == null )
            {
                return leftHandValue.Value() != defaultRightHandValue;
            }
            else
            {
                return leftHandValue.Value() != rightHandValue.Value();
            }
        }
        else if( compareType == LogicType.And )
        {
            if( leftHandValue.Value() == null )
            {
                return ((bool) defaultLeftHandValue) && ((bool) rightHandValue.Value());
            }
            else if( rightHandValue.Value() == null )
            {
                return ((bool) leftHandValue.Value()) && ((bool) defaultRightHandValue);
            }
            else
            {
                return ((bool) leftHandValue.Value()) && ((bool)rightHandValue.Value());
            }
        }
        else if( compareType == LogicType.Or )
        {
            if( leftHandValue.Value() == null )
            {
                return ((bool) defaultLeftHandValue) && ((bool) rightHandValue.Value());
            }
            else if( rightHandValue.Value() == null )
            {
                return ((bool) leftHandValue.Value()) && ((bool) defaultRightHandValue);
            }
            else
            {
                return ((bool) leftHandValue.Value()) && ((bool)rightHandValue.Value());
            }
        }
        else
        {
            try
            {
                IComparable comparer;
                if( leftHandValue.Value() != null )
                {
                    comparer = (IComparable) leftHandValue.Value();
                }
                else
                {
                    comparer = (IComparable) defaultLeftHandValue;
                }
                switch( compareType )
                {
                    case LogicType.GreaterThan:
                    {
                        if( rightHandValue.Value() != null )
                        {
                            return comparer.CompareTo(rightHandValue.Value()) > 0;
                        }
                        else
                        {
                            return comparer.CompareTo(defaultRightHandValue) > 0;
                        }
                    }
                    case LogicType.LessThan:
                    {
                        if( rightHandValue.Value() != null )
                        {
                            return comparer.CompareTo(rightHandValue.Value()) < 0;
                        }
                        else
                        {
                            return comparer.CompareTo(defaultRightHandValue) < 0;
                        }
                    }
                    case LogicType.GreaterThanOrEqual:
                    {
                        if( rightHandValue.Value() != null )
                        {
                            return comparer.CompareTo(rightHandValue.Value()) >= 0;
                        }
                        else
                        {
                            return comparer.CompareTo(defaultRightHandValue) >= 0;
                        }
                    }
                    case LogicType.LessThanOrEqual:
                    {
                        if( rightHandValue.Value() != null )
                        {
                            return comparer.CompareTo(rightHandValue.Value()) <= 0;
                        }
                        else
                        {
                            return comparer.CompareTo(defaultRightHandValue) <= 0;
                        }
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
