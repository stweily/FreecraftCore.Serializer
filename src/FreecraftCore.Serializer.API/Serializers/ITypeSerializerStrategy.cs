﻿using System;

namespace FreecraftCore.Serializer
{
	public interface ITypeSerializerStrategy
	{
		/// <summary>
		/// Indicates the <see cref="TType"/> of the serializer.
		/// </summary>
		Type SerializerType { get; }

		/// <summary>
		/// Indicates the context requirement for this serializer strategy.
		/// (Ex. If it requires context then a new one must be made or context must be provided to it for it to serializer for multiple members)
		/// </summary>
		SerializationContextRequirement ContextRequirement { get; }

		/// <summary>
		/// Perform the steps necessary to serialize this data.
		/// </summary>
		/// <param name="value">The value to be serialized.</param>
		/// <param name="dest">The writer entity that is accumulating the output data.</param>
		void Write(object value, IWireMemberWriterStrategy dest);

		/// <summary>
		/// Perform the steps necessary to deserialize this data.
		/// </summary>
		/// <param name="source">The reader providing the input data.</param>
		/// <returns>The updated / replacement value.</returns>
		object Read(IWireMemberReaderStrategy source);
	}

	//This concept is based on JAM (Blizzard's messaging system/protocol and Protobuf-net's serializer strategies https://github.com/mgravell/protobuf-net/tree/master/protobuf-net/Serializers
	/// <summary>
	/// Contract for type that providing serialization strategy for the provided TType.
	/// </summary>
	public interface ITypeSerializerStrategy<TType> : ITypeSerializerStrategy
	{
		/// <summary>
		/// Perform the steps necessary to serialize this data.
		/// </summary>
		/// <param name="value">The value to be serialized.</param>
		/// <param name="dest">The writer entity that is accumulating the output data.</param>
		void Write(TType value, IWireMemberWriterStrategy dest);

		/// <summary>
		/// Perform the steps necessary to deserialize this data.
		/// </summary>
		/// <param name="source">The reader providing the input data.</param>
		/// <returns>The updated / replacement value.</returns>
		new TType Read(IWireMemberReaderStrategy source);
	}
}