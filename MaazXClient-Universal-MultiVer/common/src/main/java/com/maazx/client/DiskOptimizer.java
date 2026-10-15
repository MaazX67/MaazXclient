package com.maazx.client;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.Executor;
import java.util.function.Supplier;

public final class DiskOptimizer {
    private final Executor executor;
    public DiskOptimizer(Executor executor) { this.executor = executor; }
    public <T> CompletableFuture<T> read(Supplier<T> diskOperation) {
        return CompletableFuture.supplyAsync(diskOperation, executor);
    }
}