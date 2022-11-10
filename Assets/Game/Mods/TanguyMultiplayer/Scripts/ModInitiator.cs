using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace ModInitiatorMod
{
    public class ModInitiator : MonoBehaviour
    {
		
		
		
		
		
        static Mod mod;
		public GameObject networkManager;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            GameObject go = new GameObject(mod.Title);
            go.AddComponent<ModInitiator>();
        }

        private void Awake()
        {
			Debug.Log("Begin mod init: Tanguy's Multiplayer");
            //networkManager = mod.GetAsset<GameObject>("NetworkManager");
        }
    }
}
