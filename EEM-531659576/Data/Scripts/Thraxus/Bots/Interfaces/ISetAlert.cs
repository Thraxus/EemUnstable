using Eem.Thraxus.Common.DataTypes;

namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface ISetAlert
	{
		bool SetAlert(AlertSetting alertSetting);
		void Close();
	}
}
