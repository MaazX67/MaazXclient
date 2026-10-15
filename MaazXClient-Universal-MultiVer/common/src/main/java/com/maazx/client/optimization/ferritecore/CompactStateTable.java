package com.maazx.client.optimization.ferritecore;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public final class CompactStateTable<T> {
    private final Map<T, Integer> ids = new HashMap<>();
    private final List<T> states = new ArrayList<>();

    public synchronized int idFor(T state) {
        Integer existing = ids.get(state);
        if (existing != null) return existing;
        int id = states.size();
        states.add(state);
        ids.put(state, id);
        return id;
    }

    public synchronized T stateFor(int id) { return states.get(id); }
    public synchronized int size() { return states.size(); }
}