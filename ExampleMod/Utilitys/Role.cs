using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ExampleMod.Utilitys;
using BepInEx.IL2CPP;
using System.Linq;

namespace ExampleMod
{
    public  class Role
    {
        public static void SetRoleInsideGame(PlayerControl targetPlayer, RoleTypes roleType)
        {
            List<PlayerControl> playerControls = new List<PlayerControl>();
            targetPlayer.roleAssigned = false;
            bool flag = RoleManager.IsGhostRole(roleType);
            if (!DestroyableSingleton<TutorialManager>.InstanceExists && targetPlayer.roleAssigned && !flag)
            {
                return;
            }
            targetPlayer.roleAssigned = true;
            if (flag)
            {
                DestroyableSingleton<RoleManager>.Instance.SetRole(targetPlayer, roleType);
                targetPlayer.Data.Role.SpawnTaskHeader(targetPlayer);
                return;
            }
            DestroyableSingleton<HudManager>.Instance.MapButton.gameObject.SetActive(true);
            DestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActive(true);
            DestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(true);
            PlayerControl.LocalPlayer.RemainingEmergencies = PlayerControl.GameOptions.NumEmergencyMeetings;
            DestroyableSingleton<RoleManager>.Instance.SetRole(targetPlayer, roleType);
            targetPlayer.Data.Role.SpawnTaskHeader(targetPlayer);
            if (!DestroyableSingleton<TutorialManager>.InstanceExists)
            {
                PlayerControl.AllPlayerControls.ForEach((Action<PlayerControl>)delegate (PlayerControl pc)
                {
                    if (pc.Data.Role.TeamType == PlayerControl.LocalPlayer.Data.Role.TeamType)
                    {
                        pc.nameText.color = pc.Data.Role.NameColor;
                    }
                });

                foreach (GameData.PlayerInfo pcd in GameData.Instance.AllPlayers)
                {
                    if (!pcd.Disconnected)
                    {
                        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || pcd.Role.TeamType == PlayerControl.LocalPlayer.Data.Role.TeamType)
                        {
                            playerControls.Add(pcd.Object);
                        }
                    }
                }

            }
        }

        //The sprite used for the AbilityButton
        public virtual Sprite abilityButtonSprite { get; set; } = new Sprite();

        public virtual string AbilityName { get; set; } = "test";    
        public int Id { get; set; }
        public RoleBehaviour RoleBehaviour { get; set; }

        //Player ids in the team
        public List<byte> Members = new List<byte>();
        //The name of the role
        public  string Name { get; set;}
        //Description of the role
        public  string Description { get; set; }
        //Longer description of the role
        public  string LongDescription { get; set; }
        //Description shown in the tasklist
        public  string TaskDescription { get; set;  }
        //Role Color
        public  Color Color { get; set; }
        //Visiblity, who can see what this player is.
        public  Visibility Visibility { get; set; }
        //This role team, who he can win with.
        public  Team Team { get; set; }
        //Should the player get tasks
        public virtual bool AssignTasks { get; set; } = true;
        
        public  bool HasToDoTasks { get; }

        public virtual int Count { get; set; } = 0;

        public virtual int MaxCount { get; set; } = 15;

        public virtual int Chance { get; set; } = 100;

