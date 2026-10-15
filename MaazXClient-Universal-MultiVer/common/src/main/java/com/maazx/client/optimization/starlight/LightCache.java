package com.maazx.client.optimization.starlight;

import java.util.LinkedHashMap;
import java.util.Map;
import java.util.OptionalInt;

public final class LightCache {
    private final int capacity;
    private final Map<Long, Integer> values;

    public LightCache(int capacity) {
        this.capacity = Math.max(1, capacity);
        values = new LinkedHashMap<>(capacity, 0.75f, true);
    }

    public synchronized OptionalInt get(long key) {
        Integer value = values.get(key);
        return value == null ? OptionalInt.empty() : OptionalInt.of(value);
    }

    public synchronized void put(long key, int value) {
        values.put(key, value);
        while (values.size() > capacity) values.remove(values.keySet().iterator().next());
    }

    public synchronized void remove(long key) { values.remove(key); }
    public synchronized void clear() { values.clear(); }
}