//
//  Interactor.m
//  Unity-iPhone
//
//  Created by pony on 2021/8/12.
//

#import "Interactor.h"

@implementation Interactor

///实例调用
- (void)logHeightWithName:(NSString *)name{
  if ([name isEqualToString:@"xiaoming"]) {
      NSLog(@"xiaoming的身高是175cm");
  }else{
      NSLog(@"dabai的身高是188cm");
  }
}
- (const char*)getAppInfo{
    NSDictionary *infoDictionary = [[NSBundle mainBundle] infoDictionary];//获取app版本信息

    NSLog(@"%@",infoDictionary);  //这里会得到很对关于app的相关信息

    // app名称
    NSString *app_Name = [infoDictionary objectForKey:@"CFBundleDisplayName"];

    // app版本
    NSString *app_Version = [infoDictionary objectForKey:@"CFBundleShortVersionString"];

    // app build版本
    NSString *app_build = [infoDictionary objectForKey:@"CFBundleVersion"];
    NSString *bundleIdentifier = [infoDictionary objectForKey:@"CFBundleIdentifier"];
    

    //设备名称
    NSString* deviceName = [[UIDevice currentDevice] systemName];

    NSLog(@"应用名称: %@",app_Name );
    NSLog(@"应用版本号: %@",app_Version );
    NSLog(@"应用包名: %@",bundleIdentifier );
    NSLog(@"AppBuild: %@",app_build );
    
    NSLog(@"设备名称: %@",deviceName );

    //手机系统版本
    NSString* phoneVersion = [[UIDevice currentDevice] systemVersion];

    NSLog(@"手机系统版本: %@", phoneVersion);
    //手机型号

    NSString* phoneModel = [[UIDevice currentDevice] model];
    NSLog(@"手机型号: %@",phoneModel );

    //地方型号  （国际化区域名称）
    NSString* localPhoneModel = [[UIDevice currentDevice] localizedModel];

    NSLog(@"国际化区域名称: %@",localPhoneModel );
    
//    NSMutableString *array=[[NSMutableString alloc] init];
//    [array appendFormat:@"appName=%@",app_Name];
//    [array appendFormat:@"&package=%@",bundleIdentifier];
//    [array appendFormat:@"&appVersion=%@",app_Version];
//    [array appendFormat:@"&appCode=%@",app_build];
//    [array appendFormat:@"&deviceName=%@",deviceName];
//
//    NSString *str = [array copy];
//    return [str UTF8String];
    
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    [dict setObject:app_Name forKey:@"appName"];
    [dict setObject:bundleIdentifier forKey:@"package"];
    [dict setObject:app_Version forKey:@"appVersion"];
    [dict setObject:app_build forKey:@"appCode"];
    [dict setObject:deviceName forKey:@"deviceName"];
    NSError *parseError = nil;

    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:&parseError];

    NSString * str = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    return [str UTF8String];
}

///静态调用
+ (void)logAgeWithName:(NSString *)name{
  if ([name isEqualToString:@"xiaoming"]) {
      NSLog(@"xiaoming今年18岁");
  }else{
      NSLog(@"dabai今年22岁");
  }
    
}

@end
