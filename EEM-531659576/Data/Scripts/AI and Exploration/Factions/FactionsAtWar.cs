using System;
using System.Collections.Generic;
using EemRdx.Helpers;
using VRage.Game.ModAPI;

namespace EemRdx.Factions
{
	public class FactionsAtWar : IEquatable<FactionsAtWar>
	{
		public IMyFaction AiFaction { get; }

		public IMyFaction PlayerFaction { get; }
		
		public int CooldownTime { get; set; }

		public FactionsAtWar(IMyFaction aiFaction, IMyFaction playerFaction)
		{
			aiFaction.DeclareWar(playerFaction);
			AiFaction = aiFaction;
			PlayerFaction = playerFaction;
			CooldownTime = Constants.FactionCooldown;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as FactionsAtWar);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((AiFaction != null ? AiFaction.GetHashCode() : 0) * 397) ^ (PlayerFaction != null ? PlayerFaction.GetHashCode() : 0);
			}
		}

		public bool Equals(FactionsAtWar other)
		{
			return other != null &&
				   EqualityComparer<IMyFaction>.Default.Equals(AiFaction, other.AiFaction) &&
				   EqualityComparer<IMyFaction>.Default.Equals(PlayerFaction, other.PlayerFaction);
		}
	}
}
