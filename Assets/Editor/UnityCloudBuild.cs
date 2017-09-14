using System;
using System.Collections.Generic;
using UnityEngine;

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
    /// UCB runs this since we have explicitly specified 'UnityCloudBuild.PreExportHandler' as the pre-export hook in the UCB web UI for our project
    /// References: https://docs.unity3d.com/Manual/UnityCloudBuildManifestAsScriptableObject.html
    /// References: https://developer.cloud.unity3d.com/support/guides/manifest/
    /// References: https://docs.unity3d.com/Manual/UnityCloudBuildPreAndPostExportMethods.html
    /// </summary>
    public static void PreExportHandler(UnityEngine.CloudBuild.BuildManifestObject buildManifest)
    {
        Debug.Log("Running UnityCloudBuild.PreExportHandler");

        // Ensure that FMOD's *.bank files are copied into the StreamingAssets/ folder before the regular build process begins
        // This is normally triggered from EventManager.Update() but in the UCB case the project is copied onto a clean machine, and building
        //  is triggered without any Editor Update() ticks being performed first
        Debug.Log("Invoking FMODUnity.EventManager.UpdateCache");
        FMODUnity.EventManager.UpdateCache();
        Debug.Log("Invoking FMODUnity.EventManager.CopyToStreamingAssets");
        FMODUnity.EventManager.CopyToStreamingAssets();
    }
}
