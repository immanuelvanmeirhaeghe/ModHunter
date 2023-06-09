﻿using System.Linq;
using UnityEngine;

namespace ModHunter
{
    class ConstructionGhostManagerExtended : ConstructionGhostManager
    {
        protected override void Update()
        {
                if ( (ModHunter.Get().IsModActiveForSingleplayer || ModHunter.Get().IsModActiveForMultiplayer) && ModHunter.Get().UseOptionF8  && Input.GetKeyDown(KeyCode.F8))
                {
                    foreach (ConstructionGhost m_Unfinished in m_AllGhosts.Where(
                                              m_Ghost => m_Ghost.gameObject.activeSelf
                                                                       && m_Ghost.GetState() != ConstructionGhost.GhostState.Ready))
                    {
                        m_Unfinished.SetState(ConstructionGhost.GhostState.Ready);
                    }
                }
                base.Update();
        }
    }
}
