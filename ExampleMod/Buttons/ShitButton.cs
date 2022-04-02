using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    public class ShitButtonHandler
    {
        public static bool shaton;
        static PlayerControl player;
        public static void DoClick()
        {
            CustomButton but = CustomButton.Buttons.Find((CustomButton but) => but.Name == "Shit");
            CustomMenu shitmenu = new CustomMenu(but,(PlayerControl plr) => { player = plr; RPC.SendShitRPC(plr, true); });
            but.IsEffectActive = false;
            /*ShapeshifterMinigame shapeshifterMinigame = UnityEngine.Object.Instantiate<ShapeshifterMinigame>(role.ShapeshifterMenu);
            shapeshifterMinigame.transform.SetParent(Camera.main.transform, false);
            shapeshifterMinigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            shapeshifterMinigame.Begin(null);
            */
        }

        public static void OnEffectEnd()        
        {
            RPC.SendShitRPC(player, false);
        }
    }
}
