using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Reactor;

namespace ExampleMod
{
    class CustomButton
    {
        public static List<CustomButton> Buttons = new List<CustomButton>();
        public static List<CustomButton> VisibleButtons => Buttons.Where(button => button.Visible && button.CouldBeUsed()).ToList();
        private Color _startColorText = new Color(255, 255, 255);
         public Sprite spr;
        public  string text;
         public Action DoClick;
         public float coolDown;
        public float maxCoolDown;
         public float effectDuration;
         public Action OnEffectEnd;
        public KillButton buttonManager;
        public AspectPosition.EdgeAlignments align;
        public Vector3 distancefromEdge;    
        public static bool HudActive;
        public bool IsEffectActive;
        public bool Visible = true;
        public bool Active = true;
        public string Name;
        public bool HasEffect => effectDuration != 0 && OnEffectEnd != null;
        public CustomButton AddButton(string name, Sprite Spr, string Text, Action doclick, float Cooldown, float Maxcooldown, float Effectduration, Action Oneffectend, AspectPosition.EdgeAlignments Align, Vector3 DistancefromEdge)
        {
            var button = new CustomButton(name, Spr, Text, doclick, Cooldown, Maxcooldown, Effectduration, Oneffectend, Align, DistancefromEdge);
            return button;
        }
        public CustomButton(string name, Sprite Spr, string Text, Action doclick, float Cooldown, float Maxcooldown, float Effectduration, Action Oneffectend, AspectPosition.EdgeAlignments Align = AspectPosition.EdgeAlignments.LeftBottom, Vector3 DistancefromEdge = new Vector3())
        {
            spr = Spr;
            text = Text;
            DoClick = doclick;
            coolDown = Cooldown;
            maxCoolDown = Maxcooldown;
            effectDuration = Effectduration;
            OnEffectEnd = Oneffectend;
             distancefromEdge = DistancefromEdge;
            align = Align;
            Name = name;
            Buttons.Add(this);
            Start();
        }

        public void Start()
        {

            if (HudManager.Instance.transform.FindChild("Custom") == null)
            {
                MakeCustomButtonHolder();
            }
            buttonManager = UnityEngine.Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform.FindChild("Buttons").FindChild("Custom"));
            buttonManager.gameObject.SetActive(true);
            buttonManager.gameObject.name = Name;
            buttonManager.graphic.enabled = true;
            buttonManager.graphic.sprite = spr;
            CooldownHelpers.SetCooldownNormalizedUvs(buttonManager.graphic);
            buttonManager.buttonLabelText.text = text;
            AspectPosition ap = buttonManager.GetComponent<AspectPosition>();
            ap.Alignment = align;
            Debug.Log(Name + distancefromEdge.ToString());
            if(distancefromEdge.x == 0 && distancefromEdge.y == 0 && distancefromEdge.z == 0)
            {
                ap.DistanceFromEdge = new Vector3(0.8f, 1.7f, -9);
            } else
            {
                ap.DistanceFromEdge = distancefromEdge;
            }
            ap.AdjustPosition();
            var button = buttonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)Handler);

            void Handler()
            {
                if(CanBeUsed() && CouldBeUsed() && buttonManager.enabled && buttonManager.gameObject.active && PlayerControl.LocalPlayer.moveable)
                {
                    buttonManager.buttonLabelText.material.color = buttonManager.graphic.color = Palette.DisabledClear;
                    DoClick();  
                    coolDown = maxCoolDown;
                    if(HasEffect)
                    {
                        IsEffectActive = true;
                        coolDown = effectDuration;
                        buttonManager.cooldownTimerText.color = new Color(0, 255, 0);
                    }
                }
            }

        }

        private void Update()
        {
            var pos = buttonManager.transform.localPosition;
            var i = VisibleButtons.IndexOf(this);

            if(coolDown < 0f && buttonManager.enabled && PlayerControl.LocalPlayer.moveable)
            {
                buttonManager.buttonLabelText.color = buttonManager.graphic.color = CanBeUsed() ? Palette.EnabledColor : Palette.DisabledClear;
                if(IsEffectActive)
                {
                    buttonManager.cooldownTimerText.color = _startColorText;
                    coolDown = maxCoolDown;
                    IsEffectActive = false;
                    OnEffectEnd();
                }
            } else
            {
                if(CouldBeUsed() && buttonManager.enabled)
                {
                    coolDown -= Time.deltaTime;
                }
                buttonManager.buttonLabelText.color = buttonManager.graphic.color = Palette.DisabledClear;
            }
            buttonManager.buttonLabelText.text = text;
            buttonManager.gameObject.SetActive(CouldBeUsed());
            buttonManager.graphic.enabled = CouldBeUsed();
            if (CouldBeUsed())
            {
                buttonManager.graphic.material.SetFloat("_Desat", 0f);
                buttonManager.buttonLabelText.material.SetFloat("_Desat", 0f);
                buttonManager.SetCoolDown(coolDown, maxCoolDown);
            }

        }

        public static void MakeCustomButtonHolder()
        {
            Debug.Log("RUN");
            var custom = new GameObject("Custom");
            custom.transform.SetParent(HudManager.Instance.transform.FindChild("Buttons"));
            custom.transform.localPosition = HudManager.Instance.transform.localPosition;
            custom.transform.position = HudManager.Instance.transform.position;
        }

        public bool CouldBeUsed()
        {
            if (PlayerControl.LocalPlayer == null)
                return false;

            if (PlayerControl.LocalPlayer.Data == null)
                return false;

            if (MeetingHud.Instance != null)
                return false;

            return true;
        }

        public bool CanBeUsed()
        {
            return coolDown < 0f && Visible;
        }


        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        internal static class HudManagerUpdatePatch
        {
            public static void Prefix(HudManager __instance)
            {
                Buttons.RemoveAll(item => item.buttonManager == null);
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var button = Buttons[i];
                    var killButton = button.buttonManager;
                    var canUse = button.CouldBeUsed();

                    Buttons[i].buttonManager.graphic.sprite = Buttons[i].   spr;

                    killButton.gameObject.SetActive(button.Visible && canUse);

                    killButton.buttonLabelText.enabled = canUse;
                    killButton.buttonLabelText.alpha = killButton.isCoolingDown ? Palette.DisabledClear.a : Palette.EnabledColor.a;

                    if (canUse && button.Visible)
                        button.Update();
                }
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        internal static class HudManagerSetHudActivePatch
        {
            public static void Prefix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
            {
                HudActive = isActive;
            }
        }
    }
}
