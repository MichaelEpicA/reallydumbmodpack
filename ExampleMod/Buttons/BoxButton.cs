    using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    class BoxButton : ActionButton
    {
        public static GameObject box = new GameObject("Box");
        public static GameObject boxcreated = new GameObject(); 
        
        public static void DoClick()
        {
            //CustomButton.Buttons.Find(CheckName).buttonManager.gameObject.AddComponent<BoxButton>();
            GameObject inst = GameObject.Instantiate(box);
            boxcreated = inst;
            inst.transform.GetChild(0).gameObject.AddComponent<CollisionDetection>();
            inst.transform.position = PlayerControl.LocalPlayer.transform.position + new Vector3(3, 0, 0);
            RPC.SendPlaceRPC(inst);
            //CustomButton.Buttons.Find(CheckName).buttonManager.gameObject.GetComponent<BoxButton>().createdbox = inst;
        }

        public static void OnEffectEnd()
        {

        }

        public static void EnableTrapAbilitys()
        {
            CustomButton.Buttons.Find(CheckName).Visible = false;
            CustomButton.Buttons.Find(name => name.Name == "Kill").Visible = true;
            CustomButton.Buttons.Find(name => name.Name == "Transform").Visible = true;
        }

        public static void DisableTrapAbilitys()
        {
            CustomButton.Buttons.Find(CheckName).Visible = true;
            CustomButton.Buttons.Find(name => name.Name == "Kill").Visible = false;
            CustomButton.Buttons.Find(name => name.Name == "Transform").Visible = false;

        }

        static bool CheckName(CustomButton button)
        {
            return button.Name == "Box";
        }
    }
}
