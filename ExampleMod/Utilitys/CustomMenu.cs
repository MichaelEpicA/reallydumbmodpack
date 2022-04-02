using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    class CustomMenu
    {
        public bool alivepeopleallowed;
        public bool deadbodysallowed;
        public bool deadpeopleallowed;
        public bool impostorsallowed;
        public Action<PlayerControl> perform;
        public CustomButton but;
        public static ShapeshifterRole role;
        public CustomMenu(CustomButton button ,Action<PlayerControl> handle, bool allowimpostors = false,bool allowalivepeople = true,bool allowdeadpeople = false,bool allowdeadbodys = false)
        {
            but = button;
            deadbodysallowed = allowdeadbodys;
            deadpeopleallowed = allowdeadpeople;
            alivepeopleallowed = allowalivepeople;
            impostorsallowed = allowimpostors;
            perform = handle;
            Begin();
        }
        public void Begin()
        {
            List<byte> bodies = new List<byte>();
            UnityEngine.Object.FindObjectsOfType<DeadBody>().ForEach(delegate (DeadBody body)
            {
                bodies.Add(body.ParentId);
            });
            List<PlayerControl> aliveplrs = new List<PlayerControl>();
            List<PlayerControl> deadplrs = new List<PlayerControl>();
            List<PlayerControl> impplrs = new List<PlayerControl>();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if(p != PlayerControl.LocalPlayer && (!p.Data.IsDead))
                {
                    aliveplrs.Add(p);
                } else if(p != PlayerControl.LocalPlayer && p.Data.IsDead)
                {
                    deadplrs.Add(p);
                } else if(p != PlayerControl.LocalPlayer && p.Data.Role.IsImpostor)
                {
                    impplrs.Add(p);
                }
            }
            if(role == null)
            {
                RoleBehaviour role2 = UnityEngine.Object.Instantiate<RoleBehaviour>(RoleManager.Instance.AllRoles.First((RoleBehaviour r) => r.Role == RoleTypes.Shapeshifter), GameData.Instance.transform);
                role = GameData.Instance.transform.FindChild("ShapeshifterRole(Clone)").GetComponent<ShapeshifterRole>();
                role.Initialize(PlayerControl.LocalPlayer);
            }
            ShapeshifterMinigame game = UnityEngine.Object.Instantiate(role.ShapeshifterMenu);
            game.transform.SetParent(Camera.main.transform, false);
            //game.transform.localPosition = new Vector3(0f, 0f, -50f);
            List<PlayerControl> add = CheckPlayers(bodies, aliveplrs, deadplrs, impplrs);
            Il2CppSystem.Collections.Generic.List<UiElement> buttons = new Il2CppSystem.Collections.Generic.List<UiElement>();
            for (int i = 0; i < add.Count; i++)
            {
                PlayerControl plr = add[i];
    
                int num = i % 3;
                int num2 = i / 3;
                bool flag = PlayerControl.LocalPlayer.Data.Role.NameColor == plr.Data.Role.NameColor;
                ShapeshifterPanel shapeshifterPanel = UnityEngine.Object.Instantiate<ShapeshifterPanel>(game.PanelPrefab, game.transform);
                shapeshifterPanel.transform.localPosition = new Vector3(game.XStart + (float)num * game.XOffset, game.YStart + (float)num2 * game.YOffset, -1f);
                shapeshifterPanel.SetPlayer(i, plr.Data, (Action)delegate {
                    perform(plr);
                    game.Close();
                    but.IsEffectActive = true;
                    but.coolDown = but.effectDuration;
                    but.buttonManager.cooldownTimerText.color = new Color(0, 255, 0);
                });
                shapeshifterPanel.NameText.color = (flag ? plr.Data.Role.NameColor : Color.white);
                game.potentialVictims.Add(shapeshifterPanel);
                buttons.Add(shapeshifterPanel.Button);
            }
            ControllerManager.Instance.OpenOverlayMenu(game.name, game.BackButton, game.DefaultButtonSelected, buttons, false);

        }
        
        private List<PlayerControl> CheckPlayers(List<byte> bodies, List<PlayerControl> aliveplrs, List<PlayerControl> deadplrs, List<PlayerControl> impplrs)
        {
            List<PlayerControl> addtomenu = new List<PlayerControl>();
            /*if (!alivepeopleallowed && deadpeopleallowed && !deadbodysallowed)
            {
                foreach (PlayerControl deadplr in deadplrs)
                {
                    if (!bodies.Contains(deadplr.PlayerId))
                    {
                        addtomenu.Add(deadplr);
                    }
                }
            }
            else if (!alivepeopleallowed && !deadpeopleallowed && deadbodysallowed)
            {
                foreach (PlayerControl deadplr in deadplrs)
                {
                    if (bodies.Contains(deadplr.PlayerId))
                    {
                        addtomenu.Add(deadplr);
                    }
                }
            }
            else if (!alivepeopleallowed && deadpeopleallowed && deadbodysallowed)
            {
                foreach (PlayerControl deadplr in deadplrs)
                {
                    addtomenu.Add(deadplr);
                }
            }
            else if (alivepeopleallowed && deadpeopleallowed && deadbodysallowed)
            {
                foreach (PlayerControl deadplr in deadplrs)
                {
                    addtomenu.Add(deadplr);
                }
                foreach (PlayerControl aliveplr in aliveplrs)
                {
                    addtomenu.Add(aliveplr);
                }
            }
            else if (alivepeopleallowed && !deadpeopleallowed && deadbodysallowed)
            {
                foreach (PlayerControl aliveplr in aliveplrs)
                {
                    addtomenu.Add(aliveplr);
                }

                foreach (PlayerControl deadplr in deadplrs)
                {
                    if (bodies.Contains(deadplr.PlayerId))
                    {
                        addtomenu.Add(deadplr);
                    }
                }
            }
            else if (alivepeopleallowed && deadpeopleallowed && !deadbodysallowed)
            {
                foreach (PlayerControl aliveplr in aliveplrs)
                {
                    addtomenu.Add(aliveplr);
                }

                foreach (PlayerControl deadplr in deadplrs)
                {
                    if (bodies.Contains(deadplr.PlayerId))
                    {
                        addtomenu.Add(deadplr);
                    }
                }
            }*/

            if(alivepeopleallowed)
            {
                foreach(PlayerControl aliveplr in aliveplrs)
                {
                    addtomenu.Add(aliveplr);
                }
            }

            if(deadpeopleallowed)
            {
                foreach(PlayerControl deadplr in deadplrs)
                {
                    addtomenu.Add(deadplr);
                }
            }

            if(deadbodysallowed)
            {
                foreach (PlayerControl deadplr in deadplrs)
                {
                    if (bodies.Contains(deadplr.PlayerId))
                    {
                        addtomenu.Add(deadplr);
                    }
                }
            }

            if(impostorsallowed)
            {
                foreach(PlayerControl impplr in impplrs)
                {
                    addtomenu.Add(impplr);
                }
            }
            return addtomenu;
        }

    }
}
