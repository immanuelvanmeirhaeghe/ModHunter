﻿using UnityEngine;

namespace ModHunter
{
    /// <summary>
    /// Inject modding interface into game only in single player mode
    /// </summary>
    class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(ModHunter)}__").AddComponent<ModHunter>();
        }
    }
}
