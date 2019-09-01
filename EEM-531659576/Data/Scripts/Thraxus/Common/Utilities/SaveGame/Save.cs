using System;
using System.IO;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.SaveGame.DataTypes;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.SaveGame
{
	public static class Save
	{
		public static void WriteToFile<T>(string fileName, T data, Type type)
		{
			if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, type))
				MyAPIGateway.Utilities.DeleteFileInWorldStorage(fileName, type);

			using (var binaryWriter = MyAPIGateway.Utilities.WriteBinaryFileInWorldStorage(fileName, type))
			{
				if (binaryWriter == null)
					return;
				var binary = MyAPIGateway.Utilities.SerializeToBinary(data);
				binaryWriter.Write(binary.Length);
				binaryWriter.Write(binary);
			}
		}


		//public static void WriteToFile(string fileName, object file, Type T)
		//{
		//	if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, T))
		//		MyAPIGateway.Utilities.DeleteFileInWorldStorage(fileName, T);

		//	using (BinaryWriter binaryWriter = MyAPIGateway.Utilities.WriteBinaryFileInWorldStorage(fileName, T))
		//	{
		//		if (binaryWriter == null)
		//			return;
		//		binaryWriter.Write(MyAPIGateway.Utilities.SerializeToBinary(file));
		//	}
		//}

		public static void WriteToSandbox(Type T)
		{

		}
	}
}
