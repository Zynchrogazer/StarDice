using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TileVisualCache
{
    private readonly Dictionary<TileType, TileVisualSetting> lookup = new Dictionary<TileType, TileVisualSetting>();
    private bool dirty = true;

    public void MarkDirty()
    {
        dirty = true;
    }

    public TileVisualSetting? Get(TileType type, List<TileVisualSetting> source)
    {
        EnsureBuilt(source);
        return lookup.TryGetValue(type, out TileVisualSetting setting) ? setting : null;
    }

    private void EnsureBuilt(List<TileVisualSetting> source)
    {
        if (!dirty) return;

        lookup.Clear();
        if (source == null)
        {
            dirty = false;
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            TileVisualSetting setting = source[i];
            if (!lookup.ContainsKey(setting.type))
            {
                lookup.Add(setting.type, setting);
            }
        }

        dirty = false;
    }
}

public static class TileVisualApplier
{
    public static void Apply(Transform node, TileVisualSetting visual)
    {
        if (node == null) return;

        SpriteRenderer spriteRenderer = node.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && visual.sprite != null)
        {
            spriteRenderer.sprite = visual.sprite;
        }

        Image uiImage = node.GetComponent<Image>();
        if (uiImage != null && visual.sprite != null)
        {
            uiImage.sprite = visual.sprite;
        }

        Renderer meshRenderer = node.GetComponent<Renderer>();
        if (meshRenderer == null)
        {
            meshRenderer = node.GetComponentInChildren<Renderer>();
        }

        if (meshRenderer == null) return;
        if (visual.material != null)
        {
            meshRenderer.sharedMaterial = visual.material;
        }

        if (visual.texture != null)
        {
            ApplyTexture(meshRenderer, visual.texture);
        }
    }

    private static void ApplyTexture(Renderer renderer, Texture texture)
    {
        if (renderer == null || texture == null) return;

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);

        if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_BaseMap"))
        {
            propertyBlock.SetTexture("_BaseMap", texture);
        }

        if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_MainTex"))
        {
            propertyBlock.SetTexture("_MainTex", texture);
        }

        renderer.SetPropertyBlock(propertyBlock);
    }
}
