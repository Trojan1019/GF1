using System;

namespace NewSideGame
{
    public partial class UserProxy
    {
        public class DailySignResult
        {
            public bool success;
            public bool alreadySignedToday;
            public int streakDays;
            public int totalDays;
            public bool unlockedSkinReward;
        }

        public bool CanSignToday()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            return userModel.dailySignLastDate != today;
        }

        public DailySignResult SignToday()
        {
            DailySignResult result = new DailySignResult();
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            string yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyyMMdd");

            if (userModel.dailySignLastDate == today)
            {
                result.success = false;
                result.alreadySignedToday = true;
                result.streakDays = userModel.dailySignStreakDays;
                result.totalDays = userModel.dailySignTotalDays;
                result.unlockedSkinReward = false;
                return result;
            }

            if (userModel.dailySignLastDate == yesterday)
                userModel.dailySignStreakDays += 1;
            else
                userModel.dailySignStreakDays = 1;

            userModel.dailySignTotalDays += 1;
            userModel.dailySignLastDate = today;
            if (userModel.dailySignHistory == null)
                userModel.dailySignHistory = new System.Collections.Generic.List<string>();
            if (!userModel.dailySignHistory.Contains(today))
                userModel.dailySignHistory.Add(today);

            bool unlocked = false;
            if (!userModel.dailySkinRewardGranted && userModel.dailySignStreakDays >= 3)
            {
                userModel.dailySkinRewardGranted = true;
                unlocked = SkinManager.Instance.TryUnlockFirstRewardSkin();
            }

            Save();

            result.success = true;
            result.alreadySignedToday = false;
            result.streakDays = userModel.dailySignStreakDays;
            result.totalDays = userModel.dailySignTotalDays;
            result.unlockedSkinReward = unlocked;
            return result;
        }
    }
}

