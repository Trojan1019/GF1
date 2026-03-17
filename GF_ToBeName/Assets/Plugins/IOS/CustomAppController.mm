//
//  CustomAppController.m
//
//
//

#import "UnityAppController.h"
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import "UnityFramework.h"

@interface CustomAppController : UnityAppController
@end
 
IMPL_APP_CONTROLLER_SUBCLASS (CustomAppController)
 
@implementation CustomAppController

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    return YES;
}
- (void)applicationDidBecomeActive:(UIApplication *)application {
    [super applicationDidBecomeActive:application];
    [self initConfig:application];
    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            if ([NSThread isMainThread]) {
                [self initConfig:application];
                NSString* attStatus = status == ATTrackingManagerAuthorizationStatusAuthorized ? @"YES" : @"NO";
                [[UnityFramework getInstance] sendMessageToGOWithName:"KbSDK" functionName:"OnRevATTStatus" message:[attStatus UTF8String]];
            }
        }];
    } else {
        [self initConfig:application];
        NSString* attStatus = @"YES";
        [[UnityFramework getInstance] sendMessageToGOWithName:"KbSDK" functionName:"OnRevATTStatus" message:[attStatus UTF8String]];
    }
}
- (void)initConfig:(UIApplication *)application {
}
//不要打开这里否则UnityAppController回调不执行，导致unity的OnApplicationPause不执行
//- (void)applicationWillResignActive:(UIApplication *)application
//{
//}
//- (void)applicationDidEnterBackground:(UIApplication *)application
//{
//}
//- (void)applicationWillEnterForeground:(UIApplication *)application
//{
//}
//- (void)applicationWillTerminate:(UIApplication *)application {
//}
@end
