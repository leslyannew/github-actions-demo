namespace github_actions_demo.Setup;

public static class SecurityHeadersConfigurationExtension
{
    public static WebApplication UseSecurityHeadersConfigurations(this WebApplication app)
    {
        //security headers
        HeaderPolicyCollection policyCollection = new HeaderPolicyCollection()
            .AddDefaultSecurityHeaders()
            .RemoveServerHeader()
            .AddPermissionsPolicy(builder =>
            {
                builder.AddAccelerometer() // accelerometer 'self' *.la.gov
                        .None();

                builder.AddAmbientLightSensor() // ambient-light-sensor 'self' *.la.gov
                        .None();

                builder.AddAutoplay() // autoplay 'self'
                        .None();

                builder.AddCamera() // camera 'none'
                        .None();

                builder.AddEncryptedMedia() // encrypted-media 'self'
                        .Self();

                builder.AddFullscreen() // fullscreen *:
                        .All();

                builder.AddGeolocation() // geolocation 'none'
                        .All();

                builder.AddGyroscope() // gyroscope 'none'
                        .None();

                builder.AddMagnetometer() // magnetometer 'none'
                        .None();

                builder.AddMicrophone() // microphone 'none'
                        .None();

                builder.AddMidi() // midi 'none'
                        .None();

                builder.AddPayment() // payment 'none'
                        .None();

                builder.AddPictureInPicture() // picture-in-picture 'none'
                        .None();

                builder.AddSpeaker() // speaker 'none'
                        .None();

                builder.AddSyncXHR() // sync-xhr 'none'
                        .None();

                builder.AddUsb() // usb 'none'
                        .None();

                builder.AddVR() // vr 'none'
                        .None();

            })
            .AddFeaturePolicy(builder =>
            {
                builder.AddAccelerometer() // accelerometer 'self' *.la.gov
                    .None();

                builder.AddAmbientLightSensor() // ambient-light-sensor 'self' *.la.gov
                    .None();

                builder.AddAutoplay() // autoplay 'self'
                    .None();

                builder.AddCamera() // camera 'none'
                    .None();

                builder.AddEncryptedMedia() // encrypted-media 'self'
                    .Self();

                builder.AddFullscreen() // fullscreen *:
                    .All();

                builder.AddGeolocation() // geolocation 'none'
                  .All();

                builder.AddGyroscope() // gyroscope 'none'
                    .None();

                builder.AddMagnetometer() // magnetometer 'none'
                    .None();

                builder.AddMicrophone() // microphone 'none'
                    .None();

                builder.AddMidi() // midi 'none'
                    .None();

                builder.AddPayment() // payment 'none'
                    .None();

                builder.AddPictureInPicture() // picture-in-picture 'none'
                    .None();

                builder.AddSpeaker() // speaker 'none'
                    .None();

                builder.AddSyncXHR() // sync-xhr 'none'
                    .None();

                builder.AddUsb() // usb 'none'
                    .None();

                builder.AddVR() // vr 'none'
                    .None();
            })
            //.AddReportingEndpoints(builder =>
            //{
            //    builder.AddDefaultEndpoint("https://localhost:5008/en-us/reports");
            //})
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddObjectSrc().Self();
                // builder.AddScriptSrc().From("*").ReportSample();
                //builder.AddReportTo("default");
                builder.AddFormAction().Self().
                        //allow anything from leo state domain
                        // qas leo has a port#
                        Sources.AddRange(new List<string> { "*.louisiana.gov",
                                "*.louisiana.gov:*" ,
                            "*.la.gov"});
                //builder.AddFrameAncestors().OverHttps();
            });

        app.UseSecurityHeaders(policyCollection);

        return app;
    }
}
