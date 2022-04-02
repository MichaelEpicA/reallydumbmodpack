using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleMod.Roles
{
    public class TestRole : Role
    {
        public override void Initialize()
        {
            Name = "Test";
            Description = "I finally did it!";
            LongDescription = "SUCK IT NOW";
            TaskDescription = "FUCK";
            Team = Utilitys.Team.Alone;
            Visibility = Utilitys.Visibility.NoOne;
            Color = new UnityEngine.Color(0.07f, 0.651f, 0.487f);
            Id = Utilitys.RoleManager.GetRoleId();
            this.RoleBehaviour = Utilitys.RoleManager.ToRoleBehaviour(this);
            Utilitys.RoleManager.RegisterRole(this);
        }
    }
}
