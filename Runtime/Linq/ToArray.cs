﻿using UnityEssentials.Threading.Tasks.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityEssentials.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static Task<TSource[]> ToArrayAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return UnityEssentials.Threading.Tasks.Linq.ToArray.ToArrayAsync(source, cancellationToken);
        }
    }

    internal static class ToArray
    {
        internal static async Task<TSource[]> ToArrayAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            var pool = ArrayPool<TSource>.Shared;
            var array = pool.Rent(16);

            TSource[] result = default;
            IUniTaskAsyncEnumerator<TSource> e = default;
            try
            {
                e = source.GetAsyncEnumerator(cancellationToken);
                var i = 0;
                while (await e.MoveNextAsync())
                {
                    ArrayPoolUtil.EnsureCapacity(ref array, i, pool);
                    array[i++] = e.Current;
                }

                if (i == 0)
                {
                    result = Array.Empty<TSource>();
                }
                else
                {
                    result = new TSource[i];
                    Array.Copy(array, result, i);
                }
            }
            finally
            {
                pool.Return(array, clearArray: !RuntimeHelpersAbstraction.IsWellKnownNoReferenceContainsType<TSource>());

                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return result;
        }
    }
}