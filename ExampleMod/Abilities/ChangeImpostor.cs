using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    [Reactor.RegisterInIl2Cpp]
    class ChangeImpostor : MonoBehaviour 
    {
        public ChangeImpostor(IntPtr ptr) : base(ptr) { }
        public static void DoClick()
        {
            RPC.SendImpostorRPC(BoxButton.boxcreated.transform.GetChild(0).GetComponent<CollisionDetection>().captured);
            RPC.SendBoxDestroyRPC(BoxButton.boxcreated);
            Destroy(BoxButton.boxcreated);
            BoxButton.DisableTrapAbilitys();
        }

        public static void OnEffectEnd()
        {
            //Do nada
        }
    }
}
