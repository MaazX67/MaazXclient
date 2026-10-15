package com.maazx.client;

public final class DynamicFPSGuarantee {
    private static final int[] PARTICLE_LIMITS = {50, 25, 10};
    private final MaazXClientConfig config;
    private int level;
    private long lastSampleNanos;
    private long recoverySinceNanos;

    public DynamicFPSGuarantee(MaazXClientConfig config) {
        this.config = config;
    }

    public Tuning sample(int fps, long nowNanos) {
        if (config.guarantee60Fps() && nowNanos - lastSampleNanos >= 1_000_000_000L) {
            lastSampleNanos = nowNanos;
            if (fps > 0 && fps < 58 && level < 2) {
                level++;
                recoverySinceNanos = 0;
            } else if (fps >= 72 && level > 0) {
                if (recoverySinceNanos == 0) recoverySinceNanos = nowNanos;
                if (nowNanos - recoverySinceNanos >= 15_000_000_000L) {
                    level--;
                    recoverySinceNanos = nowNanos;
                }
            } else {
                recoverySinceNanos = 0;
            }
        } else if (!config.guarantee60Fps()) {
            level = 0;
        }

        int renderDistance = switch (level) {
            case 1 -> Math.max(8, config.renderDistance() - 4);
            case 2 -> Math.max(4, config.renderDistance() - 8);
            default -> config.renderDistance();
        };
        int entityDistance = level == 0 ? config.entityDistance() : Math.min(config.entityDistance(), 16);
        int particles = Math.min(config.particleLimit(), PARTICLE_LIMITS[level]);
        return new Tuning(level, renderDistance, entityDistance, particles, level > 0);
    }

    public record Tuning(int level, int renderDistance, int entityDistance, int particleLimit, boolean shadowsReduced) { }
}