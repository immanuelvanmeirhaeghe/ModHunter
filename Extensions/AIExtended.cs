namespace ModHunter
{
    /// <summary>
    /// Modded
    /// </summary>
    class AIExtended : AIs.AI
    {
        protected override void UpdateSwimming()
        {
            if ((ModHunter.Get().IsModActiveForSingleplayer || ModHunter.Get().IsModActiveForMultiplayer) && ModHunter.Get().UseOptionAI)
            {
                if (!IsDead() && (IsCat() || IsEnemy() || IsPredator()))
                {
                    m_Params.m_CanSwim = true;
                }
            }
            base.UpdateSwimming();
        }
    }
}
