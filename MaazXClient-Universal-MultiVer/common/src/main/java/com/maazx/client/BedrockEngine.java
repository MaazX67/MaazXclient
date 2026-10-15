package com.maazx.client;

import com.maazx.client.optimization.starlight.LightCache;
import com.maazx.client.optimization.noisium.BiomeBlendCache;

public final class BedrockEngine {
    private final MaazXClientConfig config;
    private final DynamicFPSGuarantee fpsPolicy;
    private final LightCache lightCache;
    private final BiomeBlendCache biomeCache;

    public BedrockEngine(MaazXClientConfig config) {
        this.config = config;
        fpsPolicy = new DynamicFPSGuarantee(config);
        lightCache = new LightCache(4096);
        biomeCache = new BiomeBlendCache(2048);
    }

    public DynamicFPSGuarantee.Tuning samplePerformance(int fps, long nowNanos) {
        return fpsPolicy.sample(fps, nowNanos);
    }

    public MaazXClientConfig config() { return config; }
    public LightCache lightCache() { return lightCache; }
    public BiomeBlendCache biomeCache() { return biomeCache; }
}