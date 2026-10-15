package com.maazx.client;

public final class ParticleCuller {
    private ParticleCuller() { }

    public static int allowance(int configuredLimit, int activeCount, boolean reducedMode) {
        int limit = Math.max(0, Math.min(500, configuredLimit));
        if (reducedMode) limit = Math.min(limit, 10);
        return Math.max(0, limit - Math.max(0, activeCount));
    }
}