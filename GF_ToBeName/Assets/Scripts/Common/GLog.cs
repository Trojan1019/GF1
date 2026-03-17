// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System;
// using NewSideGame;
//
// public class GLog
// {
//     public class Constant
//     {
//     }
//
//     public static void offerwall_get_reward_balance(int balance)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>
//         {
//             { "balance", balance },
//         };
//         _Record("offerwall_get_reward_balance", dict);
//     }
//
//     public static void unity_exception(string condition, string stackTrace)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>
//         {
//             { "condition", condition },
//             { "stack_trace", stackTrace }
//         };
//         _Record("unity_exception", dict, true);
//     }
//
//     public static void remote_res_update(string action_value, int res_v, int res_s, int res_t, string res_type = "")
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         dict.Add("action_value", action_value);
//         dict.Add("action_value_str", action_value);
//         dict.Add("res_v", res_v);
//         dict.Add("res_s", res_s);
//         dict.Add("res_t", res_t);
//         dict.Add("res_type", res_type);
//         _Record("remote_res_update", dict);
//     }
//
//     public static void unity_error(string condition, string stackTrace)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>
//         {
//             { "condition", condition },
//             { "stack_trace", stackTrace }
//         };
//         _Record("unity_error", dict, true);
//     }
//
//     public static void unity_error_res(string name, string status)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>
//         {
//             { "res_name", name },
//             { "res_status", status }
//         };
//         _Record("unity_error_res", dict, true);
//     }
//
//     public static void device_measurement(string email)
//     {
// #if UNITY_IOS
//         if (!string.IsNullOrEmpty(email))
//             FirebaseAnalytics.InitiateOnDeviceConversionMeasurementWithEmailAddress(email);
// #endif
//     }
//
//     private static void _Record(string path, Dictionary<string, object> paras, bool trackOnlyTA = false)
//     {
//         if (paras == null)
//         {
//             paras = new Dictionary<string, object>();
//         }
//
//         if (trackOnlyTA) return;
//     }
//
//     private static void _Record(string path, Dictionary<string, object> paras, Dictionary<string, object> extraParas)
//     {
//         if (paras == null)
//         {
//             paras = new Dictionary<string, object>();
//         }
//
//         KBSDKManager.Usage.record(path, paras);
//         // 上报Adjust
//         AppsFlyerManager.Instance.TrackEvent(path, paras);
//         // 上报Firebase
//         if (extraParas != null && extraParas.Count > 0)
//         {
//             foreach (var eP in extraParas)
//             {
//                 if (!paras.ContainsKey(eP.Key))
//                     paras.Add(eP.Key, eP.Value);
//             }
//         }
//
//         SendFirebaseEvent(path, paras);
//     }
//     public static void payment_init(string currency_type, float pay_amount, float payment_price_usd,
//         string payment_name, string payment_id, string pay_source, string payment_id_vs)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         dict["currency_type"] = currency_type;
//         dict["pay_amount"] = pay_amount;
//         dict["payment_price_usd"] = payment_price_usd;
//         dict["payment_name"] = payment_name;
//         dict["payment_id"] = payment_id;
//         dict["payment_id_vs"] = payment_id_vs;
//         dict["pay_source"] = pay_source;
//         _Record("payment_init", dict);
//     }
//
//     public static void payment_fail(string currency_type, float pay_amount, string payment_name, string payment_id,
//         string token, string pay_source, string fail_reason, string payment_id_vs)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         dict["currency_type"] = currency_type;
//         dict["pay_amount"] = pay_amount;
//         dict["payment_name"] = payment_name;
//         dict["payment_id"] = payment_id;
//         dict["payment_id_vs"] = payment_id_vs;
//         dict["pay_source"] = pay_source;
//         dict["fail_reason"] = fail_reason;
//         _Record("payment_fail", dict);
//     }
//
//     public static void payment_state_check(string state, string payment_id, string transaction_id, string user_type,
//         string payment_id_vs)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         dict["payment_state"] = state;
//         dict["payment_id"] = payment_id;
//         dict["payment_id_vs"] = payment_id_vs;
//         dict["payment_transaction_id"] = transaction_id;
//         dict["user_purchase_type"] = user_type;
//         _Record("payment_state_check", dict);
//     }
//
//     public static void payment_success(string currency_type, float pay_amount, float payment_price_usd,
//         string payment_name, string payment_id, bool is_first_pay, int pay_type, string pay_source,
//         string transaction_id, string payment_id_vs)
//     {
//         // 是否上报Adjust
//         bool toAdjust = true;
//         float amountAdjust = pay_amount;
//         try
//         {
//             // 判断累计金额
//             // float targetMax = GameEntry.Config.GetFloat(Constant.Config.IapAdjustMax, 50f);
//             // toAdjust = ProxyManager.UserProxy.Model.totalPayAmountUsd < targetMax;
//             // // 处理单笔金额
//             // if (payment_price_usd > 2.99f)
//             // {
//             //     int discount = GameEntry.Config.GetInt(Constant.Config.IapAdjustDiscount, 30);
//             //     amountAdjust = pay_amount * (100 - discount) / 100;
//             // }
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"PaymentSuccess : {e.Message} - {e.StackTrace}");
//             toAdjust = true;
//         }
//
//         // 内购统计
//         ProxyManager.UserProxy.AddPayment(pay_amount, payment_price_usd, currency_type);
//         // 埋点
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         dict["currency_type"] = currency_type;
//         dict["pay_amount"] = pay_amount;
//         dict["payment_price_usd"] = payment_price_usd;
//         dict["payment_name"] = payment_name;
//         dict["payment_id"] = payment_id;
//         dict["payment_id_vs"] = payment_id_vs;
//         dict["is_first_pay"] = is_first_pay;
//         dict["pay_type"] = pay_type;
//         dict["pay_source"] = pay_source;
//         dict["payment_transaction_id"] = transaction_id;
//         _RecordRevenue("payment_success", dict, currency_type, pay_amount, amountAdjust, toAdjust);
//         pay_other(payment_price_usd, payment_id);
// #if UNITY_IOS
//         // 上报FB
//         try
//         {
//             // int fbTrack = GameEntry.Config.GetInt(GUGame.Constant.Config.IapFbEnableTrack, 0);
//             // if (fbTrack == 1)
//             // {
//             //     Facebook.Unity.FB.LogPurchase(pay_amount, currency_type);
//             // }
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"PaymentSuccess : {e.Message} - {e.StackTrace}");
//         }
// #endif
//     }
//
//     public static void subscribe_state_check(string type, bool state)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         dict["vip_check_type"] = type;
//         dict["vip_state"] = state;
//         _Record("subscribe_state_check", dict);
//     }
//
//     public static void transaction_check(string transaction_id, int fail_type, string msg = "")
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         dict.Add("transaction_id", transaction_id);
//         dict.Add("fail_type", fail_type);
//         dict.Add("message", msg);
//         _Record("transaction_check", dict, true);
//     }
//
//     public static void notification_rec(string content, int id)
//     {
//         var dict = new Dictionary<string, object>();
//         dict["content"] = content;
//         dict["id"] = id;
//         _Record("notification_rec", dict);
//     }
//
//     public static void notification_open(string content, int id)
//     {
//         var dict = new Dictionary<string, object>();
//         dict["content"] = content;
//         dict["id"] = id;
//         _Record("notification_open", dict);
//     }
//
//     public static void mediation_ad_show(Dictionary<string, object> dict)
//     {
//         Debug.Log("==> lyly Glog mediation_ad_show");
//         KBSDKManager.Usage.record("mediation_ad_show", dict);
//     }
//
//     public static void first_load_m_ad_event(Dictionary<string, object> dict)
//     {
//         Debug.Log("==> lyly Glog first_load_m_ad_event");
//         KBSDKManager.Usage.record("first_load_m_ad_event", dict);
//     }
//
//     public static void load_max_ad_event(Dictionary<string, object> dict)
//     {
//         Debug.Log("==> lyly Glog load_max_ad_event");
//         KBSDKManager.Usage.record("load_max_ad_event", dict);
//     }
//
//     public static void af_conversion_data(Dictionary<string, object> data)
//     {
//         Dictionary<string, object> dict = new Dictionary<string, object>();
//         if (data.ContainsKey("af_status")) dict["af_cv_status"] = data["af_status"];
//         if (data.ContainsKey("media_source")) dict["af_cv_media_source"] = data["media_source"];
//         if (data.ContainsKey("is_first_launch")) dict["af_cv_is_first_launch"] = data["is_first_launch"];
//         if (data.ContainsKey("campaign")) dict["af_cv_campaign"] = data["campaign"];
//         if (data.ContainsKey("campaign_id")) dict["af_cv_campaign_id"] = data["campaign_id"];
//         _Record("af_conversion_data", dict);
//     }
//
//     public static void ai_puzzle_level(string level_mode, int level, int use_hint_num)
//     {
//         var dict = new Dictionary<string, object>();
//         dict["level_mode"] = level_mode;
//         dict["level"] = level;
//         dict["use_hint_num"] = use_hint_num;
//         _Record("ai_puzzle_level", dict);
//     }
//
//     public static void ai_puzzle_dl(int id)
//     {
//         var dict = new Dictionary<string, object>();
//         PuzzleTravelManager.Instance.GetPuzzleBackgroundName(id, dict);
//         if (dict.Count > 0)
//             _Record("ai_puzzle_dl", dict);
//     }
//
//     private static readonly HashSet<int> Levels = new HashSet<int>
//     {
//         20, 30, 40, 50, 60, 70, 80, 90, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000,
//         1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000, 3200, 3400, 3600, 3800,
//         4000, 4200, 4400, 4600, 4800, 5000
//     };
//
//     public static void ai_puzzle_level_other(int level)
//     {
//         if (Levels.Contains(level))
//         {
//             var dict = new Dictionary<string, object>();
//             _Record($"s_level_{level}", dict);
//         }
//     }
//
//     private static readonly Dictionary<string, string> paymentEvents = new Dictionary<string, string>
//     {
//         { "tt_noads_pack_4.99", "s_noads_pack_purchase" },
//         { "tt_coin_pack_1.99", "s_coin_pack_1.99_purchase" },
//         { "tt_coin_4.99", "s_coin_4.99_purchase" },
//         { "tt_treasure_fest_0.99", "s_treasure_fest_0.99_purchase" },
//         { "tt_sign_pass_1.99", "s_sign_pass_1.99_purchase" },
//         { "tt_coin_pack_9.99", "s_coin_pack_9.99_purchase" },
//         { "tt_weekly_sub_4.99", "s_weekly_purchase" },
//         { "tt_monthly_sub_9.99", "s_monthly_purchase" },
//         { "tt_yearly_sub_99.99", "s_yearly_purchase" }
//     };
//
//     public static void pay_other(float pay_amount, string payment_id)
//     {
//         float value = pay_amount * (1 - 0.3f);
//
//         if (paymentEvents.TryGetValue(payment_id, out string eventName))
//         {
//             var dict = new Dictionary<string, object> { { "value", value.ToString("F2") } };
//             _Record(eventName, dict);
//         }
//     }
// }