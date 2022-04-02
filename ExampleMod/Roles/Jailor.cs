using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleMod.Roles
{
    public class Jailor : Role
    {
        public override void Initialize() 
        {
            Name = "Jailor";
            Description = "Trap the crewmates.";
            LongDescription = "Trap and kill the crewmates.";
            TaskDescription = "wut does this do";
            Team = Utilitys.Team.Impostor;
            Visibility = Utilitys.Visibility.NoOne;
            Color = new UnityEngine.Color(0.875f, 0.875f, 0.875f);
            Id = Utilitys.RoleManager.GetRoleId();
            this.RoleBehaviour = Utilitys.RoleManager.ToRoleBehaviour(this);
            Utilitys.RoleManager.RegisterRole(this);
        }
    }
}
