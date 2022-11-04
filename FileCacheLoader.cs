using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace MemoryCacheTest
{
    /// <summary>
    /// ファイルをキャッシュして読み込むクラス
    /// </summary>
    /// <remarks>
    /// 初回だけファイルを読み込み、データをキャッシュする。
    /// キャッシュ済みならキャッシュしたデータを返す。
    /// (インターネット一時ファイルとかPCのキャッシュとは別。アプリで一旦保持しているだけ。)
    /// </remarks>
    static class FileCacheLoader
    {
        /// <summary>
        /// パスのファイルを文字列で読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        static public string FileLoadString(string path, Encoding encoding)
        {
            var cache = MemoryCache.Default;
            lock (cache)
            {
                var cacheData = cache[path] as string;

                if (null != cacheData)
                {
                    // キャッシュ済みなのでそれを返す
                    return cacheData;
                }

                if (false == File.Exists(path))
                {
                    // ファイルが無い。
                    return null;
                }

                string addString = null;

                using (var sr = new StreamReader(path, encoding))
                {
                    addString = sr.ReadToEnd();
                }

                if (null == addString)
                {
                    // ファイル読み込み失敗
                    throw new FileLoadException();
                }

                // キャッシュに登録。登録済みなら登録されているもの返す。
                return cache.AddOrGetExisting(path, addString, new CacheItemPolicy()) as string;
            }
        }

        /// <summary>
        /// キャッシュしているデータを全て解放
        /// </summary>
        static public void RemoveAllCache()
        {
            var cache = MemoryCache.Default;
            lock (cache)
            {
                foreach (var c in cache)
                {
                    cache.Remove(c.Key);
                }
            }
        }
    }
}
