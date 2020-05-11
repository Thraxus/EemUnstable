using System;

namespace Eem.Thraxus.Bots.SessionComps.Interfaces
{
	public interface IClose
	{
		event Action<long> OnClose;
	}
}
