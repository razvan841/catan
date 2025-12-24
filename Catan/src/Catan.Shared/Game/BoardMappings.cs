internal static class BoardMappings
{
    // Which tiles does a vertex see
    internal static readonly int[][] VertexToTileAdjacencyMapping = new int[][]
    {
        new int[]{0}, // 0
        new int[]{1}, // 1
        new int[]{2}, // 2
        new int[]{0}, // 3
        new int[]{0, 1}, // 4
        new int[]{1, 2}, // 5
        new int[]{2}, // 6
        new int[]{0, 3}, // 7
        new int[]{0, 1, 4}, // 8
        new int[]{1, 2, 5}, // 9
        new int[]{2, 6}, // 10
        new int[]{3}, // 11
        new int[]{0, 3, 4}, // 12
        new int[]{1, 4, 5}, // 13
        new int[]{2, 5, 6}, // 14
        new int[]{6}, // 15
        new int[]{3, 7}, // 16
        new int[]{3, 4, 8}, // 17
        new int[]{4, 5, 9}, // 18
        new int[]{5, 6, 10}, // 19
        new int[]{6, 11}, // 20
        new int[]{7}, // 21
        new int[]{3, 7, 8}, // 22
        new int[]{4, 8, 9}, // 23
        new int[]{5, 9, 10}, // 24
        new int[]{6, 10, 11}, // 25
        new int[]{11}, // 26
        new int[]{7}, // 27
        new int[]{7, 8, 12}, // 28
        new int[]{8, 9, 13}, // 29
        new int[]{9, 10, 14}, // 30
        new int[]{10, 11, 15}, // 31
        new int[]{11}, // 32
        new int[]{7, 12}, // 33
        new int[]{8, 12, 13}, // 34
        new int[]{9, 13, 14}, // 35
        new int[]{10, 14, 15}, // 36
        new int[]{11, 15}, // 37
        new int[]{12}, // 38
        new int[]{12, 13, 16}, // 39
        new int[]{13, 14, 17}, // 40
        new int[]{14, 15, 18}, // 41
        new int[]{15}, // 42
        new int[]{12, 16}, // 43
        new int[]{13, 16, 17}, // 44
        new int[]{14, 17, 18}, // 45
        new int[]{15, 18}, // 46
        new int[]{16}, // 47
        new int[]{16, 17}, // 48
        new int[]{17, 18}, // 49
        new int[]{18}, // 50
        new int[]{16}, // 51
        new int[]{17}, // 52
        new int[]{18} // 53
    };
    // Which edges does a vertex see
    internal static readonly int[][] VertexToEdgeMapping = new int[][]
    {
        new int[]{0, 1}, // 0
        new int[]{2, 3}, // 1
        new int[]{4, 5}, // 2
        new int[]{0, 6}, // 3
        new int[]{1, 2}, // 4
        new int[]{3, 4}, // 5
        new int[]{5, 9}, // 6
        new int[]{6, 10, 11}, // 7
        new int[]{7, 12, 13}, // 8
        new int[]{8, 14, 15}, // 9
        new int[]{9, 16, 17}, // 10
        new int[]{10, 18}, // 11
        new int[]{11, 12, 19}, // 12
        new int[]{13, 14, 20}, // 13
        new int[]{15, 16, 21}, // 14
        new int[]{17, 22}, // 15
        new int[]{18, 23, 24}, // 16
        new int[]{19, 25, 26}, // 17
        new int[]{20, 27, 28}, // 18
        new int[]{21, 29, 30}, // 19
        new int[]{22, 31, 32}, // 20
        new int[]{23, 33}, // 21
        new int[]{24, 25, 34}, // 22
        new int[]{26, 27, 35}, // 23
        new int[]{28, 29, 36}, // 24
        new int[]{30, 31, 37}, // 25
        new int[]{32, 38}, // 26
        new int[]{33, 39}, // 27
        new int[]{34, 40, 41}, // 28
        new int[]{35, 42, 43}, // 29
        new int[]{36, 44, 45}, // 30
        new int[]{37, 46, 47}, // 31
        new int[]{38, 48}, // 32
        new int[]{39, 40, 49}, // 33
        new int[]{41, 42, 50}, // 34
        new int[]{43, 44, 51}, // 35
        new int[]{45, 46, 52}, // 36
        new int[]{47, 48, 53}, // 37
        new int[]{49, 54}, // 38
        new int[]{50, 55, 56}, // 39
        new int[]{51, 57, 58}, // 40
        new int[]{52, 59, 60}, // 41
        new int[]{53, 61}, // 42
        new int[]{54, 55, 62}, // 43
        new int[]{56, 57, 63}, // 44
        new int[]{58, 59, 64}, // 45
        new int[]{60, 61, 65}, // 46
        new int[]{62, 66}, // 47
        new int[]{63, 67, 68}, // 48
        new int[]{64, 69, 70}, // 49
        new int[]{65, 71}, // 50
        new int[]{66, 67}, // 51
        new int[]{68, 69}, // 52
        new int[]{70, 71} // 53
    };

    // Which tiles does a tile see
    internal static readonly int[][] TileToTileAdjacencyMapping = new int[][]
    {
        new int[]{1, 3, 4}, // 0
        new int[]{0, 2, 4, 5}, // 1
        new int[]{1, 5, 6}, // 2
        new int[]{0, 4, 7, 8}, // 3
        new int[]{0, 1, 3, 5, 8, 9}, // 4
        new int[]{1, 2, 4, 6, 9, 10}, // 5
        new int[]{2, 5, 10, 11}, // 6
        new int[]{3, 8, 12}, // 7
        new int[]{3, 4, 7, 9, 12, 13}, // 8
        new int[]{4, 5, 8, 10, 13, 14}, // 9
        new int[]{5, 6, 9, 11, 14, 15}, // 10
        new int[]{6, 10, 15}, // 11
        new int[]{7, 8, 13, 16}, // 12
        new int[]{8, 9, 12, 14, 16, 17}, // 13
        new int[]{9, 10, 13, 15, 17, 18}, // 14
        new int[]{10, 11, 14, 18}, // 15
        new int[]{12, 13, 17}, // 16
        new int[]{13, 14, 16, 18}, // 17
        new int[]{14, 15, 18}, // 18
    };
    // Which vertices does a tile see
    internal static readonly int[][] TileToVerticesAdjacencyMapping = new int[][]
    {
        new int[]{0, 1, 6, 7, 11, 12}, // 0
        new int[]{2, 3, 7, 8, 13, 14}, // 1
        new int[]{4, 5, 8, 9, 15, 16}, // 2
        new int[]{10, 11, 18, 19, 24, 25}, // 3
        new int[]{12, 13, 19, 20, 26, 27}, // 4
        new int[]{14, 15, 20, 21, 28, 29}, // 5
        new int[]{16, 17, 21, 22, 30, 31}, // 6
        new int[]{23, 24, 33, 34, 39, 40}, // 7
        new int[]{25, 26, 34, 35, 41, 42}, // 8
        new int[]{27, 28, 35, 36, 43, 44}, // 9
        new int[]{29, 30, 36, 37, 45, 46}, // 10
        new int[]{31, 32, 37, 38, 47, 48}, // 11
        new int[]{40, 41, 49, 50, 54, 55}, // 12
        new int[]{42, 43, 50, 51, 56, 57}, // 13
        new int[]{44, 45, 51, 52, 58, 59}, // 14
        new int[]{46, 47, 52, 53, 60, 61}, // 15
        new int[]{55, 56, 62, 63, 66, 67}, // 16
        new int[]{57, 58, 63, 64, 68, 69}, // 17
        new int[]{59, 60, 64, 65, 70, 71}, // 18
    };
    // Which vertexes does an edge bridge
    internal static readonly int[][] EdgeMapping = new int[][]
    {
        new int[]{0, 3}, // 0
        new int[]{0, 4}, // 1
        new int[]{1, 4}, // 2
        new int[]{1, 5}, // 3
        new int[]{2, 5}, // 4
        new int[]{2, 6}, // 5
        new int[]{3, 7}, // 6
        new int[]{4, 8}, // 7
        new int[]{5, 9}, // 8
        new int[]{6, 10}, // 9
        new int[]{7, 11}, // 10
        new int[]{7, 12}, // 11
        new int[]{8, 12}, // 12
        new int[]{8, 13}, // 13
        new int[]{9, 13}, // 14
        new int[]{9, 14}, // 15
        new int[]{10, 14}, // 16
        new int[]{10, 15}, // 17
        new int[]{11, 16}, // 18
        new int[]{12, 17}, // 19
        new int[]{13, 18}, // 20
        new int[]{14, 19}, // 21
        new int[]{15, 20}, // 22
        new int[]{16, 21}, // 23
        new int[]{16, 22}, // 24
        new int[]{17, 22}, // 25
        new int[]{17, 23}, // 26
        new int[]{18, 23}, // 27
        new int[]{18, 24}, // 28
        new int[]{19, 24}, // 29
        new int[]{19, 25}, // 30
        new int[]{20, 25}, // 31
        new int[]{20, 26}, // 32
        new int[]{21, 27}, // 33
        new int[]{22, 28}, // 34
        new int[]{23, 29}, // 35
        new int[]{24, 30}, // 36
        new int[]{25, 31}, // 37
        new int[]{26, 32}, // 38
        new int[]{27, 33}, // 39
        new int[]{28, 33}, // 40
        new int[]{28, 34}, // 41
        new int[]{29, 34}, // 42
        new int[]{29, 35}, // 43
        new int[]{30, 35}, // 44
        new int[]{30, 36}, // 45
        new int[]{31, 36}, // 46
        new int[]{31, 37}, // 47
        new int[]{32, 37}, // 48
        new int[]{33, 38}, // 49
        new int[]{34, 39}, // 50
        new int[]{35, 40}, // 51
        new int[]{36, 41}, // 52
        new int[]{37, 42}, // 53
        new int[]{38, 43}, // 54
        new int[]{39, 43}, // 55
        new int[]{39, 44}, // 56
        new int[]{40, 44}, // 57
        new int[]{40, 45}, // 58
        new int[]{41, 45}, // 59
        new int[]{41, 46}, // 60
        new int[]{42, 46}, // 61
        new int[]{43, 47}, // 62
        new int[]{44, 48}, // 63
        new int[]{45, 49}, // 64
        new int[]{46, 50}, // 65
        new int[]{47, 51}, // 66
        new int[]{48, 51}, // 67
        new int[]{48, 52}, // 68
        new int[]{49, 52}, // 69
        new int[]{49, 53}, // 70
        new int[]{50, 53}, // 71
    };
    // Ports
    internal static readonly Dictionary<string, int> PortsToPlace = new Dictionary<string, int>
    {
        {"generic", 1},
        {"stone", 1},
        {"brick", 1},
        {"wood", 1},
        {"sheep", 1},
        {"wheat", 1}
    };

    // Possible vertices for ports
    internal static readonly int[] PossiblePortVertices = new int[]
    {
        0, 1, 2, 3, 4, 5, 6, 7, 10, 11,
        15, 16, 20, 21, 26, 27, 32, 33,
        37, 38, 42, 43, 46, 47, 48, 49,
        50, 51, 52, 53
    };

    // Tile resources
    internal static readonly Dictionary<string, int> TilesToPlace = new Dictionary<string, int>
    {
        {"sand", 1},
        {"Stone", 3},
        {"Brick", 3},
        {"Wood", 4},
        {"Sheep", 4},
        {"Wheat", 4}
    };

    // Tile number tokens
    internal static readonly Dictionary<int, int> TokensToPlace = new Dictionary<int, int>
    {
        {2, 1}, {3, 2}, {4, 2}, {5, 2}, {6, 2}, 
        {8, 2}, {9, 2}, {10, 2}, {11, 2}, {12, 1}
    };
}