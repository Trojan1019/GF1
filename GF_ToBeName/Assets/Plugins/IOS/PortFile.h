//
//  PortFile.h
//  Unity-iPhone
//
//  Created by Tynn on 2018/1/18.
//

extern "C"
{
	typedef void (*OnPurchasedDelegate) (const char *object);
	typedef void (*OnShowingTransactionAsInProgressDelegate) (bool object);
	typedef void (*OnPurchasedFailedDelegate) (const char *object);

	void init();
	void initAccount();

	void login();
	void logout();

	void pay(const char* name);

	void hello(const char* name,const char* value);

	void initInteraction();

	void logHisHeightWithName(const char *name);

	void logHisAgeWithName(const char *name);

	///调用unity的方法
	void callUnity(const char* methodName,const char* msg);

}

