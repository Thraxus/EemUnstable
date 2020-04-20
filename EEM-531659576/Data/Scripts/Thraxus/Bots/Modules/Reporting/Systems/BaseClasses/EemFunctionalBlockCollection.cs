using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses
{
	internal abstract class EemFunctionalBlockCollection
	{
		protected readonly HashSet<EemFunctionalBlock> ThisSystem = new HashSet<EemFunctionalBlock>();

		protected readonly Dictionary<long, int> ThisSystemsLastReport = new Dictionary<long, int>();

		public int LastReportedIntegrityRatio { get; private set; }

		public bool IsClosed { get; private set; }

		private bool _reportedClosed;

		public SystemType Type { get; }

		protected EemFunctionalBlockCollection(SystemType type)
		{
			Type = type;
			LastReportedIntegrityRatio = 0;
		}

		public virtual void AddBlock(IMyFunctionalBlock block)
		{
			if (!ThisSystemsLastReport.ContainsKey(block.EntityId))
				ThisSystemsLastReport.Add(block.EntityId, 0);
		}

		public virtual bool UpdateCurrentFunctionalIntegrityRatio(long blockId)
		{
			bool updated = false;
			if (ThisSystem.Count == 0)
			{
				if (_reportedClosed) return false;
				if (LastReportedIntegrityRatio > 0)
					LastReportedIntegrityRatio = 0;
				_reportedClosed = true;
				return true;
			}
			
			foreach (EemFunctionalBlock system in ThisSystem)
			{
				if (system.MyId == blockId)
				{
					ThisSystemsLastReport[system.MyId] = system.CurrentFunctionalIntegrityRatio();
					updated = true;
				}
			}

			if(updated)
			{
				int x = 0;
				foreach (KeyValuePair<long, int> system in ThisSystemsLastReport)
				{
					x += system.Value;
				}

				LastReportedIntegrityRatio = (x / ThisSystemsLastReport.Count);
			}

			return updated;
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
