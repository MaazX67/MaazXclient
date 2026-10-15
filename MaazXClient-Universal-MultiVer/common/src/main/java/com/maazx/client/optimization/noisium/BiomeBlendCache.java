package com.maazx.client.optimization.noisium;

import java.util.LinkedHashMap;
import java.util.Map;
import java.util.function.IntSupplier;

public final class BiomeBlendCache {
    private final int capacity;
    private final Map<Long, Integer> values;

    public BiomeBlendCache(int capacity) {
        this.capacity = Math.max(1, capacity);
        values = new LinkedHashMap<>(capacity, 0.75f, true);
    }

    public synchronized int getOrCompute(long key, IntSupplier sampler) {
        Integer value = values.get(key);
        if (value != null) return value;
        value = sampler.getAsInt();
        values.put(key, value);
        while (values.size() > capacity) values.remove(values.keySet().iterator().next());
        return value;
    }

    public synchronized void invalidate(long key) { values.remove(key); }
    public synchronized void clear() { values.clear(); }
}