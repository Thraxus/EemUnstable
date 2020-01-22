using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses
{
	internal abstract class EemFunctionalBlockCollection
	{
		protected readonly HashSet<EemFunctionalBlock> ThisSystem = new HashSet<EemFunctionalBlock>();

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
			if (ThisSystem.Count == 0)
			{
				if (LastReportedIntegrityRatio > 0)
					LastReportedIntegrityRatio = 0;
				return;
			}

			int x = 0;
			foreach (EemFunctionalBlock system in ThisSystem)
			 	x += system.CurrentFunctionalIntegrityRatio();
			LastReportedIntegrityRatio = (x / ThisSystem.Count);
		}
		
		public void Close()
		{
			if (IsClosed) return;
			foreach (EemFunctionalBlock system in ThisSystem)
				system.Close();
			IsClosed = true;
		}

		public override string ToString()
		{
			StringBuilder rtn = new StringBuilder();
			rtn.AppendLine($"{Type} Collection");
			foreach (EemFunctionalBlock system in ThisSystem)
				rtn.AppendLine(system.ToString());
			rtn.AppendLine();
			return rtn.ToString();
		}
	}
}
