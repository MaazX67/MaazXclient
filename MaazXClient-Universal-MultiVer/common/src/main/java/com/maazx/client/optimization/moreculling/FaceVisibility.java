package com.maazx.client.optimization.moreculling;

public final class FaceVisibility {
    private FaceVisibility() { }
    public static boolean visible(boolean neighborOccludesFace) { return !neighborOccludesFace; }
}