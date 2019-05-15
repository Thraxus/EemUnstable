using VRage.ModAPI;

namespace Eem.Thraxus.Common.DataTypes
{
	public class TargetEntity
	{
		public readonly IMyEntity Target;
		public int Priority;

		public TargetEntity(IMyEntity target, int priority)
		{
			Target = target;
			Priority = priority;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			TargetEntity entity = obj as TargetEntity;
			if (entity != null)
				return Target == entity.Target;
			return false;
		}

		protected bool Equals(TargetEntity other)
		{
			return Equals(Target, other.Target);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((Target != null ? Target.GetHashCode() : 0) * 397) ^ Priority;
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Target: {Target.DisplayName} Priority: {Priority}";
		}
	}
}
