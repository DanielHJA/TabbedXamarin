using System;
using System.Collections.Generic;
using System.Linq;
using UserNotifications;
using Foundation;
using UIKit;
using PushKit;
using AudioToolbox;
using CoreFoundation;
using Newtonsoft;
using Newtonsoft.Json;

namespace TabbedApplication.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            ConfigureNotifications(app);
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }

        public void ConfigureNotifications(UIApplication app)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (approved, err) =>
                {
                    if (approved)
                    {
                        UNUserNotificationCenter.Current.Delegate = this;
                        DispatchQueue.MainQueue.DispatchAsync(() => {
                            app.RegisterForRemoteNotifications();
                        });

                    }
                });
            }
            else
            {
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
        }

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            System.Console.WriteLine(response);
        }

        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            var userInfo = notification.Request.Content.UserInfo as NSDictionary;
            var pushNotificationObject = Extensions.DictionaryExtensions.DeserializePushNotificationObject<PushNotificationObject>(userInfo);
            System.Console.WriteLine(pushNotificationObject);

            SystemSound.Vibrate.PlayAlertSound();
            SystemSound.Vibrate.PlaySystemSound();
            completionHandler(UNNotificationPresentationOptions.Alert);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            string deviceTokenStr;
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                deviceTokenStr = deviceToken.DebugDescription.Replace("<", "").Replace(">", "").Replace(" ", "");
            }
            else
            {
                deviceTokenStr = deviceToken.Description.Replace("<", "").Replace(">", "").Replace(" ", "");
            }
            System.Console.WriteLine(deviceTokenStr);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            System.Console.WriteLine(error);
        }

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            var pushNotificationObject = Extensions.DictionaryExtensions.DeserializePushNotificationObject<PushNotificationObject>(userInfo);

            completionHandler(UIBackgroundFetchResult.NewData);
        }

    }
}


namespace Extensions {
    public class DictionaryExtensions {

        public static string MyDictionaryToJson(NSDictionary dict)
        {
            return string.Join(",", dict.Select(x => string.Format("\"{0}\":\"{1}\"", x.Key, x.Value)));
        }

        public static T DeserializePushNotificationObject<T>(NSDictionary userInfo)
        {
            NSError error;
            var data = NSJsonSerialization.Serialize(userInfo["aps"], NSJsonWritingOptions.PrettyPrinted, out error);
            var json = NSString.FromData(data, NSStringEncoding.UTF8).ToString();
            return JsonConvert.DeserializeObject<T>(json);
        }

    }
}