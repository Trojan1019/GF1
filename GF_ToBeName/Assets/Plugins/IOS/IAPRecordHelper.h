//
//  IAPRecordHelper.h
//  UnityFramework
//
//  Created by 蔡卓峰 on 2021/1/6.
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

@interface IAPRecordHelper : NSObject<SKPaymentTransactionObserver>

+ (instancetype)sharedInstance;

- (void)startObserve;

@end

