using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq.Expressions;

public class MagicCircleDataLinks: MonoBehaviour
{

    public MagicCircle source;
    public MagicCircle destination;
    MagicCircle oldSource;
    MagicCircle oldDestination;

    public delegate object InputFunction();
    public InputFunction inputFunction;
    public LinkableData<object> output;

    public List<string> availableProperties;
    public List<string> availableLinkableProperties;
    public List<string> activatableFunctions;

    public string selectedProperty;
    public string selectedLinkableProperty;
    public string selectedActivatableFunction;
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
            if( source != null && destination != null && selectedProperty != null )
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

    // only links the activator method if the input is a boolean
    void LinkActivatorFunction()
    {
        MethodInfo mi = source.GetType().GetMethod( selectedProperty );
        if( mi.ReturnType == typeof( bool ) )
        {
            boolLinkedFunction = mi;
            linkedActivatableFunction = destination.GetType().GetMethod( selectedActivatableFunction );
            print("Successfully linked bool with activatable function");
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
                print("Activated the method");
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
                MonoBehaviour.print("Assembly Name: " + typeof(GameObject).AssemblyQualifiedName );
                string AssemblyName = typeof(GameObject).AssemblyQualifiedName;
                linkTypeStr = linkTypeStr + AssemblyName.Substring(AssemblyName.IndexOf(",") );
            }
            conversionTypes[0] = mi.ReturnType;
            conversionTypes[1] = Type.GetType(linkTypeStr);
            print( "you need to convert " + conversionTypes[0].ToString() + " to " + conversionTypes[1].ToString() );
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
        print("picking conversion from " + originType.ToString() + " to " + targetType.ToString() );

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

                var d1 = typeof(DataLinkConverter<>);
                Type[] typeArgs = { targetType };
                var makeme = d1.MakeGenericType( typeArgs );
                object o = Activator.CreateInstance( makeme, mi, source );
                var convertedLinkedMethod = Convert.ChangeType( Delegate.CreateDelegate( setLinkedValueMethod.GetParameters()[0].ParameterType, o, o.GetType().GetMethod("ConvertToType"), true ), setLinkedValueMethod.GetParameters()[0].ParameterType );

                object[] myParams = new object[1];
                myParams[0] = convertedLinkedMethod;
                MonoBehaviour.print(convertedLinkedMethod.ToString());

                setLinkedValueMethod.Invoke( fi.GetValue( destination ), myParams );
                print(" The conversion succeeded ");
            }
            catch( Exception e )
            {
                print(" The conversion failed with exception " + e.ToString() );
            }
        }
    }

    void GameObjectConversions( Type targetType )
    {
        print( "GameObject conversions" );
    }

    void Vector3Conversions( Type targetType )
    {
        print( "Vector3 conversions" );
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
            MonoBehaviour.print("Here is the converted value " + convertedType.ToString() );
            return convertedType;
        }
    }
}
