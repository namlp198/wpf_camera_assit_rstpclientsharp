using System;
using Wpf_DualCam_SpDriving.RawFramesDecoding.DecodedFrames;

namespace Wpf_DualCam_SpDriving.GUI
{
    public interface IVideoSource
    {
        event EventHandler<IDecodedVideoFrame> FrameReceived;
    }
}