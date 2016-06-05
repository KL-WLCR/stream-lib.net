# stream-lib.net

C# (.net) implementation HyperLogLog ( HyperLogLogPlus ) and LinearCounting problemastic cardinality estimation algorithms. 
Based on stream-lib https://github.com/addthis/stream-lib.
Added some fixes to make HyperLogLog more GC-friendly :
 - Internal buffers allocated in SOH. 
 - Added posibility to use array-pool in long-term calculations. 


