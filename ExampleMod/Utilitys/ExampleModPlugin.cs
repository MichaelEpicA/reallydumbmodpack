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
using System.Net;
using System.Diagnostics;
using System.Text;

namespace ExampleMod
{
    [BepInAutoPlugin]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]

    public partial class ExampleModPlugin : BasePlugin
    {

        public Harmony Harmony { get; } = new(Id);

        public ConfigEntry<string> ConfigName { get; private set; }

        public static bool gameStarted;
        public override void Load()
        {
            ConfigName = Config.Bind("Fake", "Name", "what");

            Harmony.PatchAll();

            //Load Asset Bundle
            Assembly asm = Assembly.GetExecutingAssembly();
            var name = asm.GetManifestResourceNames().FirstOrDefault(s => s.Contains("assets", StringComparison.CurrentCultureIgnoreCase));
            Logger<ReactorPlugin>.Message($"AssetBundle Found: {name}");
            var stream = new MemoryStream();
            asm.GetManifestResourceStream(name)!.CopyTo(stream);
            Patches.bundle = AssetBundle.LoadFromMemory(stream.ToArray());
        }

       
    }
}

