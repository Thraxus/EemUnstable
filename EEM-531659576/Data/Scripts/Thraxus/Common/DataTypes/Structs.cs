using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRage.Game.VisualScripting.ScriptBuilder.Nodes;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct FactionRelationship
	{
		public readonly long FactionId;
		public readonly int Reputation;
		public readonly string Tag;

		public FactionRelationship(long id, int rep, string tag)
		{
			FactionId = id;
			Reputation = rep;
			Tag = tag;
		}
	}

	public struct ValidTarget
	{
		public readonly int Threat;

		public readonly IMyCubeGrid Grid;

		public readonly IMyCharacter Character;

		public ValidTarget(int threat, IMyCubeGrid grid, IMyCharacter character)
		{
			Threat = threat;
			Grid = grid;
			Character = character;
		}

		public bool Equals(ValidTarget other)
		{
			if (Grid?.EntityId == other.Grid?.EntityId && Threat >= other.Threat) return true;
			return Character?.EntityId == other.Character?.EntityId && Threat >= other.Threat;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is ValidTarget && Equals((ValidTarget)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Grid != null ? Grid.GetHashCode() : 0) * 397) ^ (Character != null ? Character.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return $"{Threat} | {Grid?.EntityId} | {Character?.EntityId}";
		}
	}
}
