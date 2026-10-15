package com.maazx.client;

public final class AtlasOptimizer {
    public static final int DEFAULT_ATLAS_SIZE = 4096;
    private AtlasOptimizer() { }

    public static int pageSize(int requested, int maximumTextureSize) {
        int limit = Integer.highestOneBit(Math.max(1, Math.min(requested, maximumTextureSize)));
        return Math.max(256, limit);
    }
}