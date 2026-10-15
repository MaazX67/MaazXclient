package com.maazx.client;

public final class BlockEntityCuller {
    private BlockEntityCuller() { }
    public static boolean shouldRender(double dx, double dy, double dz) {
        return dx * dx + dy * dy + dz * dz <= 24.0 * 24.0;
    }
}