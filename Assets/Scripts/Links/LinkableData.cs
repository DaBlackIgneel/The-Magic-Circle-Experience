using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkableData<T>
{
    public delegate T GetValue();
    protected T defaultValue;
    protected GetValue linkedValue;
    protected object linkedObject;

    public LinkableData( T newDefaultValue )
    {
        defaultValue = newDefaultValue;
        linkedValue = null;
        linkedObject = null;
    }

    public virtual void SetLinkedValue( GetValue newLinkedValue )
    {
        linkedValue = newLinkedValue;
    }

    public virtual void SetLinkedValueObj( object newLinkedValue )
    {
        linkedObject = newLinkedValue;
    }

    public virtual void SetDefaultValue( T newValue )
    {
        defaultValue = newValue;
    }

    public virtual T Value()
    {
        if( linkedValue != null )
        {
            return linkedValue();
        }
        else if( linkedObject != null )
        {
            return (T)linkedObject;
        }
        else
        {
            return defaultValue;
        }
    }

    public virtual void Reset()
    {
        linkedValue = null;
        linkedObject = null;
    }

    public virtual object GetSource()
    {
        if( linkedValue != null )
        {
            return linkedValue.Target;
        }
        else
        {
            return null;
        }
    }
}