        //If this role can kill, how far can it get away?
        public virtual float KillDistance { get; set; } = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];

        public virtual bool CanKill(PlayerControl target = null)
        {
            return false;
        }

        public virtual bool CanVent { get; } = false;

        //Can they sabotage this system
        public virtual bool CanSabotage(SystemTypes? sabotage)
        {
            return false;
        }

        public virtual bool IsRoleVisible(PlayerControl playerWithRole, PlayerControl perspective)
        {
            return false;
        }

        public virtual bool _IsRoleVisible(PlayerControl playerRole, PlayerControl perspective)
        {
            //If the player who owns the role is the same person who wants to see the PlayersRole the same, then yeah its visible.
            if(playerRole.PlayerId == perspective.PlayerId)
            {
                return true;
            }

            switch(this.Visibility)
            {
                case Visibility.Role: return perspective.IsRole(this);
                case Visibility.Impostor: return perspective.Data.Role.IsImpostor;
                case Visibility.Crewmate: return true;
                case Visibility.NoOne: return false;
                case Visibility.Custom: return this.IsRoleVisible(playerRole, perspective);
                default: throw new NotImplementedException("Unknown Visibility");
            }
        }

        public virtual PlayerControl FindClosestTarget(PlayerControl from, bool protecting)
        {
            PlayerControl result = null;
            float num = KillDistance;
            if (!ShipStatus.Instance)
            {
                return null;
            }
            Vector2 truePosition = from.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != from.PlayerId && !playerInfo.IsDead && (from.GetRole().CanKill(playerInfo.Object) || protecting) && !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && @object.Collider.enabled)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }
        public virtual bool ShouldGameEnd(GameOverReason reason) => true;
        //These functions decide what to do once the game starts/ends
        public virtual void OnGameStart()
        {
            
        }

        public virtual void OnGameEnd()
        {

        }

        internal void _OnUpdate()
        {
            foreach(byte player in Members)
            {
                PlayerControl control = Utils.PlayerById(player);
                if (control == null) continue;
                if (PlayerControl.LocalPlayer == null) continue;
                if(control.IsRole(this) && _IsRoleVisible(control, PlayerControl.LocalPlayer))
                {
                    control.nameText.color = this.Color;
                    control.nameText.text = $"{ control.name}\n{Name}";
                }
            }
            OnUpdate();
        }

        public virtual void OnUpdate()
        {

        }

        internal void _OnMeetingUpdate(MeetingHud __instance)
        {
            foreach (byte player in Members)
            {
                PlayerControl control = Utils.PlayerById(player);
                if (control == null) continue;
                if (PlayerControl.LocalPlayer == null) continue;
                if (control.IsRole(this) && _IsRoleVisible(control, PlayerControl.LocalPlayer))
                {
                    control.nameText.color = this.Color;
                    control.nameText.text = $"{ control.name}\n{Name}";
                }
            }
            
            foreach(var pstate in __instance.playerStates)
            {
                var player = Utils.PlayerById(pstate.TargetPlayerId);
                if (player == null) continue;
                if (PlayerControl.LocalPlayer == null) continue;
                if(player.IsRole(this) && _IsRoleVisible(player, PlayerControl.LocalPlayer))
                {
                    pstate.NameText.color = Color;
                    pstate.NameText.text = $"{player.name}\n{Name}";
                }
            }
            OnMeetingUpdate(__instance);
        }

        public virtual void OnMeetingUpdate(MeetingHud meeting)
        {
        }

        public virtual void OnMeetingStart(MeetingHud meeting)
        {
        }

        public virtual bool PreKill(PlayerControl killer, PlayerControl victim)
        {
            return true;
        }

        public virtual void OnKill(PlayerControl killer, PlayerControl victim)
        {
        }

        public virtual bool PreExile(PlayerControl victim)
        {
            return true;
        }

        public virtual void OnExiled(PlayerControl victim)
        {
        }

        public virtual void OnRevive(PlayerControl player)
        {
        }

        public virtual void OnTaskComplete(PlayerControl player, PlayerTask task)
        {
        }

        public Role(BasePlugin plugin = null)
        {
            Id = Utilitys.RoleManager.GetRoleId();
            this.RoleBehaviour = Utilitys.RoleManager.ToRoleBehaviour(this);
            Utilitys.RoleManager.RegisterRole(this);
        }

        public virtual void Initialize()
        {
            Id = Utilitys.RoleManager.GetRoleId();
            this.RoleBehaviour = Utilitys.RoleManager.ToRoleBehaviour(this);
            Utilitys.RoleManager.RegisterRole(this);
        }
    }
}
