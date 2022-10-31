using System;
using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes
{
	public class CriticalMultiplier : Framework.FrameworkCore.Attributes.Logic.Attribute
	{
		public CriticalMultiplier(float baseValue, float upgradeValue, float baseMultiplier) : base(baseValue, upgradeValue, baseMultiplier)
		{
		}
	}
}