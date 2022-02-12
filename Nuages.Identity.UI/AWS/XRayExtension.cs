using System.Runtime.CompilerServices;
using Amazon.XRay.Recorder.Core;

namespace Nuages.Identity.UI.AWS;

public static class XRayExtension
{
    public static void BeginSubsegment(this AWSXRayRecorder recorder, [CallerMemberName] string name = "Method", 
                                        [CallerFilePath]string callerFilePath = "", DateTime? timestamp = null)
    {
        var callerTypeName = Path.GetFileNameWithoutExtension(callerFilePath);
        
        recorder.BeginSubsegment(callerTypeName + "/" + name, timestamp);
    }
}