package com.maazx.client.optimization.cullleaves;

public final class LeafFaceCuller {
    private LeafFaceCuller() { }
    public static boolean visible(boolean adjacentLeaves, boolean fancyLeaves) {
        return !adjacentLeaves || !fancyLeaves;
    }
}