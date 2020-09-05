using UnityEngine;

namespace ModHunter
{
    class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(ModHunter)}__").AddComponent<ModHunter>();
        }
    }
}
