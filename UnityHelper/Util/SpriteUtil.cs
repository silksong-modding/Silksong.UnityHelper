using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Silksong.UnityHelper.Util;

/// <summary>
/// Class containing utility methods for loading sprites.
/// </summary>
public static class SpriteUtil
{
    /// <summary>
    /// Load an image from the assembly's embedded resources, and return a Sprite.
    /// </summary>
    /// <param name="asm">The assembly to load from.</param>
    /// <param name="path">The path to the image.</param>
    /// <inheritdoc
    ///     cref="LoadSpriteFromArray(byte[], float, Vector2?, bool)"
    ///     path="//param" />
    /// <returns>A Sprite object.</returns>
    public static Sprite LoadEmbeddedSprite(
        Assembly asm, string path,
        #pragma warning disable CS1573
        float pixelsPerUnit = 64f, Vector2? pivot = null, bool makeUnreadable = false
        #pragma warning restore CS1573
    ) {
        using Stream stream = asm.GetManifestResourceStream(path);

        byte[] buffer = new byte[stream.Length];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        if (bytesRead != buffer.Length)
        {
            throw new IOException($"""
                Failed to read the entire resource stream for path '{path}' in assembly '{asm.FullName}'.
                Expected {stream.Length} bytes, but read {bytesRead}.
                """);
        }

        var result = LoadSpriteFromArray(buffer, pixelsPerUnit, pivot, makeUnreadable);
        result.name = result.texture.name = path;
        return result;
    }

    /// <inheritdoc cref="LoadEmbeddedSprite(Assembly, string, float, Vector2?, bool)" />
    public static Sprite LoadEmbeddedSprite(Assembly asm, string path, float pixelsPerUnit)
        => LoadEmbeddedSprite(asm, path, pixelsPerUnit, null);

    /// <summary>
    /// Load an image from a file on disc and return a Sprite.
    /// </summary>
    /// <param name="fileName">The path to the image file.</param>
    /// <inheritdoc
    ///     cref="LoadSpriteFromArray(byte[], float, Vector2?, bool)"
    ///     path="//param" />
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Sprite LoadSpriteFromFile(
        string fileName,
        #pragma warning disable CS1573
        float pixelsPerUnit = 64f, Vector2? pivot = null, bool makeUnreadable = false
        #pragma warning restore CS1573
    ) {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Filename cannot be empty", nameof(fileName));
        }

        if (!File.Exists(fileName))
        {
            throw new ArgumentException($"File {fileName} not found", nameof(fileName));
        }

        byte[] fileBytes = File.ReadAllBytes(fileName);

        var result = LoadSpriteFromArray(fileBytes, pixelsPerUnit, pivot, makeUnreadable);
        result.name = result.texture.name = Path.GetFileNameWithoutExtension(fileName);
        return result;
    }

    /// <inheritdoc cref="LoadSpriteFromFile(string, float, Vector2?, bool)"/>
    public static Sprite LoadSpriteFromFile(string fileName, float pixelsPerUnit)
        => LoadSpriteFromFile(fileName, pixelsPerUnit, null);

    /// <summary>
    /// Create a sprite from a byte array.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="pixelsPerUnit">The pixels per unit. Changing this value will scale the size of the sprite accordingly.</param>
    /// <param name="pivot">The pivot point of the resulting Sprite. Defaults to the center.</param>
    /// <param name="makeUnreadable">Whether or not to mark the Sprite's underlying texture as unreadable.</param>
    /// <returns></returns>
    public static Sprite LoadSpriteFromArray(byte[] buffer, float pixelsPerUnit = 64f, Vector2? pivot = null, bool makeUnreadable = false)
    {
        Texture2D tex = new(2, 2);

        tex.LoadImage(buffer, makeUnreadable);

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot ?? Vector2.one * 0.5f, pixelsPerUnit);
    }

    /// <inheritdoc cref="LoadSpriteFromArray(byte[], float, Vector2?, bool)"/>
    public static Sprite LoadSpriteFromArray(byte[] buffer, float pixelsPerUnit)
        => LoadSpriteFromArray(buffer, pixelsPerUnit, null);

    /// <summary>
    /// If the given texture is unreadable, returns a readable copy of it.
    /// If the given texture is readable, returns the same texture object.
    /// </summary>
    /// <param name="tex">The texture to make readable.</param>
    /// <returns>A readable <see cref="Texture2D"/> object.</returns>
    public static Texture2D GetReadableCopyOfTexture(Texture2D tex) {
        if (tex.isReadable)
            return tex;

        var temp = RenderTexture.GetTemporary(
            tex.width, tex.height, 0,
            RenderTextureFormat.Default, RenderTextureReadWrite.Linear
        );
        Graphics.Blit(tex, temp);

        var previous = RenderTexture.active;
        RenderTexture.active = temp;

        var readable = new Texture2D(tex.width, tex.height);
        readable.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temp);

        return readable;
    }
}
