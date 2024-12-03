﻿using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Timers;
using Wpf_DualCam_SpDriving.OnvifPTZService;
using Wpf_DualCam_SpDriving.OnvifMedia10;

namespace PTZController
{

    public class Controller
    {
        private enum Direction { None, Up, Down, Left, Right };

        MediaClient mediaClient;
        Profile profile;
        Wpf_DualCam_SpDriving.OnvifPTZService.PTZClient ptzClient;
        Wpf_DualCam_SpDriving.OnvifPTZService.PTZSpeed velocity;
        Wpf_DualCam_SpDriving.OnvifPTZService.PTZVector vector;
        Wpf_DualCam_SpDriving.OnvifPTZService.PTZConfigurationOptions options;
        bool relative = false;
        bool initialised = false;
        Timer timer;
        Direction direction;
        float panDistance;
        float tiltDistance;

        public string ErrorMessage { get; private set; }

        public bool Initialised { get { return initialised; } }

        public int PanIncrements { get; set; } = 20;

        public int TiltIncrements { get; set; } = 20;

        public double TimerInterval { get; set; } = 1500;

        public Controller(bool relative = false)
        {
            this.relative = relative;
        }

        public bool Initialise(string cameraAddress, string userName, string password)
        {
            bool result = false;

            try
            {
                var messageElement = new TextMessageEncodingBindingElement()
                {
                    MessageVersion = MessageVersion.CreateVersion(
                      EnvelopeVersion.Soap12, AddressingVersion.None)
                };
                HttpTransportBindingElement httpBinding = new HttpTransportBindingElement()
                {
                    AuthenticationScheme = AuthenticationSchemes.Digest
                };
                CustomBinding bind = new CustomBinding(messageElement, httpBinding);
                mediaClient = new MediaClient(bind,
                  new EndpointAddress($"http://{cameraAddress}/onvif/Media"));
                mediaClient.ClientCredentials.HttpDigest.AllowedImpersonationLevel =
                  System.Security.Principal.TokenImpersonationLevel.Impersonation;
                mediaClient.ClientCredentials.HttpDigest.ClientCredential.UserName = userName;
                mediaClient.ClientCredentials.HttpDigest.ClientCredential.Password = password;
                ptzClient = new PTZClient(bind,
                  new EndpointAddress($"http://{cameraAddress}/onvif/PTZ"));
                ptzClient.ClientCredentials.HttpDigest.AllowedImpersonationLevel =
                  System.Security.Principal.TokenImpersonationLevel.Impersonation;
                ptzClient.ClientCredentials.HttpDigest.ClientCredential.UserName = userName;
                ptzClient.ClientCredentials.HttpDigest.ClientCredential.Password = password;

                var profs = mediaClient.GetProfiles();
                profile = mediaClient.GetProfile(profs[0].token);

                var configs = ptzClient.GetConfigurations();

                options = ptzClient.GetConfigurationOptions(configs[0].token);

                velocity = new Wpf_DualCam_SpDriving.OnvifPTZService.PTZSpeed()
                {
                    PanTilt = new Wpf_DualCam_SpDriving.OnvifPTZService.Vector2D()
                    {
                        x = 0,
                        y = 0,
                        space = options.Spaces.ContinuousPanTiltVelocitySpace[0].URI,
                    },
                    Zoom = new Wpf_DualCam_SpDriving.OnvifPTZService.Vector1D()
                    {
                        x = 0,
                        space = options.Spaces.ContinuousZoomVelocitySpace[0].URI,
                    }
                };
                if (relative)
                {
                    timer = new Timer(TimerInterval);
                    timer.Elapsed += Timer_Elapsed;
                    velocity.PanTilt.space = options.Spaces.RelativePanTiltTranslationSpace[0].URI;
                    panDistance = (options.Spaces.RelativePanTiltTranslationSpace[0].XRange.Max -
                      options.Spaces.RelativePanTiltTranslationSpace[0].XRange.Min) / PanIncrements;
                    tiltDistance = (options.Spaces.RelativePanTiltTranslationSpace[0].YRange.Max -
                      options.Spaces.RelativePanTiltTranslationSpace[0].YRange.Min) / TiltIncrements;
                }

                vector = new PTZVector()
                {
                    PanTilt = new Wpf_DualCam_SpDriving.OnvifPTZService.Vector2D()
                    {
                        x = 0,
                        y = 0,
                        space = options.Spaces.RelativePanTiltTranslationSpace[0].URI
                    }
                };

                ErrorMessage = "";
                result = initialised = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return result;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Move();
        }

        public void PanLeft()
        {
            if (initialised)
            {
                if (relative)
                {
                    direction = Direction.Left;
                    Move();
                }
                else
                {
                    velocity.PanTilt.x = options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Min;
                    velocity.PanTilt.y = 0;
                    ptzClient.ContinuousMoveAsync(profile.token, velocity, "PT10S");
                }
            }
        }

        public void PanRight()
        {
            if (initialised)
            {
                if (relative)
                {
                    direction = Direction.Right;
                    Move();
                }
                else
                {
                    velocity.PanTilt.x = options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Max;
                    velocity.PanTilt.y = 0;
                    ptzClient.ContinuousMoveAsync(profile.token, velocity, "PT10S");
                }
            }
        }

        public void TiltUp()
        {
            if (initialised)
            {
                if (relative)
                {
                    direction = Direction.Up;
                    Move();
                }
                else
                {
                    velocity.PanTilt.x = 0;
                    velocity.PanTilt.y = options.Spaces.ContinuousPanTiltVelocitySpace[0].YRange.Max;
                    ptzClient.ContinuousMoveAsync(profile.token, velocity, "PT10S");
                }
            }
        }

        public void TiltDown()
        {
            if (initialised)
            {
                if (relative)
                {
                    direction = Direction.Down;
                    Move();
                }
                else
                {
                    velocity.PanTilt.x = 0;
                    velocity.PanTilt.y = options.Spaces.ContinuousPanTiltVelocitySpace[0].YRange.Min;
                    ptzClient.ContinuousMoveAsync(profile.token, velocity, "PT10S");
                }
            }
        }

        public void Stop()
        {
            if (initialised)
            {
                if (relative)
                    timer.Enabled = false;
                direction = Direction.None;
                ptzClient.Stop(profile.token, true, true);
            }
        }

        private void Move()
        {
            bool move = true;

            switch (direction)
            {
                case Direction.Up:
                    velocity.PanTilt.x = 0;
                    velocity.PanTilt.y = options.Spaces.ContinuousPanTiltVelocitySpace[0].YRange.Max;
                    vector.PanTilt.x = 0;
                    vector.PanTilt.y = tiltDistance;
                    break;

                case Direction.Down:
                    velocity.PanTilt.x = 0;
                    velocity.PanTilt.y = options.Spaces.ContinuousPanTiltVelocitySpace[0].YRange.Max;
                    vector.PanTilt.x = 0;
                    vector.PanTilt.y = -tiltDistance;
                    break;

                case Direction.Left:
                    velocity.PanTilt.x = options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Max;
                    velocity.PanTilt.y = 0;
                    vector.PanTilt.x = -panDistance;
                    vector.PanTilt.y = 0;
                    break;

                case Direction.Right:
                    velocity.PanTilt.x = options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Max;
                    velocity.PanTilt.y = 0;
                    vector.PanTilt.x = panDistance;
                    vector.PanTilt.y = 0;
                    break;

                case Direction.None:
                default:
                    move = false;
                    break;
            }
            if (move)
            {
                ptzClient.RelativeMove(profile.token, vector, velocity);
            }
            timer.Enabled = true;
        }
    }
}
