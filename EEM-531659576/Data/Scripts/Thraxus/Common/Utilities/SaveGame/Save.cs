using System;
using System.IO;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.SaveGame
{
	public static class Save
	{
		public static void WriteToFile<T>(string fileName, T data, Type type)
		{
			if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, type))
				MyAPIGateway.Utilities.DeleteFileInWorldStorage(fileName, type);

			using (BinaryWriter binaryWriter = MyAPIGateway.Utilities.WriteBinaryFileInWorldStorage(fileName, type))
			{
				if (binaryWriter == null)
					return;
				byte[] binary = MyAPIGateway.Utilities.SerializeToBinary(data);
				binaryWriter.Write(binary.Length);
				binaryWriter.Write(binary);
			}
		}

		public static void WriteToSandbox(Type T)
		{

		}
	}
}
