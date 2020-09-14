using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace slaveUpperComputer.util
{
    class DataConverter
    {
        private static byte[] _aesKetByte = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        private static string _aesKeyStr = Encoding.UTF8.GetString(_aesKetByte);
        static byte[] ArrayCRCHigh =
        {
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
        };
        static byte[] checkCRCLow =
        {
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
        0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
        0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
        0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
        0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
        0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
        0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
        0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
        0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
        0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
        0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
        0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
        0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
        0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
        0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
        0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
        0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
        0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
        0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
        0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
        0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
        0x43, 0x83, 0x41, 0x81, 0x80, 0x40
        };
        static UInt32[] crcTable ={
                              0x00000000, 0x77073096, 0xee0e612c, 0x990951ba,
    /*  4:*/ 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
    /*  8:*/ 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
    /*  8:*/ 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
    /* 16:*/ 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
    /* 20:*/ 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
    /* 24:*/ 0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec,
    /* 28:*/ 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
    /* 32:*/ 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
    /* 36:*/ 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
    /* 40:*/ 0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940,
    /* 44:*/ 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
    /* 48:*/ 0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116,
    /* 52:*/ 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
    /* 56:*/ 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
    /* 60:*/ 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
    /* 64:*/ 0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a,
    /* 68:*/ 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
    /* 72:*/ 0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818,
    /* 76:*/ 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
    /* 80:*/ 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
    /* 84:*/ 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
    /* 88:*/ 0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c,
    /* 92:*/ 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
    /* 96:*/ 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
    /*100:*/ 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
    /*104:*/ 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
    /*108:*/ 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
    /*112:*/ 0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086,
    /*116:*/ 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
    /*120:*/ 0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4,
    /*124:*/ 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
    /*128:*/ 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
    /*132:*/ 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
    /*136:*/ 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
    /*140:*/ 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
    /*144:*/ 0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe,
    /*148:*/ 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
    /*152:*/ 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
    /*156:*/ 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
    /*160:*/ 0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252,
    /*164:*/ 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
    /*168:*/ 0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60,
    /*172:*/ 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
    /*176:*/ 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
    /*180:*/ 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
    /*184:*/ 0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04,
    /*188:*/ 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
    /*192:*/ 0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a,
    /*196:*/ 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
    /*200:*/ 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
    /*204:*/ 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
    /*208:*/ 0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e,
    /*212:*/ 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
    /*216:*/ 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
    /*220:*/ 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
    /*224:*/ 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
    /*228:*/ 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
    /*232:*/ 0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0,
    /*236:*/ 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
    /*240:*/ 0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6,
    /*244:*/ 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
    /*248:*/ 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
    /*252:*/ 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
                                  };
        //static UInt32[] crcTable =
        //{
        //  0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005,
        //  0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61, 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd,
        //  0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
        //  0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd,
        //  0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039, 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5,
        //  0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
        //  0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95,
        //  0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1, 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d,
        //  0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
        //  0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 0x0808d07d, 0x0cc9cdca,
        //  0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde, 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02,
        //  0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
        //  0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692,
        //  0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6, 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a,
        //  0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
        //  0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a,
        //  0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637, 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb,
        //  0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
        //  0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b,
        //  0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff, 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623,
        //  0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
        //  0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3,
        //  0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7, 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b,
        //  0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
        //  0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c,
        //  0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8, 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24,
        //  0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
        //  0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654,
        //  0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0, 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c,
        //  0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
        //  0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c,
        //  0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668, 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4
        //};

        /// <summary>
        /// CRC16校验
        /// </summary>
        /// <param name="data">校验的字节数组</param>
        /// <param name="length">校验的数组长度</param>
        /// <returns>该字节数组的奇偶校验字节，高位在前低位在后</returns>
        public static byte[] CRC16(byte[] data, int arrayLength)
        {
            byte[] rest = new byte[2];
            byte CRCHigh = 0xFF;
            byte CRCLow = 0xFF;
            byte index;
            int i = 0;
            while (arrayLength-- > 0)
            {
                index = (System.Byte)(CRCHigh ^ data[i++]);
                CRCHigh = (System.Byte)(CRCLow ^ ArrayCRCHigh[index]);
                CRCLow = checkCRCLow[index];
            }
            rest[0] = CRCHigh;
            rest[1] = CRCLow;
            return rest;
            // return (Int16)(CRCHigh << 8 | CRCLow);
        }
        //一下都是CRC32
        //CRC32校验
        public static byte[] GetCRC32(byte[] bytes)
        {
            uint iCount = (uint)bytes.Length;
            uint crc = 0xFFFFFFFF;

            for (uint i = 0; i < iCount; i++)
            {
                crc = (crc << 8) ^ crcTable[(crc >> 24) ^ bytes[i]];
            }
            byte[] b = new byte[4];
            b[0] = (byte)(crc >> 24);
            b[1] = (byte)(crc >> 16 & 0x00FF);
            b[2] = (byte)(crc >> 8 & 0x0000FF);
            b[3] = (byte)(crc & 0x000000FF);
            return b;
        }

        private static ulong[] Crc32Table;
        //生成CRC32码表
        public static void GetCRC32Table()
        {
            ulong Crc;
            Crc32Table = new ulong[256];
            int i, j;
            for (i = 0; i < 256; i++)
            {
                Crc = (ulong)i;
                for (j = 8; j > 0; j--)
                {
                    if ((Crc & 1) == 1)
                        Crc = (Crc >> 1) ^ 0xEDB88320;
                    else
                        Crc >>= 1;
                }
                Crc32Table[i] = Crc;
            }
        }

        //获取字符串的CRC32校验值
        public static byte[] GetCRC32Str(string sInputString)
        {
            //生成码表
            GetCRC32Table();
            byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sInputString);
            ulong value = 0xffffffff;
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                value = (value >> 8) ^ Crc32Table[(value & 0xFF) ^ buffer[i]];
            }
            ulong crc = value ^ 0xffffffff;
            byte[] b = new byte[4];
            b[0] = (byte)(crc >> 24);
            b[1] = (byte)(crc >> 16 & 0x00FF);
            b[2] = (byte)(crc >> 8 & 0x0000FF);
            b[3] = (byte)(crc & 0x000000FF);
            return b;
        }

        //public UInt32 CRC32(UInt32[] InputData, int len)
        public static UInt32 CRC32(byte[] Input, int len)
        {
            UInt32[] InputData = new UInt32[len];
            for (int i = 0; i < len; i++)
            {
                InputData[i] = (UInt32)Input[i];
            }
            UInt32 dwPolynomial = 0x04c11db7;

            UInt32 xbit;
            UInt32 data;
            UInt32 crc_cnt = 0xFFFFFFFF; // init
            for (int i = 0; i < len; i++)
            {
                // xbit = 1 << 31;
                xbit = 0x80000000;
                data = InputData[i];
                for (int bits = 0; bits < 32; bits++)
                {
                    if ((crc_cnt & 0x80000000) > 0)
                    {
                        crc_cnt <<= 1;
                        crc_cnt ^= dwPolynomial;
                    }
                    else
                        crc_cnt <<= 1;
                    if ((data & xbit) > 0)
                        crc_cnt ^= dwPolynomial;
                    xbit >>= 1;
                }
            }
            return crc_cnt;
        }

        //byte[]数组转化为UInt32[]数组
        private static UInt32[] ByteArrayToUInt32Array(byte[] bytes)
        {
            byte[] newbytes = new byte[bytes.Length + (4 - bytes.Length % 4)];
            for (int i = 0; i < bytes.Length; i++)
                newbytes[i] = bytes[i];

            UInt32[] u32 = new UInt32[newbytes.Length / 4];
            for (int i = 0; i < newbytes.Length / 4; i++)
                u32[i] = System.BitConverter.ToUInt32(newbytes, i * 4);
            return u32;
        }

        public static byte[] Crc_CalcateCRC32(byte[] Crc_DataPtr)
        {
            long index;
            long crcTemp;
            long rest;
            int Crc_Length = Crc_DataPtr.Length;

            /* Initialization of temporary crc rest */
            crcTemp = 0xFFFFFFFF;

            for (index = 0; index < Crc_Length; ++index)
            {
                /* Impact of temporary rest on next crc rest */
                crcTemp ^= Crc_DataPtr[index];

                crcTemp = (crcTemp >> 8) ^
                           crcTable[crcTemp & 0xFF];
            }
            //rest = crcTemp ^ ((long)0xFFFFFFFF);
            rest = crcTemp;

            byte[] b = new byte[4];
            b[0] = (byte)(rest >> 24);
            b[1] = (byte)(rest >> 16 & 0x00FF);
            b[2] = (byte)(rest >> 8 & 0x0000FF);
            b[3] = (byte)(rest & 0x000000FF);

            return b;
        }

        //以上都是CRC32

        //深度复制
        public static T CloneObject<T>(T obj)
        {
            T ret = default(T);
            if (obj != null)
            {
                XmlSerializer cloner = new XmlSerializer(typeof(T));
                MemoryStream stream = new MemoryStream();
                cloner.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                ret = (T)cloner.Deserialize(stream);
            }
            return ret;
        }

        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString = "0" + hexString;
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }


        public static int byteArrayToInt(byte[] b)
        {
            int sum = 0;
            for (int i = 0; i < b.Length; i++)
            {
                //sum += Convert.ToInt32((Math.Pow(10, b.Length - 1 - i) * b[i]));
                sum |= b[i] << (8 * (b.Length - 1 - i));
            }

            return sum;
        }

        //字节转字符串
        public static string hex2String(byte b)
        {
            return b.ToString("X2");
        }

        //字节数组转16进制字符串（不含空格）
        public static string byteToHexStrForDataWithoutSpace(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i <= bytes.Length - 1; i++)
                {
                    returnStr += (bytes[i].ToString("X2"));
                }
            }
            return returnStr;
        }

        //字节数组转16进制字符串(含空格，用于显示)
        public static string byteToHexStrForData(byte[] bytes)
        {
                             string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i <= bytes.Length - 1; i++)
                {
                    returnStr += (bytes[i].ToString("X2") + " ");
                }
            }
            return returnStr;
        }

        public static string byteToHexStrForId(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        //16进制字符串转int
        public static int string2Hex(string s)
        {
            return Int32.Parse(s, System.Globalization.NumberStyles.HexNumber);
        }
        //计算传入字节数组的校验和
        public static byte getAllBytesSum(byte[] bArray)
        {
            byte b = 0x00;

            for (int i = 0; i < bArray.Length; i++)
            {
                b += bArray[i];
            }
            return b;
        }

        //计算传入字节数组的校验和
        public static byte getAllBytesSum2(byte[] bArray)
        {
            return (byte)(0x100 - (int)getAllBytesSum(bArray));
        }

        //计算传入字节数组的校验和
        public static byte getAllBytesSum3(byte[] bArray)
        {
            byte b = (byte)(0xFF - (int)getAllBytesSum(bArray));
            return (byte)(0xFF - (int)getAllBytesSum(bArray));
        }

        /**
    * 字节数组转为普通字符串（ASCII对应的字符）
    * 忽略0x00
    * @param bytearray
    *            byte[]
    * @return String
    */
        public static String bytetoAscString(byte[] bytearray)
        {
            String rest = "";
            char temp;

            int length = bytearray.Length;
            for (int i = 0; i < length; i++)
            {
                if (bytearray[i] == 0x5f) {
                    rest += "__";
                }else if (bytearray[i] != 0x00)
                {
                    temp = (char)bytearray[i];
                    rest += temp;
                }
            }
            return rest;
        }

        //string转byte[]
        public static byte[] StringToBytes(String str)
        {
            return System.Text.Encoding.Default.GetBytes(str);
        }

        public static String bytestoString(byte[] bytearray)
        {
            String s = (System.Text.Encoding.Default.GetString(bytearray)).Replace("\0", "");
            return s;
        }

        //字符串转ASCII
        public static byte[] str2ASCII(String xmlStr)
        {
            return Encoding.Default.GetBytes(xmlStr);
        }

        public static bool examine(String line)
        {
            byte[] bLine = strToHexByte(line);
            byte sum = 0x00;
            for (int i = 0; i < bLine.Length - 1; i++)
            {
                sum += bLine[i];
            }

            int rest = 0xFF - (int)sum;
            String stemp = rest.ToString("X2");
            bool b = string2Hex(stemp) == bLine[bLine.Length - 1];
            if (string2Hex(stemp) == bLine[bLine.Length - 1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //16进制字符串转int
        public static int hexStringToInt(string s)
        {
            return Convert.ToInt32(s, 16);
        }

        //判断string能否转为int
        public static Boolean canStirng2int(String s)
        {
            if (s == null || s.Equals(""))
            {
                return false;
            }
            return Regex.IsMatch(s, @"^[+-]?\d*$");
        }

        //判断string能否转为double
        public static Boolean canStirng2double(String s)
        {
            if (s == null || s.Equals(""))
            {
                return false;
            }
            return Regex.IsMatch(s, @"^[+-]?\d*[.]?\d*$");
        }

        //AES加密
        public static byte[] TextEncrypt(string content, string secretKey)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            byte[] key = Encoding.UTF8.GetBytes(secretKey);

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }

            return data;
        }

        //AES解密
        public static string TextDecrypt(byte[] data, string secretKey)
        {
            byte[] key = Encoding.UTF8.GetBytes(secretKey);

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static String AESEncrypt(String Data, String Key, String Vector)
        {
            Byte[] plainBytes = Encoding.UTF8.GetBytes(Data);

            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);
            Byte[] bVector = new Byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);

            Byte[] Cryptograph = null; // 加密后的密文

            Rijndael Aes = Rijndael.Create();
            try
            {
                // 开辟一块内存流
                using (MemoryStream Memory = new MemoryStream())
                {
                    // 把内存流对象包装成加密流对象
                    using (CryptoStream Encryptor = new CryptoStream(Memory,
                    Aes.CreateEncryptor(bKey, bVector),
                    CryptoStreamMode.Write))
                    {
                        // 明文数据写入加密流
                        Encryptor.Write(plainBytes, 0, plainBytes.Length);
                        Encryptor.FlushFinalBlock();

                        Cryptograph = Memory.ToArray();
                    }
                }
            }
            catch
            {
                Cryptograph = null;
            }

            return Convert.ToBase64String(Cryptograph);
        }

        public static String AESDecrypt(String Data, String Key, String Vector)
        {
            Byte[] encryptedBytes = Convert.FromBase64String(Data);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);
            Byte[] bVector = new Byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);

            Byte[] original = null; // 解密后的明文

            Rijndael Aes = Rijndael.Create();
            try
            {
                // 开辟一块内存流，存储密文
                using (MemoryStream Memory = new MemoryStream(encryptedBytes))
                {
                    // 把内存流对象包装成加密流对象
                    using (CryptoStream Decryptor = new CryptoStream(Memory,
                    Aes.CreateDecryptor(bKey, bVector),
                    CryptoStreamMode.Read))
                    {
                        // 明文存储区
                        using (MemoryStream originalMemory = new MemoryStream())
                        {
                            Byte[] Buffer = new Byte[1024];
                            Int32 readBytes = 0;
                            while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
                            {
                                originalMemory.Write(Buffer, 0, readBytes);
                            }

                            original = originalMemory.ToArray();
                        }
                    }
                }
            }
            catch
            {
                original = null;
            }
            return Encoding.UTF8.GetString(original);
        }
        public static string AESEncrypt(String Data, String Key)
        {
            return AESEncrypt(Data, Key, _aesKeyStr);

        }
        public static string AESDecrypt(String Data, String Key)
        {
            return AESDecrypt(Data, Key, _aesKeyStr);
        }

        //int转16进制字符串
        public static string intToHexString(int value)
        {
            String s = String.Format("{0:X}", value);
            return s.Length % 2 == 1 ? "0" + s : s;
        }
        //int转16进制字符串，并格式化为长度不小于len的字符串，不足的在前面补充“0”
        public static string intToHexString2(int value,int len)
        {
            String s = String.Format("{0:X}", value);
            for (; s.Length < len;) {
                s = "0" + s;
            }
            return s;
        }

        //16进制字符串每2个字符分割
        public static string[] hexStringToStrings(string s)
        {
            if (s.Length % 2 != 0)
            {
                return null;
            }
            string[] ss = new string[s.Length / 2];
            for (int i = 0; i < s.Length; i++)
            {
                ss[i / 2] += s[i];
            }

            return ss;
        }
        private static byte[] ltable;
        private static byte[] atable;
        private static long m_secret_key = 0xBAB11B51L;
        private static long m_secret_key2 = 0xBCB31B51L;
        private static void generate_tables()
        {
            ltable = new byte[256];
            atable = new byte[256];
            byte c;
            byte a = 1;
            byte d;
            for (c = 0; c < 255; c++)
            {
                atable[c] = a;
                /* Mtiply by three */
                d = (byte)(a & 0x80);
                a <<= 1;
                if (d == 0x80)
                {
                    a ^= 0x1b;
                }
                a ^= atable[c];
                /* Set the log table value */
                ltable[atable[c]] = c;
            }
            atable[255] = atable[0];
            ltable[0] = 0;
        }
        //uds27服务根据seed计算key，subfunction=2表示2702服务，subfunction=4表示2704服务
        public static byte[] calKeyFromSeedOnUDS(byte[] seed, int subfunction)
        {
            generate_tables();
            int flSeed = seed[0] << 24 | seed[1] << 16 | seed[2] << 8 | seed[3];
            byte[] dataByte = new byte[5];
            byte[] encDataByte = new byte[5];
            long return_data = 0;
            for (int i = 0; i < 4; i++)
            {
                dataByte[i] = (byte)((flSeed >> (8 * (3 - i))) & 0xFF);
                encDataByte[i] = box(dataByte[i]);
                return_data += encDataByte[i] << (8 * (3 - i));
            }
            if (subfunction == 2)
            {
                return_data ^= m_secret_key;
            }
            else if (subfunction == 4)
            {
                return_data ^= m_secret_key2;
            }
            byte[] Key = new byte[4];
            Key[3] = (byte)(return_data & 0xFF);
            Key[2] = (byte)((return_data >> 8) & 0xFF);
            Key[1] = (byte)((return_data >> 16) & 0xFF);
            Key[0] = (byte)((return_data >> 24) & 0xFF);
            return Key;
        }

        private static byte box(byte data_in)
        {
            byte c, s, x;
            s = x = m_inverse(data_in);
            for (c = 0; c < 4; c++)
            {
                // perform the rotation – setting “bit 0” the same as “bit 7”             
                s = (byte)((s << 1) | (s >> 7));
                // now XOR x with s                 
                x ^= s;
            }
            x ^= 0x63;
            return x;
        }

        //调整字符串长度，长了则截取，短了则补充" "(空格)
        public static string stringLengthFormat(string s,int len) {
            if (s.Length >= len)
            {
                return s.Substring(len);
            }
            else {
                for (int i = s.Length; i < len; i++) {
                    s += " ";
                }
                return s;
            }
        }

        private static byte m_inverse(byte b)
        {
            if (b == 0)
            {
                return (byte)0;
            }
            else
            {
                return atable[(255 - ltable[b])];
            }

        }
    }
}
