# UnityHelper

Library containing utility functions associated with Unity objects.

## Usage Examples

### Custom Animations

The functions in the `Tk2dUtil` class can be used to create custom animations.

For simple one-off animations, you can create animation clips directly:
```cs
Sprite[] frames = [/* load some sprites with the sprite utils */];
// (You can also use Texture2Ds)

tk2dSpriteAnimationClip
    myAnim = Tk2dUtil.CreateTk2dAnimationClip("My Anim", fps: 10, frames),
    myLoopingAnim = Tk2dUtil.CreateTk2dAnimationClip("My Loop Anim", fps: 10, frames, tk2dSpriteAnimationClip.WrapMode.Loop);
```

If you need finer control over the sprite data, animations, or frames, you can also create a sprite collection and then build frames and animations manually:
```cs
Sprite[] sprites = [/* load some sprites with the sprite utils */];
// (You can also use Texture2Ds)

tk2dSpriteCollectionData collection = Tk2dUtil.CreateTk2dSpriteCollection(sprites);

tk2dSpriteAnimationFrame[] frames = [
    collection.CreateFrame(sprites[0].name, triggerEvent: true),
    .. collection.CreateFrames(sprites.Skip(1).Select(x => x.name))
];

tk2dSpriteAnimationClip myAnim = new() {
    name = "My Anim",
    fps = 10,
    wrapMode = tk2dSpriteAnimationClip.WrapMode.Once,
    frames = frames
};
```
