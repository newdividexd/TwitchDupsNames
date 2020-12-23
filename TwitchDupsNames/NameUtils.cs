using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;

using Newtonsoft.Json.Linq;

namespace TwitchDupsNames
{
    public static class NameUtils
    {
        private readonly static Random Random = new Random();

        private static System.DateTime Last = System.DateTime.Now.AddMinutes(-1);
        private static int Count = 0;
        private static JObject Chatters;

        public static bool VerifyChatters()
        {
            if (Config.Instance == null)
            {
                return false;
            }
            if (Config.Instance.ChannelName.Length == 0)
            {
                return false;
            }
            if ((System.DateTime.Now - Last).TotalMinutes > 1)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create($"https://tmi.twitch.tv/group/user/{Config.Instance.ChannelName}/chatters");
                    request.AutomaticDecompression = DecompressionMethods.GZip;

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var result = JObject.Parse(reader.ReadToEnd());
                        Chatters = result["chatters"] as JObject;
                        Count = result.Value<int>("chatter_count");
                    }
                    Last = System.DateTime.Now;
                } catch(Exception)
                {
                    Debug.Log("TDN unable to get chatters for " + Config.Instance.ChannelName);
                    return false;
                }
            }
            return true;
        }

        private static string Capitalize(string word)
        {
            if (word.Length == 1)
            {
                return word.ToUpper();
            } else
            {
                return word.Substring(0, 1).ToUpper() + word.Substring(1);
            }
        }

        private static string Clean(string name)
        {
            var words = name.Replace('_', ' ').Trim();
            var capitalizedWords = words.Split(' ').Where(w => w.Length > 0).Select(Capitalize);
            return string.Join(" ", capitalizedWords);
        }

        private static List<string> FlattenNames()
        {
            try
            {
                var groups = Chatters.Children<JProperty>()
                    .Select(property => property.Value.Children()
                    .Select(token => token.Value<string>()));

                return groups.Aggregate(new List<string>(), (list, group) =>
                {
                    list.AddRange(group);
                    return list;
                });
            } catch(Exception)
            {
                return new List<string>();
            }
        }

        public static IEnumerable<string> GetNames(int amount)
        {
            if (Count < amount)
            {
                return FlattenNames();
            } else
            {
                HashSet<int> indexes = new HashSet<int>();
                while(indexes.Count < amount)
                {
                    indexes.Add(Random.Next(Count));
                }

                var names = FlattenNames();
                return indexes.Select(i =>
                {
                    if(Config.Instance.CleanNames)
                    {
                        return Clean(names[i]);
                    } else
                    {
                        return names[i];
                    }
                });

            }
            
        }
    }
}
