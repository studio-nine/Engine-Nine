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
    
    
    partial class SaturationEffect : Microsoft.Xna.Framework.Graphics.Effect {
        
        private static byte[] effectCode;
        
        private Microsoft.Xna.Framework.Graphics.EffectParameter _Saturation;
        
        #region Shader Byte Code
        static SaturationEffect() {
            #if XBOX360 //;
            effectCode = new byte[] {
                    254,
                    255,
                    9,
                    1,
                    0,
                    0,
                    0,
                    188,
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
                    3,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    120,
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
                    0,
                    0,
                    0,
                    1,
                    63,
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
                    4,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    96,
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
                    17,
                    83,
                    97,
                    115,
                    85,
                    105,
                    68,
                    101,
                    115,
                    99,
                    114,
                    105,
                    112,
                    116,
                    105,
                    111,
                    110,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    11,
                    83,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    0,
                    0,
                    0,
                    0,
                    0,
                    2,
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
                    6,
                    80,
                    97,
                    115,
                    115,
                    49,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    11,
                    83,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    0,
                    0,
                    0,
                    0,
                    0,
                    2,
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
                    3,
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
                    40,
                    0,
                    0,
                    0,
                    68,
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
                    76,
                    0,
                    0,
                    0,
                    72,
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
                    1,
                    0,
                    0,
                    0,
                    160,
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
                    140,
                    0,
                    0,
                    0,
                    136,
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
                    1,
                    0,
                    0,
                    0,
                    32,
                    71,
                    101,
                    116,
                    115,
                    32,
                    111,
                    114,
                    32,
                    115,
                    101,
                    116,
                    115,
                    32,
                    115,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    32,
                    97,
                    109,
                    111,
                    117,
                    110,
                    116,
                    46,
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
                    0,
                    0,
                    1,
                    164,
                    16,
                    42,
                    17,
                    0,
                    0,
                    0,
                    1,
                    16,
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
                    196,
                    0,
                    0,
                    0,
                    236,
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
                    156,
                    0,
                    0,
                    0,
                    28,
                    0,
                    0,
                    0,
                    143,
                    255,
                    255,
                    3,
                    0,
                    0,
                    0,
                    0,
                    2,
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
                    136,
                    0,
                    0,
                    0,
                    68,
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
                    76,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    92,
                    0,
                    2,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    104,
                    0,
                    0,
                    0,
                    120,
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
                    83,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    0,
                    171,
                    0,
                    0,
                    0,
                    3,
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
                    1,
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
                    225,
                    71,
                    174,
                    62,
                    153,
                    153,
                    154,
                    63,
                    23,
                    10,
                    61,
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
                    16,
                    1,
                    31,
                    31,
                    246,
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
                    190,
                    192,
                    0,
                    176,
                    1,
                    255,
                    0,
                    200,
                    15,
                    0,
                    1,
                    4,
                    108,
                    0,
                    0,
                    224,
                    0,
                    1,
                    0,
                    200,
                    15,
                    128,
                    0,
                    0,
                    0,
                    108,
                    108,
                    171,
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
                    0,
                    0,
                    0};
            #else //;
            effectCode = new byte[] {
                    1,
                    9,
                    255,
                    254,
                    188,
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
                    3,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    120,
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
                    63,
                    1,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    4,
                    0,
                    0,
                    0,
                    96,
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
                    17,
                    0,
                    0,
                    0,
                    83,
                    97,
                    115,
                    85,
                    105,
                    68,
                    101,
                    115,
                    99,
                    114,
                    105,
                    112,
                    116,
                    105,
                    111,
                    110,
                    0,
                    0,
                    0,
                    0,
                    11,
                    0,
                    0,
                    0,
                    83,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    0,
                    0,
                    2,
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
                    6,
                    0,
                    0,
                    0,
                    80,
                    97,
                    115,
                    115,
                    49,
                    0,
                    0,
                    0,
                    11,
                    0,
                    0,
                    0,
                    83,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    0,
                    0,
                    2,
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
                    3,
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
                    40,
                    0,
                    0,
                    0,
                    68,
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
                    76,
                    0,
                    0,
                    0,
                    72,
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
                    1,
                    0,
                    0,
                    0,
                    160,
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
                    140,
                    0,
                    0,
                    0,
                    136,
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
                    1,
                    0,
                    0,
                    0,
                    32,
                    0,
                    0,
                    0,
                    71,
                    101,
                    116,
                    115,
                    32,
                    111,
                    114,
                    32,
                    115,
                    101,
                    116,
                    115,
                    32,
                    115,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    32,
                    97,
                    109,
                    111,
                    117,
                    110,
                    116,
                    46,
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
                    76,
                    1,
                    0,
                    0,
                    0,
                    2,
                    255,
                    255,
                    254,
                    255,
                    50,
                    0,
                    67,
                    84,
                    65,
                    66,
                    28,
                    0,
                    0,
                    0,
                    143,
                    0,
                    0,
                    0,
                    0,
                    2,
                    255,
                    255,
                    2,
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
                    136,
                    0,
                    0,
                    0,
                    68,
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
                    76,
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
                    2,
                    0,
                    0,
                    0,
                    1,
                    0,
                    0,
                    0,
                    104,
                    0,
                    0,
                    0,
                    120,
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
                    83,
                    97,
                    116,
                    117,
                    114,
                    97,
                    116,
                    105,
                    111,
                    110,
                    0,
                    171,
                    0,
                    0,
                    3,
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
                    0,
                    0,
                    0,
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
                    1,
                    0,
                    15,
                    160,
                    154,
                    153,
                    153,
                    62,
                    61,
                    10,
                    23,
                    63,
                    174,
                    71,
                    225,
                    61,
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
                    1,
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
                    2,
                    0,
                    1,
                    128,
                    1,
                    0,
                    228,
                    128,
                    1,
                    0,
                    228,
                    160,
                    18,
                    0,
                    0,
                    4,
                    0,
                    0,
                    15,
                    128,
                    0,
                    0,
                    0,
                    160,
                    1,
                    0,
                    228,
                    128,
                    2,
                    0,
                    0,
                    128,
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
        
        /// <summary>
        /// Gets or sets saturation amount.
        /// </summary>
        public float Saturation {
            get {
                return this._Saturation.GetValueSingle();
            }
            set {
                this._Saturation.SetValue(value);
            }
        }
        
        private void InitializeComponent() {
            this._Saturation = this.Parameters["Saturation"];
        }
    }
}