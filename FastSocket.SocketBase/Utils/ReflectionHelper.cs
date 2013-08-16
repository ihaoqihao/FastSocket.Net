using System;
using System.Linq;
using System.Reflection;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// 反射帮助类。
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 获取实现了指定类口类型的基类实例。
        /// </summary>
        /// <typeparam name="T">接口类型</typeparam>
        /// <param name="assembly">指定的程序集</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">assembly is null</exception>
        static public T[] GetImplementObjects<T>(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            return assembly.GetExportedTypes().Where(c =>
            {
                if (c.IsClass && !c.IsAbstract)
                {
                    var interfaces = c.GetInterfaces();
                    if (interfaces != null) return interfaces.Contains(typeof(T));
                }
                return false;
            }).Select(c => (T)c.GetConstructor(new Type[0]).Invoke(new object[0])).ToArray();
        }
    }
}