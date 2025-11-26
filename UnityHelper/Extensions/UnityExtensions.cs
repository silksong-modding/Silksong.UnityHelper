using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logging.Logger;

namespace Silksong.UnityHelper.Extensions;

/// <summary>
/// Class containing utils and extension methods for Unity objects.
/// </summary>
public static class UnityExtensions
{
    private static readonly ManualLogSource Log = Logger.CreateLogSource($"{nameof(UnityHelperPlugin)}.{nameof(UnityExtensions)}");

    /// <summary>
    /// Get the first component on a game object of a given type.
    /// 
    /// If none found, one will be added and returned.
    /// </summary>
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }

    /// <inheritdoc cref="GetNameInHierarchy(Transform)"/>
    public static string GetNameInHierarchy(this GameObject go) => go.transform.GetNameInHierarchy();

    /// <summary>
    /// Get the name of the gameobject in the form .../grandparent/parent/self starting at the root.
    /// There is no leading slash.
    /// </summary>
    public static string GetNameInHierarchy(this Transform t)
    {
        List<string> parts = [t.gameObject.name];

        while (t.parent != null)
        {
            t = t.parent;
            parts.Add(t.name);
        }

        parts.Reverse();
        return string.Join('/', parts);
    }

    /// <summary>
    /// Find a child of the provided GameObject.
    /// </summary>
    /// <param name="go">The root object to find the child of.</param>
    /// <param name="path">The sub-path to the child, with '/' separating parent GameObjects from child GameObjects.</param>
    /// <returns>The child GameObject if found; null if not.</returns>
    public static GameObject? FindChild(this GameObject go, string path)
    {
        var transform = go.transform.Find(path);
        return transform != null ? transform.gameObject : null;
    }

    /// <summary>
    /// Find a game object by name in the scene. The object's name must be given in the hierarchy.
    /// </summary>
    /// <param name="scene">The scene to search.</param>
    /// <param name="path">The name of the object in the hierarchy, with '/' separating parent GameObjects from child GameObjects.</param>
    /// <returns>The GameObject if found; null if not.</returns>
    public static GameObject? FindGameObject(this Scene scene, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string[] pathParts = path.Split('/', 2);
        string rootName = pathParts[0];

        GameObject? root = scene.GetRootGameObjects().FirstOrDefault(x => x.name == rootName);
        if (root == null)
        {
            return null;
        }

        if (pathParts.Length == 1)
        {
            return root;
        }

        return root.FindChild(pathParts[1]);
    }

    /// <summary>
    /// Execute the given action after the specified number of seconds.
    /// </summary>
    /// <param name="component">The component used to start the coroutine. Often a plugin instance or <see cref="GameManager.instance"/>.</param>
    /// <param name="toInvoke">An Action to invoke.</param>
    /// <param name="seconds">The number of seconds to wait before invoking.</param>
    public static void InvokeAfterSeconds(this MonoBehaviour component, Action toInvoke, float seconds)
    {
        component.StartCoroutine(doInvoke());

        IEnumerator doInvoke()
        {
            yield return new WaitForSeconds(seconds);
            
            try
            {
                toInvoke();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error invoking action {toInvoke.Method.Name}\n" + ex);
            }
        }
    }

    /// <summary>
    /// Execute the given action after the specified number of frames.
    /// </summary>
    /// <param name="component"><inheritdoc cref="InvokeAfterSeconds"/></param>
    /// <param name="toInvoke"><inheritdoc cref="InvokeAfterSeconds"/></param>
    /// <param name="numFrames">The number of frames to wait.</param>
    public static void InvokeAfterFrames(this MonoBehaviour component, Action toInvoke, int numFrames)
    {
        component.StartCoroutine(doInvoke());

        IEnumerator doInvoke()
        {
            for (int i = 0; i < numFrames; i++)
            {
                yield return null;
            }

            try
            {
                toInvoke();
            }
            catch (Exception ex)
            {
                Log.LogError($"Error invoking action {toInvoke.Method.Name}\n" + ex);
            }
        }
    }

    /// <summary>
    /// Execute the given action after 1 frame has passed.
    /// </summary>
    /// <param name="component"><inheritdoc cref="InvokeAfterSeconds"/></param>
    /// <param name="toInvoke"><inheritdoc cref="InvokeAfterSeconds"/></param>
    public static void InvokeNextFrame(this MonoBehaviour component, Action toInvoke) => component.InvokeAfterFrames(toInvoke, 1);

}
