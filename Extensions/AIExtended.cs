using Enums;
using UnityEngine;

namespace ModHunter.Extensions
{
    class AIExtended : AIs.AI
    {
        protected override void UpdateSwimming()
        {
            if (!IsDead() && (IsCat() || IsEnemy() || IsPredator()))
            {
                m_Params.m_CanSwim = true;
            }
            base.UpdateSwimming();
        }
    }
}
