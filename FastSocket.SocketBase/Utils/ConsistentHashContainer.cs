using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// 一致性哈希container
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ConsistentHashContainer<T>
    {
        #region Private Members
        private readonly Dictionary<uint, T> _dic = new Dictionary<uint, T>();
        private readonly T[] _arr = null;
        private readonly uint[] _keys = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException">source is null</exception>
        public ConsistentHashContainer(IDictionary<string, T> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            var servers = new List<T>();
            var keys = new List<uint>();

            foreach (var child in source)
            {
                for (int i = 0; i < 250; i++)
                {
                    uint key = BitConverter.ToUInt32(new ModifiedFNV1_32().ComputeHash(Encoding.ASCII.GetBytes(child.Key + "-" + i)), 0);
                    if (!this._dic.ContainsKey(key))
                    {
                        this._dic[key] = child.Value;
                        keys.Add(key);
                    }
                }
                servers.Add(child.Value);
            }

            this._arr = servers.ToArray();
            keys.Sort();
            this._keys = keys.ToArray();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Given an item key hash, 
        /// this method returns the Server which is closest on the server key continuum.
        /// </summary>
        /// <param name="consistentKey"></param>
        /// <returns></returns>
        private T Get(uint consistentKey)
        {
            int i = Array.BinarySearch(this._keys, consistentKey);

            //If not exact match...
            if (i < 0)
            {
                //Get the index of the first item bigger than the one searched for.
                i = ~i;
                //If i is bigger than the last index, it was bigger than the last item = use the first item.
                if (i >= this._keys.Length) i = 0;
            }
            return this._dic[this._keys[i]];
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// get
        /// </summary>
        /// <param name="consistentKey"></param>
        /// <returns></returns>
        public T Get(byte[] consistentKey)
        {
            if (this._arr.Length == 0) return default(T);
            //Quick return if we only have one.
            if (this._arr.Length == 1) return this._arr[0];
            return Get(BitConverter.ToUInt32(new ModifiedFNV1_32().ComputeHash(consistentKey), 0));
        }
        #endregion

        #region FNV1_32
        /// <summary>
        /// Fowler-Noll-Vo hash, variant 1, 32-bit version.
        /// http://www.isthe.com/chongo/tech/comp/fnv/
        /// </summary>
        public class FNV1_32 : HashAlgorithm
        {
            private static readonly uint FNV_prime = 16777619;
            private static readonly uint offset_basis = 2166136261;
            /// <summary>
            /// hash
            /// </summary>
            protected uint hash;

            /// <summary>
            /// new
            /// </summary>
            public FNV1_32()
            {
                HashSizeValue = 32;
            }
            /// <summary>
            /// init
            /// </summary>
            public override void Initialize()
            {
                hash = offset_basis;
            }
            /// <summary>
            /// hashcore
            /// </summary>
            /// <param name="array"></param>
            /// <param name="ibStart"></param>
            /// <param name="cbSize"></param>
            protected override void HashCore(byte[] array, int ibStart, int cbSize)
            {
                int length = ibStart + cbSize;
                for (int i = ibStart; i < length; i++) hash = (hash * FNV_prime) ^ array[i];
            }
            /// <summary>
            /// hash final
            /// </summary>
            /// <returns></returns>
            protected override byte[] HashFinal()
            {
                return BitConverter.GetBytes(hash);
            }
        }
        #endregion

        #region ModifiedFNV1_32
        /// <summary>
        /// Modified Fowler-Noll-Vo hash, 32-bit version.
        /// http://home.comcast.net/~bretm/hash/6.html
        /// </summary>
        public class ModifiedFNV1_32 : FNV1_32
        {
            /// <summary>
            /// hashFinal.
            /// </summary>
            /// <returns></returns>
            protected override byte[] HashFinal()
            {
                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return BitConverter.GetBytes(hash);
            }
        }
        #endregion
    }
}