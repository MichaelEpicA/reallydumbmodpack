using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    [Reactor.RegisterInIl2Cpp]
    class TrapKill : MonoBehaviour
    {
        public TrapKill(IntPtr ptr) : base(ptr) { }
        public static void DoClick()
        {
            //Impostor murders a crewmate
            PlayerControl.LocalPlayer.RpcMurderPlayer(BoxButton.boxcreated.transform.GetChild(0).GetComponent<CollisionDetection>().captured);
            //Delete box
            RPC.SendBoxDestroyRPC(BoxButton.boxcreated);
            GameObject.Destroy(BoxButton.boxcreated);
            //Renable box button
            BoxButton.DisableTrapAbilitys();
        }

        public static void OnEffectEnd()
        {
            //Do nada
        }

    }
}
