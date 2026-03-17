//
//  PortFile.m
//  Unity-iPhone
//
//  Created by Tynn on 2018/1/18.
//

#import "PortFile.h"
#import "Interactor.h"
#import "UnityInterface.h"

//@implementation PortFile
//
//@end

Interactor *inter = nil;

void init(){
    initInteraction();
    const char* appInfo = [inter getAppInfo];
    callUnity("InitReturn", appInfo);
}

void initAccount(){
  NSLog(@"ios initAccount");
  callUnity("InitAccountReturn", "ok");
}
void login(){
    NSLog(@"ios-login");
    callUnity("LoginReturn", "ok");
}
void logout(){
    NSLog(@"ios-logout");
    callUnity("LogoutReturn", "ok");
}

void pay(const char* name){
//    NSString *str = [[NSString alloc] initWithFormat:@"ios pay, %s", name];
    NSLog(@"ios pay, %s", name);
}

void hello(const char* name,const char* value){
    NSLog(@"ios hello:%s" , name);
}


//实例化Interaction
void initInteraction(){
  inter = [[Interactor alloc]init];
    NSLog(@"ios initInteraction");
}

void logHisHeightWithName(const char *name){
  //将C字符串转化为OC字符串
  NSString *hisName = [NSString stringWithCString:name encoding:NSUTF8StringEncoding];
  //调用Interaction的对象方法
  [inter logHeightWithName:hisName];
}

void logHisAgeWithName(const char *name){
  //将C字符串转化为OC字符串
  NSString *hisName = [NSString stringWithCString:name encoding:NSUTF8StringEncoding];
  //调用Interaction的类方法
  [Interactor logAgeWithName:hisName];
}

void callUnity(const char* methodName,const char* msg){
    UnitySendMessage("BridgeService", methodName, msg);
}
