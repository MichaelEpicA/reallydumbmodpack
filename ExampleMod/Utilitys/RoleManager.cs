using HarmonyLib;
using Reactor;
using System.Linq;
using Reactor.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExampleMod.Utilitys
{
    class RoleManager
    {
        public static void InitalizeRoles()
        {
            new Roles.TestRole().Initialize();
            new Roles.Jailor().Initialize();
        }

        public static List<Role> createdRoles = new List<Role>();

        public static int GetRoleId() => createdRoles.Count;

        public static void RegisterRole(Role role) => createdRoles.Add(role);

        internal static RoleBehaviour ToRoleBehaviour(Role brole)
        {
            if (GameObject.Find($"{brole.Name}-Role"))
            {
                return GameObject.Find($"{brole.Name}-Role").GetComponent<RoleBehaviour>();
            }

            var roleObj = new GameObject($"{brole.Name}-Role");
            roleObj.DontDestroy();

            var role = roleObj.AddComponent<RoleBehaviour>();
            role.StringName = CustomStringName.Register(brole.Name);
            role.BlurbName = CustomStringName.Register(brole.Description);
            role.BlurbNameLong = CustomStringName.Register(brole.LongDescription);
            role.BlurbNameMed = CustomStringName.Register(brole.Name);
            role.Role = (RoleTypes) (6 + brole.Id);

            var abilitybuttonsettings = ScriptableObject.CreateInstance<AbilityButtonSettings>();
            abilitybuttonsettings.Image = brole.abilityButtonSprite;
            abilitybuttonsettings.Text = CustomStringName.Register(brole.AbilityName);
            role.Ability = abilitybuttonsettings;

            role.TeamType = brole.Team switch
            {
                Team.Alone => (RoleTeamTypes)3,
                Team.Role => (RoleTeamTypes)3,
                Team.Crewmate => RoleTeamTypes.Crewmate,
                Team.Impostor => RoleTeamTypes.Impostor,
                _ => RoleTeamTypes.Crewmate,
            };
            role.MaxCount = brole.MaxCount;
            role.TasksCountTowardProgress = brole.HasToDoTasks;
            role.CanVent = brole.CanVent;
            role.CanUseKillButton = brole.CanKill();

            PlayerControl.GameOptions.RoleOptions.SetRoleRate(role.Role, brole.MaxCount, brole.Count);

            global::RoleManager.Instance.AllRoles.AddItem(role);


            return role;
        }
        
        public static void ResetRoles()
        {
            //Clearing players in your team
            foreach(Role r in createdRoles)
            {
                r.Members.Clear();
            }
        }

        public static Role GetRole(int id)
        {
            foreach(Role r in createdRoles)
            {
                if(r.Id == id)
                {
                    return r;
                }
            }
            return null;
        }

        public static T GetRole<T>() where T : Role
        {
            foreach (var _role in createdRoles)
            {
                if (_role.GetType() == typeof(T))
                    return (T)_role;
            }

            return null;
        }

        public static class HostMod
        {
            public static Dictionary<Role, bool> IsRole = new Dictionary<Role, bool>();
            public static bool IsImpostor;
        }
    }
}
