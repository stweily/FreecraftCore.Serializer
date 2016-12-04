﻿using Fasterflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore.Payload.Serializer
{
	public class DefaultSerializerDecoratorService : ISerializerDecoratorService
	{
		private enum DecorationRequired
		{
			None = 0,
			Array = 1,
			List = 2, //not handled yet
			IEnumerable = 3, //not handled yet
			Enum = 4,
			Subtypeable = 5
		}

		/// <summary>
		/// Service that can register type contracts for serialization.
		/// </summary>
		private ISerializationContractRegister contractRegistry { get; }

		private ISerializerFactory serializerFactoryService { get; }

		public DefaultSerializerDecoratorService(ISerializationContractRegister registry, ISerializerFactory serializerFactory)
		{
			if (registry == null)
				throw new ArgumentNullException(nameof(registry), $"Provided {nameof(ISerializationContractRegister)} was null.");

			contractRegistry = registry;
			serializerFactoryService = serializerFactory;
		}

		//TODO: Remove for enum check
		public bool RequiresDecorating(Type t)
		{
			//TODO: Make the rules cleaner
			if (t.IsEnum)
				return true;

			if (t.IsArray)
				return true;

			if (typeof(IEnumerable).IsAssignableFrom(t)) //handles non-array collection detection
				return true;

			if (t.GetCustomAttributes<WireMessageBaseTypeAttribute>(false).Count() != 0)
				return true;

			return false;
		}



		public ITypeSerializerStrategy TryProduceDecoratedSerializer(Type typeBeingSerialized, SerializerDecorationContext context)
		{
			//If a decorator was requested then we try to build one from the context.

			//TODO: Refactor individual decoration handling into stratigies.
			switch(DetectDecorationRequried(typeBeingSerialized))
			{
				//Right now only two kinds are handled
				case DecorationRequired.Enum:
					//create an Enum decorator and provide the base type serializer to the decorator
					return typeof(EnumSerializerDecorator<,>).MakeGenericType(typeBeingSerialized, typeBeingSerialized.GetEnumUnderlyingType())
						.CreateInstance(context.KnownSerializers.FirstOrDefault(x => x.Value.SerializerType == typeBeingSerialized.GetEnumUnderlyingType()).Value as ITypeSerializerStrategy) as ITypeSerializerStrategy; 
				case DecorationRequired.Array:
					Type elementType = typeBeingSerialized.GetElementType();
					ITypeSerializerStrategy serializer = null;

					//Check if the element type requires decoration
					if (RequiresDecorating(elementType))
					{
						//We must multiple decorate the serializers if there is something like EnumType[]
						serializer = this.TryProduceDecoratedSerializer(elementType, new SerializerDecorationContext(Enumerable.Empty<Attribute>(), context.KnownSerializers));
					}
					else
						serializer = context.KnownSerializers.First(x => x.Value.SerializerType == elementType).Value;

					//TODO: Cleanup
					//In Trinitycore some arrays aren't serialized with their length. They are fixed length. We must provide that functionality with the decorator.
					if (isSizeKnown(context))
					{
						//if the size is known then the serialization process should not write the collection size
						KnownSizeAttribute knownSizeAttribute = context.MemberAttributes.First(x => x.GetType() == typeof(KnownSizeAttribute)) as KnownSizeAttribute;

						return typeof(ArraySerializerDecorator<>).MakeGenericType(typeBeingSerialized.GetElementType())
							.CreateInstance(serializer, knownSizeAttribute.KnownSize) as ITypeSerializerStrategy;
					}
					else
					{
						return typeof(ArraySerializerDecorator<>).MakeGenericType(typeBeingSerialized.GetElementType())
							.CreateInstance(serializer, 0) as ITypeSerializerStrategy;
					}

				case DecorationRequired.Subtypeable:
					//First all children must be registered. Their object graphs may contain members that are complex
					IEnumerable<WireMessageBaseTypeAttribute> subtypeAttributes = typeBeingSerialized.GetCustomAttributes<WireMessageBaseTypeAttribute>(false);

					foreach (Type childType in subtypeAttributes.Select(x => x.ChildType))
					{
						//Call generic registry
						contractRegistry.CallMethod(new Type[] { childType }, "RegisterType"); //todo: replace reflection
					}

					//At this point all children are registered. A SubComplexTypeDecorator would be able to gather and map the serializers
					ITypeSerializerStrategy generatedSerializer = typeof(SubComplexTypeSerializerDecorator<>).MakeGenericType(typeBeingSerialized).CreateInstance(serializerFactoryService) as ITypeSerializerStrategy;

					if (generatedSerializer == null)
						throw new InvalidOperationException($"Failed to generate decorated serializer for Type: {typeBeingSerialized}.");

					return generatedSerializer;
				default:
					throw new NotImplementedException($"Advanced serialization for type unavailable.");
			}
		}

		private bool isSizeKnown(SerializerDecorationContext context)
		{
			return context.MemberAttributes.FirstOrDefault(x => x.GetType() == typeof(KnownSizeAttribute)) != null;
		}

		private DecorationRequired DetectDecorationRequried(Type t)
		{
			if (t.IsEnum)
				return DecorationRequired.Enum;

			if (t.IsArray)
				return DecorationRequired.Array;

			//If the type has been marked with basetype metadata then it has children types that could be deserialized to
			if (t.GetCustomAttributes<WireMessageBaseTypeAttribute>(false).Count() != 0)
				return DecorationRequired.Subtypeable;

			throw new NotImplementedException($"The serializer doesn't yet implement {typeof(List<>)} serialization or {typeof(IEnumerable<>)} serialization.");
		}

		public IEnumerable<Type> GrabTypesThatRequiresRegister(Type t)
		{
			if(RequiresDecorating(t))
			{
				DecorationRequired decorationMode = DetectDecorationRequried(t);

				switch (decorationMode)
				{
					//Grab the element list
					case DecorationRequired.Array:
						if (!RequiresDecorating(t.GetElementType())) //check for decoration requirements. If the type must be decorated then it will be handled by the decorator
							return new List<Type>() { t.GetElementType() };
						else
							return Enumerable.Empty<Type>();
					case DecorationRequired.Enum:
						return Enumerable.Empty<Type>(); //need register nothing when enum
					case DecorationRequired.Subtypeable:
						return Enumerable.Empty<Type>(); //we need to register object graph for subtype but that should be handled during registering the object. Not here
					default:
						throw new NotImplementedException("Only array registeration is enabled.");
				}
			}
			else
				return new List<Type>() { t }; //just return the original type.
		}
	}
}