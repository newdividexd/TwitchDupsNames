using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;

namespace TwitchDupsNames
{
    public class Config
    {
        static Config instance;
        const string CONFIG_RESOURCE = "TwitchDupsNames.config.json";

        public readonly string ChannelName;
        public readonly bool CleanNames;
        public readonly Dictionary<string, double> Weights;

        //public static readonly string[] ViewerTypes = {"broadcaster", "vips", "moderators", "staff", "admins", "global_mods", "viewers"};

        Config(string channelName, bool cleanNames)
        {
            this.ChannelName = channelName;
            this.CleanNames = cleanNames;
        }

        public static Config Instance { get{ return instance; } }

        static void CreateConfigFile(string configFile)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CONFIG_RESOURCE))
            using (StreamReader reader = new StreamReader(stream))
            {
                string contents = reader.ReadToEnd();
                try
                {
                    File.WriteAllText(configFile, contents);
                }
                catch (Exception e)
                {
                    Debug.Log("TDN unable to create config file " + e.Message);
                }
            }
        }

        static void Load()
        {
            string channelName = "";
            bool cleanNames = true;
            //Dictionary<string, double> weights = ViewerTypes.ToDictionary(type => type, type => 0D);

            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = Path.Combine(folder, "config.json");

            if (File.Exists(configFile))
            {
                try
                {
                    string data = File.ReadAllText(configFile);

                    var configDef = JObject.Parse(data);

                    channelName = configDef["channel"].Value<string>().ToLower();
                    cleanNames = configDef["clean"].Value<bool>();
                }
                catch (Exception e)
                {
                    Debug.Log("TDN unable to read config file " + e.Message);
                    CreateConfigFile(configFile);
                }
            }
            else
            {
                CreateConfigFile(configFile);
            }

            instance = new Config(channelName, cleanNames);
        }

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                Load();
                NameUtils.VerifyChatters();
            }
        }
    }
}
