using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DurableFunctions.StackSerialization.JsonConverters;

/// <summary>
/// Converter for any Stack<T> that prevents Json.NET from reversing its order when deserializing.
/// https://github.com/JamesNK/Newtonsoft.Json/issues/971
/// https://stackoverflow.com/a/39481981/4865541
/// </summary>
public class StackConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return StackParameterType(objectType) != null;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        try
        {
            Type parameterType = StackParameterType(objectType);
            MethodInfo method = GetType().GetMethod(nameof(ReadJsonGeneric), BindingFlags.NonPublic | BindingFlags.Static);

            MethodInfo genericMethod = method.MakeGenericMethod(new[] { parameterType });
            return genericMethod.Invoke(this, new object[] { reader, objectType, existingValue, serializer });
        }
        catch (TargetInvocationException ex)
        {
            // Wrap the TargetInvocationException in a JsonSerializerException
            throw new JsonSerializationException("Failed to deserialize " + objectType, ex);
        }
    }

    public override bool CanWrite { get { return false; } }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static Type StackParameterType(Type objectType)
    {
        while (objectType != null)
        {
            if (objectType.IsGenericType)
            {
                Type genericType = objectType.GetGenericTypeDefinition();
                if (genericType == typeof(Stack<>))
                {
                    return objectType.GetGenericArguments()[0];
                }
            }
            objectType = objectType.BaseType;
        }

        return null;
    }

    private static object ReadJsonGeneric<T>(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        List<T> list = serializer.Deserialize<List<T>>(reader);
        Stack<T> stack = existingValue as Stack<T> ?? (Stack<T>)serializer.ContractResolver.ResolveContract(objectType).DefaultCreator();
        for (int i = list.Count - 1; i >= 0; i--)
        {
            stack.Push(list[i]);
        }

        return stack;
    }
}
