using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DurableFunctions.Isolated.StackSerialization.Converters;

/// <summary>
/// Converter for any Stack<T>. System.Text.Json loses order when serializing/deserializing stack.
/// https://github.com/dotnet/docs/blob/main/docs/standard/serialization/system-text-json/snippets/how-to/csharp/JsonConverterFactoryForStackOfT.cs
public class JsonConverterFactoryForStackOfT : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert.IsGenericType
		&& typeToConvert.GetGenericTypeDefinition() == typeof(Stack<>);

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Debug.Assert(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Stack<>));

		Type elementType = typeToConvert.GetGenericArguments()[0];

		var converter = (JsonConverter)Activator.CreateInstance(
			typeof(JsonConverterForStackOfT<>).MakeGenericType([elementType]),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: null,
			culture: null)!;

		return converter;
	}
}

public class JsonConverterForStackOfT<T> : JsonConverter<Stack<T>>
{
	public override Stack<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
		{
			throw new JsonException();
		}
		reader.Read();

		var elements = new Stack<T>();

		while (reader.TokenType != JsonTokenType.EndArray)
		{
			elements.Push(JsonSerializer.Deserialize<T>(ref reader, options)!);

			reader.Read();
		}

		return elements;
	}

	public override void Write(Utf8JsonWriter writer, Stack<T> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();

		var reversed = new Stack<T>(value);

		foreach (T item in reversed)
		{
			JsonSerializer.Serialize(writer, item, options);
		}

		writer.WriteEndArray();
	}
}