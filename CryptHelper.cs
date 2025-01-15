using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GeneralUtility
{
    /// <summary>
    /// 暗号復号化クラス
    /// </summary>
    public class CryptHelper
    {
        /// <summary>CryptKey値</summary>
        public string Key { private get; set; }
        /// <summary>CryptIV値</summary>
        public string IV { private get; set; }

        /// <summary>デフォルトCryptKey値</summary>
        public const string DefaultCryptKey = "9Fix4L4HB4PKeKWY";
        /// <summary>デフォルトCryptIV値</summary>
        public const string DefaultCryptIV = "pf69DL6GrWFyZcMK";

        /// <summary>
        /// 暗号化されたBase64形式の入力文字列をAES復号して平文の文字列を返すメソッド
        /// </summary>
        /// <param name="base64Text">暗号化された文字列</param>
        /// <returns>平文の文字列</returns>
        public string Decrypt(string base64Text)
        {
            if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(IV) || string.IsNullOrWhiteSpace(base64Text))
                return base64Text;

            return DecryptFromBase64(base64Text, Key, IV);
        }

        /// <summary>
        /// 入力文字列をAES暗号化してBase64形式で返すメソッド
        /// </summary>
        /// <param name="plainText">平文文字列</param>
        /// <returns></returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(IV))
                return plainText;

            return EncryptToBase64(plainText, Key, IV);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cryptKey">CryptKey値</param>
        /// <param name="cryptIV">CryptIV値</param>
        public CryptHelper(string cryptKey = null, string cryptIV = null)
        {
            Key = cryptKey ?? DefaultCryptKey;
            IV = cryptIV ?? DefaultCryptIV;
        }

        /// <summary>
        /// クラス内容を表すサマリ文字列を取得する
        /// </summary>
        public override string ToString() => $"Key=\"{Key}\", IV=\"{IV}\"";



        /// <summary>
        /// 入力文字列をAES暗号化してBase64形式で返す静的メソッド
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        /// <remarks>https://www.atmarkit.co.jp/ait/articles/1709/06/news020.html</remarks>
        public static string EncryptToBase64(string plainText, byte[] key, byte[] iv)
        {
            // 入力文字列をバイト型配列に変換
            byte[] src = Encoding.Unicode.GetBytes(plainText);
            // 出力例：平文のバイト型配列の長さ=60

            // Encryptor（暗号化器）を用意する
            using var am = new AesManaged();
            using var encryptor = am.CreateEncryptor(key, iv);
            // ファイルを入力とするなら、ここでファイルを開く
            // using (FileStream inStream = new FileStream(FilePath, ……省略……
            // 出力ストリームを用意する
            using var outStream = new MemoryStream();
            // 暗号化して書き出す
            using (var cs = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(src, 0, src.Length);
                // 入力がファイルなら、inStreamから一定量ずつバイトバッファーに読み込んで
                // cse.Writeで書き込む処理を繰り返す（復号のサンプルコードを参照）
            }
            // 出力がファイルなら、以上で完了

            // Base64文字列に変換して返す
            byte[] result = outStream.ToArray();
            // 出力例：暗号のバイト型配列の長さ=64
            // 出力サイズはBlockSize（既定値16バイト）の倍数になる
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 入力文字列をAES暗号化してBase64形式で返す静的メソッド
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string EncryptToBase64(string plainText, string key, string iv)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteIv = Encoding.UTF8.GetBytes(iv);
            return EncryptToBase64(plainText, byteKey, byteIv);
        }


        /// <summary>
        /// 暗号化されたBase64形式の入力文字列をAES復号して平文の文字列を返す静的メソッド
        /// </summary>
        /// <param name="base64Text"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        /// <remarks>https://www.atmarkit.co.jp/ait/articles/1709/06/news020.html</remarks>
        public static string DecryptFromBase64(string base64Text, byte[] key, byte[] iv)
        {
            // Base64文字列をバイト型配列に変換
            byte[] src = Convert.FromBase64String(base64Text);

            // Decryptor（復号器）を用意する
            using var am = new AesManaged();
            using var decryptor = am.CreateDecryptor(key, iv);
            // 入力ストリームを開く
            using var inStream = new MemoryStream(src, false);
            // 出力ストリームを用意する
            using var outStream = new MemoryStream();
            // 復号して一定量ずつ読み出し、それを出力ストリームに書き出す
            using (var cs = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read))
            {
                byte[] buffer = new byte[4096]; // バッファーサイズはBlockSizeの倍数にする
                int len = 0;
                while ((len = cs.Read(buffer, 0, 4096)) > 0)
                    outStream.Write(buffer, 0, len);
            }
            // 出力がファイルなら、以上で完了

            // 文字列に変換して返す
            byte[] result = outStream.ToArray();
            return Encoding.Unicode.GetString(result);
        }


        /// <summary>
        /// 暗号化されたBase64形式の入力文字列をAES復号して平文の文字列を返す静的メソッド
        /// </summary>
        /// <param name="base64Text"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string DecryptFromBase64(string base64Text, string key, string iv)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteIv = Encoding.UTF8.GetBytes(iv);
            return DecryptFromBase64(base64Text, byteKey, byteIv);
        }
    }
}
