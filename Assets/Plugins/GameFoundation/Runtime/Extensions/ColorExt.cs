using UnityEngine;

namespace GameFoundation.Extensions
{
    public static class ColorExt
    {
        public static Color WithAlpha(this Color color, float alpha) =>
            new(color.r, color.g, color.b, alpha);
    }
}
