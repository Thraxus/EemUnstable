using System;
using System.IO;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.SaveGame
{
	internal static class Load
	{
		public static T ReadFromFile<T>(string fileName, Type type)
		{
			if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, type))
				return default(T);

			using (BinaryReader binaryReader = MyAPIGateway.Utilities.ReadBinaryFileInWorldStorage(fileName, type))
			{
				return MyAPIGateway.Utilities.SerializeFromBinary<T>(binaryReader.ReadBytes(binaryReader.ReadInt32()));

			}
		}

		//public static object ReadFromFile(string fileName, Type T)
		//{
		//	if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, T))
		//		return null;

		//	const int bufferSize = 4096;
		//	byte[] data = new byte[bufferSize];
		//	using (BinaryReader binaryReader = MyAPIGateway.Utilities.ReadBinaryFileInWorldStorage(fileName, T))
		//	{
		//		if (binaryReader == null)
		//			return null;

		//		binaryReader.Read(data, 0, data.Length);
		//	}
		//	return MyAPIGateway.Utilities.SerializeFromBinary<object>(data);
		//}

		public static void WriteToSandbox()
		{
			//MyAPIGateway.Utilities.se
		}
	}
}