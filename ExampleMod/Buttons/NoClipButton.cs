using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace ExampleMod
{ 
    [Reactor.RegisterInIl2Cpp]
    class NoClipButton : ActionButton
    {
        Rigidbody2D player;
        public float defaultcooldownSecondsRemaining = 10;
        public float defaultfillUpTime = 2;
        public float cooldownSecondsRemaining = 10;
        public float fillUpTime = 2;

        public void Awake()
        {
            base.graphic = this.GetComponentInChildren<SpriteRenderer>();
            base.glyph = this.GetComponentInChildren<ActionMapGlyphDisplay>();
            base.buttonLabelText = this.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        }
        
        public override void DoClick()
        {
            if (!base.isActiveAndEnabled)
            {
                return;
            }
            if (!PlayerControl.LocalPlayer)
            {
                return;
            }
            if(LobbyBehaviour.Instance == null)
            {
                return;
            }
            player.isKinematic = true;
        }

        public void FixedUpdate()
        {
            try
            {
                player = PlayerControl.LocalPlayer.GetComponent<Rigidbody2D>();
            } catch
            {
                return;
            }
            if (LobbyBehaviour.Instance == null)
            {
                return;
            }
        }

    }
}
