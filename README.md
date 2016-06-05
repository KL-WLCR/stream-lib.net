# stream-lib.net

C# (.net) implementation HyperLogLog ( HyperLogLogPlus ) and LinearCounting algorithms. 
Based on stream-lib https://github.com/addthis/stream-lib.
Added some fixes to make library more GC-friendly. ( Add posibility to preallocate memory, avoid allocations on LOH ) 
