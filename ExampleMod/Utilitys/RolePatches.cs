using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ExampleMod.Utilitys
{
    [HarmonyPatch]
    class RolePatches
    {
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        [HarmonyPrefix]
        public static void OnGameEndPatch(AmongUsClient __instance)
        {
            RoleManager.createdRoles.Do(r => r.OnGameEnd());
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
        [HarmonyPrefix]
        public static void OnGameStartPatch(AmongUsClient __instance)
        {
            RoleManager.createdRoles.Do(r => r.OnGameStart());
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
        [HarmonyPrefix]
        public static void ResetRolePatch(AmongUsClient __instance)
        {
            PlayerControl.AllPlayerControls.ToArray().Where(player => player != null).Do(player => player.SetRole(null));
        }

        [HarmonyPatch(typeof(global::RoleManager), nameof(global::RoleManager.SelectRoles))]
        [HarmonyPrefix]
        public static void InitalizeRoles()
        {
            RoleManager.InitalizeRoles();
            RPC.SendRoleInitRPC();
        }

        [HarmonyPatch(typeof(global::RoleManager), nameof(global::RoleManager.AssignRolesFromList))]
        [HarmonyPrefix]
        public static bool ChangeImpostors(Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> players, int teamMax, Il2CppSystem.Collections.Generic.List<RoleTypes> roleList, ref int rolesAssigned)
        {
            while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
            {
                int index = HashRandom.FastNext(roleList.Count);
                Debug.Log("Index: " + index.ToString());
                Debug.Log("Count: " + roleList.Count.ToString());
                RoleTypes roleType = roleList.ToArray()[index];
                roleList.RemoveAt(index);
                int index2 = global::RoleManager.IsImpostorRole(roleType) && RoleManager.HostMod.IsImpostor
                    ? 0
                    : HashRandom.FastNext(players.Count);
                players.ToArray()[index2].Object.RpcSetRole(roleType);
                players.RemoveAt(index2);
                rolesAssigned++;
            }

            return false;
        }



        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.SetUpRoleText))]
        [HarmonyPostfix]
        public static void RoleTextPatch(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.GetRole() != null)
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                var scene = __instance;

                scene.RoleText.text = role.Name;
                scene.RoleBlurbText.text = role.Description;
                scene.RoleText.color = role.Color;
                scene.RoleBlurbText.color = role.Color;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        [HarmonyPostfix]
        public static void TeamTextPatch(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.GetRole() != null)
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                var scene = __instance;
                scene.TeamTitle.text = role.Name;
                scene.ImpostorText.gameObject.SetActive(true);
                scene.ImpostorText.text = role.Description;
                scene.BackgroundBar.material.color = role.Color;
                scene.TeamTitle.color = role.Color;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        [HarmonyPrefix]
        public static void RoleTeamPatch(IntroCutscene __instance, [HarmonyArgument(0)] ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            if (PlayerControl.LocalPlayer.GetRole() != null)
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                if (role.Team == Team.Alone)
                {
                    yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    yourTeam.Add(PlayerControl.LocalPlayer);
                }
                else if (role.Team == Team.Role)
                {
                    yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    yourTeam.Add(PlayerControl.LocalPlayer);
                    foreach (var player in role.Members)
                    {
                        if (player != PlayerControl.LocalPlayer.PlayerId)
                        {
                            yourTeam.Add(Utils.PlayerById(player));
                        }
                    }
                }
                else if (role.Team == Team.Impostor)
                {
                    yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    yourTeam.Add(PlayerControl.LocalPlayer);
                    foreach (var player in role.Members)
                    {
                        if (player != PlayerControl.LocalPlayer.PlayerId &&
                           Utils.PlayerById(player).Data.Role.IsImpostor)
                            yourTeam.Add(Utils.PlayerById(player));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudManagerUpdatePatch
        {
            public static void Prefix(HudManager __instance)
            {
                if (GameData.Instance && ShipStatus.Instance && AmongUsClient.Instance &&
                       (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started ||
                        AmongUsClient.Instance.GameMode == global::GameModes.FreePlay))
                {
                    RoleManager.createdRoles.Do(r => r._OnUpdate());
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class MeetingUpdatePatch
        {
            public static void Postfix(MeetingHud __instance)
            {
                RoleManager.createdRoles.Do(r => r._OnMeetingUpdate(__instance));
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class PlayerControlFixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (ExampleModPlugin.gameStarted)
                {
                    var localRole = PlayerControl.LocalPlayer.GetRole();

                    if (localRole != null && __instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        HudManager.Instance.KillButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead &&
                                                                            localRole.CanKill(null) && CustomButton.HudActive);

                        if (localRole.CanKill(null) && __instance.CanMove && !__instance.Data.IsDead)
                        {
                            if (!__instance.Data.Role.IsImpostor)
                                __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);
                            PlayerControl target = __instance.FindClosestTarget(false);
                            HudManager.Instance.KillButton.SetTarget(target);
                        }
                        else
                        {
                            HudManager.Instance.KillButton.SetTarget(null);
                            HudManager.Instance.KillButton.SetDisabled();
                        }

                        HudManager.Instance.SabotageButton.gameObject.SetActive(
                            !PlayerControl.LocalPlayer.Data.IsDead && localRole.CanSabotage(null) && CustomButton.HudActive);

                        if (localRole.CanSabotage(null) && __instance.CanMove && !__instance.Data.IsDead)
                        {
                            HudManager.Instance.SabotageButton.SetEnabled();
                        }
                        else
                        {
                            HudManager.Instance.SabotageButton.SetDisabled();
                        }

                        HudManager.Instance.ImpostorVentButton.gameObject.SetActive(
                            !PlayerControl.LocalPlayer.Data.IsDead && localRole.CanVent && CustomButton.HudActive);

                        if (localRole.CanVent && __instance.CanMove && !__instance.Data.IsDead)
                        {
                            HudManager.Instance.ImpostorVentButton.SetEnabled();
                        }
                        else
                        {
                            HudManager.Instance.ImpostorVentButton.SetDisabled();
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
        [HarmonyPrefix]
        public static bool RemoveCheckMurder(KillButton __instance)
        {
            var target = __instance.currentTarget;
            var killer = PlayerControl.LocalPlayer;
            if (__instance.isActiveAndEnabled && target && !__instance.isCoolingDown && !killer.Data.IsDead && killer.CanMove)
            {
                if (AmongUsClient.Instance.IsGameOver)
                {
                    return false;
                }
                if (!target || killer.Data.IsDead || killer.Data.Disconnected)
                {
                    int num = target ? target.PlayerId : -1;
                    Debug.LogWarning(string.Format("Bad kill from {0} to {1}", killer.PlayerId, num));
                    return false;
                }
                GameData.PlayerInfo data = target.Data;
                if (data == null || data.IsDead || target.inVent)
                {
                    Debug.LogWarning("Invalid target data for kill");
                    return false;
                }
                PlayerControl.LocalPlayer.RpcMurderPlayer(__instance.currentTarget);
                __instance.SetTarget(null);
            }
            return false;
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        public static class PlayerControlSetKillTimerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
            {
                if (__instance.GetRole() != null && __instance.GetRole().CanKill() || __instance.Data.Role.CanUseKillButton)
                {
                    if (PlayerControl.GameOptions.KillCooldown <= 0f)
                        return false;
                    __instance.killTimer = Mathf.Clamp(time, 0f, PlayerControl.GameOptions.KillCooldown);
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, PlayerControl.GameOptions.KillCooldown);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class PlayerControlMurderPlayerPatch
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                RoleManager.createdRoles.Do(r => r.OnKill(__instance, target));
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        [HarmonyPostfix]
        public static void OnPlayerExiledPatch(PlayerControl __instance)
        {
            RoleManager.createdRoles.Do(r => r.OnExiled(__instance));
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoStartMeeting))]
        [HarmonyPrefix]
        public static void OnMeetingStart(MeetingHud __instance)
        {
            RoleManager.createdRoles.Do(r => r.OnMeetingStart(__instance));
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FindClosestTarget))]
        public static class PlayerControlFindClosestTargetPatch
        {
            public static bool Prefix(PlayerControl __instance, out PlayerControl __result,
                [HarmonyArgument(0)] bool protecting)
            {
                if (__instance.GetRole() != null)
                {
                    __result = __instance.GetRole().FindClosestTarget(__instance, protecting);
                    return false;
                }

                __result = null;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl._CoSetTasks_d__102), nameof(PlayerControl._CoSetTasks_d__102.MoveNext))]
        public static class PlayerControlSetTasks
        {
            public static void Postfix(PlayerControl._CoSetTasks_d__102 __instance)
            {
                if (__instance == null)
                    return;

                var player = __instance.__4__this;
                var role = player.GetRole();

                if (role == null)
                    return;

                if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    return;

                if (!role.AssignTasks)
                    player.ClearTasks();

                if (role.TaskDescription == null)
                    return;

                if (!player.Data.Role.IsImpostor && !role.HasToDoTasks && role.AssignTasks)
                {
                    var fakeTasks = new GameObject("FakeTasks").AddComponent<ImportantTextTask>();
                    fakeTasks.transform.SetParent(player.transform, false);
                    fakeTasks.Text = $"</color>{role.Color.ToTextColor()}Fake Tasks:</color>";
                    player.myTasks.Insert(0, fakeTasks);
                }

                var roleTask = new GameObject(role.Name + "Task").AddComponent<ImportantTextTask>();
                roleTask.transform.SetParent(player.transform, false);
                roleTask.Text = $"</color>Role: {role.Color.ToTextColor()}{role.Name}\n{role.TaskDescription}</color>";
                player.myTasks.Insert(0, roleTask);
            }
        }
        [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
        public static class UseButtonManagerDoClickPatch
        {
            public static bool Prefix(SabotageButton __instance)
            {
                if (__instance.isActiveAndEnabled && ExampleModPlugin.gameStarted)
                {
                    var role = PlayerControl.LocalPlayer.GetRole();

                    if (role == null)
                        return true;

                    HudManager.Instance.ShowMap((Action<MapBehaviour>)(map =>
                    {
                        foreach (MapRoom mapRoom in map.infectedOverlay.rooms.ToArray()
                            .Where(room => !role.CanSabotage(room.room)))
                        {
                            mapRoom.gameObject.SetActive(false);
                        }

                        map.ShowSabotageMap();
                    }));

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
        public static class MapBehaviourShowSabotageMapPatch
        {
            public static bool Prefix(MapBehaviour __instance)
            {
                if (ExampleModPlugin.gameStarted)
                {
                    var role = PlayerControl.LocalPlayer.GetRole();

                    if (role == null)
                        return true;

                    HudManager.Instance.ShowMap((Action<MapBehaviour>)(map =>
                    {
                        foreach (MapRoom mapRoom in map.infectedOverlay.rooms.ToArray())
                        {
                            mapRoom.gameObject.SetActive(role.CanSabotage(mapRoom.room));
                        }
                    }));

                    //return false;
                }

                return true;
            }
        }
        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        [HarmonyPriority(Priority.First)]
        public static class VentCanUsePatch
        {
            public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool canUse,
                [HarmonyArgument(2)] ref bool couldUse, ref float __result)
            {
                Role role = PlayerControl.LocalPlayer.GetRole();

                if (role == null)
                    return;

                couldUse = canUse = role.CanVent;
                __result = float.MaxValue;

                if (canUse)
                {
                    Vector3 center = PlayerControl.LocalPlayer.Collider.bounds.center;
                    Vector3 position = __instance.transform.position;

                    __result = Vector2.Distance(center, position);
                    canUse &= (__result <= __instance.UsableDistance &&
                               !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider, center, position,
                                   Constants.ShipOnlyMask, false));
                }
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcEndGame))]
        [HarmonyPrefix]
        private static bool ShouldGameEndPatch(ShipStatus __instance, [HarmonyArgument(0)] GameOverReason endReason)
        {
            return RoleManager.createdRoles.Count(r => r.Members.Count != 0 && !r.ShouldGameEnd(endReason)) == 0;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
        [HarmonyPrefix]
        private static void OnTaskCompletePatch(PlayerControl __instance, [HarmonyArgument(0)] uint idx)
        {
            PlayerTask playerTask = __instance.myTasks.ToArray().ToList().Find(p => p.Id == idx);
            RoleManager.createdRoles.Do(r => r.OnTaskComplete(__instance, playerTask));
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
        [HarmonyPrefix]
        private static void OnRevivePatch(PlayerControl __instance)
        {
            RoleManager.createdRoles.Do(r => r.OnRevive(__instance));
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        [HarmonyPrefix]
        private static bool PreExiledPatch(PlayerControl __instance)
        {
            return RoleManager.createdRoles.Count(r => r.Members.Count != 0 && !r.PreExile(__instance)) == 0;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        [HarmonyPrefix]
        private static bool PreKillPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            return RoleManager.createdRoles.Count(r => r.Members.Count != 0 && !r.PreKill(__instance, target)) == 0;
        }

    }
}
