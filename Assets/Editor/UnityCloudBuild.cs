using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

#if !UNITY_CLOUD_BUILD
namespace UnityEngine.CloudBuild
{
    /// <summary>
    /// Mock implementation of UCB-specific BuildManifestObject. This type is injected into the process when the project is built by UCB.
    /// The mock is used so that the export hook can build outside of UCB as well.
    /// </summary>
    public class BuildManifestObject : ScriptableObject
    {
        // Tries to get a manifest value - returns true if key was found and could be cast to type T, false otherwise.
        public bool TryGetValue<T>(string key, out T result) { throw new NotImplementedException(); }

        // Retrieve a manifest value or throw an exception if the given key isn't found.
        public T GetValue<T>(string key) { throw new NotImplementedException(); }

        // Sets the value for a given key.
        public void SetValue(string key, object value) { throw new NotImplementedException(); }

        // Copy values from a dictionary. ToString() will be called on dictionary values before being stored.
        public void SetValues(Dictionary<string, object> sourceDict) { throw new NotImplementedException(); }

        // Remove all key/value pairs
        public void ClearValues() { throw new NotImplementedException(); }

        // Returns a Dictionary that represents the current BuildManifestObject
        public Dictionary<string, object> ToDictionary() { throw new NotImplementedException(); }

        // Returns a JSON formatted string that represents the current BuildManifestObject
        public string ToJson() { throw new NotImplementedException(); }

        // Returns an INI formatted string that represents the current BuildManifestObject
        public override string ToString() { throw new NotImplementedException(); }
    }
}
#endif

public class UnityCloudBuild : MonoBehaviour
{
    /// <summary>
    /// Callback which will be invoked by UCB before the build begins
    /// UCB will run this if 'UnityCloudBuild.PreExportHandler' is specified as the pre-export hook in the UCB web UI for the project
    /// References: https://docs.unity3d.com/Manual/UnityCloudBuildManifestAsScriptableObject.html
    /// References: https://developer.cloud.unity3d.com/support/guides/manifest/
    /// References: https://docs.unity3d.com/Manual/UnityCloudBuildPreAndPostExportMethods.html
    ///
    /// Test-run this logic by invoking a command like "<path to unity>/Unity.exe -batchmode -quit -executeMethod UnityCloudBuild.PreExportHandler" against a clean repository
    /// References: https://docs.unity3d.com/Manual/CommandLineArguments.html
    /// </summary>
    public static void PreExportHandler(UnityEngine.CloudBuild.BuildManifestObject buildManifest)
    {
        Debug.Log("Running UnityCloudBuild.PreExportHandler");

        UpdateFMODBanks();
    }

    /// <summary>
    /// Ensure that FMOD's *.bank files are copied into the StreamingAssets/ folder before the regular build process begins
    ///
    /// The cache updating and bank copying is normally triggered simply by having the project open for a second in the Editor.
    /// However, if FMODStudioCache.asset* and StreamingAssets/*.bank files are excluded from source control, then FMOD's update logic needs to be run manually
    ///   at the start of the automated build process.
    /// </summary>
    public static void UpdateFMODBanks()
    {
        // Extract countdownTimer from FMODUnity.EventManager.
        // FMODUnity.EventManager.UpdateCache() needs to be invoked this many times, plus once, before it will actually update the cache.
        FieldInfo countdownTimerField = typeof(FMODUnity.EventManager).GetField("countdownTimer", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(countdownTimerField);
        int countdownTimer = (int) countdownTimerField.GetValue(null);

        // This is normally triggered from EventManager.Update() but in the UCB case the project is copied onto a clean machine, and building
        //  is triggered without any Editor Update() ticks being performed first
        for (int i = 0; i < countdownTimer + 1; i++)
        {
            Debug.Log("Invoking FMODUnity.EventManager.UpdateCache");
            FMODUnity.EventManager.UpdateCache();
        }
        Debug.Log("Invoking FMODUnity.EventManager.CopyToStreamingAssets");
        FMODUnity.EventManager.CopyToStreamingAssets();
    }
}
