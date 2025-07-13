﻿using Ardalis.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceToolkit.Abstractions.Specifications
{
    public class PaginatedResult<TResult>
    {
        public PaginatedResult(List<TResult> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
        public List<TResult> Items { get; }
        public int TotalCount { get; }
    }
}
