using System;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct ValidTarget : IEquatable<ValidTarget>, IComparable<ValidTarget>
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

		public int SortByThreatAscending(int threatLeft, int threatRight)
		{
			return threatLeft.CompareTo(threatRight);
		}

		public int CompareTo(ValidTarget validTarget)
		{
			return Threat.CompareTo(validTarget.Threat);
		}

		public override string ToString()
		{
			return $"{Threat} | {Grid?.EntityId} | {Character?.EntityId}";
		}
	}
}