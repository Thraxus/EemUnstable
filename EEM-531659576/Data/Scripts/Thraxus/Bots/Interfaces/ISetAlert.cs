using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface ISetAlert
	{
		bool SetAlert(AlertSetting alertSetting);
		void Close();
	}
}
