#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;


#endregion

namespace Nine
{
    /// <summary>
    /// http://www.millermansworld.com/2011/08/27/garbage-detector-from-my-gamefest-slides/
    /// </summary>
    struct GarbageDetector : IDisposable
    {
        public long StartMemory;
        public long FailAfterAllocationAmount;
        public bool CollectAtEnd;

        public GarbageDetector(long failAfterMemory, bool collectAtStart, bool collectAtEnd)
        {
            CollectAtEnd = collectAtEnd;
            StartMemory = GC.GetTotalMemory(collectAtStart);
            FailAfterAllocationAmount = failAfterMemory;
        }

        public GarbageDetector(long failAfterMemory) : this(failAfterMemory, true, false)
        {

        }
        
        public void Dispose()
        {
            long difference = GC.GetTotalMemory(CollectAtEnd) - StartMemory;
            if (difference > FailAfterAllocationAmount)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}