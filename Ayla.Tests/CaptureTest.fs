namespace Ayla.Tests

open Ayla
open FlashCap
open NUnit.Framework

[<TestFixture>]
type CaptureTest() =
    
    [<Test>]
    member x.TestDevicesQuery() =
        let devices = Capture.Devices.list()
        Assert.IsNotEmpty(devices)
        
    [<Test>]
    member x.TestFormatsQuery() =
        let devices = Capture.Devices.list()
        let formatsByDevice =
            devices
            |> Seq.map(fun device -> device.Name, Capture.Devices.formats device)
            |> Map.ofSeq
        let formats =
            formatsByDevice
            |> Map.toSeq
            |> Seq.collect (fun (name, formats) ->
                formats
                |> Seq.map (fun format -> format.Description))
            |> Set.ofSeq
        Assert.IsNotEmpty(formats)
        
        
    //YUYV, NV12 (640x360) for Dell, (1920x1080) for Logitech
    [<Test>]
    member x.TestConsumeStream() =
        let devices = Capture.Devices.list()
        let logitech = devices |> Seq.find (fun device -> device.Description = "Logitech BRIO (DirectShow)")
        let formats = Capture.Devices.formats logitech
        let yuyv30fps =
            formats
            |> Seq.filter (fun format -> format.PixelFormat = PixelFormats.YUYV)
            |> Seq.filter (fun format -> format.FramesPerSecond.Numerator = 30 &&
                                         format.FramesPerSecond.Denominator = 1)
            |> Seq.maxBy (fun format -> format.Height)
        let frames =
            Capture.stream(logitech, yuyv30fps)
            |> Seq.take 10
            |> Seq.toList
        ()