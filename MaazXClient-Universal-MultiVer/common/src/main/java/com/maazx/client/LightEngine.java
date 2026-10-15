package com.maazx.client;

import java.util.OptionalInt;
import com.maazx.client.optimization.starlight.LightCache;

public final class LightEngine {
    private final LightCache cache;

    public LightEngine(LightCache cache) { this.cache = cache; }
    public OptionalInt cachedLight(long positionKey) { return cache.get(positionKey); }
    public void storeLight(long positionKey, int packedLight) { cache.put(positionKey, packedLight); }
    public void invalidate(long positionKey) { cache.remove(positionKey); }
}