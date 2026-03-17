//
//  Interactor.h
//  Unity-iPhone
//
//  Created by pony on 2021/8/12.
//

#import <Foundation/Foundation.h>
@interface Interactor :NSObject

- (const char*)getAppInfo;

//根据名字来输出他的身高
 - (void)logHeightWithName:(NSString *)name;

 //根据名字来输出他的年龄
 + (void)logAgeWithName:(NSString *)name;

@end
