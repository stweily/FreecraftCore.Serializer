﻿using System;
using System.CodeDom;
using System.IO;
using JetBrains.Annotations;

#if !NET35
using System.Threading.Tasks;
#endif

namespace FreecraftCore.Serializer
{
	/// <summary>
	/// Contract for objects that provide wire stream reading.
	/// </summary>
	public interface IWireStreamReaderStrategy : IDisposable
	{	
		/// <summary>
		/// Reads a byte from the stream.
		/// </summary>
		byte ReadByte();

		/// <summary>
		/// Reads a byte from the stream.
		/// Doesn't remove it from the stream or move it forward.
		/// </summary>
		/// <returns>The byte peeked.</returns>
		byte PeekByte();
		
		/// <summary>
		/// Reads all bytes from the stream.
		/// </summary>
		/// <returns>Returns all bytes left in the stream. If there are no bytes left it returns an empty non-null array.</returns>
		[NotNull]
		byte[] ReadAllBytes();

		/// <summary>
		/// Reads <paramref name="count"/> many bytes from the stream.
		/// </summary>
		/// <param name="count">How many bytes to read.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the provided <see cref="count"/> is negative or exceeds the length of the underlying data.</exception>
		/// <returns>A byte array of the read bytes.</returns>
		[NotNull]
		byte[] ReadBytes(int count);

		/// <summary>
		/// Peeks <paramref name="count"/> many bytes from the stream.
		/// </summary>
		/// <param name="count">How many bytes to peek.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the provided <see cref="count"/> is negative or exceeds the length of the underlying data.</exception>
		/// <returns>A byte array of the peeked bytes.</returns>
		[NotNull]
		byte[] PeakBytes(int count);

#if !NET35
		/// <summary>
		/// Reads a byte from the stream.
		/// </summary>
		Task<byte> ReadByteAsync();

		/// <summary>
		/// Reads a byte from the stream.
		/// Doesn't remove it from the stream or move it forward.
		/// </summary>
		/// <returns>The byte peeked.</returns>
		Task<byte> PeekByteAsync();

		/// <summary>
		/// Reads all bytes from the stream.
		/// </summary>
		/// <returns>Returns all bytes left in the stream. If there are no bytes left it returns an empty non-null array.</returns>
		[NotNull]
		Task<byte[]> ReadAllBytesAsync();

		/// <summary>
		/// Reads <paramref name="count"/> many bytes from the stream.
		/// </summary>
		/// <param name="count">How many bytes to read.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the provided <see cref="count"/> is negative or exceeds the length of the underlying data.</exception>
		/// <returns>A byte array of the read bytes.</returns>
		[NotNull]
		Task<byte[]> ReadBytesAsync(int count);

		/// <summary>
		/// Peeks <paramref name="count"/> many bytes from the stream.
		/// </summary>
		/// <param name="count">How many bytes to peek.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the provided <see cref="count"/> is negative or exceeds the length of the underlying data.</exception>
		/// <returns>A byte array of the peeked bytes.</returns>
		[NotNull]
		Task<byte[]> PeakBytesAsync(int count);
#endif
	}
}
