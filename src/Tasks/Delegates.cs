﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks
{
    public delegate void Action();
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

    public delegate TResult Func<T1,T2,TResult>(T1 arg1,T2 arg2);
    public delegate TResult Func<T1,TResult>(T1 arg1);
    public delegate TResult Func<TResult>();
}
