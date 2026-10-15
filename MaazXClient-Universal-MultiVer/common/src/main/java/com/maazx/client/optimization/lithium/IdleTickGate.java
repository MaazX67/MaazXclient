package com.maazx.client.optimization.lithium;

public final class IdleTickGate {
    private long lastChangeTick = Long.MIN_VALUE;
    private boolean enabled = true;

    public void setEnabled(boolean value) { enabled = value; }
    public void markChanged(long tick) { lastChangeTick = tick; }
    public boolean shouldTick(long tick, int interval) {
        return !enabled || tick - lastChangeTick < Math.max(1, interval);
    }
}