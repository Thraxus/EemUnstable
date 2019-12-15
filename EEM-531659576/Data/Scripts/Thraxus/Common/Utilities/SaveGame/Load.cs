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
	}
}