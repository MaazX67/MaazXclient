package com.maazx.client.optimization.lazydfu;

import java.util.Objects;
import java.util.function.Supplier;

public final class LazyValue<T> implements Supplier<T> {
    private Supplier<T> initializer;
    private volatile T value;

    public LazyValue(Supplier<T> initializer) { this.initializer = Objects.requireNonNull(initializer); }

    @Override
    public T get() {
        T current = value;
        if (current != null) return current;
        synchronized (this) {
            if (value == null) {
                value = Objects.requireNonNull(initializer.get());
                initializer = null;
            }
            return value;
        }
    }
}