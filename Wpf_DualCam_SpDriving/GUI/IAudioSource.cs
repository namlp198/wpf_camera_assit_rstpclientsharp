using System;
using Wpf_DualCam_SpDriving.RawFramesDecoding.DecodedFrames;

namespace Wpf_DualCam_SpDriving.GUI
{
    interface IAudioSource
    {
        event EventHandler<IDecodedAudioFrame> FrameReceived;
    }
}
