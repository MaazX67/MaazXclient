package com.maazx.client.optimization.sodium;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public final class ChunkBuildScheduler implements AutoCloseable {
    private final ExecutorService workers = Executors.newFixedThreadPool(3, task -> {
        Thread thread = new Thread(task, "maazx-mesh");
        thread.setDaemon(true);
        return thread;
    });

    public <T> CompletableFuture<T> build(java.util.concurrent.Callable<T> task) {
        return CompletableFuture.supplyAsync(() -> {
            try {
                return task.call();
            } catch (Exception exception) {
                throw new java.util.concurrent.CompletionException(exception);
            }
        }, workers);
    }

    @Override public void close() { workers.shutdown(); }
}