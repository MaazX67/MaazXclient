package com.maazx.client;

public final class EntityCuller {
    private EntityCuller() { }

    public static boolean withinDistance(double dx, double dy, double dz, int distance) {
        double limit = (double) distance * distance;
        return dx * dx + dy * dy + dz * dz <= limit;
    }

    public static boolean insideFrustum(Plane[] planes, Sphere sphere) {
        for (Plane plane : planes) {
            if (plane.x * sphere.x + plane.y * sphere.y + plane.z * sphere.z + plane.offset < -sphere.radius) return false;
        }
        return true;
    }

    public record Plane(double x, double y, double z, double offset) { }
    public record Sphere(double x, double y, double z, double radius) { }
}