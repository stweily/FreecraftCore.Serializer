﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MarshalPerfTest
{
	class Program
	{
		public unsafe static void Main(string[] args)
		{
			Stopwatch strucToPointerWatch = new Stopwatch();
			Stopwatch strucToPointerWatchGeneric = new Stopwatch();
			Stopwatch marshalCopy = new Stopwatch();
			Stopwatch bitConverterWatch = new Stopwatch();
			Stopwatch pointerMagicWatch = new Stopwatch();
			Stopwatch binaryWriterWatch = new Stopwatch();



			int intTestVar = 5;
			byte[] testByteArray = new byte[sizeof(int)];
			
			int testIterations = 100000000;
			
			strucToPointerWatch.Start();
			
			for(int i = 0; i < testIterations; i++)
			{			
				fixed(byte* bytePtr = &testByteArray[0])
				{
					Marshal.StructureToPtr(i, new IntPtr(bytePtr), false);
				}
			}
			
			strucToPointerWatch.Stop();
			
			Console.WriteLine("StructureToPtr Non-Generic write time: " + strucToPointerWatch.ElapsedMilliseconds);
			
			//explict collect after test
			GC.Collect();
			
			strucToPointerWatch.Reset();
			strucToPointerWatch.Start();
			
			/*for(int i = 0; i < testIterations; i++)
			{		
				fixed(byte* bytePtr = &testByteArray[0])
				{
					int val = (int)Marshal.PtrToStructure(new IntPtr(bytePtr), typeof(int));
				}
			}*/
			
			strucToPointerWatch.Stop();
			
			Console.WriteLine("StructureToPtr Non-Generic read time: " + strucToPointerWatch.ElapsedMilliseconds);
			
			GC.Collect();
			
			strucToPointerWatchGeneric.Start();
			
			for(int i = 0; i < testIterations; i++)
			{		
				fixed(byte* bytePtr = &testByteArray[0])
				{
					//Marshal.StructureToPtr<(i, new IntPtr(bytePtr), false);
				}
			}
			
			strucToPointerWatchGeneric.Stop();
			
			Console.WriteLine("StructureToPtr Generic time: " + strucToPointerWatchGeneric.ElapsedMilliseconds);
			
			GC.Collect();
			
			marshalCopy.Start();
			
			for(int i = 0; i < testIterations; i++)
			{			
				Marshal.Copy(new IntPtr(&intTestVar), testByteArray, 0, sizeof(int));
			}
			
			marshalCopy.Stop();
			
			Console.WriteLine("Marshal Copy time: " + marshalCopy.ElapsedMilliseconds);
			
			GC.Collect();
			
			bitConverterWatch.Start();
			
			for(int i = 0; i < testIterations; i++)
			{			
				byte[] bytes = BitConverter.GetBytes(intTestVar);
				
				if(bytes[0] == 0)
					continue;
			}
			
			bitConverterWatch.Stop();
			
			Console.WriteLine("Bitconverter write test: " + bitConverterWatch.ElapsedMilliseconds);
			
			bitConverterWatch.Reset();
			bitConverterWatch.Start();
			
			for(int i = 0; i < testIterations; i++)
			{			
				int val = BitConverter.ToInt32(testByteArray, 0);
			}
			
			bitConverterWatch.Stop();
			
			Console.WriteLine("Bitconverter read test: " + bitConverterWatch.ElapsedMilliseconds);
			
			GC.Collect();
			
			pointerMagicWatch.Start();
			
			for(int i = 0; i < testIterations; i++)
			{		
				fixed(byte* bytePtr = &testByteArray[0])
				{
					*((int*)bytePtr) = intTestVar;
				}
			}
			
			pointerMagicWatch.Stop();
			
			Console.WriteLine("Pointer magic write: " + pointerMagicWatch.ElapsedMilliseconds);
			
			GC.Collect();
			
			pointerMagicWatch.Reset();
			pointerMagicWatch.Start();
			
			for(int i = 0; i < testIterations; i++)
			{			
				fixed(byte* bytePtr = &testByteArray[0])
				{
					int value = *((int*)bytePtr);
				}
			}
			
			pointerMagicWatch.Stop();
			GC.Collect();
			Console.WriteLine("Pointer magic read: " + pointerMagicWatch.ElapsedMilliseconds);

			binaryWriterWatch.Start();

			MemoryStream newMemoryStream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(newMemoryStream);
			for (int i = 0; i < testIterations; i++)
			{
				writer.Write(5);
				newMemoryStream.Position = 0;
			}
			binaryWriterWatch.Stop();

			Console.WriteLine("Binary writer: " + binaryWriterWatch.ElapsedMilliseconds);

			Console.ReadKey();
		}
	}
}