using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using Reactor;
using UnityEngine;
using Hazel;
using System.Linq;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;
using Reactor.Patches;
using TMPro;
using System.Threading;

namespace ExampleMod
{
    class Patches
    {
        static Sprite text;
        public static bool emergencybutton;
        public static bool buttonactive;
        public static bool ranextract = false;
        public static AssetBundle bundle;
        public static bool moved;
        public static List<CustomButton> buttonstomake = new List<CustomButton>();

        /*[HarmonyPatch(typeof(ReactorVersionShower), nameof(ReactorVersionShower.UpdateText))]
        public static class UpdateTextPatch
        {
            public static void Postfix(EmergencyMinigame __instance)
            {
                try {
                    GameObjectExtensions.SetLocalZ(ReactorVersionShower.Text.GetComponent<Transform>(), -10f);
                    ThreadStart start = new ThreadStart(Update.CheckForUpdates);
                    Thread thr = new Thread(start);
                    thr.Start();
                } catch (Exception e) {
                    Debug.LogError("Update checker failed!\n" + e);
                }

                ReactorVersionShower.Text.text = "<size=3><color=green>ExampleMod</color> <color=blue>v" + ExampleModPlugin.Version + "</color></size>\n" + ReactorVersionShower.Text.text;
                // ReactorVersionShower.Text!.text = "<size=3><color=green>ExampleMod</color> <color=blue>v" + ExampleModPlugin.Version + "</color></size>\n";
                // ReactorVersionShower.Text.text += "Reactor " + typeof(ReactorPlugin).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                // ReactorVersionShower.Text.text += "\nBepInEx " + Paths.BepInExVersion;
                // ReactorVersionShower.Text.text += "\nMods: " + IL2CPPChainloader.Instance.Plugins.Count;
            }
        } */
       

        [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
        public static class CheckButtonActive
        {
            public static void Postfix(EmergencyMinigame __instance)
            {
                __instance.StartCoroutine(CheckButton(__instance));
            }
            public static System.Collections.IEnumerator CheckButton(EmergencyMinigame __instance)
            {
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                    buttonactive = __instance.ButtonActive;
                }

            }
        }

