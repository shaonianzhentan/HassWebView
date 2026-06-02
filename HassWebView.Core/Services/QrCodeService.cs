using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HassWebView.Core.Services
{
    /// <summary>
    /// 二维码生成服务 - 纯C#实现，不依赖第三方库
    /// 支持 QR Code Model 2，版本 1-10
    /// </summary>
    public static class QrCodeService
    {
        // QR Code 模式指示符
        private const int MODE_NUMERIC = 1;
        private const int MODE_ALPHANUMERIC = 2;
        private const int MODE_BYTE = 4;

        // 纠错级别
        public enum ErrorCorrectionLevel
        {
            L = 0, // 7%
            M = 1, // 15%
            Q = 2, // 25%
            H = 3  // 30%
        }

        // 版本容量表 (字符数) - [版本-1][模式][纠错级别]
        // 模式: 0=Numeric, 1=Alphanumeric, 2=Byte
        private static readonly int[][][] CAPACITY_TABLE = new int[][][]
        {
            new int[][] { new int[] { 41, 25, 17, 10 }, new int[] { 34, 20, 14, 8 }, new int[] { 27, 16, 11, 7 }, new int[] { 17, 10, 7, 4 } },
            new int[][] { new int[] { 77, 47, 32, 20 }, new int[] { 63, 38, 26, 16 }, new int[] { 48, 29, 20, 12 }, new int[] { 34, 20, 14, 8 } },
            new int[][] { new int[] { 127, 77, 53, 32 }, new int[] { 101, 61, 42, 26 }, new int[] { 77, 47, 32, 20 }, new int[] { 58, 35, 24, 15 } },
            new int[][] { new int[] { 187, 114, 78, 48 }, new int[] { 149, 90, 62, 38 }, new int[] { 111, 67, 46, 28 }, new int[] { 82, 50, 34, 21 } },
            new int[][] { new int[] { 255, 154, 106, 65 }, new int[] { 202, 122, 84, 52 }, new int[] { 144, 87, 60, 37 }, new int[] { 106, 64, 44, 27 } },
            new int[][] { new int[] { 322, 195, 134, 82 }, new int[] { 255, 154, 106, 65 }, new int[] { 178, 108, 74, 45 }, new int[] { 139, 84, 58, 36 } },
            new int[][] { new int[] { 370, 224, 154, 95 }, new int[] { 293, 178, 122, 75 }, new int[] { 207, 125, 86, 53 }, new int[] { 154, 93, 64, 39 } },
            new int[][] { new int[] { 461, 279, 192, 118 }, new int[] { 365, 221, 152, 93 }, new int[] { 259, 157, 108, 66 }, new int[] { 202, 122, 84, 52 } },
            new int[][] { new int[] { 552, 335, 230, 141 }, new int[] { 432, 262, 180, 111 }, new int[] { 312, 189, 130, 80 }, new int[] { 235, 143, 98, 60 } },
            new int[][] { new int[] { 652, 395, 271, 167 }, new int[] { 513, 311, 213, 131 }, new int[] { 364, 221, 151, 93 }, new int[] { 288, 174, 119, 74 } },
        };

        // 纠错码字数表 - [版本-1][纠错级别]
        private static readonly int[][] EC_CODEWORDS_TABLE = new int[][]
        {
            new int[] { 7, 10, 13, 17 },
            new int[] { 10, 16, 22, 28 },
            new int[] { 15, 26, 36, 44 },
            new int[] { 20, 36, 52, 64 },
            new int[] { 26, 48, 72, 88 },
            new int[] { 36, 64, 96, 112 },
            new int[] { 40, 72, 108, 130 },
            new int[] { 48, 88, 132, 156 },
            new int[] { 60, 110, 160, 192 },
            new int[] { 72, 130, 192, 224 },
        };

        // 数据码字数表 - [版本-1][纠错级别]
        private static readonly int[][] DATA_CODEWORDS_TABLE = new int[][]
        {
            new int[] { 19, 16, 13, 9 },
            new int[] { 34, 28, 22, 16 },
            new int[] { 55, 44, 34, 26 },
            new int[] { 80, 64, 48, 36 },
            new int[] { 108, 86, 62, 46 },
            new int[] { 136, 108, 76, 60 },
            new int[] { 156, 124, 88, 66 },
            new int[] { 194, 154, 110, 86 },
            new int[] { 232, 182, 132, 100 },
            new int[] { 274, 216, 154, 122 },
        };

        /// <summary>
        /// 生成二维码并返回 SVG 字符串
        /// </summary>
        public static string GenerateSvg(string text, int size = 200, ErrorCorrectionLevel level = ErrorCorrectionLevel.H)
        {
            var matrix = GenerateMatrix(text, level);
            if (matrix == null) return string.Empty;

            return MatrixToSvg(matrix, size);
        }

        /// <summary>
        /// 生成二维码并返回 Base64 编码的 SVG 数据 URI
        /// </summary>
        public static string GenerateSvgDataUri(string text, int size = 200, ErrorCorrectionLevel level = ErrorCorrectionLevel.H)
        {
            var svg = GenerateSvg(text, size, level);
            if (string.IsNullOrEmpty(svg)) return string.Empty;

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
            return $"data:image/svg+xml;base64,{base64}";
        }

        /// <summary>
        /// 生成二维码矩阵
        /// </summary>
        private static bool[,]? GenerateMatrix(string text, ErrorCorrectionLevel level)
        {
            if (string.IsNullOrEmpty(text)) return null;

            // 检测编码模式
            int mode = DetectMode(text);
            
            // 选择版本
            int version = SelectVersion(text.Length, mode, level);
            if (version < 1) return null;

            int size = 17 + version * 4;
            var matrix = new bool[size, size];

            // 添加功能图案
            AddFinderPatterns(matrix, size);
            AddSeparators(matrix, size);
            AddTimingPatterns(matrix, size);
            AddDarkModule(matrix, version);
            AddAlignmentPatterns(matrix, version);

            // 创建格式信息
            var formatInfo = CreateFormatInfo(level, 0);
            AddFormatInfo(matrix, size, formatInfo);

            // 编码数据并填充
            var dataBits = EncodeData(text, mode, version, level);
            FillData(matrix, dataBits, size);

            // 应用掩码模式 0
            ApplyMask(matrix, size, 0);

            return matrix;
        }

        /// <summary>
        /// 检测编码模式
        /// </summary>
        private static int DetectMode(string text)
        {
            bool isNumeric = text.All(c => char.IsDigit(c));
            if (isNumeric) return MODE_NUMERIC;

            bool isAlphanumeric = text.All(c => IsAlphanumeric(c));
            if (isAlphanumeric) return MODE_ALPHANUMERIC;

            return MODE_BYTE;
        }

        /// <summary>
        /// 检查字符是否是字母数字模式支持的字符
        /// </summary>
        private static bool IsAlphanumeric(char c)
        {
            return char.IsDigit(c) || (c >= 'A' && c <= 'Z') || " $%*+-./:".Contains(c);
        }

        /// <summary>
        /// 选择二维码版本
        /// </summary>
        private static int SelectVersion(int length, int mode, ErrorCorrectionLevel level)
        {
            int modeIndex = mode == MODE_NUMERIC ? 0 : (mode == MODE_ALPHANUMERIC ? 1 : 2);
            int levelIndex = (int)level;

            for (int version = 1; version <= 10; version++)
            {
                if (version <= CAPACITY_TABLE.Length)
                {
                    int capacity = CAPACITY_TABLE[version - 1][modeIndex][levelIndex];
                    if (capacity >= length)
                    {
                        return version;
                    }
                }
            }

            return -1; // 文本太长
        }

        /// <summary>
        /// 编码数据
        /// </summary>
        private static List<bool> EncodeData(string text, int mode, int version, ErrorCorrectionLevel level)
        {
            var bits = new List<bool>();
            
            // 模式指示符 (4 bits)
            AppendBits(bits, mode, 4);
            
            // 字符计数指示符
            int charCountBits = GetCharCountBits(version, mode);
            AppendBits(bits, text.Length, charCountBits);
            
            // 数据编码
            switch (mode)
            {
                case MODE_NUMERIC:
                    EncodeNumeric(bits, text);
                    break;
                case MODE_ALPHANUMERIC:
                    EncodeAlphanumeric(bits, text);
                    break;
                case MODE_BYTE:
                    EncodeByte(bits, text);
                    break;
            }
            
            // 终止符
            int dataCapacity = DATA_CODEWORDS_TABLE[version - 1][(int)level] * 8;
            int remainingBits = dataCapacity - bits.Count;
            if (remainingBits > 0)
            {
                int terminatorBits = Math.Min(remainingBits, 4);
                AppendBits(bits, 0, terminatorBits);
            }
            
            // 填充到字节边界
            while (bits.Count % 8 != 0)
            {
                bits.Add(false);
            }
            
            // 填充字节
            byte[] paddingBytes = { 0xEC, 0x11 };
            int paddingIndex = 0;
            while (bits.Count < dataCapacity)
            {
                AppendBits(bits, paddingBytes[paddingIndex], 8);
                paddingIndex = 1 - paddingIndex;
            }
            
            return bits;
        }

        /// <summary>
        /// 获取字符计数位数
        /// </summary>
        private static int GetCharCountBits(int version, int mode)
        {
            if (version <= 9)
            {
                return mode switch
                {
                    MODE_NUMERIC => 10,
                    MODE_ALPHANUMERIC => 9,
                    MODE_BYTE => 8,
                    _ => 8
                };
            }
            return mode switch
            {
                MODE_NUMERIC => 12,
                MODE_ALPHANUMERIC => 11,
                MODE_BYTE => 16,
                _ => 16
            };
        }

        /// <summary>
        /// 添加位到列表
        /// </summary>
        private static void AppendBits(List<bool> bits, int value, int length)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                bits.Add(((value >> i) & 1) == 1);
            }
        }

        /// <summary>
        /// 数字编码
        /// </summary>
        private static void EncodeNumeric(List<bool> bits, string text)
        {
            for (int i = 0; i < text.Length; i += 3)
            {
                int length = Math.Min(3, text.Length - i);
                int value = int.Parse(text.Substring(i, length));
                AppendBits(bits, value, length == 3 ? 10 : (length == 2 ? 7 : 4));
            }
        }

        /// <summary>
        /// 字母数字编码
        /// </summary>
        private static void EncodeAlphanumeric(List<bool> bits, string text)
        {
            string alphanumericChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";
            
            for (int i = 0; i < text.Length; i += 2)
            {
                if (i + 1 < text.Length)
                {
                    int value1 = alphanumericChars.IndexOf(text[i]);
                    int value2 = alphanumericChars.IndexOf(text[i + 1]);
                    AppendBits(bits, value1 * 45 + value2, 11);
                }
                else
                {
                    int value = alphanumericChars.IndexOf(text[i]);
                    AppendBits(bits, value, 6);
                }
            }
        }

        /// <summary>
        /// 字节编码
        /// </summary>
        private static void EncodeByte(List<bool> bits, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            foreach (byte b in bytes)
            {
                AppendBits(bits, b, 8);
            }
        }

        /// <summary>
        /// 添加定位图案
        /// </summary>
        private static void AddFinderPatterns(bool[,] matrix, int size)
        {
            AddFinderPattern(matrix, 0, 0);
            AddFinderPattern(matrix, 0, size - 7);
            AddFinderPattern(matrix, size - 7, 0);
        }

        /// <summary>
        /// 添加单个定位图案
        /// </summary>
        private static void AddFinderPattern(bool[,] matrix, int row, int col)
        {
            for (int r = 0; r < 7; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    bool isBlack = (r == 0 || r == 6 || c == 0 || c == 6) || 
                                   (r >= 2 && r <= 4 && c >= 2 && c <= 4);
                    matrix[row + r, col + c] = isBlack;
                }
            }
        }

        /// <summary>
        /// 添加分隔符
        /// </summary>
        private static void AddSeparators(bool[,] matrix, int size)
        {
            int[] positions = new int[] { 7, size - 8 };
            
            foreach (int pos in positions)
            {
                for (int i = 0; i <= 7 && i < size; i++)
                {
                    if (pos < size) matrix[pos, i] = false;
                    if (pos < size && size - 1 - i >= 0) matrix[pos, size - 1 - i] = false;
                    if (i < size && pos < size) matrix[i, pos] = false;
                    if (size - 1 - i >= 0 && pos < size) matrix[size - 1 - i, pos] = false;
                }
            }
        }

        /// <summary>
        /// 添加时序图案
        /// </summary>
        private static void AddTimingPatterns(bool[,] matrix, int size)
        {
            for (int i = 8; i < size - 8; i++)
            {
                bool isBlack = i % 2 == 0;
                matrix[6, i] = isBlack;
                matrix[i, 6] = isBlack;
            }
        }

        /// <summary>
        /// 添加暗模块
        /// </summary>
        private static void AddDarkModule(bool[,] matrix, int version)
        {
            int size = matrix.GetLength(0);
            matrix[4 * version + 9, 8] = true;
        }

        /// <summary>
        /// 添加校正图案
        /// </summary>
        private static void AddAlignmentPatterns(bool[,] matrix, int version)
        {
            if (version < 2) return;

            var positions = GetAlignmentPatternPositions(version);
            
            foreach (var row in positions)
            {
                foreach (var col in positions)
                {
                    if ((row < 9 && col < 9) || (row < 9 && col > matrix.GetLength(1) - 10) || 
                        (row > matrix.GetLength(0) - 10 && col < 9))
                    {
                        continue;
                    }

                    AddAlignmentPattern(matrix, row, col);
                }
            }
        }

        /// <summary>
        /// 获取校正图案位置
        /// </summary>
        private static int[] GetAlignmentPatternPositions(int version)
        {
            if (version == 1) return new int[0];
            
            int[] positions = version switch
            {
                2 => new int[] { 6, 18 },
                3 => new int[] { 6, 22 },
                4 => new int[] { 6, 26 },
                5 => new int[] { 6, 30 },
                6 => new int[] { 6, 34 },
                7 => new int[] { 6, 22, 38 },
                8 => new int[] { 6, 24, 42 },
                9 => new int[] { 6, 26, 46 },
                10 => new int[] { 6, 28, 50 },
                _ => new int[] { 6 }
            };
            
            return positions;
        }

        /// <summary>
        /// 添加单个校正图案
        /// </summary>
        private static void AddAlignmentPattern(bool[,] matrix, int row, int col)
        {
            for (int r = -2; r <= 2; r++)
            {
                for (int c = -2; c <= 2; c++)
                {
                    int absR = Math.Abs(r);
                    int absC = Math.Abs(c);
                    bool isBlack = (absR == 2 || absC == 2) || (absR <= 1 && absC <= 1);
                    matrix[row + r, col + c] = isBlack;
                }
            }
        }

        /// <summary>
        /// 创建格式信息
        /// </summary>
        private static int CreateFormatInfo(ErrorCorrectionLevel level, int maskPattern)
        {
            int levelBits = (int)level << 3;
            int formatInfo = levelBits | maskPattern;
            
            // BCH(15,5) 纠错码
            int generator = 0x537;
            int formatInfoCopy = formatInfo << 10;
            
            while (GetBitLength(formatInfoCopy) >= GetBitLength(generator))
            {
                formatInfoCopy ^= generator << (GetBitLength(formatInfoCopy) - GetBitLength(generator));
            }
            
            formatInfo = (formatInfo << 10) | formatInfoCopy;
            formatInfo ^= 0x5412;
            
            return formatInfo;
        }

        /// <summary>
        /// 获取整数位数
        /// </summary>
        private static int GetBitLength(int value)
        {
            if (value == 0) return 1;
            int length = 0;
            while (value > 0)
            {
                length++;
                value >>= 1;
            }
            return length;
        }

        /// <summary>
        /// 添加格式信息到矩阵
        /// </summary>
        private static void AddFormatInfo(bool[,] matrix, int size, int formatInfo)
        {
            // 左上区域
            for (int i = 0; i < 6; i++)
            {
                matrix[8, i] = ((formatInfo >> i) & 1) == 1;
            }
            matrix[8, 7] = ((formatInfo >> 6) & 1) == 1;
            matrix[8, 8] = ((formatInfo >> 7) & 1) == 1;
            matrix[7, 8] = ((formatInfo >> 8) & 1) == 1;
            for (int i = 9; i < 15; i++)
            {
                matrix[14 - i, 8] = ((formatInfo >> i) & 1) == 1;
            }

            // 左下和右上区域
            for (int i = 0; i < 8; i++)
            {
                matrix[size - 1 - i, 8] = ((formatInfo >> i) & 1) == 1;
            }
            for (int i = 8; i < 15; i++)
            {
                matrix[8, size - 15 + i] = ((formatInfo >> i) & 1) == 1;
            }
        }

        /// <summary>
        /// 填充数据到矩阵
        /// </summary>
        private static void FillData(bool[,] matrix, List<bool> dataBits, int size)
        {
            int bitIndex = 0;
            bool upward = true;
            
            for (int col = size - 1; col > 0; col -= 2)
            {
                if (col == 6) col--; // 跳过时序图案列
                
                while (true)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        int currentCol = col - c;
                        if (currentCol < 0) continue;
                        
                        int row = upward ? size - 1 - ((bitIndex / 2) % size) : (bitIndex / 2) % size;
                        
                        if (!IsFunctionPattern(matrix, row, currentCol, size) && bitIndex < dataBits.Count)
                        {
                            matrix[row, currentCol] = dataBits[bitIndex];
                            bitIndex++;
                        }
                    }
                    
                    if ((bitIndex / 2) % size == 0)
                    {
                        upward = !upward;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 应用掩码
        /// </summary>
        private static void ApplyMask(bool[,] matrix, int size, int maskPattern)
        {
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (IsFunctionPattern(matrix, row, col, size)) continue;
                    
                    bool shouldMask = maskPattern switch
                    {
                        0 => (row + col) % 2 == 0,
                        1 => row % 2 == 0,
                        2 => col % 3 == 0,
                        3 => (row + col) % 3 == 0,
                        4 => ((row / 2) + (col / 3)) % 2 == 0,
                        5 => ((row * col) % 2) + ((row * col) % 3) == 0,
                        6 => (((row * col) % 2) + ((row * col) % 3)) % 2 == 0,
                        7 => (((row + col) % 2) + ((row * col) % 3)) % 2 == 0,
                        _ => false
                    };
                    
                    if (shouldMask)
                    {
                        matrix[row, col] = !matrix[row, col];
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否是功能图案位置
        /// </summary>
        private static bool IsFunctionPattern(bool[,] matrix, int row, int col, int size)
        {
            // 定位图案
            if ((row < 9 && col < 9) || (row < 9 && col > size - 10) || (row > size - 10 && col < 9))
                return true;

            // 时序图案
            if (row == 6 || col == 6)
                return true;

            // 暗模块
            if (row == 4 * ((size - 17) / 4) + 9 && col == 8)
                return true;

            return false;
        }

        /// <summary>
        /// 将矩阵转换为 SVG
        /// </summary>
        private static string MatrixToSvg(bool[,] matrix, int size)
        {
            int moduleCount = matrix.GetLength(0);
            int moduleSize = size / moduleCount;
            if (moduleSize < 1) moduleSize = 1;
            
            int actualSize = moduleCount * moduleSize;
            int quietZone = moduleSize * 4;
            int totalSize = actualSize + quietZone * 2;

            var sb = new StringBuilder();
            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {totalSize} {totalSize}\">");
            sb.AppendLine($"  <rect width=\"{totalSize}\" height=\"{totalSize}\" fill=\"white\"/>");

            for (int row = 0; row < moduleCount; row++)
            {
                for (int col = 0; col < moduleCount; col++)
                {
                    if (matrix[row, col])
                    {
                        int x = quietZone + col * moduleSize;
                        int y = quietZone + row * moduleSize;
                        sb.AppendLine($"  <rect x=\"{x}\" y=\"{y}\" width=\"{moduleSize}\" height=\"{moduleSize}\" fill=\"black\"/>");
                    }
                }
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }
    }
}