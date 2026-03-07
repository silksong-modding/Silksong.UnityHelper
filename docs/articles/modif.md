# Modifying base game objects

Quite often, you want to modify base game objects as part of your mod. These modifications
can include:

* Destroying a game object, or toggling its active state
* Changing the position, rotation or parent of a game object
* Adding/removing/modifying components attached to a game object

There are two common ways to achieve this, depending on the situation.

## Capturing the game object on scene change

The pattern goes as follows. Listen to one of the Unity scene events, look for the game object using
@"Silksong.UnityHelper.Extensions.UnityExtensions.FindGameObject(UnityEngine.SceneManagement.Scene,System.String)",
and then make your modifications.

For example, suppose you want to destroy the Silk Spool object in Ward_01. You could do so as follows:

```cs
// In your plugin's Awake method, subscribe to the activeSceneChanged event
UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

// Define this function in your plugin class
private void OnSceneChange(Scene oldScene, Scene newScene)
{
    if (newScene.name != "Ward_01")
    {
        // We don't want to do anything in another scene, so we don't bother to make the modification
        return;
    }
    
    GameObject spool = newScene.FindGameObject("Silk Spool");
    // We can do any modifications we like, now that we have a reference to the game object
    UnityEngine.Object.Destroy(spool);
}
```

Alternatively, you can use the `sceneLoaded` event:

```cs
// In your plugin's Awake method, subscribe to the sceneLoaded event
UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoad;

// Define this function in your plugin class
private void OnSceneLoad(Scene scene, LoadSceneMode _)
{
    if (scene.name != "Ward_01")
    {
        // We don't want to do anything in another scene, so we don't bother to make the modification
        return;
    }
    
    GameObject spool = scene.FindGameObject("Silk Spool");
    // We can do any modifications we like, now that we have a reference to the game object
    UnityEngine.Object.Destroy(spool);
}
```

Most of the time, there isn't a strong reason to choose one or the other.

Note that for the FindGameObject method, you have to provide the full path to a game object.
For instance, if you want to modify the `Remasker New - Centre` child of the `Mask Parent` object, you can get it
by doing `scene.FindGameObject("Mask Parent/Remasker New - Centre")`. Or alternatively,
simply do `scene.FindGameObject("Mask Parent").FindChild("Remasker New - Centre")`.

## Capturing the game object by listening to the relevant component

If you want to modify a game object with a component, a guaranteed way to capture the game object
is by hooking (using MonoMod/Harmony/MonoDetour) a method on the game object. For example,
if you want to modify the `wisp temporary block/terrain collider temp blocker` object in Greymoor_06,
which has an ActivateIfPlayerDataFalse component, you can listen to the ActivateIfPlayerDataFalse.Start
method as follows:

```cs
[HarmonyPatch(typeof(ActivateIfPlayerDataFalse), nameof(ActivateIfPlayerDataFalse.Start))]
[HarmonyPrefix]
public static void OnObjectActivate(ActivateIfPlayerDataFalse __instance)
{
    GameObject go = __instance.gameObject;

    if (go.GetNameInHierarchy() == "wisp temporary block/terrain collider temp blocker")
    {
        // Code goes here
    }
}
```

## Common gotchas

Using the GameObject.Find method, particularly in the activeSceneChanged event, is not recommended.
This is for the following reasons:
* If the old scene and the new scene are the same, then GameObject.Find will search the old scene
before the new scene. Since the objects in the old and new scene are the same, the object
modified will be the one in the old scene (which is probably not intended).
* If the old scene was a boss scene, then the new scene may activate before the activeSceneChanged event
fires. If an object destroys itself, then you may miss the chance to modify it; in this case, it is recommended
to hook a component on the gameObject (such as the component destroying the object as in the example).
