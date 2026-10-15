package com.maazx.client;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.ThreadFactory;
import java.util.concurrent.atomic.AtomicInteger;

public final class AsyncChunkEngine implements AutoCloseable {
    private final ExecutorService workers;

    public AsyncChunkEngine() {
        AtomicInteger sequence = new AtomicInteger();
        ThreadFactory factory = task -> {
            Thread thread = new Thread(task, "maazx-io-" + sequence.incrementAndGet());
            thread.setDaemon(true);
            return thread;
        };
        workers = Executors.newFixedThreadPool(3, factory);
    }

    public <T> CompletableFuture<T> submitIo(java.util.concurrent.Callable<T> task) {
        return CompletableFuture.supplyAsync(() -> {
            try {
                return task.call();
            } catch (Exception exception) {
                throw new java.util.concurrent.CompletionException(exception);
            }
        }, workers);
    }

    @Override
    public void close() {
        workers.shutdown();
    }
}