using Microsoft.Kinect.Toolkit.Interaction;

namespace KinectMouseV1.Utilities
{
    internal class HandGripInteractionClient : IInteractionClient
    {
        public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingId, InteractionHandType handType, double x, double y)
        {
            return new InteractionInfo()
            {
                IsGripTarget = true
            };
        }
    }
}
