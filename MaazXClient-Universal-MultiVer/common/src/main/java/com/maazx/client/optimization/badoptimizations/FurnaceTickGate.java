package com.maazx.client.optimization.badoptimizations;

public final class FurnaceTickGate {
    private FurnaceTickGate() { }
    public static boolean needsTick(boolean lit, boolean hasInput, boolean hasFuel, boolean outputBlocked) {
        return lit || (hasInput && hasFuel && !outputBlocked);
    }
}