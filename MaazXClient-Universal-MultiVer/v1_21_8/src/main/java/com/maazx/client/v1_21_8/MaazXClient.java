package com.maazx.client.v1_21_8;

import com.maazx.client.BedrockEngine;
import com.maazx.client.MaazXClientConfig;
import net.fabricmc.api.ClientModInitializer;
import net.fabricmc.fabric.api.client.rendering.v1.HudRenderCallback;
import net.fabricmc.loader.api.FabricLoader;
import net.minecraft.client.MinecraftClient;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public final class MaazXClient implements ClientModInitializer {
    private static final Logger LOGGER = LoggerFactory.getLogger("MaazXClient");
    private static MaazXClientConfig config;
    private static BedrockEngine engine;

    @Override
    public void onInitializeClient() {
        try {
            config = MaazXClientConfig.load(FabricLoader.getInstance().getConfigDir().resolve("maazxclient.properties"));
            engine = new BedrockEngine(config);
        } catch (Exception exception) {
            LOGGER.error("Unable to load MaazXClient settings", exception);
            config = new MaazXClientConfig();
            engine = new BedrockEngine(config);
        }
        HudRenderCallback.EVENT.register((drawContext, tickCounter) -> renderHud(drawContext));
    }

    private static void renderHud(net.minecraft.client.gui.DrawContext drawContext) {
        MinecraftClient client = MinecraftClient.getInstance();
        if (client.player == null || !config.hudEnabled()) return;
        int fps = client.getCurrentFps();
        var tuning = engine.samplePerformance(fps, System.nanoTime());
        String text = "MX  " + fps + " FPS  |  RD " + tuning.renderDistance() + "  |  E " + tuning.entityDistance();
        drawContext.drawTextWithShadow(client.textRenderer, text,
                client.getWindow().getScaledWidth() - client.textRenderer.getWidth(text) - 6, 6, 0xFF55FFFF);
    }
}