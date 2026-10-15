package com.maazx.client.optimization.entityculling;

import com.maazx.client.EntityCuller;

public final class EntityVisibility {
    public static final int DEFAULT_DISTANCE = 32;
    private EntityVisibility() { }
    public static boolean inRange(double dx, double dy, double dz, int configuredDistance) {
        return EntityCuller.withinDistance(dx, dy, dz, Math.max(1, configuredDistance));
    }
}