//
//  CNPNativeMethods.m
//  Unity-iPhone
//
//  Created by lok on 2020/5/6.
//

//#import "CNPNativeMethods.h"
#import <UserNotifications/UserNotifications.h>
#import <Foundation/Foundation.h>
#import "UnityAppController.h"
//#import <AppsFlyerLib/AppsFlyerLib.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <StoreKit/StoreKit.h>

#ifdef __cplusplus
extern "C"
{
#endif

typedef void(^LoginBlock) (const char*);

void checkCurrentNotificationStatus()
{
     [[UNUserNotificationCenter currentNotificationCenter] getNotificationSettingsWithCompletionHandler:^(UNNotificationSettings * _Nonnull settings) {
        
        if (settings.authorizationStatus == UNAuthorizationStatusAuthorized)
        {
//            [[CloudUsageManager sharedManager] record:@"noti_enable" values:@{@"enable_status":@"true"}];
        }else{
            // 没权限
//            [[CloudUsageManager sharedManager] record:@"noti_enable" values:@{@"enable_status":@"false"}];
        }
         
    }];
}


bool getTestConfig()
{
    
    return false;
}

void AppFlyerEventTrack(float price,const char* iso,const char* ID){
//    NSString *isoString = [NSString stringWithUTF8String:iso];
//    NSString *IDString = [NSString stringWithUTF8String:ID];
//    NSMutableDictionary *dicM = [NSMutableDictionary dictionary];
//    if (price > 0) {
//        [dicM setValue:@(price) forKey:AFEventParamRevenue];
//    }
//    if (IDString.length) {
//        [dicM setValue:IDString forKey:AFEventParamContentId];
//    }
//    if (isoString.length) {
//        [dicM setValue:isoString forKey:AFEventParamCurrency];
//    }
//    [[AppsFlyerLib shared] logEvent:AFEventPurchase withValues:dicM];
}

void requestTrackingAuthorizationWithCompletionHandler() {
      if (@available(iOS 14, *)) {
          [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
              NSString *stringInt = [NSString stringWithFormat:@"%lu",(unsigned long)status];
              const char* charStatus = [stringInt UTF8String];
              UnitySendMessage("ATTManager", "HandleATTRequestAuthorization", charStatus);
          }];
      } else {
          UnitySendMessage("ATTManager", "HandleATTRequestAuthorization", "-1");
      }
  }
 
int getAppTrackingAuthorizationStatus() {
      if (@available(iOS 14, *)) {
          return (int)[ATTrackingManager trackingAuthorizationStatus];
      } else {
          return -1;
      }
}

bool isAvailableIOSSysterVersion(float version){
    
    if (@available(iOS 14.5, *)) {
        return  true;
    }
    return false;
}

void sendRecordImmediately(){
    
//    [[CloudUsageManager sharedManager] sendImmediately];
}

bool showFiveStarComment()
{
    if([SKStoreReviewController respondsToSelector:@selector(requestReview)]) {// iOS 10.3 以上支持
        [SKStoreReviewController requestReview];
        return true;
    } else { // iOS 10.3 之前的使用这个
        return false;
    }
}

void Vibrate(int level)
{
  UIImpactFeedbackGenerator *feedBackGenertor = [[UIImpactFeedbackGenerator alloc] initWithStyle:(UIImpactFeedbackStyle)level];
  [feedBackGenertor impactOccurred];

}

int _SaveToGallery(const char *path)
{
    NSString *imagePath = [NSString stringWithUTF8String:path];
    if(![[NSFileManager defaultManager] fileExistsAtPath:imagePath])
    {
        return 0;
    }
    
    UIImage *image = [UIImage imageWithContentsOfFile:imagePath];
    if(image)
    {
        UIImageWriteToSavedPhotosAlbum( image, nil, NULL, NULL );
        return 1;
    }
    return 0;
}

#ifdef __cplusplus
}
#endif