        [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.CallMeeting))]
        public static class EmergencyMap
        {
            public static bool Prefix(EmergencyMinigame __instance)
            {
                __instance.Close();
                __instance.Close();
                //Say that we opened the map from the emergency button. 
                emergencybutton = true;
                HudManager.Instance.StartCoroutine(WaitMap());
                buttonactive = __instance.ButtonActive;
                return false;
            }


            private static System.Collections.IEnumerator WaitMap()
            {
                yield return new WaitForEndOfFrame();
                HudManager.Instance.OpenMap();
            }

        }
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
        public static class MapEmergency
        {
            public static bool Prefix(MapBehaviour __instance)
            {
                __instance.Close();
                var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
                var specials = system.specials.ToArray();
                var saboActive = specials.Any(s => s.IsActive);
                //Checking if this was launched by the emergency button, if yes, set it to no, and return.
                if (emergencybutton)
                {
                    emergencybutton = false;
                    return true;
                }
                if (MeetingHud.Instance == null && !PlayerControl.LocalPlayer.Data.IsDead && !saboActive && buttonactive && !CheckIfExileCutsceneIsOn())
                {
                    RPC.SendEmergencyRPC();
                }
                return false;
            }

            public static bool CheckIfExileCutsceneIsOn()
            {
                if (ExileController.Instance == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


        }



        /*
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.PlayStepSound))]
        public static class MovementPatch
        {
            static ParticleSystem ps;
            static ParticleSystemRenderer psr;
            public static void Postfix()
            {
                if (!ranextract)
                {
                    Debug.LogError("YO AM I RUNNING?");
                    ps = PlayerControl.LocalPlayer.gameObject.AddComponent<ParticleSystem>();
                    psr = PlayerControl.LocalPlayer.gameObject.GetComponent<ParticleSystemRenderer>();
                    //Extracting asset bundle
                    //LoadParticle(psr);
                    Shader sha = Resources.GetBuiltinResource<Shader>(@"Resources\unity_builtin_extra\Standard Unlit.shader");
                    Material mat = new Material(sha);
                    Debug.LogError("Shader: " + sha + "Material: " + mat);
                    psr.SetMaterial(mat);
                    ranextract = true;
                }   
                else
                {
                    ps.Play();
                }
            }

            public static System.Collections.IEnumerator LoadParticle(ParticleSystemRenderer psr)
            {
                /*
                Utils.ExtractEmbeddedResource(Application.streamingAssetsPath + "dustParticle", Assembly.GetExecutingAssembly().GetName().Name + ".Resources", "dustparticle");
                AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "dustParticle");
                AssetBundleRequest abr = ab.LoadAssetAsync<Material>("Dust");
                yield return abr;
                Material mat = abr.asset as Material;
                psr.SetMaterial(mat);
                yield return null;
            }
        }*/
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class SizeChanger
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (Input.GetKey(KeyCode.J))
                {
                    ScaleBean(__instance, 0.5f);
                }
                else if (Input.GetKey(KeyCode.K) && __instance == PlayerControl.LocalPlayer)
                {
                    ScaleBean(__instance, 1);
                }
                else if (Input.GetKey(KeyCode.L) && __instance == PlayerControl.LocalPlayer)
                {
                    ScaleBean(__instance, 2);
                }
                else if (Input.GetKey(KeyCode.Semicolon) && __instance == PlayerControl.LocalPlayer)
                {
                    ScaleBean(__instance, 5);
                }
            }


            public static void ScaleBean(PlayerControl pc, float scale)
            {
                RPC.SendSizeRPC(scale);
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class FinishTasks
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (Input.GetKey(KeyCode.T))
                {
                    foreach (PlayerTask pt in PlayerControl.LocalPlayer.myTasks)
                    {
                        PlayerControl.LocalPlayer.RpcCompleteTask(pt.Id);
                    }
                }
            }

        }

        [HarmonyPatch(typeof(PlainDoor), nameof(PlainDoor.SetDoorway))]
        public static class DoorsYawn
        {
            private static AudioClip _clip;
            public static void Postfix(PlainDoor __instance, [HarmonyArgument(0)] bool open)
            {
                if (_clip == null) LoadSound();
                if (open)
                {
                    SoundManager.Instance.PlaySound(_clip, false);
                }
            }
            private static void LoadSound()
            {
                var asm = typeof(DoorsYawn).Assembly;
                var name = asm.GetManifestResourceNames().FirstOrDefault(s => s.Contains("assets", StringComparison.CurrentCultureIgnoreCase));
                Logger<ReactorPlugin>.Message($"Audio Name: {name}");
                var stream = new MemoryStream();
                asm.GetManifestResourceStream(name)!.CopyTo(stream);
                _clip = bundle.LoadAllAssets().FirstOrDefault(a => a.TryCast<AudioClip>() != null)?.TryCast<AudioClip>();
                Logger<ReactorPlugin>.Message($"Audio Clip Null: {_clip == null}");
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class HoleFollow
        {

            static GameObject hole;
            public static void Postfix(PlayerControl __instance)
            {
                if (PlayerControl.LocalPlayer == __instance)
                {

                    if (text == null) LoadHole(__instance);
                        if (!moved) return;
                    FollowPlayerControl(__instance);
                }

            }

            public static void LoadHole(PlayerControl __instance)
            {
                if (LobbyBehaviour.Instance != null) return;
                text = bundle.LoadAllAssets().FirstOrDefault(asset => asset.name == "hole" && asset.TryCast<Sprite>() != null)?.TryCast<Sprite>();
                hole = new GameObject(__instance.name + "'s hole");
                hole.AddComponent<SpriteRenderer>().sprite = text;
                hole.transform.position = new Vector3(__instance.transform.position.x + 100, __instance.transform.position.y + 100, -10);
                hole.AddComponent<Rigidbody2D>();
                hole.transform.SetParent(__instance.transform);
            }

            public static void FollowPlayerControl(PlayerControl pc)
            {
                if (pc.Data.IsDead) return;
                if (LobbyBehaviour.Instance != null) return;
                Transform target = pc.transform;
                Rigidbody2D rb2d = hole.GetComponent<Rigidbody2D>();
                float speed = 3.0f;
                float minDistance = 0f;
                //Find direction
                Vector3 dir = (target.transform.position - rb2d.transform.position).normalized;
                //Check if we need to follow object then do so 
                if (Vector3.Distance(target.transform.position, rb2d.transform.position) > minDistance && moved && MeetingHud.Instance == null && ExileController.Instance == null)
                {
                    rb2d.MovePosition(rb2d.transform.position + dir * speed * Time.fixedDeltaTime);
                    RPC.SendHoleRPC(hole);
                }
                if (Vector3.Distance(target.transform.position, rb2d.transform.position) < 10)
                {
                    //Player is touching hole, kill them
                    pc.RpcMurderPlayer(pc);
                    //Destroy the hole for other players
                    RPC.SendHoleDestroyRPC(hole);
                    //Destroy the hole on your client
                    GameObject.Destroy(hole);

                }
            }

        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.PlayStepSound))]
        public static class MovementMonitor
        {
            public static void Postfix()
            {
                moved = true;
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        public static class NoClipButtonPatch
        {
             public static void Postfix(GameStartManager __instance)
            {
                
                if (HudManager.Instance.transform.FindChild("Buttons").FindChild("Custom") == null)
                {
                    var custom = new GameObject("Custom");
                    custom.transform.SetParent(HudManager.Instance.transform.FindChild("Buttons"));
                    custom.transform.localPosition = HudManager.Instance.transform.localPosition;
                    custom.transform.position = HudManager.Instance.transform.position;
                }
                Sprite spr = new Sprite();
                foreach (UnityEngine.Object obj2 in bundle.LoadAllAssets())
                {
                    Logger<ReactorPlugin>.Message(obj2.name);
                    if (obj2.name == "noclip")
                    {
                        spr = obj2.TryCast<Sprite>();
                    }
                }
                /*
                NoClipButton button = noclip.AddComponent<NoClipButton>();
                noclip.AddComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)button.DoClick);
                GameObject spritehandler = new GameObject("Noclip");
                spritehandler.transform.parent = noclip.transform;
                spritehandler.AddComponent<SpriteRenderer>().sprite = spr;
                AspectPosition ap = noclip.AddComponent<AspectPosition>();
                ap.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
                ap.AdjustPosition();
                GameObject tmp = new GameObject("Text_TMP");
                tmp.AddComponent<RectTransform>();
                tmp.AddComponent<TextMeshPro>().text = "NOCLIP";
                */
                
                CustomButton Noclip = new CustomButton("Noclip", spr, "NOCLIP", DoClick, 20, 20, 10, OnEffectEnd, AspectPosition.EdgeAlignments.LeftBottom);
                Noclip.Visible = false;
            }

            public static void DoClick()
            {
                PlayerControl.LocalPlayer.MyPhysics.body.isKinematic = true;
            }

            public static void OnEffectEnd()
            {
                PlayerControl.LocalPlayer.MyPhysics.body.isKinematic = false;
            }
        } 
            [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
            public static class NoClipButtonVisible
            {
                public static void Postfix(RoleTypes roleType)
                {
                    if (roleType == RoleTypes.Crewmate)
                    {
                        CustomButton.Buttons.Find(button => button.Name == "Noclip").Visible = true;
                    } 

                }
            }
            [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
            public static class BoxButtonPatch
            {
                
                public static void Postfix()
                {
                    Sprite spr2 = new Sprite();
                    foreach (UnityEngine.Object obj2 in bundle.LoadAllAssets())
                    {
                        if (obj2.name == "drop box")
                        {
                            spr2 = obj2.TryCast<Sprite>();
                        }
                        else if (obj2.name == "box")
                        {
                            BoxButton.box = obj2.TryCast<GameObject>();
                        }
                    }
                    CustomButton Box = new CustomButton("Box", spr2, "PLACE BOX", BoxButton.DoClick, 30, 30, 1, BoxButton.OnEffectEnd, AspectPosition.EdgeAlignments.LeftBottom, new Vector3(1.8f, 1.7f, -9));
                    Box.Visible = false;

                    CustomButton Kill = new CustomButton("Kill", HudManager.Instance.KillButton.GetComponent<SpriteRenderer>().sprite, "KILL TRAPPED", TrapKill.DoClick, 0, 0, 0, TrapKill.OnEffectEnd);
                    Kill.Visible = false;
                    CustomButton Transform = new CustomButton("Transform", HudManager.Instance.SabotageButton.graphic.sprite, "TRANSFORM", ChangeImpostor.DoClick, 0, 0, 0, ChangeImpostor.OnEffectEnd, AspectPosition.EdgeAlignments.LeftBottom, new Vector3(1.8f, 1.7f, -9));
                    Transform.Visible = false;
                }

            }
            [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
            public static class BoxButtonVisible
            {
                public static void Postfix()
                {

                    CustomButton.Buttons.Find(button => button.Name == "Box").Visible = true;
                }
            }

            [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
            public static class ShitButton
            {
                public static void Postfix()
                {
                    CustomButton Shit = new CustomButton("Shit", HudManager.Instance.KillButton.GetComponent<SpriteRenderer>().sprite, "SHIT", ShitButtonHandler.DoClick, 10, 10, 10, ShitButtonHandler.OnEffectEnd, AspectPosition.EdgeAlignments.LeftBottom, new Vector3(2.5f, 1.7f, -9));
                    Shit.Visible = false;
                }


            }
            [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
            public static class ShitButtonVisible
            {
                public static void Postfix()
                {

                    CustomButton.Buttons.Find(button => button.Name == "Shit").Visible = true;
                }
            }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class LightModifier
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (ShitButtonHandler.shaton)
                {
                    __instance.myLight.LightRadius = ShipStatus.Instance.MinLightRadius;
                }
            }
        }
    }
}
