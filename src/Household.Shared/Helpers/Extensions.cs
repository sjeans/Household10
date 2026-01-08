using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json;

namespace Household.Shared.Helpers;

public static class Extensions
{
    /// <summary>
    /// Returns the inner most exception message
    /// </summary>
    /// <param name="exc">the exception that may have inner exceptions</param>
    /// <returns>the innermost exception message</returns>
    public static string GetInnerMessage(this Exception exc)
    {
        Exception e = exc;
        while (e.InnerException != null)
            e = e.InnerException;

        return e.Message;
    }

    /// <summary>
    /// Returns the description annotation of the enumeration value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerationValue"></param>
    /// <returns>enumeration value description</returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetDescription<T>(this T enumerationValue)
    where T : struct
    {
        Type type = enumerationValue.GetType();
        if (!type.IsEnum)
            throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));

        //Tries to find a DescriptionAttribute for a potential friendly name for the enum
        MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString() ?? "");
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)  //Pull out the description value
                return ((DescriptionAttribute)attrs[0]).Description;

        }

        //If we have no description attribute, just return the ToString of the enum
        return enumerationValue.ToString() ?? "";
    }

    /// <summary>
    /// Easier check for null or whitespace on a given string.
    /// </summary>
    /// <param name="input">the string value to check</param>
    /// <returns>true if null or whitespace, false otherwise</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? input)
    {
        return string.IsNullOrWhiteSpace(input);
    }

    public static bool DeepCompare(this object obj, object another)
    {
        if (ReferenceEquals(obj, another)) return true;
        if ((obj == null) || (another == null)) return false;
        //Compare two object's class, return false if they are difference
        if (obj.GetType() != another.GetType()) return false;

        var result = true;
        //Get all properties of obj
        //And compare each other
        foreach (var property in obj.GetType().GetProperties())
        {
            var objValue = property.GetValue(obj) ?? new object();
            var anotherValue = property.GetValue(another) ?? new object();
            //if (objValue is null && anotherValue is null) result = false;
            //if (objValue != null && anotherValue != null)
            if (!objValue.Equals(anotherValue)) result = false;
        }

        return result;
    }

    public static bool CompareEx(this object obj, object another)
    {
        if (ReferenceEquals(obj, another)) return true;
        if ((obj == null) || (another == null)) return false;
        if (obj.GetType() != another.GetType()) return false;

        //properties: int, double, DateTime, etc, not class
        if (!obj.GetType().IsClass) return obj.Equals(another);

        var result = true;
        foreach (var property in obj.GetType().GetProperties())
        {
            var objValue = property.GetValue(obj) ?? new object();
            var anotherValue = property.GetValue(another) ?? new object();
            //Recursion
            if (!objValue.DeepCompare(anotherValue)) result = false;
        }
        return result;
    }

    public static bool JsonCompare(this object obj, object another)
    {
        if (ReferenceEquals(obj, another)) return true;
        if ((obj == null) || (another == null)) return false;
        if (obj.GetType() != another.GetType()) return false;

        string objJson = JsonConvert.SerializeObject(obj);
        string anotherJson = JsonConvert.SerializeObject(another);

        return objJson == anotherJson;
    }
}
