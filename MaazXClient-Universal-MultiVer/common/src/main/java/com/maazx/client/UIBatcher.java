package com.maazx.client;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Consumer;

public final class UIBatcher<T> {
    private final List<T> commands = new ArrayList<>();

    public void add(T command) { commands.add(command); }

    public void flush(Consumer<List<T>> renderer) {
        if (commands.isEmpty()) return;
        renderer.accept(List.copyOf(commands));
        commands.clear();
    }
}