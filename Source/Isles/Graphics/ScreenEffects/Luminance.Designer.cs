// -----------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:v2.0.50727
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// -----------------------------------------------------------------------------
namespace Isles.Graphics.ScreenEffects {
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    
    
    partial class Luminance : Microsoft.Xna.Framework.Graphics.Effect {
        
        private static byte[] effectCode;
        
        #region Shader Byte Code
        static Luminance() {
            #if XBOX360 //;
            effectCode = new byte[] {
                    254,
                    255,
                    9,
                    1,
                    0,
                    0,
                    0,
                    88,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    10,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    28,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    8,
                    83,
                    97,
                    109,
                    112,
                    108,
                    101,
                    114,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    15,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    3,
                    112,
                    48,
                    0,
                    0,
                    0,
                    0,
                    0,
                    10,
                    76,
                    117,
                    109,
                    105,
                    110,
                    97,
                    110,
                    99,
                    101,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    3,
                    0,
                    0,
                    0,
                    2,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    24,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    72,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    64,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    93,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    44,
                    0,
                    0,
                    0,
                    40,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    255,
                    255,
                    255,
                    255,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    100,
                    16,
                    42,
                    17,
                    0,
                    0,
                    0,
                    0,
                    208,
                    0,
                    0,
                    0,
                    148,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    36,
                    0,
                    0,
                    0,
                    132,
                    0,
                    0,
                    0,
                    172,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    92,
                    0,
                    0,
                    0,
                    28,
                    0,
                    0,
                    0,
                    79,
                    255,
                    255,
                    3,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    28,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    72,
                    0,
                    0,
                    0,
                    48,
                    0,
                    3,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    56,
                    0,
                    0,
                    0,
                    0,
                    83,
                    97,
                    109,
                    112,
                    108,
                    101,
                    114,
                    0,
                    0,
                    4,
                    0,
                    12,
                    0,
                    1,
                    0,
                    1,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    112,
                    115,
                    95,
                    51,
                    95,
                    48,
                    0,
                    50,
                    46,
                    48,
                    46,
                    56,
                    50,
                    55,
                    53,
                    46,
                    48,
                    0,
                    171,
                    171,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    20,
                    1,
                    252,
                    0,
                    16,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    64,
                    0,
                    0,
                    0,
                    84,
                    16,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    8,
                    33,
                    0,
                    1,
                    0,
                    1,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    48,
                    80,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    61,
                    233,
                    120,
                    213,
                    62,
                    153,
                    22,
                    135,
                    63,
                    22,
                    69,
                    162,
                    56,
                    209,
                    183,
                    23,
                    63,
                    49,
                    114,
                    24,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    16,
                    2,
                    0,
                    0,
                    18,
                    0,
                    196,
                    0,
                    0,
                    0,
                    0,
                    0,
                    48,
                    3,
                    0,
                    0,
                    34,
                    0,
                    0,
                    0,
                    0,
                    0,
                    16,
                    8,
                    0,
                    1,
                    31,
                    31,
                    250,
                    136,
                    0,
                    0,
                    64,
                    0,
                    200,
                    1,
                    0,
                    0,
                    0,
                    0,
                    62,
                    0,
                    111,
                    254,
                    0,
                    0,
                    64,
                    32,
                    0,
                    0,
                    0,
                    0,
                    0,
                    108,
                    226,
                    0,
                    0,
                    128,
                    168,
                    50,
                    192,
                    0,
                    0,
                    0,
                    0,
                    65,
                    194,
                    0,
                    0,
                    255,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0};
            #else //;
            effectCode = new byte[] {
                    1,
                    9,
                    255,
                    254,
                    88,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    10,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    28,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    8,
                    0,
                    0,
                    0,
                    83,
                    97,
                    109,
                    112,
                    108,
                    101,
                    114,
                    0,
                    1,
                    0,
                    0,
                    0,
                    15,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    3,
                    0,
                    0,
                    0,
                    112,
                    48,
                    0,
                    0,
                    10,
                    0,
                    0,
                    0,
                    76,
                    117,
                    109,
                    105,
                    110,
                    97,
                    110,
                    99,
                    101,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    3,
                    0,
                    0,
                    0,
                    2,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    24,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    72,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    64,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    147,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    44,
                    0,
                    0,
                    0,
                    40,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    255,
                    255,
                    255,
                    255,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    108,
                    1,
                    0,
                    0,
                    0,
                    2,
                    255,
                    255,
                    254,
                    255,
                    34,
                    0,
                    67,
                    84,
                    65,
                    66,
                    28,
                    0,
                    0,
                    0,
                    79,
                    0,
                    0,
                    0,
                    0,
                    2,
                    255,
                    255,
                    1,
                    0,
                    0,
                    0,
                    28,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    72,
                    0,
                    0,
                    0,
                    48,
                    0,
                    0,
                    0,
                    3,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    56,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    83,
                    97,
                    109,
                    112,
                    108,
                    101,
                    114,
                    0,
                    4,
                    0,
                    12,
                    0,
                    1,
                    0,
                    1,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    112,
                    115,
                    95,
                    50,
                    95,
                    48,
                    0,
                    77,
                    105,
                    99,
                    114,
                    111,
                    115,
                    111,
                    102,
                    116,
                    32,
                    40,
                    82,
                    41,
                    32,
                    68,
                    51,
                    68,
                    88,
                    57,
                    32,
                    83,
                    104,
                    97,
                    100,
                    101,
                    114,
                    32,
                    67,
                    111,
                    109,
                    112,
                    105,
                    108,
                    101,
                    114,
                    32,
                    57,
                    46,
                    49,
                    53,
                    46,
                    55,
                    55,
                    57,
                    46,
                    48,
                    48,
                    48,
                    48,
                    0,
                    171,
                    171,
                    171,
                    254,
                    255,
                    1,
                    0,
                    80,
                    82,
                    69,
                    83,
                    81,
                    0,
                    0,
                    5,
                    0,
                    0,
                    15,
                    160,
                    135,
                    22,
                    153,
                    62,
                    162,
                    69,
                    22,
                    63,
                    213,
                    120,
                    233,
                    61,
                    23,
                    183,
                    209,
                    56,
                    81,
                    0,
                    0,
                    5,
                    1,
                    0,
                    15,
                    160,
                    0,
                    0,
                    128,
                    63,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    81,
                    0,
                    0,
                    5,
                    2,
                    0,
                    15,
                    160,
                    24,
                    114,
                    49,
                    63,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    31,
                    0,
                    0,
                    2,
                    0,
                    0,
                    0,
                    128,
                    0,
                    0,
                    3,
                    176,
                    31,
                    0,
                    0,
                    2,
                    0,
                    0,
                    0,
                    144,
                    0,
                    8,
                    15,
                    160,
                    66,
                    0,
                    0,
                    3,
                    0,
                    0,
                    15,
                    128,
                    0,
                    0,
                    228,
                    176,
                    0,
                    8,
                    228,
                    160,
                    8,
                    0,
                    0,
                    3,
                    0,
                    0,
                    1,
                    128,
                    0,
                    0,
                    228,
                    128,
                    0,
                    0,
                    228,
                    160,
                    2,
                    0,
                    0,
                    3,
                    0,
                    0,
                    8,
                    128,
                    0,
                    0,
                    0,
                    128,
                    0,
                    0,
                    255,
                    160,
                    15,
                    0,
                    0,
                    2,
                    0,
                    0,
                    8,
                    128,
                    0,
                    0,
                    255,
                    128,
                    5,
                    0,
                    0,
                    3,
                    0,
                    0,
                    1,
                    128,
                    0,
                    0,
                    255,
                    128,
                    2,
                    0,
                    0,
                    160,
                    1,
                    0,
                    0,
                    2,
                    0,
                    0,
                    6,
                    128,
                    1,
                    0,
                    210,
                    160,
                    1,
                    0,
                    0,
                    2,
                    0,
                    0,
                    8,
                    128,
                    1,
                    0,
                    170,
                    160,
                    1,
                    0,
                    0,
                    2,
                    0,
                    8,
                    15,
                    128,
                    0,
                    0,
                    228,
                    128,
                    255,
                    255,
                    0,
                    0};
            #endif //;
        }
        #endregion
        
        private void InitializeComponent() {
        }
    }
}
