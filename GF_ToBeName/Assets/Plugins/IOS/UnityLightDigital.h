#pragma once

@interface UnityLightDigital : NSObject

+ (UnityLightDigital *)sharedInstance;

- (NSString*) getDeviceId;

@property (nonatomic, assign) int level;

@end

