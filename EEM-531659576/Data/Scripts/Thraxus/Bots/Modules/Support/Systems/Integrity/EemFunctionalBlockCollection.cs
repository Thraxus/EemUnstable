using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.Integrity
{
	internal class EemFunctionalBlockCollection
	{
		private readonly HashSet<EemFunctionalBlock> _shipSystems = new HashSet<EemFunctionalBlock>();

		public int LastReportedIntegrityRatio { get; private set; }

		public bool IsClosed { get; private set; }

		public SystemType Type { get; }

		public EemFunctionalBlockCollection(SystemType type)
		{
			Type = type;
			LastReportedIntegrityRatio = 0;
		}

		public void AddBlock(IMyFunctionalBlock block)
		{
			_shipSystems.Add(new EemFunctionalBlock(Type, block));
		}
		
		public void UpdateCurrentFunctionalIntegrityRatio()
		{
			if (_shipSystems.Count == 0)
			{
				if (LastReportedIntegrityRatio > 0)
					LastReportedIntegrityRatio = 0;
				return;
			}

			int x = 0;
			foreach (EemFunctionalBlock system in _shipSystems)
			 	x += system.CurrentFunctionalIntegrityRatio();
			LastReportedIntegrityRatio = (x / _shipSystems.Count);
		}
		
		public void Close()
		{
			if (IsClosed) return;
			foreach (EemFunctionalBlock system in _shipSystems)
				system.Close();
			IsClosed = true;
		}

		public override string ToString()
		{
			StringBuilder rtn = new StringBuilder();
			rtn.AppendLine($"{Type} Collection");
			foreach (EemFunctionalBlock system in _shipSystems)
				rtn.AppendLine(system.ToString());
			rtn.AppendLine();
			return rtn.ToString();
		}
	}
}
