using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using ExampleMod.Utilitys;
using System.Linq;
using Reactor.Extensions;

namespace ExampleMod
{
	public static class Utils
	{
		public static PlayerControl PlayerById(byte id)
		{
			foreach (var player in PlayerControl.AllPlayerControls)
				if (player.PlayerId == id)
					return player;
			return null;
		}

		public static void ExtractEmbeddedResource(string outputDir, string resourceLocation, string file)
		{
			foreach (string s in Assembly.GetExecutingAssembly().GetManifestResourceNames())
			{
				Debug.Log(s);
			}
			using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + "." + file))
			{
				using (FileStream fileStream = new FileStream(outputDir, FileMode.Create))
				{
					int num = 0;
					while ((long)num < manifestResourceStream.Length)
					{
						fileStream.WriteByte((byte)manifestResourceStream.ReadByte());
						num++;
					}
				}
			}
		}

		public static Role GetRole(this PlayerControl plr)
		{
			foreach (Role r in Utilitys.RoleManager.createdRoles)
			{
				if (r.Members.Contains(plr.PlayerId))
				{
					return r;
				}
			}
			return null;
		}

		public static T? GetRole<T>(this PlayerControl player) where T : Role
		   => player.GetRole() as T;

		public static bool IsRole(this PlayerControl player, Role role) => player.GetRole() == role;

		public static bool IsRole<T>(this PlayerControl player) where T : Role
			=> player.GetRole<T>() != null;


		public static void SetRole(this PlayerControl player, Role? role)
		{
			var oldRole = Utilitys.RoleManager.createdRoles.Where(r => r.Members.Contains(player.PlayerId)).ToList();
			if (oldRole.Count != 0)
				oldRole[0].Members.Remove(player.PlayerId);

			if (role != null)
			{
				role.Members.Add(player.PlayerId);
			}
			else if (player.IsLocal())
			{
				var isImpostor = player.Data.Role.IsImpostor;
				var isDead = player.Data.IsDead;

				if (oldRole.Count != 0)
					GameObject.Find(oldRole[0].Name + "Task").Destroy();
				HudManager.Instance.SabotageButton.gameObject.SetActive(isImpostor);
				HudManager.Instance.KillButton.gameObject.SetActive(isImpostor && !isDead);
				HudManager.Instance.ImpostorVentButton.gameObject.SetActive(isImpostor && !isDead);

				player.nameText.color = isImpostor ? Palette.ImpostorRed : Color.white;
				player.nameText.text = player.name;
			}
		}

		public static bool IsLocal(this PlayerControl player)
		{
			return player.PlayerId == PlayerControl.LocalPlayer.PlayerId;
		}
	}
}
