using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

static class MyCustomSettings
{
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        var provider = new SettingsProvider("User/Mr.Puppet", SettingsScope.User)
        {

        };
        return provider;
    }
}
