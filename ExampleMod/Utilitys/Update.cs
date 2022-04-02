using BepInEx.IL2CPP.Utils;
using Reactor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using TMPro;
using UnityEngine;

namespace ExampleMod
{
    class Update
    {

        static string ver = "";
        static string GetLatestVersion()
        {
            StringBuilder tagb = new StringBuilder();
            string tag = "";
            Uri uri = new Uri("https://github.com/MichaelEpicA/reallydumbmodpack/releases/latest");
            WebRequest wr = WebRequest.Create(uri);
            WebResponse wre = wr.GetResponse();
            Uri response = wre.ResponseUri;
            //Combine missing characters
            tag = response.ToString().Split('/')[7];
            return tag;
        }

        static void DownloadLatestVersion()
        {
            try
            {
                WebClient client = new WebClient();
                string tag = ver;
                Uri downloadurl = new Uri("https://github.com/MichaelEpicA/reallydumbmodpack/releases/download/" + tag + "/ExampleMod.dll");
                client.DownloadFile(downloadurl, Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "ExampleMod_update.dll"));
            }
            catch (Exception ex)
            {
            Logger<ReactorPlugin>.Message(ex);
            }

        }

        public static void CheckForUpdates()
        {
            ver = GetLatestVersion();
              Logger<ReactorPlugin>.Message("Latest Version: " + ver + " Version: " + ExampleModPlugin.Version);
            if (!VersionMatch("v" + ExampleModPlugin.Version, ver))
            {
                AskForUpdate();
            }
        }

        static void InstallUpdate()
        {
            Utils.ExtractEmbeddedResource(Application.streamingAssetsPath + "install.bat", "ExampleMod.Resources", "install.bat");
            string path = Application.streamingAssetsPath + "install.bat";
            ProcessStartInfo startinfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = '"' + Environment.CurrentDirectory + '"'
            };
            Process.Start(startinfo).WaitForExit();
            File.Delete(path);
        }
        public static bool VersionMatch(string a, string b)
        {
            string c = a.Substring(a.IndexOf("v") + 1, a.IndexOf("-"));
            string s = b.Substring(b.IndexOf("v") + 1, b.IndexOf("-"));
            string[] client = c.Split(".");
            string[] server = s.Split(".");
            if(a.Length != b.Length)
            {
                return false;
            }
            if (client.Length != server.Length)
            {
                return false;
            }
            for (int i = 0; i < client.Length; i++)
            {
                bool isInt = true;
                int cv = new int();
                int sv = new int();
                isInt = Int32.TryParse(client[i], out cv) && isInt;
                isInt = Int32.TryParse(server[i], out sv) && isInt;
                if (isInt)
                {
                    if (cv < sv)
                    {
                        return false;
                    }
                }
                else
                {
                    if (client[i] != server[i])
                    {
                        return false;
                    }
                }       
            }
            return true;
        }

        static bool AskForUpdate()
        {
            GooglePlayAssetHandler handler = UnityEngine.Object.FindObjectOfType<GooglePlayAssetHandler>();
            handler.confirmPopUp.SetActive(true);
            Transform TMP = handler.confirmPopUp.transform.FindChild("InfoText_TMP");
            TMP.GetComponent<TextMeshPro>().text = "New update available! \n \n A new update is available for ExampleMod! Choose Update to update or Cancel to not update and stay on this old version.";
            UnityEngine.Object.Destroy(TMP.GetComponent<TextTranslatorTMP>());
            handler.confirmPopUp.transform.FindChild("AcceptButton").transform.FindChild("Text_TMP").GetComponent<TextMeshPro>().text = "Update";
            UnityEngine.Object.Destroy(handler.confirmPopUp.transform.FindChild("AcceptButton").transform.FindChild("Text_TMP").GetComponent<TextTranslatorTMP>());
            handler.confirmPopUp.transform.FindChild("AcceptButton").GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            handler.confirmPopUp.transform.FindChild("AcceptButton").GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)delegate () {
                GooglePlayAssetHandler handler = UnityEngine.Object.FindObjectOfType<GooglePlayAssetHandler>();
                handler.confirmPopUp.SetActive(false);
                handler.popup.SetActive(true);
                handler.downloading = true;
                handler.StartCoroutine(handler.DoRun());
                DownloadLatestVersion();
                InstallUpdate();
            });
            handler.confirmPopUp.transform.FindChild("CloseButton").GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)delegate ()
            {
                handler.downloading = false;

            });
            return handler.downloading;

        }
    }
}
