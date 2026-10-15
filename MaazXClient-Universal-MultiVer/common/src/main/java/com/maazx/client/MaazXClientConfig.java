package com.maazx.client;

import java.io.IOException;
import java.io.Reader;
import java.io.Writer;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Properties;

public final class MaazXClientConfig {
    private int entityDistance = 32;
    private int renderDistance = 12;
    private int particleLimit = 50;
    private int lodDistance = 4;
    private boolean asyncDisk = true;
    private boolean guarantee60Fps = true;
    private boolean hudEnabled = true;

    public int entityDistance() { return entityDistance; }
    public int renderDistance() { return renderDistance; }
    public int particleLimit() { return particleLimit; }
    public int lodDistance() { return lodDistance; }
    public boolean asyncDisk() { return asyncDisk; }
    public boolean guarantee60Fps() { return guarantee60Fps; }
    public boolean hudEnabled() { return hudEnabled; }

    public void setEntityDistance(int value) { entityDistance = clamp(value, 8, 128); }
    public void setRenderDistance(int value) { renderDistance = clamp(value, 2, 32); }
    public void setParticleLimit(int value) { particleLimit = clamp(value, 0, 500); }
    public void setLodDistance(int value) { lodDistance = clamp(value, 0, 32); }
    public void setAsyncDisk(boolean value) { asyncDisk = value; }
    public void setGuarantee60Fps(boolean value) { guarantee60Fps = value; }
    public void setHudEnabled(boolean value) { hudEnabled = value; }

    public static MaazXClientConfig load(Path path) throws IOException {
        MaazXClientConfig config = new MaazXClientConfig();
        if (!Files.exists(path)) return config;
        Properties values = new Properties();
        try (Reader reader = Files.newBufferedReader(path)) {
            values.load(reader);
        }
        config.setEntityDistance(integer(values, "entity_distance", config.entityDistance));
        config.setRenderDistance(integer(values, "render_distance", config.renderDistance));
        config.setParticleLimit(integer(values, "particle_limit", config.particleLimit));
        config.setLodDistance(integer(values, "lod_distance", config.lodDistance));
        config.setAsyncDisk(Boolean.parseBoolean(values.getProperty("async_disk", "true")));
        config.setGuarantee60Fps(Boolean.parseBoolean(values.getProperty("guarantee_60fps", "true")));
        config.setHudEnabled(Boolean.parseBoolean(values.getProperty("hud_enabled", "true")));
        return config;
    }

    public void save(Path path) throws IOException {
        Files.createDirectories(path.toAbsolutePath().getParent());
        Properties values = new Properties();
        values.setProperty("entity_distance", Integer.toString(entityDistance));
        values.setProperty("render_distance", Integer.toString(renderDistance));
        values.setProperty("particle_limit", Integer.toString(particleLimit));
        values.setProperty("lod_distance", Integer.toString(lodDistance));
        values.setProperty("async_disk", Boolean.toString(asyncDisk));
        values.setProperty("guarantee_60fps", Boolean.toString(guarantee60Fps));
        values.setProperty("hud_enabled", Boolean.toString(hudEnabled));
        try (Writer writer = Files.newBufferedWriter(path)) {
            values.store(writer, "MaazXClient settings");
        }
    }

    private static int integer(Properties properties, String key, int fallback) {
        try {
            return Integer.parseInt(properties.getProperty(key, Integer.toString(fallback)).trim());
        } catch (NumberFormatException ignored) {
            return fallback;
        }
    }

    private static int clamp(int value, int minimum, int maximum) {
        return Math.max(minimum, Math.min(maximum, value));
    }
}