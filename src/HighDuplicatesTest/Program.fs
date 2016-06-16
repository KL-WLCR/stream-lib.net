open System
open System.Diagnostics

module Hash =
    open Murmur

    let hash64 (data: byte[]) : UInt64 =
        let hash128 = MurmurHash.Create128(managed = false).ComputeHash data
        BitConverter.ToUInt64(hash128, 0)

module Source =
    let gen count = Array.init count (fun _ -> Hash.hash64 (Guid.NewGuid().ToByteArray()))

module StreamLib =
    open StreamLib.Cardinality

    let run uniqueCount duplicateCount =
      let pool = HyperLogLogPlus.CreateMemPool()
      use hll = new HyperLogLogPlus(16u, 25u, pool)
      //use hll = new HyperLogLogPlus(16u, 25u)
      let uniques = Source.gen uniqueCount

      let sw = Stopwatch.StartNew()

      for x in uniques do
          for _i = 1 to duplicateCount do
            hll.OfferHashed x |> ignore

      let cardinality = hll.Cardinality()
      printfn "\nStreamLib: count = %d, %O elapsed" cardinality sw.Elapsed

[<EntryPoint>]
let main _ =
    let uniqueCount, duplicateCount = 10, 10000000
    printfn "Source generated: Unique count = %d, duplicateCount = %d" uniqueCount duplicateCount
    StreamLib.run uniqueCount duplicateCount


    printfn "DONE."

    Console.ReadKey() |> ignore
    0