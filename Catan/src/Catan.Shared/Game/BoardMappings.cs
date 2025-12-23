internal static class BoardMappings
{
    internal static readonly int[][] VertexAdjacencyMapping = new int[][]
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

    internal static readonly int[][] TileAdjacencyMapping = new int[][]
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
        {"stone", 3},
        {"brick", 3},
        {"wood", 4},
        {"sheep", 4},
        {"wheat", 4}
    };

    // Tile number tokens
    internal static readonly Dictionary<int, int> TokensToPlace = new Dictionary<int, int>
    {
        {2, 1}, {3, 2}, {4, 2}, {5, 2}, {6, 2}, 
        {8, 2}, {9, 2}, {10, 2}, {11, 2}, {12, 1}
    };
}