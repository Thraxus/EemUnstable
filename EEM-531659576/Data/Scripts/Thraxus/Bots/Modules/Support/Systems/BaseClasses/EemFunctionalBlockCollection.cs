using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses
{
	internal abstract class EemFunctionalBlockCollection
	{
		protected readonly HashSet<EemFunctionalBlock> ShipSystems = new HashSet<EemFunctionalBlock>();

		public int LastReportedIntegrityRatio { get; private set; }

		public bool IsClosed { get; private set; }

		public SystemType Type { get; }

		protected EemFunctionalBlockCollection(SystemType type)
		{
			Type = type;
			LastReportedIntegrityRatio = 0;
		}

		public abstract void AddBlock(IMyFunctionalBlock block);

		public virtual void UpdateCurrentFunctionalIntegrityRatio()
		{
			if (ShipSystems.Count == 0)
			{
				if (LastReportedIntegrityRatio > 0)
					LastReportedIntegrityRatio = 0;
				return;
			}

			int x = 0;
			foreach (EemFunctionalBlock system in ShipSystems)
			 	x += system.CurrentFunctionalIntegrityRatio();
			LastReportedIntegrityRatio = (x / ShipSystems.Count);
		}
		
		public void Close()
		{
			if (IsClosed) return;
			foreach (EemFunctionalBlock system in ShipSystems)
				system.Close();
			IsClosed = true;
		}

		public override string ToString()
		{
			StringBuilder rtn = new StringBuilder();
			rtn.AppendLine($"{Type} Collection");
			foreach (EemFunctionalBlock system in ShipSystems)
				rtn.AppendLine(system.ToString());
			rtn.AppendLine();
			return rtn.ToString();
		}
	}
}
