//
//  IAPRecordHelper.m
//  UnityFramework
//
//  Created by 蔡卓峰 on 2021/1/6.
//

#import "IAPRecordHelper.h"
#import "UnityFramework.h"

@interface IAPRecordHelper ()

@end

@implementation IAPRecordHelper

+ (instancetype)sharedInstance
{
    static IAPRecordHelper *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[IAPRecordHelper alloc] init];
    });
    return sharedInstance;
}

- (void)startObserve {
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    // 发送票据
    [self sendAppReceiptWhenAppStart];
}

- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions {
    for(SKPaymentTransaction *transaction in transactions) {
        switch (transaction.transactionState) {

            case SKPaymentTransactionStatePurchasing:
                // Item is still in the process of being purchased
                if (transaction.payment != nil) {
                    [[UnityFramework getInstance] sendMessageToGOWithName:"KbSDK" functionName:"OnRevPurchasing" message:[transaction.payment.productIdentifier UTF8String]];
                }
                break;

            case SKPaymentTransactionStatePurchased: {
                //[IAP.sharedInstance uploadTransactionInfo:transaction];
                if (transaction.payment != nil) {
                    [[UnityFramework getInstance] sendMessageToGOWithName:"KbSDK" functionName:"OnRevPurchasedProduct" message:[transaction.payment.productIdentifier UTF8String]];
                }
                break;
            }
            case SKPaymentTransactionStateRestored: {
                if (transaction.payment != nil) {
                    [[UnityFramework getInstance] sendMessageToGOWithName:"KbSDK" functionName:"OnRevRestoredProduct" message:[transaction.payment.productIdentifier UTF8String]];
                }
                break;
            }
            case SKPaymentTransactionStateDeferred:
                break;
            case SKPaymentTransactionStateFailed: {
            }
                break;
        }
    }
    // 发送票据
    [self sendAppReceipt];
}

- (void)sendAppReceiptWhenAppStart
{
    NSString* receipt = [self getAppReceipt];
    [[UnityFramework getInstance] sendMessageToGOWithName:"KbSDK" functionName:"OnRevAppReceiptWhenAppStart" message:[receipt UTF8String]];
}

- (void)sendAppReceipt
{
    NSString* receipt = [self getAppReceipt];
    [[UnityFramework getInstance] sendMessageToGOWithName:"KbSDK" functionName:"OnRevAppReceipt" message:[receipt UTF8String]];
}

- (NSString*)getAppReceipt
{
    NSBundle* bundle = [NSBundle mainBundle];
    if ([bundle respondsToSelector: @selector(appStoreReceiptURL)])
    {
        NSURL *receiptURL = [bundle appStoreReceiptURL];
        if ([[NSFileManager defaultManager] fileExistsAtPath: [receiptURL path]])
        {
            NSData *receipt = [NSData dataWithContentsOfURL: receiptURL];

#if MAC_APPSTORE
            // The base64EncodedStringWithOptions method was only added in OSX 10.9.
            NSString* result = [receipt mgb64_base64EncodedString];
#else
            NSString* result = [receipt base64EncodedStringWithOptions: 0];
#endif

            return result;
        }
    }

    NSLog(@"No App Receipt found");
    return @"";
}

@end
