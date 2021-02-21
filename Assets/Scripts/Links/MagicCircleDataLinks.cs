using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq.Expressions;

public class MagicCircleDataLinks: MagicCircleLinks
{

    // These are declared in the parent class
    // public MagicCircle source;
    // public MagicCircle destination;
    SpellNode oldSource;
    SpellNode oldDestination;

    public delegate object InputFunction();
    public InputFunction inputFunction;
    public LinkableData<object> output;

    public List<string> availableProperties;
    public List<string> availableLinkableProperties;
    public List<string> activatableFunctions;

    public string selectedProperty;
    public string selectedLinkableProperty;
    public string selectedActivatableFunction;
    private string previousLinkedProperty;
    public bool link;
    public bool invert;

    MethodInfo boolLinkedFunction;
    MethodInfo linkedActivatableFunction;
    bool lastActiveState;

    void Update()
    {
        if( source != oldSource )
        {
            selectedProperty = null;
            boolLinkedFunction = null;
            linkedActivatableFunction = null;

            if( source != null )
            {
                availableProperties = new List<string>( LinkableFinder.FindLinkableFunctions(source) );
                oldSource = source;
            }
        }

        if( destination != oldDestination )
        {
            selectedLinkableProperty = null;
            linkedActivatableFunction = null;

            if( destination != null )
            {
                availableLinkableProperties = new List<string>( LinkableFinder.FindAllLinkableProperty(destination) );
                activatableFunctions = new List<string>( LinkableFinder.FindLinkableFunctions(destination, true) );
                oldDestination = destination;
            }
        }


        if( link )
        {
            link = false;
            if( source != null && destination != null )
            {
                if( previousLinkedProperty != null && previousLinkedProperty != selectedLinkableProperty )
                {
                    ResetProperty( previousLinkedProperty );
                }
                if( selectedProperty != null )
                {
                    // Link properties
                    if( selectedLinkableProperty != null )
                    {
                        Type[] conversionTypes = GetConversion();
                        if( conversionTypes[0] != null )
                        {
                            PickConversionTypes( conversionTypes[0], conversionTypes[1] );
                        }
                        else
                        {
                            Debug.LogWarning("Failed to match one value with the destination or source");
                        }
                    }
                    // Link bool to activator function
                    if( selectedActivatableFunction != null )
                    {
                        LinkActivatorFunction();
                    }
                }
            }
        }

        DrawLink();
    }

    public void ResetProperty( string property )
    {
        if( destination != null && property != null && property.Length > 1 )
        {
            var lastPropertyExistance = destination.GetType().GetField( property );
            if( lastPropertyExistance != null )
            {
                var lastProperty = lastPropertyExistance.GetValue( destination );
                var resetValueFunction = lastProperty.GetType().GetMethod( "Reset" );
                if( resetValueFunction != null )
                {
                    resetValueFunction.Invoke( lastProperty, null );
                }
            }
        }
    }

    // only links the activator method if the input is a boolean
    void LinkActivatorFunction()
    {
        MethodInfo mi = source.GetType().GetMethod( selectedProperty );
        if( mi.ReturnType == typeof( bool ) )
        {
            boolLinkedFunction = mi;
            linkedActivatableFunction = destination.GetType().GetMethod( selectedActivatableFunction );
            Debug.Log("Successfully linked bool with activatable function");
        }
        else
        {
            boolLinkedFunction = null;
            linkedActivatableFunction = null;
        }
    }

    void FixedUpdate()
    {
        if( linkedActivatableFunction != null )
        {
            // Only Activate the method when the bool changes to true
            bool activeState = (bool) boolLinkedFunction.Invoke(source, null);
            if( activeState != invert && activeState != lastActiveState )
            {
                // Activate the function
                linkedActivatableFunction.Invoke(destination, null);
                Debug.Log("Activated the method");
            }
            lastActiveState = activeState;
        }
    }

    Type[] GetConversion()
    {
        Type[] conversionTypes = new Type[2];
        MethodInfo mi = source.GetType().GetMethod( selectedProperty );
        FieldInfo fi = destination.GetType().GetField( selectedLinkableProperty );
        if( fi != null && mi != null )
        {
            string linkTypeStr = fi.FieldType.ToString().Split('[')[1].Split(']')[0];
            if( linkTypeStr.Contains("UnityEngine") )
            {
                string AssemblyName = typeof(GameObject).AssemblyQualifiedName;
                linkTypeStr = linkTypeStr + AssemblyName.Substring(AssemblyName.IndexOf(",") );
            }
            conversionTypes[0] = mi.ReturnType;
            conversionTypes[1] = Type.GetType(linkTypeStr);
            return conversionTypes;
        }
        else
        {
            conversionTypes[0] = null;
            conversionTypes[1] = null;
            return conversionTypes;
        }
    }

