using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System;
using System.Reflection;

public class LinkableFinder
{
    public static string[] FindAllLinkableProperty( SpellNode mc )
    {
        System.Reflection.FieldInfo[] fields = mc.GetType().GetFields();
        List<string> linkFields = new List<string>();
        foreach( FieldInfo fi in fields )
        {
            if( fi.FieldType.ToString().Contains("LinkableData" ) )
            {
                // MonoBehaviour.print( fi.Name + ": is a field of type " + fi.FieldType );
                linkFields.Add( fi.Name );
            }
        }
        return linkFields.ToArray();
    }

    public static string[] FindPropertiesToLinkTo( string sinkPropName, SpellNode sinkMC, SpellNode sourceMC )
    {
        string linkTypeStr = sinkMC.GetType().GetField(sinkPropName).FieldType.ToString().Split('[')[1].Split(']')[0];
        if( linkTypeStr.Contains("UnityEngine") )
        {
            // MonoBehaviour.print("Assembly Name: " + typeof(GameObject).AssemblyQualifiedName );
            string AssemblyName = typeof(GameObject).AssemblyQualifiedName;
            linkTypeStr = linkTypeStr + AssemblyName.Substring(AssemblyName.IndexOf(",") );
        }
        // MonoBehaviour.print("This is the type of the string: " + Type.GetType(typeof(GameObject).AssemblyQualifiedName));
        MonoBehaviour.print(sourceMC.GetType());
        System.Reflection.MethodInfo[] methods = sourceMC.GetType().GetMethods();
        List<string> linkFields = new List<string>();
        foreach( MethodInfo mi in methods )
        {
            if( mi.GetParameters().Length == 0 )
            {
                if( mi.ReturnType.ToString() == Type.GetType(linkTypeStr).ToString() )
                {
                    // MonoBehaviour.print(mi.Name + ": Has no parameters with a return type of " + mi.ReturnType);
                    linkFields.Add( mi.Name );
                }
            }
        }
        return linkFields.ToArray();
    }

    public static string[] FindLinkableFunctions( SpellNode mc, bool findVoid = false )
    {
        System.Reflection.MethodInfo[] methods = mc.GetType().GetMethods();
        List<string> linkableFunctions = new List<string>();
        foreach( MethodInfo mi in methods )
        {
            if( mi.GetParameters().Length == 0 )
            {
                if( !findVoid )
                {
                    if( mi.ReturnType.ToString() != typeof(void).ToString() )
                    {
                        // MonoBehaviour.print(mi.Name + ": Has no parameters with a return type of " + mi.ReturnType);
                        linkableFunctions.Add( mi.Name );
                    }
                }
                else
                {
                    if( mi.ReturnType.ToString() == typeof(void).ToString() )
                    {
                        // MonoBehaviour.print(mi.Name + ": Has no parameters with a return type of " + mi.ReturnType);
                        linkableFunctions.Add( mi.Name );
                    }
                }
            }
        }
        return linkableFunctions.ToArray();
    }

    public static string[] FindLinkablePropertyToLinkTo( string functionName, SpellNode sourceMc, SpellNode sinkMc )
    {
        MethodInfo mi = sourceMc.GetType().GetMethod( functionName );
        System.Reflection.FieldInfo[] fields = sinkMc.GetType().GetFields();
        List<string> linkFields = new List<string>();
        foreach( FieldInfo fi in fields )
        {
            if( fi.FieldType.ToString().Contains("LinkableData" ) )
            {
                string linkTypeStr = fi.FieldType.ToString().Split('[')[1].Split(']')[0];
                if( linkTypeStr.Contains("UnityEngine") )
                {
                    // MonoBehaviour.print("Assembly Name: " + typeof(GameObject).AssemblyQualifiedName );
                    string AssemblyName = typeof(GameObject).AssemblyQualifiedName;
                    linkTypeStr = linkTypeStr + AssemblyName.Substring(AssemblyName.IndexOf(",") );
                }
                if( Type.GetType(linkTypeStr).ToString() == mi.ReturnType.ToString() )
                {
                    // MonoBehaviour.print( fi.Name + ": is a field of type " + fi.FieldType );
                    linkFields.Add( fi.Name );
                }
            }
        }
        return linkFields.ToArray();
        // return mc.GetType();
    }

    public static void LinkField( string sinkPropName, SpellNode sinkMC, string sourcePropName, SpellNode sourceMC )
    {
        FieldInfo fi = sinkMC.GetType().GetField( sinkPropName );
        MethodInfo mi = sourceMC.GetType().GetMethod( sourcePropName );
        // Debug.Log( "Linking " + mi.Name + " from " + sourceMC.GetType().ToString() + " to "+ fi.Name + " from " + sinkMC.GetType().ToString() );
        MethodInfo setLinkedValueMethod = fi.FieldType.GetMethod("SetLinkedValue");
        object convertedLinkedMethod = Convert.ChangeType(Delegate.CreateDelegate( setLinkedValueMethod.GetParameters()[0].ParameterType, sourceMC, mi, true ), setLinkedValueMethod.GetParameters()[0].ParameterType );
        object[] myParams = new object[1];
        myParams[0] = convertedLinkedMethod;
        // MonoBehaviour.print(setLinkedValueMethod.GetGenericArguments()[0].ToString());
        setLinkedValueMethod.Invoke(fi.GetValue(sinkMC), myParams );
        // MonoBehaviour.print( setLinkedValueMethod.Name + " with return type " + setLinkedValueMethod.ReturnType + " and parameter " + setLinkedValueMethod.GetParameters()[0].ParameterType );
        // MonoBehaviour.print("convertedLinkedMethod" + convertedLinkedMethod.GetType() );
        // fi.GetType().GetMethod("SetLinkedValue").Invoke(fi.GetValue(sinkMC), myParams );
    }
}
