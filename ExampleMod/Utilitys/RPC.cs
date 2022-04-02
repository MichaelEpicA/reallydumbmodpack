using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Hazel;
using HarmonyLib;
using System.Linq;
using Reactor.Extensions;

namespace ExampleMod
{
    class RPC
    {
        static Sprite text;
        public enum CustomRPC
        {
            BarryButton = 80,
            SizeChanged = 81,
            HoleMove = 82,
            HoleDestroy = 83,
            PlaceBox = 84,
            TrapRPC = 85,
            ChangeRPC = 86,
            BoxDestroy = 87,
            ShitRPC = 88,
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class HandleRpc
        {
            public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                //if (callId >= 43) //System.Console.WriteLine("Received " + callId);
                switch ((RPC.CustomRPC)callId)
                {
                    case RPC.CustomRPC.BarryButton:
                        var buttonBarry = Utils.PlayerById(reader.ReadByte());
                        if (AmongUsClient.Instance.AmHost)
                        {
                            MeetingRoomManager.Instance.reporter = buttonBarry;
                            MeetingRoomManager.Instance.target = null;
                            AmongUsClient.Instance.DisconnectHandlers.AddUnique(MeetingRoomManager.Instance
                                .Cast<IDisconnectHandler>());
                            if (ShipStatus.Instance.CheckTaskCompletion()) return;
                            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(buttonBarry);
                            buttonBarry.RpcStartMeeting(null);
                        }
                        break;

                    case RPC.CustomRPC.SizeChanged:
                        var playerWhoChangedSize = Utils.PlayerById(reader.ReadByte());
                        float scale = reader.ReadSingle();
                        PlayerControl.LocalPlayer.StartCoroutine(playerWhoChangedSize.ScalePlayer(scale, 1));
                        break;
                    case RPC.CustomRPC.HoleMove:
                        string objectname = reader.ReadString();
                        string vector3string = reader.ReadString();
                        GameObject hole = null;
                        if (!GameObject.Find(objectname))
                        {
                            hole = new GameObject(objectname);
                            hole.AddComponent<SpriteRenderer>().sprite = text;
                        }
                        else
                        {
                            hole = GameObject.Find(objectname);
                        }

                        string[] strs = vector3string.Split(',');
                        hole.transform.position = new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
                        break;
                    case RPC.CustomRPC.HoleDestroy:
                        string objectname2 = reader.ReadString();
                        GameObject.Destroy(GameObject.Find(objectname2));
                        break;
                    case RPC.CustomRPC.PlaceBox:
                        string vector3 = reader.ReadString();
                        string[] vector3s = vector3.Split(',');
                        GameObject box = GameObject.Instantiate(BoxButton.box);
                        box.transform.position = new Vector3(float.Parse(vector3s[0]), float.Parse(vector3s[1]), float.Parse(vector3s[2]));
                        box.transform.GetChild(0).gameObject.AddComponent<CollisionDetection>();
                        break;
                    case RPC.CustomRPC.ChangeRPC:
                        PlayerControl trapped = Utils.PlayerById(reader.ReadByte());
                        trapped.roleAssigned = false;
                        Role.SetRoleInsideGame(trapped, RoleTypes.Impostor);
                        break;
                    case RPC.CustomRPC.BoxDestroy:
                        string name = reader.ReadString();
                        GameObject destroy = GameObject.Find(name);
                        GameObject.Destroy(destroy);
                        break;
                    case RPC.CustomRPC.ShitRPC:
                        PlayerControl shaton = Utils.PlayerById(reader.ReadByte());
                        bool enable = reader.ReadBoolean();
                        if (shaton.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        {
                            if (enable)
                            {
                                ShitButtonHandler.shaton = enable;
                                shaton.myLight.LightRadius = ShipStatus.Instance.MinLightRadius;
                            }
                            else
                            {
                                ShitButtonHandler.shaton = enable;
                            }
                        }
                        break;

                }
            }
        }
        public static void SendEmergencyRPC()
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
(byte)CustomRPC.BarryButton, SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            if (AmongUsClient.Instance.AmHost)
            {
                MeetingRoomManager.Instance.reporter = PlayerControl.LocalPlayer;
                MeetingRoomManager.Instance.target = null;
                AmongUsClient.Instance.DisconnectHandlers.AddUnique(
                    MeetingRoomManager.Instance.Cast<IDisconnectHandler>());
                if (ShipStatus.Instance.CheckTaskCompletion()) return;
                DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.RpcStartMeeting(null);
            }
        }

