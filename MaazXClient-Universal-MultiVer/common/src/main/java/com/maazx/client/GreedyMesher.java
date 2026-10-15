package com.maazx.client;

import java.util.ArrayList;
import java.util.List;

public final class GreedyMesher {
    private GreedyMesher() { }

    public static List<Rectangle> merge(boolean[][] occupied) {
        int height = occupied.length;
        if (height == 0) return List.of();
        int width = occupied[0].length;
        boolean[][] consumed = new boolean[height][width];
        List<Rectangle> result = new ArrayList<>();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (!occupied[y][x] || consumed[y][x]) continue;
                int right = x + 1;
                while (right < width && occupied[y][right] && !consumed[y][right]) right++;
                int bottom = y + 1;
                while (bottom < height && rowAvailable(occupied, consumed, y, x, right, bottom)) bottom++;
                for (int row = y; row < bottom; row++) {
                    for (int column = x; column < right; column++) consumed[row][column] = true;
                }
                result.add(new Rectangle(x, y, right - x, bottom - y));
            }
        }
        return List.copyOf(result);
    }

    private static boolean rowAvailable(boolean[][] occupied, boolean[][] consumed, int startY, int left, int right, int row) {
        for (int x = left; x < right; x++) if (!occupied[row][x] || consumed[row][x]) return false;
        return row > startY;
    }

    public record Rectangle(int x, int y, int width, int height) { }
}