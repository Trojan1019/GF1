#import "UnityLightDigital.h"
#import <Foundation/Foundation.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/ASIdentifierManager.h>

@implementation UnityLightDigital

+ (UnityLightDigital *)sharedInstance {
    static UnityLightDigital *manager = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        if(!manager) {
            manager = [[self alloc] init];
            manager.level = 0;
        }
    });
    return manager;
}

- (NSString *) getDeviceId {
    return [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
}


//C#直接调用的C函数
#if defined (__cplusplus)
extern "C" {
#endif

void nativeVibrate()
{
    UIImpactFeedbackGenerator *feedBackGenertor = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
    [feedBackGenertor impactOccurred];
}

float nativeSystemVersion()
{
    NSString *version= [UIDevice currentDevice].systemVersion;
    return version.doubleValue;
}

void nativeOpenAppSetting()
{
    NSURL *url = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
    if ([[UIApplication sharedApplication] canOpenURL:url]) {
        if (@available(iOS 10.0, *)) {
            [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:^(BOOL success) {
            }];
        }
    }
}

bool nativeCheckTrackingAuthorizationStatus()
{
    if (@available(iOS 14, *)) {
        ATTrackingManagerAuthorizationStatus status = ATTrackingManager.trackingAuthorizationStatus;
        return status == ATTrackingManagerAuthorizationStatusAuthorized;
    } else {
       return [ASIdentifierManager.sharedManager isAdvertisingTrackingEnabled];
    }
}

void nativeSetUserLevel(int level)
{
    NSLog(@"nativeSetUserLevel level=%d", level);
}

#if defined (__cplusplus)
}
#endif

@end