    void PickConversionTypes( Type originType, Type targetType )
    {
        if( originType == targetType )
        {
            LinkableFinder.LinkField( selectedLinkableProperty, destination, selectedProperty, source );
        }

        if( originType == typeof( GameObject ) )
        {
            GameObjectConversions( targetType );
        }
        else if( originType == typeof( Vector3 ) )
        {
            Vector3Conversions( targetType );
        }
        else
        {
            try
            {
                var convertTest = Convert.ChangeType( Activator.CreateInstance( originType ), targetType );

                MethodInfo mi = source.GetType().GetMethod( selectedProperty );
                FieldInfo fi = destination.GetType().GetField( selectedLinkableProperty );
                Debug.Log( "Linking " + mi.Name + " from " + source.GetType().ToString() + " to "+ fi.Name + " from " + destination.GetType().ToString() );

                MethodInfo setLinkedValueMethod = fi.FieldType.GetMethod("SetLinkedValue");

                object makemeSource = source;
                MethodInfo makemeMI = mi;
                if( invert && originType == typeof(bool) )
                {
                    BoolFunctionInverter bfi = new BoolFunctionInverter( mi, source );
                    makemeMI = bfi.GetType().GetMethod("InvertFunction");
                    makemeSource = bfi;

                    // Func<bool> invertedMethod = (() => !(bool) mi.Invoke( source, null ));
                    // makemeMI = RuntimeReflectionExtensions.GetMethodInfo(invertedMethod);
                    // makemeSource = this;
                    print( mi.ToString() );
                    print( makemeMI );
                }
                var d1 = typeof(DataLinkConverter<>);
                Type[] typeArgs = { targetType };
                var makeme = d1.MakeGenericType( typeArgs );
                object o = Activator.CreateInstance( makeme, makemeMI, makemeSource );
                var convertedLinkedMethod = Convert.ChangeType( Delegate.CreateDelegate( setLinkedValueMethod.GetParameters()[0].ParameterType, o, o.GetType().GetMethod("ConvertToType"), true ), setLinkedValueMethod.GetParameters()[0].ParameterType );

                object[] myParams = new object[1];
                myParams[0] = convertedLinkedMethod;
                MonoBehaviour.print(convertedLinkedMethod.ToString());

                // if( fi.GetType() == typeof( bool ) && invert )
                // {
                //     setLinkedValueMethod.Invoke( !fi.GetValue( destination ), myParams );
                // }
                // else
                // {
                    setLinkedValueMethod.Invoke( fi.GetValue( destination ), myParams );
                // }
                Debug.Log(" The conversion succeeded ");
            }
            catch( Exception e )
            {
                Debug.LogWarning(" The conversion failed with exception " + e.ToString() );
            }
        }
    }

    public override void SetSource( SpellNode newSource )
    {
        if( newSource != oldSource )
        {
            selectedProperty = null;
            boolLinkedFunction = null;
            linkedActivatableFunction = null;

            if( newSource != null )
            {
                availableProperties = new List<string>( LinkableFinder.FindLinkableFunctions( newSource ) );
                oldSource = newSource;
                source = newSource;
            }
        }
    }

    public override void SetDestination( SpellNode newDestination )
    {
        if( newDestination != oldDestination )
        {
            selectedLinkableProperty = null;
            linkedActivatableFunction = null;

            if( newDestination != null )
            {
                availableLinkableProperties = new List<string>( LinkableFinder.FindAllLinkableProperty(newDestination) );
                activatableFunctions = new List<string>( LinkableFinder.FindLinkableFunctions(newDestination, true) );
                oldDestination = newDestination;
                destination = newDestination;
            }
        }
    }

    void GameObjectConversions( Type targetType )
    {
        Debug.LogWarning( "GameObject conversions not implemented yet" );
    }

    void Vector3Conversions( Type targetType )
    {
        Debug.LogWarning( "Vector3 conversions not implemented yet" );
    }

    public class DataLinkConverter<T>
    {
        MethodInfo mi;
        object source;
        public DataLinkConverter( MethodInfo newMi, object newSource )
        {
            mi = newMi;
            source = newSource;
        }
        public T ConvertToType()
        {
            T convertedType = (T)Convert.ChangeType( mi.Invoke(source, null), typeof(T) );
            // MonoBehaviour.print("Here is the converted value " + convertedType.ToString() );
            return convertedType;
        }
    }
    public class BoolFunctionInverter
    {
        MethodInfo mi;
        object source;
        public BoolFunctionInverter( MethodInfo newMi, object newSource )
        {
            mi = newMi;
            source = newSource;
        }
        public bool InvertFunction()
        {
            return !(bool) mi.Invoke( source, null );
        }
    }

    public void UpdateSourceAndDestination()
    {
        oldSource = source;
        oldDestination = destination;

        if( source != null )
        {
            availableProperties = new List<string>( LinkableFinder.FindLinkableFunctions(source) );
        }

        if( destination != null )
        {
            availableLinkableProperties = new List<string>( LinkableFinder.FindAllLinkableProperty(destination) );
            activatableFunctions = new List<string>( LinkableFinder.FindLinkableFunctions(destination, true) );
        }
    }

    void DrawLink()
    {
        if( source != null && destination != null )
        {
            Debug.DrawLine( source.transform.position, destination.transform.position, Color.green );
        }
    }

    public override LinkTypes GetLinkType()
    {
        return LinkTypes.Data;
    }
}
