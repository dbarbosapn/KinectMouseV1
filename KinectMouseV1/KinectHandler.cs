using KinectMouseV1.Utilities;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace KinectMouseV1
{
    internal class KinectHandler : IDisposable
    {
        private readonly KinectSensor _kinectSensor;
        private readonly InteractionStream _interactionStream;

        private Action<SkeletonPoint> _onHandPositionChange;
        private Action<bool> _onHandClosedChange;

        private int _dominantSkeletonId = -1;
        private InteractionHandType _dominantHandType = InteractionHandType.None;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public KinectHandler(MainApplicationContext context)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            var sensor = KinectSensor.KinectSensors.FirstOrDefault();
            if (sensor == null)
            {
                MessageBox.Show("Unable to find Kinect Sensor. Connect it before running this application.");
                context.Icon.Visible = false;
                Application.Exit();
                Environment.Exit(0);
                return;
            }

            _kinectSensor = sensor;
            _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            _kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            _kinectSensor.SkeletonStream.Enable();

            _kinectSensor.SkeletonFrameReady += OnSkeletonFrameReady;
            _kinectSensor.DepthFrameReady += OnDepthFrameReady;

            _interactionStream = new(_kinectSensor, new HandGripInteractionClient());
            _interactionStream.InteractionFrameReady += OnInteractionFrameReady;

            _onHandPositionChange = (_) => { };
        }

        public void SetOnHandPositionChange(Action<SkeletonPoint> action)
        {
            _onHandPositionChange = action;
        }

        public void SetOnHandClosedChange(Action<bool> action)
        {
            _onHandClosedChange = action;
        }

        public void Start()
        {
            _kinectSensor.Start();
        }

        public CoordinateMapper GetCoordinateMapper()
        {
            return _kinectSensor.CoordinateMapper;
        }

        public void Dispose()
        {
            _kinectSensor.Stop();
        }

        private void OnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using var frame = e.OpenDepthImageFrame();
            if (frame != null)
            {
                _interactionStream.ProcessDepth(frame.GetRawPixelData(), frame.Timestamp);
            }
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using var frame = e.OpenSkeletonFrame();
            if (frame == null) return;

            var skeletons = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(skeletons);
            _interactionStream.ProcessSkeleton(skeletons, _kinectSensor.AccelerometerGetCurrentReading(), frame.Timestamp);

            foreach (var skeleton in skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    Joint? hand = null;

                    var rightHand = skeleton.Joints[JointType.HandRight];
                    var leftHand = skeleton.Joints[JointType.HandLeft];

                    if (rightHand != null && rightHand.TrackingState == JointTrackingState.Tracked)
                    {
                        hand = rightHand;
                        _dominantHandType = InteractionHandType.Right;
                    }
                    else if (leftHand != null && leftHand.TrackingState == JointTrackingState.Tracked)
                    {
                        hand = leftHand;
                        _dominantHandType = InteractionHandType.Left;
                    }
                    else if (rightHand != null && rightHand.TrackingState == JointTrackingState.Inferred)
                    {
                        hand = rightHand;
                        _dominantHandType = InteractionHandType.Right;
                    }
                    else if (leftHand != null && leftHand.TrackingState == JointTrackingState.Inferred)
                    {
                        hand = leftHand;
                        _dominantHandType = InteractionHandType.Left;
                    }

                    if (hand != null)
                    {
                        _dominantSkeletonId = skeleton.TrackingId;
                        _onHandPositionChange.Invoke(hand.Value.Position);
                    }
                }
            }
        }

        private void OnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using var frame = e.OpenInteractionFrame();
            if (frame == null) return;

            var users = new UserInfo[InteractionFrame.UserInfoArrayLength];
            frame.CopyInteractionDataTo(users);

            foreach (var user in users)
            {
                if (user.SkeletonTrackingId != _dominantSkeletonId) continue;
                foreach (var hand in user.HandPointers)
                {
                    if (!hand.IsTracked) continue;
                    if (hand.HandType != _dominantHandType) continue;

                    if (hand.HandEventType == InteractionHandEventType.Grip)
                    {
                        _onHandClosedChange?.Invoke(true);
                    }
                    else if (hand.HandEventType == InteractionHandEventType.GripRelease)
                    {
                        _onHandClosedChange?.Invoke(false);
                    }

                    return;
                }
            }
        }
    }
}
