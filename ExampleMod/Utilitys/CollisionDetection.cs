using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    [Reactor.RegisterInIl2Cpp]
    public class CollisionDetection : MonoBehaviour 
    {
        public CollisionDetection(IntPtr ptr) : base(ptr) { }   
        public PlayerControl captured;
        public bool capturedset;
        public void OnCollisionEnter2D(Collision2D col)
        {
            if (capturedset) return;
            captured = col.gameObject.GetComponent<PlayerControl>();
            if(captured.Data.Role.IsImpostor)
            {
                captured = null;
            } else
            {
                captured.transform.position = gameObject.transform.position;
                capturedset = true;
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    BoxButton.EnableTrapAbilitys();
                }
            }
            
        }
    }
}
