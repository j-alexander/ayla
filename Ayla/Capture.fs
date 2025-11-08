namespace Ayla

open System
open System.Collections.Concurrent
open System.Threading
open FlashCap

module Capture =
    
    module Devices =
        
        let list() =
            let devices = new CaptureDevices()
            devices.GetDescriptors()
            |> List.ofArray
            
        let formats(descriptor:CaptureDeviceDescriptor) =
            descriptor.Characteristics
            |> List.ofArray
        
    let stream(device:CaptureDeviceDescriptor, format:VideoCharacteristics) =
        seq {
            use queue = new BlockingCollection<_>()
            let enqueue (arrived:PixelBufferScope) =
                let buffer : PixelBuffer = arrived.Buffer
                let event =
                    let index = buffer.FrameIndex
                    let timestamp = buffer.Timestamp
                    let bytes = buffer.CopyImage()
                    index, timestamp, bytes
                arrived.ReleaseNow()
                queue.Add(event)
            let arrived = new PixelBufferArrivedDelegate(enqueue)
            use source = new CancellationTokenSource()
            use subscription = device.OpenAsync(format,arrived,source.Token).Result

            subscription.StartAsync().Wait()
            use unsubscribe =
                { new IDisposable with member x.Dispose() = subscription.StopAsync().Wait() }
            while true do
                yield queue.Take(source.Token)
        }