        public static void SendSizeRPC(float scale)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SizeChanged, SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(scale);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            PlayerControl.LocalPlayer.StartCoroutine(PlayerControl.LocalPlayer.ScalePlayer(scale, 1));
        }

        public static void SendHoleRPC(GameObject hole)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.HoleMove, SendOption.Reliable, -1);
            writer.Write(hole.name);
            string vector3string = hole.transform.position.x + "," + hole.transform.position.y + "," + hole.transform.position.z + ",";
            writer.Write(vector3string);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void SendHoleDestroyRPC(GameObject hole)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.HoleDestroy, SendOption.Reliable, -1);
            writer.Write(hole.name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void SendPlaceRPC(GameObject trap)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceBox, SendOption.Reliable, -1);
            string vector3string = trap.transform.position.x + "," + trap.transform.position.y + "," + trap.transform.position.z + ",";
            writer.Write(vector3string);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void SendTrapRPC(PlayerControl trapped)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TrapRPC, SendOption.Reliable, -1);
            writer.Write(trapped.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void SendImpostorRPC(PlayerControl trapped)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ChangeRPC, SendOption.Reliable, -1);
            writer.Write(trapped.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Role.SetRoleInsideGame(trapped, RoleTypes.Impostor);
        }

        public static void SendBoxDestroyRPC(GameObject box)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BoxDestroy, SendOption.Reliable, -1);
            writer.Write(box.name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void SendShitRPC(PlayerControl shaton, bool enable)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShitRPC, SendOption.Reliable, -1);
            writer.Write(shaton.PlayerId);
            writer.Write(enable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            if(shaton == PlayerControl.LocalPlayer)
            {
               if(enable)
                {
                    ShitButtonHandler.shaton = enable;
                    shaton.myLight.LightRadius = 0f;
                } else
                {
                    ShitButtonHandler.shaton = enable;
                }   
            }
        }

        public static void SendRoleInitRPC()
        {
            if(AmongUsClient.Instance.AmHost)
            {
                if(AmongUsClient.Instance.GameMode != global::GameModes.FreePlay)
                {
                    //Calculate roles to be added
                    var rolesForPlayers = new List<Role>();
                    var roles = Utilitys.RoleManager.createdRoles.Where(r => r.Chance == 100);
                    foreach(Role r in roles)
                    {
                        rolesForPlayers.Add(r);
                        Debug.Log(r.Name);
                    }
                    var roles2 = (from role in Utilitys.RoleManager.createdRoles
                                  where role.Chance > 0 && role.Chance < 100
                                  select role).ToList();
                    foreach(Role r in roles2)
                    {
                        rolesForPlayers.Add(r);
                        Debug.Log(r.Name);
                    }

                    for(int i = 0; i < roles2.Count; i++)
                    {
                        var role = roles2.Random();
                        rolesForPlayers.Add(role);
                        var temp = roles2;
                        temp.Remove(role);
                        roles2 = temp;
                    }
                    rolesForPlayers.Do(GiveRole);
                }
            }
        }

        public static void GiveRole(Role brole)
        {
                if(brole.Team == Utilitys.Team.Impostor)
                {
                    foreach(PlayerControl pc in PlayerControl.AllPlayerControls) 
                    {
                        if(pc.Data.Role.IsImpostor) 
                        {
                        Debug.Log("PlayerControl: " + pc.name + " Role being set: " + brole.Name);
                        pc.SetRole(brole);
                        brole.RoleBehaviour.Initialize(pc);
                        pc.Data.Role = brole.RoleBehaviour;
                    }
                    }
                } else if(brole.Team != Utilitys.Team.Impostor)
                {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (! pc.Data.Role.IsImpostor)
                    {
                        Debug.Log("PlayerControl: " + pc.name + " Role being set: " + brole.Name);
                        pc.SetRole(brole);
                        brole.RoleBehaviour.Initialize(pc);
                        pc.Data.Role = brole.RoleBehaviour;
                    }
                }
            }
        }
        
    }
}
