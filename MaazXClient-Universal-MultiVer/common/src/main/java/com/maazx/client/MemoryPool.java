package com.maazx.client;

import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.function.Supplier;

public final class MemoryPool<T> {
    private final ConcurrentLinkedQueue<T> available = new ConcurrentLinkedQueue<>();
    private final Supplier<T> factory;
    private final int capacity;

    public MemoryPool(Supplier<T> factory, int capacity) {
        this.factory = factory;
        this.capacity = Math.max(0, capacity);
    }

    public T acquire() {
        T value = available.poll();
        return value == null ? factory.get() : value;
    }

    public void release(T value) {
        if (value != null && available.size() < capacity) available.offer(value);
    }
}