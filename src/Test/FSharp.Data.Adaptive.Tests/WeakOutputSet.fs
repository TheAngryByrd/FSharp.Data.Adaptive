﻿module WeakOutputSet

open System
open FsUnit
open NUnit.Framework
open FsCheck
open FsCheck.NUnit
open FSharp.Data.Adaptive


/// A dummy type failing on GetHashCode/Equals to ensure that
/// WeakRef/WeakSet only operate on reference-hashes/equality
type NonEqualObject() =
    inherit AdaptiveObject()
    override x.GetHashCode() = failwith "BrokenEquality.GetHashCode should not be called"
    override x.Equals _o = failwith "BrokenEquality.Equals should not be called"

let relevantSizes = [0;1;2;4;8;9;20]

[<Test>]
let ``[WeakOutputSet] add``() =
    relevantSizes |> List.iter (fun cnt ->
        let set = WeakOutputSet()
    
        let many = Array.init cnt (fun _ -> NonEqualObject())
        for m in many do
            set.Add m |> should be True

        let arr = ref (Array.zeroCreate 8)
        let cnt = set.Consume(arr)
        cnt |> should equal many.Length

        for i in 0 .. cnt - 1 do
            let a = arr.Value.[i]
            many |> Array.exists (fun m -> Object.ReferenceEquals(m, a)) |> should be True
    )

[<Test>]
let ``[WeakOutputSet] remove``() =
    relevantSizes |> List.iter (fun cnt ->
        let set = WeakOutputSet()
    
        let many = Array.init cnt (fun _ -> NonEqualObject())
        for m in many do set.Add m |> should be True
        for m in many do set.Remove m |> should be True
        
        let arr = ref (Array.zeroCreate 8)
        let cnt = set.Consume(arr)
        cnt |> should equal 0
    )


[<Test>]
let ``[WeakOutputSet] actually weak``() =
    relevantSizes |> List.iter (fun cnt ->
        let set = WeakOutputSet()
        let addDead() =
            let many = Array.init cnt (fun _ -> NonEqualObject())
            for m in many do
                set.Add m |> should be True

        addDead()
        ensureGC()
        let arr = ref (Array.zeroCreate 8)
        let cnt = set.Consume(arr)
        cnt |> should equal 0
    )
