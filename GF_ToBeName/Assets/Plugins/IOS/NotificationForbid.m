#import <objc/runtime.h>
#import <UIKit/UIKit.h>
#import <UserNotifications/UserNotifications.h>

void SwizzleMethod(Class class, SEL originalSelector, SEL swizzledSelector) {
    Method originalMethod = class_getInstanceMethod(class, originalSelector);
    Method swizzledMethod = class_getInstanceMethod(class, swizzledSelector);
    method_exchangeImplementations(originalMethod, swizzledMethod);
}

@implementation UNUserNotificationCenter (DisableRequest)
+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        SwizzleMethod([self class],
                      @selector(requestAuthorizationWithOptions:completionHandler:),
                      @selector(swizzled_requestAuthorizationWithOptions:completionHandler:));
    });
}

- (void)swizzled_requestAuthorizationWithOptions:(UNAuthorizationOptions)options
                               completionHandler:(void (^)(BOOL granted, NSError * _Nullable error))completionHandler {
    NSLog(@"==> lyly Intercepted notification authorization request. Returning denied.");
    if (completionHandler) {
        completionHandler(NO, nil); // 直接返回未授权
    }
}
@end

@implementation UIApplication (DisableRemoteNotifications)
+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        SwizzleMethod([self class],
                      @selector(registerForRemoteNotifications),
                      @selector(swizzled_registerForRemoteNotifications));
    });
}

- (void)swizzled_registerForRemoteNotifications {
    NSLog(@"==> lyly Intercepted remote notifications registration. Skipping registration.");
    // 直接不执行任何逻辑
}
@end
