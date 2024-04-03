﻿using Models.Interfaces;
using Models.Queries;

namespace Exceptions
{
    public class RecordException<T> : Exception
    {
        public T EntityQuery { get; private set; }

        public RecordException(string message, T entityQuery) : base(message)
        {
            EntityQuery = entityQuery;
        }
    }
}
