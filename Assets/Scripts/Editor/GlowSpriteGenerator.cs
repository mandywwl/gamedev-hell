using System.IO;
using UnityEditor;
using UnityEngine;

// One-click way to get a soft radial glow sprite for effects like FlickeringLight, since the
// project has no such texture (built-in or imported) and SpriteRenderer needs an actual
// Sprite asset, not a raw Texture2D.
public static class GlowSpriteGenerator
{
    private const string OutputPath = "Assets/Images/GlowSprite.png";
    private const int Size = 256;

    [MenuItem("Tools/Effects/Generate Glow Sprite")]
    private static void GenerateGlowSprite()
    {
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(Size / 2f, Size / 2f);
        float maxDist = Size / 2f;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(dist / maxDist);
                float alpha = Mathf.SmoothStep(1f, 0f, t); // opaque center, transparent edge
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string dir = Path.GetDirectoryName(OutputPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(OutputPath, png);

        AssetDatabase.ImportAsset(OutputPath);

        if (AssetImporter.GetAtPath(OutputPath) is TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        Debug.Log($"Glow sprite generated at {OutputPath}");
    }
}
