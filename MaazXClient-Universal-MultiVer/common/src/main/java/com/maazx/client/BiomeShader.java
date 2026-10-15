package com.maazx.client;

public final class BiomeShader {
    private BiomeShader() { }

    public static int blendRgb(int[] colors, float[] weights) {
        if (colors.length != weights.length || colors.length == 0) throw new IllegalArgumentException("Matching non-empty arrays required");
        float red = 0, green = 0, blue = 0, total = 0;
        for (int index = 0; index < colors.length; index++) {
            float weight = Math.max(0, weights[index]);
            red += ((colors[index] >>> 16) & 255) * weight;
            green += ((colors[index] >>> 8) & 255) * weight;
            blue += (colors[index] & 255) * weight;
            total += weight;
        }
        if (total == 0) return colors[0] & 0xFFFFFF;
        return (Math.round(red / total) << 16) | (Math.round(green / total) << 8) | Math.round(blue / total);
    }
}