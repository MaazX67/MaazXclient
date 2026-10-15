package com.maazx.client.optimization.clumps;

public final class XpOrbBatcher {
    private final int maximum;
    private int stored;

    public XpOrbBatcher(int maximum) { this.maximum = Math.max(1, maximum); }
    public int add(int amount) {
        int accepted = Math.min(Math.max(0, amount), maximum - stored);
        stored += accepted;
        return amount - accepted;
    }
    public int take(int maximumAmount) {
        int result = Math.min(stored, Math.max(0, maximumAmount));
        stored -= result;
        return result;
    }
}