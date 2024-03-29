﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRepository<T>
        where T : class
    {
        IEnumerable<T> Find(Func<T, Boolean> predicate);
        void Create(T item);
        IEnumerable<T> ReadAll();
        T Read(string id);
        void Update(T item);
        void Delete(string id);
    }
}