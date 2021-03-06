﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Serializer;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace FreecraftCore.Serialization
{
	[TestFixture]
	public class AddedSizeSendSizeTests
	{
		[Test]
		public void Test_Can_Register_AddedSize_Array_Type()
		{
			//arrange
			SerializerService serializer = new SerializerService();
			serializer.RegisterType<TestAddedSizeArrayType>();
			serializer.Compile();
		}

		[Test]
		public void Test_Can_Serialize_AddedSize_Array_Type()
		{
			//arrange
			SerializerService serializer = new SerializerService();
			serializer.RegisterType<TestAddedSizeArrayType>();
			serializer.Compile();
			int[] values = new int[] {5, 5, 5, 6, 7, 8};

			//act
			byte[] bytes = serializer.Serialize(new TestAddedSizeArrayType(values));
			TestAddedSizeArrayType deserialized = serializer.Deserialize<TestAddedSizeArrayType>(bytes);

			//assert
			Assert.AreEqual(values.Length * sizeof(int) + sizeof(ushort), bytes.Length);
			Assert.AreEqual(bytes[0], values.Length - 2);

			for(int i = 0; i < values.Length; i++)
				Assert.AreEqual(values[i], deserialized.Values[i]);
		}

		[Test]
		public void Test_Can_Register_AddedSize_String_Type()
		{
			//arrange
			SerializerService serializer = new SerializerService();
			serializer.RegisterType<TestAddedSizeStringType>();
			serializer.Compile();
		}

		[Test]
		public void Test_Can_Serialize_AddedSize_String_Type()
		{
			//arrange
			SerializerService serializer = new SerializerService();
			serializer.RegisterType<TestAddedSizeStringType>();
			serializer.Compile();
			string value = "sega made me have to do this";

			//act
			byte[] bytes = serializer.Serialize(new TestAddedSizeStringType(value));
			TestAddedSizeStringType deserialized = serializer.Deserialize<TestAddedSizeStringType>(bytes);

			//assert
			Assert.AreEqual(value.Length * 1 + sizeof(ushort) + 1 /*for null*/, bytes.Length); //use 1 instead of sizeof(char) due to ASCII
			Assert.AreEqual(bytes[0], value.Length - 2 + 1 /*for null*/);

			Assert.AreEqual(value, deserialized.Value);
		}

		[Test]
		public void Test_Can_Serialize_RemovedSize_Array_Type()
		{
			//arrange
			SerializerService serializer = new SerializerService();
			serializer.RegisterType<TestRemovedSizeArrayType>();
			serializer.Compile();
			RefInt[] values = new RefInt[] { new RefInt(5), new RefInt(16) };

			//act
			byte[] bytes = serializer.SerializeAsync(new TestRemovedSizeArrayType(values)).Result;
			TestRemovedSizeArrayType deserialized = serializer.DeserializeAsync<TestRemovedSizeArrayType>(bytes).Result;

			//assert
			Assert.AreEqual(values.Length * sizeof(int) + sizeof(int), bytes.Length, "Bytes size");
			Assert.AreEqual(bytes[0], values.Length + 1);

			for(int i = 0; i < values.Length; i++)
				Assert.AreEqual(values[i].Value, deserialized.Values[i].Value);
		}
	}

	[WireDataContract]
	public class RefInt
	{
		[WireMember(1)]
		public int Value { get; }

		/// <inheritdoc />
		public RefInt(int value)
		{
			Value = value;
		}

		public RefInt()
		{
			
		}
	}

	[WireDataContract]
	public class TestAddedSizeArrayType
	{
		[SendSize(SendSizeAttribute.SizeType.UShort, 2)]
		[WireMember(1)]
		public int[] Values { get; }

		public TestAddedSizeArrayType(int[] values)
		{
			if(values == null) throw new ArgumentNullException(nameof(values));

			Values = values;
		}

		public TestAddedSizeArrayType()
		{
			
		}
	}

	[WireDataContract]
	public class TestRemovedSizeArrayType
	{
		[SendSize(SendSizeAttribute.SizeType.Int32, -1)]
		[WireMember(1)]
		public RefInt[] Values { get; }

		public TestRemovedSizeArrayType(RefInt[] values)
		{
			if(values == null) throw new ArgumentNullException(nameof(values));

			Values = values;
		}

		public TestRemovedSizeArrayType()
		{

		}
	}

	[WireDataContract]
	public class TestAddedSizeStringType
	{
		[SendSize(SendSizeAttribute.SizeType.UShort, 2)]
		[WireMember(1)]
		public string Value { get; }

		public TestAddedSizeStringType(string value)
		{
			if(string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

			Value = value;
		}

		public TestAddedSizeStringType()
		{

		}
	}
}
