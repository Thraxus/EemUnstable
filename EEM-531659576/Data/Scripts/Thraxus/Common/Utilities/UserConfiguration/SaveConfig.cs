using System;
using System.IO;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.UserConfiguration
{
	internal static class SaveConfig
	{
		private const string ConfigFileName = "config.xml";

		private static void SaveConfigFile(UserConfiguration configData)
		{
			if (MyAPIGateway.Utilities.FileExistsInLocalStorage(ConfigFileName, typeof(UserConfiguration)))
				MyAPIGateway.Utilities.DeleteFileInLocalStorage(ConfigFileName, typeof(UserConfiguration));

			using (BinaryWriter binaryWriter = MyAPIGateway.Utilities.WriteBinaryFileInLocalStorage(ConfigFileName, typeof(UserConfiguration)))
			{
				if (binaryWriter == null)
					return;
				binaryWriter.Write(MyAPIGateway.Utilities.SerializeToBinary(configData));
			}
		}
	}
}
