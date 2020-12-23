using System;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Harmony;
using System.Collections.Generic;

namespace TwitchDupsNames
{


    [HarmonyPatch(typeof(Db), "Initialize")]
    public class DBInitialize
    {

        [HarmonyPostfix]
        public static void Postfix(Db __instance)
        {
            if(NameUtils.VerifyChatters())
            {
                var resources = __instance.Personalities.resources;
                var names = NameUtils.GetNames(resources.Count).ToList();
                for (int i = 0; i < names.Count; i++)
                {
                    resources[i].Name = names[i];
                }
            }
        }

    }
}
